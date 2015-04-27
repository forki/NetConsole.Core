﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetConsole.Core.Extensions
{
    public static class TypeLoadableExtensions
    {

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }


        public static IEnumerable<Type> GetTypesWithInterface<T>(this IEnumerable<Type> iEnum) where T : class
        {
            var it = typeof (T);
            
            if (!it.IsInterface) 
                throw new Exception("Expected to be called with a generic T representing an interface");

            return iEnum.Where(t => !t.IsInterface && it.IsAssignableFrom(t)).ToList();
        } 
    }
}