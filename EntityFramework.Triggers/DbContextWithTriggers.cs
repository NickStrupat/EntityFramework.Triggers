using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace EntityFramework.Triggers {
	/// <summary>
	/// A <see cref="System.Data.Entity.DbContext"/> class with <see cref="Triggers{TTriggerable}"/> support
	/// </summary>
	public abstract class DbContextWithTriggers : DbContext {
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return this.SaveChangesWithTriggers(acceptAllChangesOnSuccess);
		}
#if !NET40
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.SaveChangesWithTriggersAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
#endif
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Data.Entity.DbContext"/> class. The
		///                 <see cref="M:Microsoft.Data.Entity.DbContext.OnConfiguring(Microsoft.Data.Entity.DbContextOptionsBuilder)"/>
		///                 method will be called to configure the database (and other options) to be used for this context.
		/// 
		/// </summary>
		protected DbContextWithTriggers() : base() {}

		/// <summary>
		/// 
		/// <para>
		/// Initializes a new instance of the <see cref="T:Microsoft.Data.Entity.DbContext"/> class using an <see cref="T:System.IServiceProvider"/>.
		/// 
		/// </para>
		/// 
		/// <para>
		/// The service provider must contain all the services required by Entity Framework (and the database being
		///                     used).
		///                     The Entity Framework services can be registered using the
		///                     <see cref="M:Microsoft.Framework.DependencyInjection.EntityFrameworkServiceCollectionExtensions.AddEntityFramework(Microsoft.Framework.DependencyInjection.IServiceCollection)"/> method.
		///                     Most databases also provide an extension method on <see cref="T:Microsoft.Framework.DependencyInjection.IServiceCollection"/> to register the services
		///                     required.
		/// 
		/// </para>
		/// 
		/// <para>
		/// If the <see cref="T:System.IServiceProvider"/> has a <see cref="T:Microsoft.Data.Entity.Infrastructure.DbContextOptions"/> or
		///                     <see cref="T:Microsoft.Data.Entity.Infrastructure.DbContextOptions`1"/>
		///                     registered, then this will be used as the options for this context instance. The <see cref="M:Microsoft.Data.Entity.DbContext.OnConfiguring(Microsoft.Data.Entity.DbContextOptionsBuilder)"/>
		///                     method
		///                     will still be called to allow further configuration of the options.
		/// 
		/// </para>
		/// 
		/// </summary>
		/// <param name="serviceProvider">The service provider to be used.</param>
		protected DbContextWithTriggers(IServiceProvider serviceProvider) : base(serviceProvider) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Data.Entity.DbContext"/> with the specified options. The
		///                 <see cref="M:Microsoft.Data.Entity.DbContext.OnConfiguring(Microsoft.Data.Entity.DbContextOptionsBuilder)"/> method will still be called to allow further
		///                 configuration of the options.
		/// 
		/// </summary>
		/// <param name="options">The options for this context.</param>
		protected DbContextWithTriggers(DbContextOptions options) : base(options) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Data.Entity.DbContext"/> class using an <see cref="T:System.IServiceProvider"/>
		///                 and the specified options.
		/// 
		/// <para>
		/// The <see cref="M:Microsoft.Data.Entity.DbContext.OnConfiguring(Microsoft.Data.Entity.DbContextOptionsBuilder)"/> method will still be called to allow further
		///                     configuration of the options.
		/// 
		/// </para>
		/// 
		/// <para>
		/// The service provider must contain all the services required by Entity Framework (and the databases being
		///                     used).
		///                     The Entity Framework services can be registered using the
		///                     <see cref="M:Microsoft.Framework.DependencyInjection.EntityFrameworkServiceCollectionExtensions.AddEntityFramework(Microsoft.Framework.DependencyInjection.IServiceCollection)"/> method.
		///                     Most databases also provide an extension method on <see cref="T:Microsoft.Framework.DependencyInjection.IServiceCollection"/> to register the services
		///                     required.
		/// 
		/// </para>
		/// 
		/// <para>
		/// If the <see cref="T:System.IServiceProvider"/> has a <see cref="T:Microsoft.Data.Entity.Infrastructure.DbContextOptions"/> or
		///                     <see cref="T:Microsoft.Data.Entity.Infrastructure.DbContextOptions`1"/>
		///                     registered, then this will be used as the options for this context instance. The <see cref="M:Microsoft.Data.Entity.DbContext.OnConfiguring(Microsoft.Data.Entity.DbContextOptionsBuilder)"/>
		///                     method
		///                     will still be called to allow further configuration of the options.
		/// 
		/// </para>
		/// 
		/// </summary>
		/// <param name="serviceProvider">The service provider to be used.</param><param name="options">The options for this context.</param>
		protected DbContextWithTriggers(IServiceProvider serviceProvider, DbContextOptions options) : base(serviceProvider, options) {}
	}
}
