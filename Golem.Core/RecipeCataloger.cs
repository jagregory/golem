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
        public ReadOnlyCollection<Recipe> Recipes
        { 
            get
            {
                return (from la in _loadedAssemblies from r in la.FoundRecipes select r).ToList().AsReadOnly();
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
        public IList<Recipe> CatalogueRecipes()
        {
            var fileSearch = new RecipeFileSearch(_searchPaths);
            fileSearch.BuildFileList();

            PreLoadAssembliesToPreventAssemblyNotFoundError(
                fileSearch.FoundAssemblyFiles.ToArray()
                );
            
            ExtractRecipesFromPreLoadedAssemblies();
            return Recipes.ToArray();
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
            if (!typeof(RecipeBase).IsAssignableFrom(type)) return;
            if (type.IsAbstract) return;
            
            //create recipe details from attribute
            Recipe recipe = CreateRecipeForType(type);

            //associate recipe with assembly
            la.FoundRecipes.Add(recipe);

            //trawl through and add the tasks
            AddTasksToRecipe(type, recipe);
            
        }

        private static Recipe CreateRecipeForType(Type type)
        {
            var recipeName = type.Name.ToLower();

            if (recipeName.EndsWith("recipe"))
                recipeName = recipeName.Substring(0, recipeName.Length - 6);

            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>(false);

            return new Recipe
            {
                Class = type,
                Name = recipeName,
                Description = descriptionAttribute == null ? "" : descriptionAttribute.Description
            };
        }

        private static void AddTasksToRecipe(Type type, Recipe recipe)
        {
            //loop through methods in class
            foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly ))
            {
                var dependsAttribute = method.GetCustomAttribute<DependsOnAttribute>(false);
                var dependencies = new string[] {};

                if (dependsAttribute != null)
                    dependencies = dependsAttribute.Dependencies;

                //get the task based on attribute contents
                Task t = CreateTaskFromMethod(method);

                //build list of dependent tasks
                CreateDependentTasks(type, dependencies, t);

                //add the task to the recipe
                recipe.Tasks.Add(t);
            }
        }

        private static void CreateDependentTasks(Type type, string[] dependencies, Task task)
        {
            foreach(string methodName in dependencies)
            {
                var dependee = type.GetMethod(methodName);
                if(dependee == null) throw new Exception(String.Format("No dependee method {0}",methodName));
                task.DependsOnMethods.Add(dependee);
            }
        }

        private static Task CreateTaskFromMethod(MethodInfo method)
        {
            string name = method.Name.ToLower();
            var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>(false);

            if (name.EndsWith("task"))
                name = name.Substring(0, name.Length - 4);

            return new Task
            {
                Name = name,
                Method = method,
                Description = descriptionAttribute == null ? "" : descriptionAttribute.Description
            };
        }
    }
}