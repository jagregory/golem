using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Golem.Core;
using NUnit.Framework;
using Golem.Core;

namespace Golem.Test
{
    [TestFixture]
    public class RecipeDiscoveryFixture
    {
        private RecipeCataloger cataloger;
        List<Recipe> found;

        [SetUp]
        public void Before_Each_Test_Is_Run()
        {
            cataloger = new RecipeCataloger(Environment.CurrentDirectory + "..\\..\\..\\..\\");
            found = cataloger.CatalogueRecipes();   
        }

        [Test]
        public void Can_Discover_Recipes()
        {
            foreach(var r in found)
            {
                Console.WriteLine(r.Name);
            }
            Assert.AreEqual(6, found.Count);
        }

        [Test]
        public void Can_Discover_Tasks_And_Details()
        {
            var recipeInfo = found.Find(r => r.Name == "demo");

            Assert.AreEqual(3, recipeInfo.Tasks.Count);
            Assert.AreEqual("list", recipeInfo.Tasks[1].Name);
            Assert.AreEqual("List all NUnit tests in solution", recipeInfo.Tasks[1].Description);
        }

        [Test]
        public void Can_Discover_Tasks_In_Namespace()
        {
            var recipeInfo = found.Find(r => r.Name == "nsdemo");

            Assert.AreEqual(7, recipeInfo.Tasks.Count);

            foreach (var task in recipeInfo.Tasks)
            {
                Console.WriteLine(task.FullName);
            }
        }

        [Test]
        public void FullName_Includes_Fully_Nested_Namespaces()
        {
            var recipeInfo = found.Find(r => r.Name == "nsdemo");

            Assert.AreEqual(7, recipeInfo.Tasks.Count);

            foreach (var task in recipeInfo.Tasks)
            {
                if (task.Name == "update-db") Assert.AreEqual("update-db", task.FullName);
                if (task.Name == "backup") Assert.AreEqual("db:backup", task.FullName);
                if (task.Name == "replace") Assert.AreEqual("db:replace", task.FullName);
                if (task.Name == "run-scripts") Assert.AreEqual("db:run-scripts", task.FullName);
                if (task.Name == "views") Assert.AreEqual("db:scripts:views", task.FullName);
                if (task.Name == "functions") Assert.AreEqual("db:scripts:functions", task.FullName);
                if (task.Name == "sprocs") Assert.AreEqual("db:scripts:sprocs", task.FullName);
            }
        }

        [Test]
        public void Can_Run_Task()
        {
            var recipeInfo = found[1];
            var runner = new TaskRunner(cataloger);
            runner.Run(recipeInfo, recipeInfo.Tasks[0]);
            Assert.AreEqual("TEST", AppDomain.CurrentDomain.GetData("TEST"));
        }

        [Test]
        public void Can_Run_Task_By_Name()
        {
            
            var runner = new TaskRunner(cataloger);
            Locations.StartDirs = new[] {Environment.CurrentDirectory + "..\\..\\..\\..\\"};
            runner.Run("demo","list");
            Assert.AreEqual("LIST", AppDomain.CurrentDomain.GetData("TEST"));
        }

        
        [Test, Ignore]
        public void Can_Run_All_Default_Tasks()
        {
            Assert.Fail();
        }

        [Test]
        public void Can_Set_Dependencies()
        {
            var demo2 = found[0];
            Assert.AreEqual("demo2", demo2.Name);
            Assert.AreEqual(2, demo2.Tasks[0].ResolvedDependencies.Count);
            Assert.AreEqual(demo2.Tasks[0].ResolvedDependencies[0].Name, "three");
        }

        [Test]
        public void Functions_Are_Called_In_Correct_Order_With_Dependencies()
        {
            var runner = new TaskRunner(cataloger);
            runner.Run("demo2", "one");          

        }

        [Test]
        public void Can_Infer_Recipe_Category_And_Task_Name()
        {
            var runner = new TaskRunner(cataloger);
            runner.Run("demo3", "hello");          
        }

        [Test, Ignore]
        public void Can_Override_Current_Root_Folder()
        {
            Assert.Fail();
        }

        [Test, Ignore]
        public void Can_Fetch_List_Of_Available_Recipes_From_Server()
        {
            Assert.Fail();
        }

        [Test]
        public void Recipes_Inheriting_RecipeBase_Have_Contextual_Information()
        {
            var demo4 = found[3];
            var runner = new TaskRunner(cataloger);
            runner.Run(demo4,demo4.Tasks[0]);
        }

        [Test]
        public void Can_Run_Extension_Tasks()
        {
            var runner = new TaskRunner(cataloger);
            runner.Run("customtasksupport", "wrapped");
        }
    }
}
