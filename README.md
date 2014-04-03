EntityFrameworkTriggers
=======================

Adds events for entity inserting, inserted, updating, updated, deleting, and deleted

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFrameworkTriggers/

<strong>async/await supported</strong>

This version targets the latest .NET and Entity Framework versions.

For .NET 4.0 and EF5, check out https://github.com/NickStrupat/EntityFrameworkCodeFirstTriggers (NuGet link in that repository's README)

## Usage

    class Person : EntityWithTriggers<Person, Context> {
        public Int64 Id { get; protected set; }
        public DateTime InsertDateTime { get; protected set; }
        public DateTime UpdateDateTime { get; protected set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Person() {
            Inserting += (context, entity) => entity.InsertDateTime = entity.UpdateDateTime = DateTime.Now;
            Updating += (context, entity) => entity.UpdateDateTime = DateTime.Now;
        }
    }
	
	class LogEntry {
        public Int64 Id { get; protected set; }
		public String Message { get; set; }
	}
    
    class Context : DbContextWithTriggers<Context> {
        public DbSet<Person> People { get; set; }
        public DbSet<LogEntry> Log { get; set; }
    }
    
    using (var context = new Context()) {
        var nickStrupat = new Person {
                                         FirstName = "Nick",
                                         LastName = "Strupat"
        };
        nickStrupat.Inserting += (c, e) => {
			c.Log.Add(new LogEntry {Message = "Insert trigger first for " + e.FirstName};
			Console.WriteLine("Inserting " + e.FirstName);
		};
        nickStrupat.Updating += (c, e) => Console.WriteLine("Updating " + e.FirstName);
        nickStrupat.Deleting += (c, e) => Console.WriteLine("Deleting " + e.FirstName);
        nickStrupat.Inserted += (c, e) => Console.WriteLine("Inserted " + e.FirstName);
        nickStrupat.Updated += (c, e) => Console.WriteLine("Updated " + e.FirstName);
        nickStrupat.Deleted += (c, e) => Console.WriteLine("Deleted " + e.FirstName);
        
        context.People.Add(nickStrupat);
        context.SaveChanges();
        
        nickStrupat.FirstName = "Nicholas";
        context.SaveChanges();
        
        context.People.Remove(nickStrupat);
        await context.SaveChangesAsync();
        
        context.Database.Delete();
    }
