using ITrade.DB;
using ITrade.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITrade.ApiServices.Helpers
{
    public class DatabaseSeedService : IDatabaseSeedService
    {
        public void MigrateDatabase(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<Context>();

            // Apply pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                var strat = context.Database.CreateExecutionStrategy();
                strat.Execute(() => context.Database.Migrate());
            }

            // Always attempt to seed (flag ensures one-time run per DB)
            SeedDatabase(context).GetAwaiter().GetResult();
        }

        public async Task SeedDatabase(Context database)
        {
            var seedRecord = await database.SeedStatuses.FirstOrDefaultAsync();
            if (seedRecord == null)
            {
                seedRecord = new SeedStatus { ShouldSeedDatabase = false };
                database.SeedStatuses.Add(seedRecord);
                await database.SaveChangesAsync();
            }
            else if (!seedRecord.ShouldSeedDatabase)
            {
                return;
            }

            //Seed methods
            await SeedUserRoles(database);
            await SeedTokenTypes(database);
            await SeedProjectStatusTypes(database);
            await SeedRequestTypes(database);

        }

        private static async Task SeedProjectStatusTypes(Context database)
        {
            if (!await database.ProjectStatusTypes.AnyAsync())
            {
                var statusTypes = new List<ProjectStatusType>
                {
                    new ProjectStatusType { Name = "Hiring" },
                    new ProjectStatusType { Name = "In Progress" },
                    new ProjectStatusType { Name = "Completed" },
                    new ProjectStatusType { Name = "On Hold" },
                    new ProjectStatusType { Name = "Cancelled" }
                };
                database.ProjectStatusTypes.AddRange(statusTypes);
                await database.SaveChangesAsync();
            }
        }

        private static async Task SeedRequestTypes(Context database)
        {
            if (!await database.ProjectRequestTypes.AnyAsync())
            {
                var requestTypes = new List<ProjectRequestType>
                {
                    new ProjectRequestType { Name = "Invitation" },
                    new ProjectRequestType { Name = "Application" },
                };
                database.ProjectRequestTypes.AddRange(requestTypes);
                await database.SaveChangesAsync();
            }
        }

        private static async Task SeedTokenTypes(Context database)
        {
            if (!await database.TokenTypes.AnyAsync())
            {
                var tokenTypes = new List<TokenType>
                {
                    new TokenType { Name = "Refresh" },
                    new TokenType { Name = "VerifyEmail" },
                    new TokenType { Name = "ForgotPassword" }
                };
                database.TokenTypes.AddRange(tokenTypes);
                await database.SaveChangesAsync();
            }
        }

        private static async Task SeedUserRoles(Context database)
        {
            if (!await database.UserRoles.AnyAsync())
            {
                var userRoles = new List<UserRole>
                {
                    new UserRole { Name = "Client" },
                    new UserRole { Name = "Specialist" },
                    new UserRole { Name = "Admin" }
                };
                database.UserRoles.AddRange(userRoles);
                await database.SaveChangesAsync();
            }
        }
    }
}
