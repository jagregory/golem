using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Golem.Core
{
    public abstract class Recipe
    {
        private readonly List<Task> tasks = new List<Task>();
        private readonly Queue<string> currentNamespace = new Queue<string>();

        protected void Namespace(string name, Action action)
        {
            currentNamespace.Enqueue(name);

            action();

            currentNamespace.Dequeue();
        }

        protected Task Task(string name)
        {
            return Task(name, null);
        }

        protected Task Task(string name, string description)
        {
            var task = new Task
            {
                Name = name,
                Namespace = string.Join(":", currentNamespace.ToArray()),
                Description = description
            };

            tasks.Add(task);

            return task;
        }

        public abstract void RegisterTasks();

        public List<Task> Tasks
        {
            get { return tasks; }
        }

        public string Name
        {
            // make this smarter, should support plain RecipeName, but also RecipeNameRecipe
            get { return GetType().Name.ToLower(); }
        }
    }
}