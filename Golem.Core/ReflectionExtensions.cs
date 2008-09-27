using System;
using System.Reflection;

namespace Golem.Core
{
    public static class ReflectionExtensions
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), inherit);

            return attributes.Length > 0 ? attributes[0] as T : null;
        }

        public static T GetCustomAttribute<T>(this MethodInfo method, bool inherit) where T : Attribute
        {
            var attributes = method.GetCustomAttributes(typeof(T), inherit);

            return attributes.Length > 0 ? attributes[0] as T : null;
        }

    }
}