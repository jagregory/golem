using System;

namespace Golem.Core
{
    internal class InvalidDependentTaskException : Exception
    {
        public InvalidDependentTaskException(string dependency)
            : base("Unable to resolve dependent task: '" + dependency + "'.")
        {}
    }
}