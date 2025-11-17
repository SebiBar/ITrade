using ITrade.DB;

namespace ITrade.ApiServices.Helpers
{
    public interface IDatabaseSeedService
    {
        void MigrateDatabase(IServiceScope serviceScope);
        Task SeedDatabase(Context database);
    }
}
