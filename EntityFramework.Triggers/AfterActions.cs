using System;
using System.Collections.Generic;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace EntityFramework.Triggers {
	internal struct AfterActions {
		public List<Action<DbContext>> Static;
		public List<Action<DbContext>> Instance;
	}
}