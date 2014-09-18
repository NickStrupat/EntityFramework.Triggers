namespace EntityFramework.Triggers {
	public interface ITriggerable {}
	public interface ITriggerable<TTriggerable> : ITriggerable where TTriggerable : ITriggerable<TTriggerable> {}
}