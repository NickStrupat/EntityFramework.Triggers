using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public sealed class Triggers<TEntity, TDbContext> : ITriggers<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public Triggers(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			IServiceProvider GetServiceProvider() => serviceProvider;

			inserting    = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			insertFailed = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			inserted     = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			deleting     = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			deleteFailed = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			deleted      = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			updating     = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			updateFailed = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
			updated      = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		}

		private readonly TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> inserting   ;
		private readonly TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> insertFailed;
		private readonly TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> inserted    ;
		private readonly TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> deleting    ;
		private readonly TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> deleteFailed;
		private readonly TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> deleted     ;
		private readonly TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> updating    ;
		private readonly TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> updateFailed;
		private readonly TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> updated     ;

		TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserting    => inserting   ;
		TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.InsertFailed => insertFailed;
		TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserted     => inserted    ;
		TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleting     => deleting    ;
		TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.DeleteFailed => deleteFailed;
		TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleted      => deleted     ;
		TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updating     => updating    ;
		TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.UpdateFailed => updateFailed;
		TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updated      => updated     ;
		
		public static TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> Inserting    = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> InsertFailed = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> Inserted     = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Deleting     = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> DeleteFailed = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Deleted      = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Updating     = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> UpdateFailed = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);
		public static TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Updated      = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>(GetServiceProvider);

		static IServiceProvider GetServiceProvider() => Triggers.ServiceProvider;
	}

	public static class Triggers
	{
		public static IServiceProvider ServiceProvider { get; set; }
	}

	public sealed class Triggers<TEntity> : ITriggers<TEntity, DbContext>
	where TEntity : class
	{
		private readonly ITriggers<TEntity, DbContext> triggers;
		public Triggers(IServiceProvider serviceProvider) => triggers = new Triggers<TEntity, DbContext>(serviceProvider);

		TriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Inserting    => triggers.Inserting   ;
		TriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.InsertFailed => triggers.InsertFailed;
		TriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Inserted     => triggers.Inserted    ;
		TriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Deleting     => triggers.Deleting    ;
		TriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.DeleteFailed => triggers.DeleteFailed;
		TriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Deleted      => triggers.Deleted     ;
		TriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Updating     => triggers.Updating    ;
		TriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.UpdateFailed => triggers.UpdateFailed;
		TriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Updated      => triggers.Updated     ;

		public static TriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity, DbContext> Inserting    => Triggers<TEntity, DbContext>.Inserting   ;
		public static TriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity, DbContext> InsertFailed => Triggers<TEntity, DbContext>.InsertFailed;
		public static TriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity, DbContext> Inserted     => Triggers<TEntity, DbContext>.Inserted    ;
		public static TriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity, DbContext> Deleting     => Triggers<TEntity, DbContext>.Deleting    ;
		public static TriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity, DbContext> DeleteFailed => Triggers<TEntity, DbContext>.DeleteFailed;
		public static TriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity, DbContext> Deleted      => Triggers<TEntity, DbContext>.Deleted     ;
		public static TriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity, DbContext> Updating     => Triggers<TEntity, DbContext>.Updating    ;
		public static TriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity, DbContext> UpdateFailed => Triggers<TEntity, DbContext>.UpdateFailed;
		public static TriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity, DbContext> Updated      => Triggers<TEntity, DbContext>.Updated	    ;
	}
}
