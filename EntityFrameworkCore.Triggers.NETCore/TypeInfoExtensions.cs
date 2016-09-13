using System;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.Triggers {
	public static class TypeInfoExtensions {
		public static Type[] GetDeclaredInterfaces(this Type t) {
			var allInterfaces = t.GetInterfaces();
			var baseInterfaces = t.GetTypeInfo().BaseType?.GetInterfaces();
			return baseInterfaces == null ? allInterfaces : allInterfaces.Except(baseInterfaces).ToArray();
		}
	}
}
