using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Golem.Core
{
    public class TaskRunner
    {
        private RecipeCataloger cataloger;
        
        public TaskRunner(RecipeCataloger aCataloger)
        {
            cataloger = aCataloger;
        }

        public void Run(Recipe recipe, Task task)
        {
            //TODO: Tasks should run in their own appdomain. 
            //      We need to create an app domain that has the 
            //      base dir the same as the target assembly
            foreach (var dependency in task.ResolvedDependencies)
            {
                Run(recipe, dependency);
            }

            task.Execute();
        }

        //TODO: Run is Too long
        //TODO: Nesting depth is too deep
        public void Run(string recipeName, string taskName)
        {
            if(cataloger.Recipes.Count==0)
                cataloger.CatalogueRecipes();

            foreach (var r in cataloger.Recipes)
            {
                if(r.Name.ToLower() == recipeName.ToLower())
                {
                    foreach(var t in r.Tasks)
                    {
                        if(t.Name.ToLower() == taskName.ToLower())
                        {
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
//            var cataloger = new RecipeCataloger();
//            var found = cataloger.CatalogueRecipes();
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