﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace bUtility.Reflection
{
    public static partial class ReflectionExtensions
    {
        static BindingFlags defaultBindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static object GetInstance(this Type type, params object[] parameters)
        {
            var constructor = type.GetConstructor(System.Type.EmptyTypes);
            if (constructor != null)
            {
                var instance = constructor.Invoke(parameters);
                return instance;
            }
            return null;
        }

        public static object GetValue(this object obj, string propertyName)
        {
            if (obj != null)
            {
                try
                {
                    var pinfo = obj.GetType().GetProperty(propertyName, defaultBindings);
                    if (pinfo != null)
                    {
                        return pinfo.GetValue(obj);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception( $"failed to retrieve property {propertyName} from Type:{obj.GetType().Name} ", ex);
                }
            }
            return null;
        }

        public static T GetValue<T>(this object obj, string propertyName, T defaultValue) where T : class
        {
            return GetValue(obj, propertyName) as T ?? defaultValue;
        }

        public static void SetValue(this object obj, string propertyName, object value)
        {
            if (obj != null)
            {
                try
                {
                    var pinfo = obj.GetType().GetProperty(propertyName, defaultBindings);
                    if (pinfo != null)
                    {
                        pinfo.SetValue(obj, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Type: " + obj.GetType().Name + " PropertyName: " + propertyName, ex);
                }
            }
        }

        public static void Invoke(this object obj, string methodName, params object[] parameters)
        {
            if (obj != null)
            {
                try
                {
                    var minfo = obj.GetType().GetMethod(methodName, defaultBindings);
                    if (minfo != null)
                    {
                        minfo.Invoke(obj, parameters);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Type: " + obj.GetType().Name + " MethodName: " + methodName, ex);
                }
            }
        }

        public static IEnumerable<T> GetMembers<T>(this object data, Func<T, Boolean> filter = null) where T : MemberInfo
        {
            return data.GetType().GetMembers(filter);
        }
        public static IEnumerable<T> GetMembers<T>(this Type type, Func<T, Boolean> filter = null) where T :MemberInfo
        {
            foreach (var p in type?.GetMembers(defaultBindings))
            {
                if (p is T && p != null)
                {
                    if (filter == null || filter(p as T))
                    {
                        yield return p as T;
                    }
                }
            }
        }

        public static IEnumerable<string> GetMemberNames<T>(this Type type, Func<T, Boolean> filter = null) where T : MemberInfo
        {
            return type?.GetMembers<T>(filter)?.Select(p => p.Name);
        }

        public static IEnumerable<string> GetMemberNames<T>(this object obj, Func<T, Boolean> filter = null) where T : MemberInfo
        {
            return obj?.GetType().GetMemberNames<T>(filter);
        }

        public static T GetMemberInfo<T>(this Type type, string memberName) where T : MemberInfo
        {
            return type?.GetMember(memberName, defaultBindings) as T;
        }
        public static T GetMemberInfo<T>(this object obj, string memberName) where T : MemberInfo
        {
            return obj?.GetType().GetMemberInfo<T>(memberName);
        }

        public static T Copy<T>(this T data) where T : new()
        {
            return Copy<T, T>(data);
        }
        public static T Copy<T, R>(this R data) where T : R, new()
        {
            var copy = new T();
            typeof(R).GetMembers<PropertyInfo>().ToList().ForEach(p => copy.SetValue(p.Name, data.GetValue(p.Name)));
            return copy;
        }

        public static T GetCustomAttribute<T>(this MemberInfo memberInfo) where T : System.Attribute
        {
            if (memberInfo == null) return null;
            var attributes = memberInfo.GetCustomAttributes<T>();
            if (attributes != null) return attributes.FirstOrDefault(i => true);
            return null;
        }

        public static Assembly FindAssembly(this string name)
        {
            if (name == null) return null;
            var aList = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = aList.FirstOrDefault(a => a.FullName == name);
            return assembly;
        }

    }
}
