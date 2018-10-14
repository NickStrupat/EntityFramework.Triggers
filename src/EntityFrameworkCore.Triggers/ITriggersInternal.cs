#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal interface ITriggersInternal
	{
		ITriggerEvent Inserting    { get; }
		ITriggerEvent InsertFailed { get; }
		ITriggerEvent Inserted     { get; }
		ITriggerEvent Deleting     { get; }
		ITriggerEvent DeleteFailed { get; }
		ITriggerEvent Deleted      { get; }
		ITriggerEvent Updating     { get; }
		ITriggerEvent UpdateFailed { get; }
		ITriggerEvent Updated      { get; }
	}
}