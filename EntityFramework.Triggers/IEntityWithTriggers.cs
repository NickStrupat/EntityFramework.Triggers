namespace EntityFramework.Triggers {
	internal interface IEntityWithTriggers<in TDbContext> : ITriggers<TDbContext> where TDbContext : DbContextWithTriggers<TDbContext> {}
}