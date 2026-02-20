using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITrade.ApiServices.Helpers
{
    public interface IDevDataSeederService
    {
        Task SeedDevDataAsync(Context database);
    }

    public class DevDataSeederService : IDevDataSeederService
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        // All dev users will have this password
        private const string DefaultPassword = "Password123!";

        public DevDataSeederService(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public async Task SeedDevDataAsync(Context database)
        {
            //Only seed if no users exist (beyond system/admin accounts)
            if (await database.Users.CountAsync() > 0)
            {
                return;
            }

            // Seed in order of dependencies
            var tags = await SeedTagsAsync(database);
            var users = await SeedUsersAsync(database);
            await SeedUserProfileTagsAsync(database, users, tags);
            var projects = await SeedProjectsAsync(database, users, tags);
            await SeedRequestsAsync(database, users, projects);
            await SeedReviewsAsync(database, users);
        }

        private static async Task<List<Tag>> SeedTagsAsync(Context database)
        {
            if (await database.Tags.AnyAsync())
            {
                return await database.Tags.ToListAsync();
            }

            var tags = new List<Tag>
            {
                new Tag { Name = "C#" },
                new Tag { Name = ".NET" },
                new Tag { Name = "React" },
                new Tag { Name = "TypeScript" },
                new Tag { Name = "JavaScript" },
                new Tag { Name = "Python" },
                new Tag { Name = "SQL" },
                new Tag { Name = "Azure" },
                new Tag { Name = "Docker" },
                new Tag { Name = "Kubernetes" },
                new Tag { Name = "REST API" },
                new Tag { Name = "GraphQL" },
                new Tag { Name = "Machine Learning" },
                new Tag { Name = "Data Science" },
                new Tag { Name = "DevOps" },
                new Tag { Name = "UI/UX Design" },
                new Tag { Name = "Mobile Development" },
                new Tag { Name = "Web Development" },
                new Tag { Name = "Database Design" },
                new Tag { Name = "Cloud Architecture" }
            };

            database.Tags.AddRange(tags);
            await database.SaveChangesAsync();

            return tags;
        }

        private async Task<List<User>> SeedUsersAsync(Context database)
        {
            var users = new List<User>
            {
                // Clients
                CreateUser("client1@test.com", "ClientAlice", UserRoleEnum.Client, -30),
                CreateUser("client2@test.com", "ClientBob", UserRoleEnum.Client, -25),
                CreateUser("client3@test.com", "ClientCharlie", UserRoleEnum.Client, -20),

                // Specialists
                CreateUser("specialist1@test.com", "SpecialistDana", UserRoleEnum.Specialist, -28),
                CreateUser("specialist2@test.com", "SpecialistEvan", UserRoleEnum.Specialist, -22),
                CreateUser("specialist3@test.com", "SpecialistFiona", UserRoleEnum.Specialist, -15),
                CreateUser("specialist4@test.com", "SpecialistGeorge", UserRoleEnum.Specialist, -10),

                // Admin
                CreateUser("admin@test.com", "AdminHelen", UserRoleEnum.Admin, -60)
            };

            database.Users.AddRange(users);
            await database.SaveChangesAsync();

            return users;
        }

        private User CreateUser(string email, string username, UserRoleEnum role, int createdDaysAgo)
        {
            var user = new User
            {
                Email = email,
                Username = username,
                UserRoleId = (int)role,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(createdDaysAgo)
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, DefaultPassword);
            return user;
        }

        private static async Task SeedUserProfileTagsAsync(Context database, List<User> users, List<Tag> tags)
        {
            var specialists = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Specialist).ToList();
            var userProfileTags = new List<UserProfileTag>();

            // Dana - Backend specialist
            var dana = specialists.FirstOrDefault(s => s.Username == "SpecialistDana");
            if (dana != null)
            {
                var danaTags = tags.Where(t => t.Name is "C#" or ".NET" or "SQL" or "REST API" or "Azure").ToList();
                userProfileTags.AddRange(danaTags.Select(t => new UserProfileTag
                {
                    UserId = dana.Id,
                    TagId = t.Id,
                    TagName = t.Name
                }));
            }

            // Evan - Frontend specialist
            var evan = specialists.FirstOrDefault(s => s.Username == "SpecialistEvan");
            if (evan != null)
            {
                var evanTags = tags.Where(t => t.Name is "React" or "TypeScript" or "JavaScript" or "UI/UX Design" or "Web Development").ToList();
                userProfileTags.AddRange(evanTags.Select(t => new UserProfileTag
                {
                    UserId = evan.Id,
                    TagId = t.Id,
                    TagName = t.Name
                }));
            }

            // Fiona - Full stack specialist
            var fiona = specialists.FirstOrDefault(s => s.Username == "SpecialistFiona");
            if (fiona != null)
            {
                var fionaTags = tags.Where(t => t.Name is "C#" or ".NET" or "React" or "TypeScript" or "Docker" or "Azure").ToList();
                userProfileTags.AddRange(fionaTags.Select(t => new UserProfileTag
                {
                    UserId = fiona.Id,
                    TagId = t.Id,
                    TagName = t.Name
                }));
            }

            // George - DevOps specialist
            var george = specialists.FirstOrDefault(s => s.Username == "SpecialistGeorge");
            if (george != null)
            {
                var georgeTags = tags.Where(t => t.Name is "Docker" or "Kubernetes" or "Azure" or "DevOps" or "Cloud Architecture").ToList();
                userProfileTags.AddRange(georgeTags.Select(t => new UserProfileTag
                {
                    UserId = george.Id,
                    TagId = t.Id,
                    TagName = t.Name
                }));
            }

            database.UserProfileTags.AddRange(userProfileTags);
            await database.SaveChangesAsync();
        }

        private static async Task<List<Project>> SeedProjectsAsync(Context database, List<User> users, List<Tag> tags)
        {
            var clients = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Client).ToList();
            var specialists = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Specialist).ToList();

            var alice = clients.FirstOrDefault(c => c.Username == "ClientAlice");
            var bob = clients.FirstOrDefault(c => c.Username == "ClientBob");
            var charlie = clients.FirstOrDefault(c => c.Username == "ClientCharlie");
            var dana = specialists.FirstOrDefault(s => s.Username == "SpecialistDana");
            var fiona = specialists.FirstOrDefault(s => s.Username == "SpecialistFiona");

            var projects = new List<Project>
            {
                // Hiring projects (no worker assigned)
                new Project
                {
                    Name = "E-commerce Platform Backend",
                    Description = "Build a scalable REST API for an e-commerce platform with user authentication, product catalog, and order management.",
                    Deadline = DateTime.UtcNow.AddDays(60),
                    OwnerId = alice!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Project
                {
                    Name = "Mobile App Dashboard",
                    Description = "Create a responsive dashboard for tracking mobile app analytics with charts and real-time data.",
                    Deadline = DateTime.UtcNow.AddDays(45),
                    OwnerId = alice.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Project
                {
                    Name = "CI/CD Pipeline Setup",
                    Description = "Set up automated deployment pipelines using Azure DevOps for a microservices architecture.",
                    Deadline = DateTime.UtcNow.AddDays(30),
                    OwnerId = bob!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },

                // In Progress projects (worker assigned)
                new Project
                {
                    Name = "Customer Portal Redesign",
                    Description = "Modernize the customer portal with React and improve user experience.",
                    Deadline = DateTime.UtcNow.AddDays(40),
                    OwnerId = bob.Id,
                    WorkerId = fiona!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Project
                {
                    Name = "API Integration Service",
                    Description = "Build a service to integrate with third-party payment providers and shipping APIs.",
                    Deadline = DateTime.UtcNow.AddDays(25),
                    OwnerId = charlie!.Id,
                    WorkerId = dana!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },

                // Completed projects
                new Project
                {
                    Name = "Landing Page Development",
                    Description = "Create a marketing landing page with contact form and newsletter signup.",
                    Deadline = DateTime.UtcNow.AddDays(-5),
                    OwnerId = alice.Id,
                    WorkerId = fiona.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Project
                {
                    Name = "Database Migration",
                    Description = "Migrate legacy SQL Server database to Azure SQL with data validation.",
                    Deadline = DateTime.UtcNow.AddDays(-10),
                    OwnerId = charlie.Id,
                    WorkerId = dana.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },

                // On Hold project
                new Project
                {
                    Name = "Machine Learning Model",
                    Description = "Develop a recommendation engine using Python and ML algorithms.",
                    Deadline = DateTime.UtcNow.AddDays(90),
                    OwnerId = bob.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.OnHold,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                }
            };

            database.Projects.AddRange(projects);
            await database.SaveChangesAsync();

            // Add tags to projects
            var projectTags = new List<ProjectTag>();

            // E-commerce Platform Backend tags
            var ecommerceProject = projects.First(p => p.Name == "E-commerce Platform Backend");
            var ecommerceTags = tags.Where(t => t.Name is "C#" or ".NET" or "REST API" or "SQL" or "Azure").ToList();
            projectTags.AddRange(ecommerceTags.Select(t => new ProjectTag { ProjectId = ecommerceProject.Id, TagId = t.Id }));

            // Mobile App Dashboard tags
            var dashboardProject = projects.First(p => p.Name == "Mobile App Dashboard");
            var dashboardTags = tags.Where(t => t.Name is "React" or "TypeScript" or "UI/UX Design").ToList();
            projectTags.AddRange(dashboardTags.Select(t => new ProjectTag { ProjectId = dashboardProject.Id, TagId = t.Id }));

            // CI/CD Pipeline Setup tags
            var cicdProject = projects.First(p => p.Name == "CI/CD Pipeline Setup");
            var cicdTags = tags.Where(t => t.Name is "DevOps" or "Docker" or "Kubernetes" or "Azure").ToList();
            projectTags.AddRange(cicdTags.Select(t => new ProjectTag { ProjectId = cicdProject.Id, TagId = t.Id }));

            // Customer Portal Redesign tags
            var portalProject = projects.First(p => p.Name == "Customer Portal Redesign");
            var portalTags = tags.Where(t => t.Name is "React" or "TypeScript" or "UI/UX Design" or "Web Development").ToList();
            projectTags.AddRange(portalTags.Select(t => new ProjectTag { ProjectId = portalProject.Id, TagId = t.Id }));

            // API Integration Service tags
            var integrationProject = projects.First(p => p.Name == "API Integration Service");
            var integrationTags = tags.Where(t => t.Name is "C#" or ".NET" or "REST API").ToList();
            projectTags.AddRange(integrationTags.Select(t => new ProjectTag { ProjectId = integrationProject.Id, TagId = t.Id }));

            // Landing Page Development tags
            var landingProject = projects.First(p => p.Name == "Landing Page Development");
            var landingTags = tags.Where(t => t.Name is "JavaScript" or "UI/UX Design" or "Web Development").ToList();
            projectTags.AddRange(landingTags.Select(t => new ProjectTag { ProjectId = landingProject.Id, TagId = t.Id }));

            // Database Migration tags
            var migrationProject = projects.First(p => p.Name == "Database Migration");
            var migrationTags = tags.Where(t => t.Name is "SQL" or "Azure" or "Database Design").ToList();
            projectTags.AddRange(migrationTags.Select(t => new ProjectTag { ProjectId = migrationProject.Id, TagId = t.Id }));

            // Machine Learning Model tags
            var mlProject = projects.First(p => p.Name == "Machine Learning Model");
            var mlTags = tags.Where(t => t.Name is "Python" or "Machine Learning" or "Data Science").ToList();
            projectTags.AddRange(mlTags.Select(t => new ProjectTag { ProjectId = mlProject.Id, TagId = t.Id }));

            database.ProjectTags.AddRange(projectTags);
            await database.SaveChangesAsync();

            return projects;
        }

        private static async Task SeedRequestsAsync(Context database, List<User> users, List<Project> projects)
        {
            var clients = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Client).ToList();
            var specialists = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Specialist).ToList();

            var alice = clients.First(c => c.Username == "ClientAlice");
            var bob = clients.First(c => c.Username == "ClientBob");
            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var evan = specialists.First(s => s.Username == "SpecialistEvan");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");
            var george = specialists.First(s => s.Username == "SpecialistGeorge");

            var ecommerceProject = projects.First(p => p.Name == "E-commerce Platform Backend");
            var dashboardProject = projects.First(p => p.Name == "Mobile App Dashboard");
            var cicdProject = projects.First(p => p.Name == "CI/CD Pipeline Setup");

            var requests = new List<Request>
            {
                // Applications from specialists to hiring projects (pending)
                new Request
                {
                    Message = "I have extensive experience with .NET and REST APIs. I'd love to work on this e-commerce platform!",
                    SenderId = dana.Id,
                    ReceiverId = alice.Id,
                    ProjectId = ecommerceProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Request
                {
                    Message = "Full stack developer here. I can handle both the API and any frontend needs for this project.",
                    SenderId = fiona.Id,
                    ReceiverId = alice.Id,
                    ProjectId = ecommerceProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },

                // Application for dashboard project (pending)
                new Request
                {
                    Message = "React and TypeScript are my specialties. I can create beautiful, responsive dashboards.",
                    SenderId = evan.Id,
                    ReceiverId = alice.Id,
                    ProjectId = dashboardProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = null,
                    CreatedAt = DateTime.UtcNow.AddHours(-12)
                },

                // Application for CI/CD project (pending)
                new Request
                {
                    Message = "DevOps is my passion. I've set up pipelines for multiple enterprise clients using Azure DevOps.",
                    SenderId = george.Id,
                    ReceiverId = bob.Id,
                    ProjectId = cicdProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = null,
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                },

                // Invitation from client to specialist (pending)
                new Request
                {
                    Message = "Hi Dana, I saw your profile and think you'd be perfect for our e-commerce project. Would you like to join?",
                    SenderId = alice.Id,
                    ReceiverId = dana.Id,
                    ProjectId = ecommerceProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Invitation,
                    Accepted = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },

                // Rejected application
                new Request
                {
                    Message = "I'm interested in the CI/CD pipeline project.",
                    SenderId = evan.Id,
                    ReceiverId = bob.Id,
                    ProjectId = cicdProject.Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },

                // Accepted application (this led to an in-progress project)
                new Request
                {
                    Message = "I'd like to work on the customer portal redesign.",
                    SenderId = fiona.Id,
                    ReceiverId = bob.Id,
                    ProjectId = projects.First(p => p.Name == "Customer Portal Redesign").Id,
                    RequestTypeId = (int)ProjectRequestTypeEnum.Application,
                    Accepted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                }
            };

            database.Requests.AddRange(requests);
            await database.SaveChangesAsync();
        }

        private static async Task SeedReviewsAsync(Context database, List<User> users)
        {
            var clients = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Client).ToList();
            var specialists = users.Where(u => u.UserRoleId == (int)UserRoleEnum.Specialist).ToList();

            var alice = clients.First(c => c.Username == "ClientAlice");
            var charlie = clients.First(c => c.Username == "ClientCharlie");
            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");

            var reviews = new List<Review>
            {
                // Reviews for completed projects
                // Alice reviews Fiona (Landing Page project)
                new Review
                {
                    Title = "Excellent work on landing page",
                    Comment = "Fiona delivered the landing page ahead of schedule with great attention to detail. Very responsive to feedback and made all requested changes quickly.",
                    Rating = 5,
                    ReviewerId = alice.Id,
                    RevieweeId = fiona.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },

                // Fiona reviews Alice (as a client)
                new Review
                {
                    Title = "Great client to work with",
                    Comment = "Alice provided clear requirements and timely feedback throughout the project. Would work with again!",
                    Rating = 5,
                    ReviewerId = fiona.Id,
                    RevieweeId = alice.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },

                // Charlie reviews Dana (Database Migration project)
                new Review
                {
                    Title = "Professional database migration",
                    Comment = "Dana handled our complex database migration flawlessly. Zero data loss and minimal downtime. Highly recommended for database work.",
                    Rating = 5,
                    ReviewerId = charlie.Id,
                    RevieweeId = dana.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },

                // Dana reviews Charlie
                new Review
                {
                    Title = "Good collaboration",
                    Comment = "Charlie was helpful in providing access to legacy systems and answering questions about the data structure.",
                    Rating = 4,
                    ReviewerId = dana.Id,
                    RevieweeId = charlie.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },

                // Additional review - showing a 4-star review
                new Review
                {
                    Title = "Solid work overall",
                    Comment = "Good technical skills but communication could have been better. Final result was what we needed.",
                    Rating = 4,
                    ReviewerId = alice.Id,
                    RevieweeId = dana.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                }
            };

            database.Reviews.AddRange(reviews);
            await database.SaveChangesAsync();
        }
    }
}
