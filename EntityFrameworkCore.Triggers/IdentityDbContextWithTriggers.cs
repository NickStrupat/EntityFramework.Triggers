using System;
using System.Threading;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EntityFramework.Triggers
{
#endif

#if EF_CORE
	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
	public abstract class IdentityDbContextWithTriggers : IdentityDbContext<IdentityUser, IdentityRole, string> {
		public Boolean TriggersEnabled { get; set; } = true;

        protected IdentityDbContextWithTriggers() : base() { }
		protected IdentityDbContextWithTriggers(DbContextOptions options) : base(options) { }

		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess) : base.SaveChanges(acceptAllChangesOnSuccess);
		}
		
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
		
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken) : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}
    
	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
	public abstract class IdentityDbContextWithTriggers<TUser> : IdentityDbContext<TUser, IdentityRole, string> where TUser : IdentityUser
    {
        public Boolean TriggersEnabled { get; set; } = true;

		protected IdentityDbContextWithTriggers() : base() { }
		protected IdentityDbContextWithTriggers(DbContextOptions options) : base(options) { }

		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess) : base.SaveChanges(acceptAllChangesOnSuccess);
		}
		
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
		
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken) : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}
    
	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
	public abstract class IdentityDbContextWithTriggers<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public Boolean TriggersEnabled { get; set; } = true;

		protected IdentityDbContextWithTriggers() : base() { }
		protected IdentityDbContextWithTriggers(DbContextOptions options) : base(options) { }

		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess) : base.SaveChanges(acceptAllChangesOnSuccess);
		}
		
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
		
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken) : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}
    
	/// <summary>
	/// A <see cref="DbContext"/>-derived class with trigger functionality called automatically
	/// </summary>
    /// <typeparam name="TUser">The type of user objects.</typeparam>
    /// <typeparam name="TRole">The type of role objects.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
    /// <typeparam name="TUserClaim">The type of the user claim object.</typeparam>
    /// <typeparam name="TUserRole">The type of the user role object.</typeparam>
    /// <typeparam name="TUserLogin">The type of the user login object.</typeparam>
    /// <typeparam name="TRoleClaim">The type of the role claim object.</typeparam>
    /// <typeparam name="TUserToken">The type of the user token object.</typeparam>
	public abstract class IdentityDbContextWithTriggers<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityUserContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
    #region IdentityDbContext
                /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of User roles.
        /// </summary>
        public DbSet<TUserRole> UserRoles { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of roles.
        /// </summary>
        public DbSet<TRole> Roles { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of role claims.
        /// </summary>
        public DbSet<TRoleClaim> RoleClaims { get; set; }

        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <param name="builder">
        /// The builder being used to construct the model for this context.
        /// </param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<TUser>(b =>
            {
                b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });

            builder.Entity<TRole>(b =>
            {
                b.HasKey(r => r.Id);
                b.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique();
                b.ToTable("AspNetRoles");
                b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                b.Property(u => u.Name).HasMaxLength(256);
                b.Property(u => u.NormalizedName).HasMaxLength(256);

                b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
            });

            builder.Entity<TRoleClaim>(b =>
            {
                b.HasKey(rc => rc.Id);
                b.ToTable("AspNetRoleClaims");
            });

            builder.Entity<TUserRole>(b =>
            {
                b.HasKey(r => new { r.UserId, r.RoleId });
                b.ToTable("AspNetUserRoles");
            });
        }
    #endregion

        public Boolean TriggersEnabled { get; set; } = true;

		protected IdentityDbContextWithTriggers() : base() { }
		protected IdentityDbContextWithTriggers(DbContextOptions options) : base(options) { }

		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		
		public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges, acceptAllChangesOnSuccess) : base.SaveChanges(acceptAllChangesOnSuccess);
		}
		
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
		
		public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess, cancellationToken) : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}

#else
    public abstract class IdentityDbContextWithTriggers : IdentityDbContext<IdentityUser> {
		public Boolean TriggersEnabled { get; set; } = true;

		protected IdentityDbContextWithTriggers() : base() { }
		protected IdentityDbContextWithTriggers(DbCompiledModel model) : base(model) { }
		protected IdentityDbContextWithTriggers(String nameOrConnectionString) : base(nameOrConnectionString) { }
		protected IdentityDbContextWithTriggers(DbConnection existingConnection, Boolean contextOwnsConnection) : base(existingConnection, contextOwnsConnection) { }
		//protected IdentityDbContextWithTriggers(ObjectContext objectContext, Boolean dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) { }
		protected IdentityDbContextWithTriggers(String nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) { }
		protected IdentityDbContextWithTriggers(DbConnection existingConnection, DbCompiledModel model, Boolean contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) { }
		
		public override Int32 SaveChanges() {
			return TriggersEnabled ? this.SaveChangesWithTriggers(base.SaveChanges) : base.SaveChanges();
		}
		public override Task<Int32> SaveChangesAsync(CancellationToken cancellationToken) {
			return TriggersEnabled ? this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, cancellationToken) : base.SaveChangesAsync(cancellationToken);
		}
	}
#endif
}
