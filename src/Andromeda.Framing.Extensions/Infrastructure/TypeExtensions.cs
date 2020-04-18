using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andromeda.Framing.Extensions.Infrastructure
{
    internal static class TypeExtensions
    {
        public static bool IsPrimitiveOrString(this Type t) => t.IsPrimitive || t == typeof(string);

        public static bool IsDefined<T>(this MemberInfo member) where T : Attribute => Attribute.IsDefined(member, typeof(T));
        public static bool IsDefined<T>(this Type t) where T : Attribute => t.GetCustomAttribute<T>() != null;

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
            => givenType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType) ||
               givenType.BaseType != null && (givenType.BaseType.IsGenericType && givenType.BaseType.GetGenericTypeDefinition() == genericType ||
                                              givenType.BaseType.IsAssignableToGenericType(genericType));

        public static string NameWithoutGeneric(this Type type, bool includeNamespace = false)
            => type.IsGenericType ? !includeNamespace
                    ? $"{type.Name.Remove(type.Name.IndexOf('`'))}" : type.Namespace + $".{type.Name.Remove(type.Name.IndexOf('`'))}"
                : type.Name;

        public static string GetTypeName(this Type t, bool includeNamespace = false)
        {
            if (t.IsPrimitiveOrString()) return includeNamespace ? t.FullName : _primitivesTypesStr[t];
            if (t.IsArray) return includeNamespace ? t.FullName : t.GetElementType().GetTypeName() + "[]";
            if (!t.IsGenericType) return includeNamespace ? t.FullName : t.Name;

            var genericTypes = string.Join(",", t.GetGenericArguments()
                .Select(x => x.GetTypeName()).ToArray());

            return $"{t.NameWithoutGeneric(includeNamespace)}<{genericTypes}>";
        }

        public static IEnumerable<Type> GetBaseTypes(this Type t)
        {
            var temp = t;
            while (temp.BaseType != null)
            {
                yield return temp.BaseType;
                temp = temp.BaseType;
                
            }
        }

        private static readonly IReadOnlyDictionary<Type, string> _primitivesTypesStr = new Dictionary<Type, string>
        {
            {typeof(bool), "bool"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(decimal), "decimal"},
            {typeof(double), "double"},
            {typeof(float), "float"},
            {typeof(string), "string"}
        };
    }
}
