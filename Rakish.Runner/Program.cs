﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rakish.Core;

namespace Rakish.Runner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var finder = new RecipeFinder();
            var found = finder.FindRecipesInAssemblies();

            if(args.Length > 0)
            {
                if(args[0].ToLower() == "-t")
                {
                    ShowList(found);
                    return;
                }

                var parts = args[0].Split(':');
                var runner = new TaskRunner();
                
                if(parts.Length == 2)
                    runner.Run(parts[0],parts[1]);
                else
                    Console.WriteLine("Error: don't know what to do with that. \n\nTry: rakish -t\n\n...to see commands.");
            }
            else
            {
                ShowList(found);
            }
            
        }

        private static void ShowList(IList<Recipe> found)
        {
            foreach(var recipe in found)
            {
                foreach(var task in recipe.Tasks)
                {
                    Console.WriteLine("rakish " + recipe.Name + ":" + task.Name + " - " + task.Description);
                }
            }
        }
    }
}
