using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers {
#endif

	internal static class ParentDbContext<TDbContext> where TDbContext : DbContext {
		public delegate      Int32  SaveChangesDelegateType     (TDbContext dbContext, Boolean acceptAllChangesOnSuccess);
		public delegate Task<Int32> SaveChangesAsyncDelegateType(TDbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken);

		public static readonly SaveChangesDelegateType      SaveChanges      = CreateBaseSaveChangesDelegate<SaveChangesDelegateType     >(nameof(DbContext.SaveChanges     ));
		public static readonly SaveChangesAsyncDelegateType SaveChangesAsync = CreateBaseSaveChangesDelegate<SaveChangesAsyncDelegateType>(nameof(DbContext.SaveChangesAsync));

		private static TDelegate CreateBaseSaveChangesDelegate<TDelegate>(String nameOfSaveChangesMethod) {
			var invokeMethodInfo = typeof(TDelegate).GetMethod("Invoke");
			var parameterTypes = invokeMethodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
			var dynamicMethod = new DynamicMethod("DbContextBase" + invokeMethodInfo.Name, invokeMethodInfo.ReturnType, parameterTypes, typeof(ParentDbContext<TDbContext>).GetTypeInfo().Module);
			var baseSaveChangesMethodInfo = typeof(TDbContext).GetTypeInfo().BaseType.GetMethod(nameOfSaveChangesMethod, parameterTypes);

			var ilGenerator = dynamicMethod.GetILGenerator();
			for (var i = 0; i < parameterTypes.Length; i++)
				ilGenerator.Emit(OpCodes.Ldarg, i);
			ilGenerator.Emit(OpCodes.Call, baseSaveChangesMethodInfo);
			ilGenerator.Emit(OpCodes.Ret);

			return (TDelegate)(Object)dynamicMethod.CreateDelegate(typeof(TDelegate));
		}
	}
}