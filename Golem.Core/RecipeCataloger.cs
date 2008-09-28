using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using Golem.Core;

namespace Golem.Core
{

    public class RecipeCatalogue
    {
        public RecipeCatalogue(List<LoadedAssemblyInfo> found)
        {
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecipeCataloger
    {
        //loads assemblies
        //building tree of recipes
        //reflecting on types

        private readonly string[] _searchPaths;
        private readonly List<LoadedAssemblyInfo> _loadedAssemblies;
        
        public RecipeCataloger(params string[] searchPaths)
        {
            _searchPaths = searchPaths;
            _loadedAssemblies = new List<LoadedAssemblyInfo>();
        }
        
        public ReadOnlyCollection<LoadedAssemblyInfo> LoadedAssemblies { get { return _loadedAssemblies.AsReadOnly(); } }
        

        /// <summary>
        /// Queries loaded assembly info to list all assemblies examined
        /// </summary>
        public ReadOnlyCollection<Assembly> AssembliesExamined
        {
            get
            {
                return (from la in _loadedAssemblies select la.Assembly).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Queries loaded assembly info to list the associated recipes with each
        /// </summary>
        public List<Recipe> Recipes
        { 
            get
            {
                return (from la in _loadedAssemblies from r in la.FoundRecipes select r).ToList();
            }
        }
 
        /// <summary>
        /// Lists assemblies that contained recipes
        /// </summary>
        public List<LoadedAssemblyInfo> LoadedAssembliesContainingRecipes
        { 
            get
            {
                return (from la in _loadedAssemblies where la.FoundRecipes.Count > 0 select la).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Recipe> CatalogueRecipes()
        {
            var fileSearch = new RecipeFileSearch(_searchPaths);
            fileSearch.BuildFileList();

            PreLoadAssembliesToPreventAssemblyNotFoundError(
                fileSearch.FoundAssemblyFiles.ToArray()
                );
            
            ExtractRecipesFromPreLoadedAssemblies();
            return Recipes;
        }
        
        private void PreLoadAssembliesToPreventAssemblyNotFoundError(FileInfo[] assemblyFiles)
        {   
            //TODO: Preloading all assemblies in the solution is BRUTE FORCE! Should separate discovery of recipes
            //      from invocation. Perhpas use LoadForReflection during discovery. We'd then need to have 
            //      Assemblies loaded for real before invokation (see TaskRunner), along with satellite assemblies.

            foreach (var file in assemblyFiles)
                //loading the core twice is BAD because "if(blah is RecipeAttribute)" etc will always fail
                if( ! file.Name.StartsWith("Golem.Core") && ! LoadedAssemblies.Any(la=>la.File.Name == file.Name))
                {
                    try
                    {
                        var i =  new LoadedAssemblyInfo
                                {
                                    Assembly = Assembly.LoadFrom(file.FullName),
                                    File = file
                                };
                        _loadedAssemblies.Add(i);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Ooops: " + e.Message);
                    }
                }

            
        }

        private void ExtractRecipesFromPreLoadedAssemblies()
        {
            foreach(var la in LoadedAssemblies)
                try
                {
                    foreach (var type in la.Assembly.GetTypes())
                        ExtractRecipesFromType(type, la);
                }
                catch (ReflectionTypeLoadException e)
                {
//                    foreach(var ex in e.LoaderExceptions)
//                        Console.WriteLine("Load Exception: " + ex.Message);
                }
            
        }

        
        private void ExtractRecipesFromType(Type type, LoadedAssemblyInfo la)
        {
            if (!typeof(Recipe).IsAssignableFrom(type)) return;
            if (type.IsAbstract) return;
            
            //create recipe details from attribute
            Recipe recipe = (Recipe)Activator.CreateInstance(type);

            //associate recipe with assembly
            la.FoundRecipes.Add(recipe);

            recipe.RegisterTasks();

            //trawl through and add the tasks
            foreach (var task in recipe.Tasks)
            {
                for (var i = 0; i < task.Dependencies.Count; i++)
                {
                    var dependency = task.Dependencies[i];
                    var dependentTask = FindDependentTask(recipe.Tasks, dependency);

                    if (dependentTask == null)
                        throw new InvalidDependentTaskException(dependency);

                    task.ResolvedDependencies.Add(dependentTask);
                }
            }
        }

        private Task FindDependentTask(List<Task> tasks, string dependency)
        {
            foreach (var task in tasks)
            {
                // TODO: Improve this! Especially the EndsWith one, because that can cause
                //       all kinds of conflicts. Should figure out if the current task is
                //       at the same level as the iterated task, then use that as a starting
                //       point for the match.
                //
                //       Currently:
                //       dependecy = "Core.Bacon" will match "Cheese.Core.Bacon" and "House.Core.Bacon"
                if (task.Name == dependency) return task;
                if (task.FullName == dependency) return task;
                if (task.FullName.EndsWith(dependency)) return task;
            }

            return null;
        }
    }
}