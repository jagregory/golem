using System;

namespace Golem.Core
{
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(params string[] dependencies)
        {
            Dependencies = dependencies;
        }

        public string[] Dependencies { get; private set; }
    }
}