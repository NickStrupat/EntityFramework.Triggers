using System;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	internal interface ITriggerEvent
	{
		void Raise(Object entry, IServiceProvider serviceProvider);
	}
}