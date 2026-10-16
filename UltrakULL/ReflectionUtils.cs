using System;
using System.Collections.Generic;
using System.Reflection;

namespace UltrakULL
{
	public class ReflectionUtils
	{
		private static readonly BindingFlags BindingFlagsFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		// FieldInfo lookups are relatively expensive and the same (Type, field) pairs are
		// requested repeatedly by Harmony patches, so cache them. A cached null is valid
		// and preserves the original behavior (GetValue on a missing field throws).
		private static readonly object FieldCacheLock = new object();
		private static readonly Dictionary<(Type, string), FieldInfo> FieldCache = new Dictionary<(Type, string), FieldInfo>();

		private static FieldInfo GetCachedField(Type classType, string field)
		{
			var key = (classType, field);
			FieldInfo cached;
			lock (FieldCacheLock)
			{
				if (!FieldCache.TryGetValue(key, out cached))
				{
					cached = classType.GetField(field, BindingFlagsFields);
					FieldCache[key] = cached;
				}
			}
			return cached;
		}

		public static void SetPrivate<T, V>(T instance, Type classType, string field, V value)
		{
			GetCachedField(classType, field).SetValue(instance, value);
		}

		public static V GetPrivate<T, V>(T instance, Type classType, string field)
		{
			return (V)GetCachedField(classType, field).GetValue(instance);
		}
	}
}
