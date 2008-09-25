using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Golem.Core;
using Golem.Core;

namespace Golem.Runner
{
    public class Program
    {
        private static readonly IList<string> preferredLocations = new[] { "recipes" };

        public static void Main(string[] args)
        {
            Console.WriteLine("Golem (Beta) 2008\nYour friendly executable .NET build tool. \n");

            IList<Recipe> found;
            RecipeCataloger finder = CreateCataloger(out found);
            
            if(args.Length > 0)
            {
                if(args[0] == "-T")
                {
                    
                    ShowList(found);
                    return;
                }
                else if(args[0].ToLower() == "-?")
                {
                    Console.WriteLine("Help: \n");
                    Console.WriteLine("golem -T   # List build tasks");
                    Console.WriteLine("golem -?   # Show this help");
                    Console.WriteLine("golem -?   # Show this help");
                    return;
                }

                var parts = args[0].Split(':');
                var runner = new TaskRunner(finder);
                
                if(parts.Length == 2)
                    runner.Run(parts[0],parts[1]);
                else
                {
                    Console.WriteLine("Type golem -? for help, or try one of the following tasks:\n");
                    ShowList(found);
                }
            }
            else
            {
                
                ShowList(found);
            }
            
        }

        private static RecipeCataloger CreateCataloger(out IList<Recipe> found)
        {
            RecipeCataloger finder;
            
            var config = new Configuration();

            //TODO: Remove false when ready. Caching location of recipes isn't going to be useful until we can dynamically load dependant assemblies on the fly
            if (false && config.SearchPaths.Count > 0)
                finder = new RecipeCataloger(config.SearchPaths.ToArray());
            else
            {
                Console.WriteLine("Scanning directories for Build Recipes...");
                
                var searchPaths = GetSearchPaths();

                finder = new RecipeCataloger(searchPaths.ToArray());
            }

            found = finder.CatalogueRecipes();

            if(config.SearchPaths.Count == 0)
            {
                config.SearchPaths.AddRange(finder.LoadedAssemblies.Select(s=>s.File.FullName));
                config.Save();
            }
            return finder;
        }

        /// <summary>
        /// Gets the paths to search for recipes in.
        /// </summary>
        /// <returns>List of search paths</returns>
        private static List<string> GetSearchPaths()
        {
            Console.WriteLine("Scanning for preferred locations...");
            Console.WriteLine();

            var searchPaths = new List<string>();

            // looking for any preferred locations, which are pre-named folders where
            // assemblies can be dropped. Doing this currently because the regular
            // search dies on my rather large folder structure.
            foreach (var location in preferredLocations)
            {
                if (Directory.Exists(location))
                {
                    Console.WriteLine("Found: " + location);
                    searchPaths.Add(location);
                }
            }

            if (searchPaths.Count == 0)
            {
                Console.WriteLine("None found, recursively searching  (could take a while)...");
                searchPaths.Add(Environment.CurrentDirectory);
            }

            return searchPaths;
        }

        private static void ShowList(IList<Recipe> found)
        {
            foreach(var recipe in found)
            {
                //Console.WriteLine("\n{0}\n",!String.IsNullOrEmpty(recipe.Description) ? recipe.Description : recipe.Name);
                foreach(var task in recipe.Tasks)
                {
                    var start = "golem " + recipe.Name + ":" + task.Name;
                    Console.WriteLine(start.PadRight(30) +"# " + task.Description);
                }
            }

            if (found.Count == 0)
                Console.WriteLine("No recipes found under {0}", new DirectoryInfo(Environment.CurrentDirectory).Name);
        }
    }
}
