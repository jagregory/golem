using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Golem.Core
{
    public class RecipeBase
    {
        public IList<Assembly> AllAssemblies = new List<Assembly>();
        public IList<Recipe> AllRecipes = new List<Recipe>();
    }

    public class TaskRunner
    {
        private RecipeSearch search;
        
        public TaskRunner(RecipeSearch aSearch)
        {
            search = aSearch;
        }

        public void Run(Recipe recipe, Task task)
        {
            var recipeInstance = Activator.CreateInstance(recipe.Class);
            SetContextualInformationIfInheritsRecipeBase(recipeInstance);
            task.Method.Invoke(recipeInstance, null);
        }

        private void SetContextualInformationIfInheritsRecipeBase(object recipeInstance)
        {
            var tmpRecipe = recipeInstance as RecipeBase;
            
            if(tmpRecipe == null)
                return;

            tmpRecipe.AllAssemblies = search.AssembliesExamined;
            tmpRecipe.AllRecipes = search.Recipes;
            
        }

        //TODO: Run is Too long
        //TODO: Nesting depth is too deep
        public void Run(string recipeName, string taskName)
        {
            if(search.Recipes.Count==0)
                search.FindRecipesInFiles();

            foreach (var r in search.Recipes)
            {
                if(r.Name.ToLower() == recipeName.ToLower())
                {
                    foreach(var t in r.Tasks)
                    {
                        if(t.Name.ToLower() == taskName.ToLower())
                        {
                            foreach(var methodInfo in t.DependsOnMethods)
                            {
                                Run(r, r.GetTaskForMethod(methodInfo));
                            }
                            Run(r, t);
                            return;
                        }
                    }
                }
            }
        }

//        public RunManifest BuildRunManifest(string recipeName, string taskName)
//        {
//            var manifest = new RunManifest();
//            var search = new RecipeSearch();
//            var found = search.FindRecipesInFiles();
//
//            foreach (var r in found)
//            {
//                if (r.Name.ToLower() == recipeName.ToLower())
//                {
//                    foreach (var t in r.Tasks)
//                    {
//                        if (t.Name.ToLower() == taskName.ToLower())
//                        {
//                            foreach(var d in t.DependsOnMethods)
//                            {
//                                manifest.Add(null);
//                            }
//                            manifest.Add(t);
//                            
//                        }
//                    }
//                }
//            }
//            return manifest;
//        }


    }

    public class RunManifest
    {

        public OrderedDictionary ToRun;
        public void Add(Task t)
        {
            if(! ToRun.Contains(t))
            {
                ToRun.Add(t,new RunManifestItem{Task=t});
            }
        }
        public RunManifest()
        {
            ToRun = new OrderedDictionary();
        }

        public Task TaskAt(int position)
        {
            return (Task) ToRun[position];
        }
    }
    public class RunManifestItem
    {
        public Task Task;
        public bool HasRun;
    }
}