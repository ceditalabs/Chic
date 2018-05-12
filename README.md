# Chic
Lightweight, intuitive and performant Data Layer (Repositories, Change Tracking) that sits on top of Dapper.

**Labs Notice** - This is a Cedita Labs project, and is offered unsupported. We encourage external contributions.

## Get Started
Chic is available as a Prerelease package from NuGet.

    Install-Package Chic -pre
    
### Configuration w/ ASP.NET Core
The easiest way to use Chic is through Dependency Injection. With ASP.NET Core, the configuration is quite simple to wire up your repositories for usage.

    public void ConfigureServices(IServiceCollection services)
    {
        // ..
        
        // Database Connection
        services.AddTransient<IDbConnection>(db => new SqlConnection(Configuration.GetConnectionString("Default")));
        // Chic Generic Repositories
        services.AddScoped(typeof(Chic.Abstractions.IRepository<>), typeof(Chic.Repository<>));
        
        // ..
    }

### Your First Model
Chic currently supports entities that have a Key property (of any type - but the below example uses the int key method). These entities must inherit from `IKeyedEntity`.

    public class DemoModel : IKeyedEntity 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEpic { get; set; }
    }
    
### Using your first Auto Repository
By registering our abstraction for `IRepository<>` we can use it with any `IKeyedEntity` automatically. As an example service below, we simply inject our `IRepository<DemoModel>` and have access to the database directly.

    public class DataService
    {
        private readonly IRepository<DemoModel> demoModelRepository;
        public DataService(IRepository<DemoModel> demoModelRepository)
        {
            this.demoModelRepository = demoModelRepository;
        }
        
        public Task AddDemo()
        {
            await demoModeRepository.InsertAsync(new DemoModel { Name = "Hello World", IsEpic = true });
        }
    }
    
### Querying with WHERE
Your repository provides basic extensions for querying your single table with a WHERE.

    var epicDemoModels = await demoModelRepository.GetByWhereAsync("IsEpic = 1");
    
## Performance Considerations
The driving force behind Chic is to enable performant applications without exerting unnecessary effort to implementation detail of said performance.

Under the hood we use Dapper for object mapping, which incurs a hit on first use of mapping for each type used. This can not be pre-warmed.

Chic's internals use Expressions (rather than Reflection) for accessing properties on objects. On creation of a repository, Chic incurs a hit to generate a type map that is then stored in memory. This too has no direct support for pre-warmup, however can be simulated by creating an instance of each repository type you intend to use at startup.

### Bulk Insertions
Chic supports SqlBulkCopy for mass data insertion. Instead of looping on an `InsertAsync()` call, `InsertManyAsync()` should be used instead.

### Updates
In a future version of Chic, you will be able to specify columns to update on a model either automatically through change tracking, or by manually specifying which columns to update. This will internally affect the query generation used in UPDATE statements.
