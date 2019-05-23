using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdvorangesUtils
{
	/// <summary>
	/// Actions for reflection.
	/// </summary>
	public static class ReflectionUtils
	{
		/// <summary>
		/// Gets the underlying type of a memberinfo. This only supports events, fields, methods, and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static Type GetUnderlyingType(this MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Event:
					return ((EventInfo)member).EventHandlerType;
				case MemberTypes.Field:
					return ((FieldInfo)member).FieldType;
				case MemberTypes.Method:
					return ((MethodInfo)member).ReturnType;
				case MemberTypes.Property:
					return ((PropertyInfo)member).PropertyType;
				default:
					throw new ArgumentException($"Must be {nameof(EventInfo)}, {nameof(FieldInfo)}, {nameof(MethodInfo)}, or {nameof(PropertyInfo)}.", nameof(member));
			}
		}
		/// <summary>
		/// Gets the value of a memberinfo. This only supports fields and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object GetValue(this MemberInfo member, object obj)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					return ((FieldInfo)member).GetValue(obj);
				case MemberTypes.Property:
					return ((PropertyInfo)member).GetValue(obj);
				default:
					throw new ArgumentException($"Must be {nameof(FieldInfo)} or {nameof(PropertyInfo)}.", nameof(member));
			}
		}
		/// <summary>
		/// Sets the value of a memberinfo. This only supports fields and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public static void SetValue(this MemberInfo member, object obj, object value)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					((FieldInfo)member).SetValue(obj, value);
					return;
				case MemberTypes.Property:
					((PropertyInfo)member).SetValue(obj, value);
					return;
				default:
					throw new ArgumentException($"Must be {nameof(FieldInfo)} or {nameof(PropertyInfo)}.", nameof(member));
			}
		}
		/// <summary>
		/// Tests whether the specified generic type is a parent of the current type or is the current type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool InheritsFromGeneric(this Type type, Type c)
		{
			if (!c.IsGenericType)
			{
				throw new ArgumentException($"Use {nameof(Type.IsAssignableFrom)} rather than this method if passing in a non generic type.");
			}
			if (type == typeof(object))
			{
				return false;
			}
			if (type == c || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(c)))
			{
				return true;
			}
			return type.BaseType.InheritsFromGeneric(c);
		}
		/// <summary>
		/// Gets the specified attribute from the supplied attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attrs"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this IEnumerable<Attribute> attrs)
		{
			foreach (var attr in attrs)
			{
				if (attr is T t)
				{
					return t;
				}
			}
			return default;
		}
	}
}