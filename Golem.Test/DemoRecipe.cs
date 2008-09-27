using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Golem.Core;
using Golem.Core;

namespace Golem.Test
{
    public class Demo2 : Recipe 
    {
        public override void RegisterTasks()
        {
            Task("one").Depends("three", "two").Do(() =>
            {
                Console.WriteLine("One!");
            });

            Task("two").Do(() =>
            {
                Console.WriteLine("Two!");
            });

            Task("three").Do(() =>
            {
                Console.WriteLine("Three!");
            });
        }
    }

    public class Demo : Recipe
    {
        public override void RegisterTasks()
        {
            Task("run").Do(() =>
            {
                AppDomain.CurrentDomain.SetData("TEST", "TEST");
            });

            Task("list", "List all NUnit tests in solution").Do(() =>
            {
                AppDomain.CurrentDomain.SetData("TEST", "LIST");
            });

            Task("stats", "Lists line counts for all types of files").Do(() =>
            {
                string rootDir = Locations.StartDirs[0];
                Console.WriteLine(rootDir);
                var count = new Dictionary<string, long>()
                {{"lines", 0}, {"classes", 0}, {"files", 0}, {"enums", 0}, {"methods", 0}, {"comments", 0}};
                GetLineCount(rootDir, "*.cs", count);


                Console.WriteLine("c# Files:\t\t{0}", count["files"]);
                Console.WriteLine("c# Classes:  \t{0}", count["classes"]);
                Console.WriteLine("c# Methods:  \t{0}", count["methods"]);
                Console.WriteLine("c# Lines:\t\t{0}", count["lines"]);
                Console.WriteLine("c# Comment Lines:\t\t{0}", count["comments"]);
                Console.WriteLine("Avg Methods Per Class:\t\t{0}", count["methods"]/count["classes"]);
            });
        }

        private static void GetLineCount(string rootDir, string fileFilter, Dictionary<string, long> counts)
        {
            var files = Directory.GetFiles(rootDir, fileFilter, SearchOption.AllDirectories);
            long lineCount = 0;
            foreach (var file in files)
            {
                using (var r = File.OpenText(file))
                {
                    counts["files"] += 1;

                    var line = r.ReadLine();
                    while (line != null)
                    {
                        if (fileFilter == "*.cs" && Regex.Match(line, ".+[public|private|internal|protected].+class.+").Length > 0)
                            counts["classes"] += 1;

                        if (fileFilter == "*.cs" && Regex.Match(line, ".+[public|private|internal|protected].+enum.+").Length > 0)
                            counts["enums"] += 1;

                        if (fileFilter == "*.cs" && Regex.Match(line, ".+[public|private|internal|protected].+\\(.*\\).+").Length > 0)
                            counts["methods"] += 1;

                        if (fileFilter == "*.cs" && Regex.Match(line, ".+//.+").Length > 0)
                            counts["comments"] += 1;

                        counts["lines"] += 1;

                        line = r.ReadLine();
                    }
                }
            }

        }
    }

    public class Demo3 : Recipe
    {
        public override void RegisterTasks()
        {
            Task("hello").Do(() =>
            {
                Console.WriteLine("Hello!");
            });
        }
    }

    public class Demo4 : Recipe
    {
        public override void RegisterTasks()
        {
            Task("hello").Do(() =>
            {
                Console.WriteLine("Not implemented yet: this.AllAssemblies.Count");
                Console.WriteLine("Not implemented yet: this.AllRecipes.Count");
            });
        }
    }

    public abstract class SpecialRecipe : Recipe
    {
        public WrapperTask WrapTask(string name)
        {
            var task = new WrapperTask { Name = name };

            Tasks.Add(task);

            return task;
        }
    }

    public class WrapperTask : Task
    {
        public override void Execute()
        {
            Console.WriteLine("I'm a wrapping task...");

            base.Execute();

            Console.WriteLine("... wraptastic!");
        }
    }

    public class CustomTaskSupport : SpecialRecipe
    {
        public override void RegisterTasks()
        {
            WrapTask("wrapped").Do(() =>
            {
                Console.WriteLine("I should be wrapped");
            });
        }
    }
}