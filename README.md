EntityFrameworkTriggers
=======================

Adds events for entity inserting, inserted, updating, updated, deleting, and deleted

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFrameworkTriggers/

<strong>qsync/await supported</strong>

This version targets the latest .NET and Entity Framework versions. For >= .NET 4.0 and >= EF5, get https://github.com/NickStrupat/EntityFrameworkCodeFirstTriggers (NuGet link in that repository's README)

## Usage

    class Person : EntityWithTriggers<Person> {
        [Key]
        public Int64 Id { get; protected set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
    }
    
    class Context : DbContextWithTriggers {
        public DbSet<Person> People { get; set; }
    }
    
    using (var context = new Context()) {
        var nickStrupat = new Person {
                                         FirstName = "Nick",
                                         LastName = "Strupat"
        };
        nickStrupat.Inserting += e => Console.WriteLine("Inserting " + e.FirstName);
        nickStrupat.Updating += e => Console.WriteLine("Updating " + e.FirstName);
        nickStrupat.Deleting += e => Console.WriteLine("Deleting " + e.FirstName);
        nickStrupat.Inserted += e => Console.WriteLine("Inserted " + e.FirstName);
        nickStrupat.Updated += e => Console.WriteLine("Updated " + e.FirstName);
        nickStrupat.Deleted += e => Console.WriteLine("Deleted " + e.FirstName);
        
        context.People.Add(nickStrupat);
        context.SaveChanges();
        
        nickStrupat.FirstName = "Nicholas";
        context.SaveChanges();
        
        context.People.Remove(nickStrupat);
        await context.SaveChangesAsync();
        
        context.Database.Delete();
    }
