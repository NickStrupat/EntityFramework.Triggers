using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	#region Base classes
	#region Non-generic service
	internal abstract class Entry<TEntity, TDbContext> : IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected Entry(TEntity entity, TDbContext context, IServiceProvider service) {
			Entity = entity;
			Context = context;
			Service = service;
		}
		public TEntity Entity { get; }
		Object IEntry.Entity => Entity;
		public TDbContext Context { get; }
		public IServiceProvider Service { get; }
		DbContext IEntry.Context => Context;
	}

	internal abstract class BeforeEntry<TEntity, TDbContext> : Entry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected BeforeEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service) => Cancel = cancel;
		public Boolean Cancel { get; set; }
	}

	internal abstract class BeforeChangeEntry<TEntity, TDbContext> : BeforeEntry<TEntity, TDbContext>, IBeforeChangeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected BeforeChangeEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) {}
		private TEntity original;
		public TEntity Original => original ?? (original = (TEntity)Context.Entry(Entity).OriginalValues.ToObject());
	}

	internal abstract class FailedEntry<TEntity, TDbContext> : Entry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected FailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service)
		{
			Exception = exception;
			Swallow = swallow;
		}
		public Exception Exception { get; }
		public Boolean Swallow { get; set; }
	}

	internal abstract class ChangeFailedEntry<TEntity, TDbContext> : FailedEntry<TEntity, TDbContext>, IChangeFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected ChangeFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}
	#endregion
	#region Generic service
	internal abstract class Entry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext>, IEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected Entry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) => Service = ServiceRetrieval<TService>.GetService(service);
		public new TService Service { get; }
	}

	internal abstract class BeforeEntry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext, TService>, IBeforeEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected BeforeEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service) => Cancel = cancel;
		public Boolean Cancel { get; set; }
	}

	internal abstract class BeforeChangeEntry<TEntity, TDbContext, TService> : BeforeEntry<TEntity, TDbContext, TService>, IBeforeChangeEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected BeforeChangeEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) {}
		private TEntity original;
		public TEntity Original => original ?? (original = (TEntity)Context.Entry(Entity).OriginalValues.ToObject());
	}

	internal abstract class FailedEntry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext, TService>, IFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected FailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service)
		{
			Exception = exception;
			Swallow = swallow;
		}
		public Exception Exception { get; }
		public Boolean Swallow { get; set; }
	}

	internal abstract class ChangeFailedEntry<TEntity, TDbContext, TService> : FailedEntry<TEntity, TDbContext, TService>, IChangeFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		protected ChangeFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}
	#endregion
	#endregion
	
	#region Final classes
	#region Non-generic service
	internal class InsertingEntry<TEntity, TDbContext> : BeforeEntry<TEntity, TDbContext>, IInsertingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) { }
	}

	internal class UpdatingEntry<TEntity, TDbContext> : BeforeChangeEntry<TEntity, TDbContext>, IUpdatingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdatingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) { }
	}

	internal class DeletingEntry<TEntity, TDbContext> : BeforeChangeEntry<TEntity, TDbContext>, IDeletingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeletingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) { }
	}

	internal class InsertFailedEntry<TEntity, TDbContext> : FailedEntry<TEntity, TDbContext>, IInsertFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class UpdateFailedEntry<TEntity, TDbContext> : ChangeFailedEntry<TEntity, TDbContext>, IUpdateFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdateFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class DeleteFailedEntry<TEntity, TDbContext> : ChangeFailedEntry<TEntity, TDbContext>, IDeleteFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeleteFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class InsertedEntry<TEntity, TDbContext> : Entry<TEntity, TDbContext>, IInsertedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}

	internal class UpdatedEntry<TEntity, TDbContext> : Entry<TEntity, TDbContext>, IUpdatedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdatedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}

	internal class DeletedEntry<TEntity, TDbContext> : Entry<TEntity, TDbContext>, IDeletedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeletedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}
	#endregion
	#region Generic service
	internal class InsertingEntry<TEntity, TDbContext, TService> : BeforeEntry<TEntity, TDbContext, TService>, IInsertingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) {}
	}

	internal class UpdatingEntry<TEntity, TDbContext, TService> : BeforeChangeEntry<TEntity, TDbContext, TService>, IUpdatingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdatingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) { }
	}

	internal class DeletingEntry<TEntity, TDbContext, TService> : BeforeChangeEntry<TEntity, TDbContext, TService>, IDeletingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeletingEntry(TEntity entity, TDbContext context, IServiceProvider service, Boolean cancel) : base(entity, context, service, cancel) { }
	}

	internal class InsertFailedEntry<TEntity, TDbContext, TService> : FailedEntry<TEntity, TDbContext, TService>, IInsertFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class UpdateFailedEntry<TEntity, TDbContext, TService> : ChangeFailedEntry<TEntity, TDbContext, TService>, IUpdateFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdateFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class DeleteFailedEntry<TEntity, TDbContext, TService> : ChangeFailedEntry<TEntity, TDbContext, TService>, IDeleteFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeleteFailedEntry(TEntity entity, TDbContext context, IServiceProvider service, Exception exception, Boolean swallow) : base(entity, context, service, exception, swallow) {}
	}

	internal class InsertedEntry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext, TService>, IInsertedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public InsertedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}

	internal class UpdatedEntry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext, TService>, IUpdatedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public UpdatedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}

	internal class DeletedEntry<TEntity, TDbContext, TService> : Entry<TEntity, TDbContext, TService>, IDeletedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		public DeletedEntry(TEntity entity, TDbContext context, IServiceProvider service) : base(entity, context, service) {}
	}
	#endregion
	#endregion

	#region Wrapped
	internal class WrappedInsertingEntry<TEntity, TDbContext, TService> : IInsertingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IInsertingEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedInsertingEntry(IInsertingEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Boolean Cancel { get => entry.Cancel; set => entry.Cancel = value; }
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedUpdatingEntry<TEntity, TDbContext, TService> : IUpdatingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IUpdatingEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedUpdatingEntry(IUpdatingEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Boolean Cancel { get => entry.Cancel; set => entry.Cancel = value; }
		public TEntity Original => entry.Original;
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedDeletingEntry<TEntity, TDbContext, TService> : IDeletingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IDeletingEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedDeletingEntry(IDeletingEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Boolean Cancel { get => entry.Cancel; set => entry.Cancel = value; }
		public TEntity Original => entry.Original;
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedInsertFailedEntry<TEntity, TDbContext, TService> : IInsertFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IInsertFailedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedInsertFailedEntry(IInsertFailedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Exception Exception => entry.Exception;
		public Boolean Swallow { get => entry.Swallow; set => entry.Swallow = value; }
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedUpdateFailedEntry<TEntity, TDbContext, TService> : IUpdateFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IUpdateFailedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedUpdateFailedEntry(IUpdateFailedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Exception Exception => entry.Exception;
		public Boolean Swallow { get => entry.Swallow; set => entry.Swallow = value; }
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedDeleteFailedEntry<TEntity, TDbContext, TService> : IDeleteFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IDeleteFailedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedDeleteFailedEntry(IDeleteFailedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public Exception Exception => entry.Exception;
		public Boolean Swallow { get => entry.Swallow; set => entry.Swallow = value; }
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedInsertedEntry<TEntity, TDbContext, TService> : IInsertedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IInsertedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedInsertedEntry(IInsertedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedUpdatedEntry<TEntity, TDbContext, TService> : IUpdatedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IUpdatedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedUpdatedEntry(IUpdatedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}

	internal class WrappedDeletedEntry<TEntity, TDbContext, TService> : IDeletedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IDeletedEntry<TEntity, TDbContext> entry;
		private readonly TService service;

		public WrappedDeletedEntry(IDeletedEntry<TEntity, TDbContext> entry)
		{
			this.entry = entry;
			service = ServiceRetrieval<TService>.GetService(entry.Service);
		}

		Object IEntry.Entity => ((IEntry) entry).Entity;
		DbContext IEntry.Context => ((IEntry) entry).Context;
		public TDbContext Context => entry.Context;
		public TEntity Entity => entry.Entity;
		public IServiceProvider Service => entry.Service;
		TService IEntry<TEntity, TDbContext, TService>.Service => service;
	}
	#endregion
}
