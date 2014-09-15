namespace EntityFrameworkTriggers {
	internal interface IEntityWithTriggers<in TDbContext> : ITriggers<TDbContext> where TDbContext : DbContextWithTriggers<TDbContext> {}
}