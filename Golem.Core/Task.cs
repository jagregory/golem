using System;
using System.Collections.Generic;

namespace Golem.Core
{
    public class Task
    {
        private Action executeAction = delegate {};
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Description { get; set; }
        public IList<string> Dependencies { get; private set; }
        public List<Task> ResolvedDependencies { get; private set; }

        public string FullName
        {
            get { return !string.IsNullOrEmpty(Namespace) ? Namespace + ":" + Name : Name; }
        }

        public Task()
        {
            Dependencies = new List<string>();
            ResolvedDependencies = new List<Task>();
        }

        public Task Do(Action action)
        {
            executeAction = action;

            return this;
        }

        public Task Depends(params string[] dependencies)
        {
            foreach (var dependency in dependencies)
            {
                Dependencies.Add(dependency);
            }

            return this;
        }

        public virtual void Execute()
        {
            executeAction();
        }
    }
}