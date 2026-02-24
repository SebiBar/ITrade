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
                new Tag { Name = "Cloud Architecture" },
                
                // Expanded Tags
                new Tag { Name = "AWS" },
                new Tag { Name = "Vue.js" },
                new Tag { Name = "Angular" },
                new Tag { Name = "Node.js" },
                new Tag { Name = "Java" },
                new Tag { Name = "Spring Boot" },
                new Tag { Name = "PHP" },
                new Tag { Name = "Laravel" },
                new Tag { Name = "Cybersecurity" },
                new Tag { Name = "Blockchain" }
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
                CreateUser("client1@test.com", "ClientAlice", UserRoleEnum.Client, -60),
                CreateUser("client2@test.com", "ClientBob", UserRoleEnum.Client, -55),
                CreateUser("client3@test.com", "ClientCharlie", UserRoleEnum.Client, -50),
                CreateUser("client4@test.com", "ClientDavid", UserRoleEnum.Client, -40),
                CreateUser("client5@test.com", "ClientEmma", UserRoleEnum.Client, -35),
                CreateUser("client6@test.com", "ClientFrank", UserRoleEnum.Client, -20),

                // Specialists
                CreateUser("specialist1@test.com", "SpecialistDana", UserRoleEnum.Specialist, -58),
                CreateUser("specialist2@test.com", "SpecialistEvan", UserRoleEnum.Specialist, -52),
                CreateUser("specialist3@test.com", "SpecialistFiona", UserRoleEnum.Specialist, -45),
                CreateUser("specialist4@test.com", "SpecialistGeorge", UserRoleEnum.Specialist, -40),
                CreateUser("specialist5@test.com", "SpecialistHannah", UserRoleEnum.Specialist, -30),
                CreateUser("specialist6@test.com", "SpecialistIan", UserRoleEnum.Specialist, -25),
                CreateUser("specialist7@test.com", "SpecialistJack", UserRoleEnum.Specialist, -15),
                CreateUser("specialist8@test.com", "SpecialistKaren", UserRoleEnum.Specialist, -10),

                // Admin
                CreateUser("admin@test.com", "AdminHelen", UserRoleEnum.Admin, -90)
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
                userProfileTags.AddRange(danaTags.Select(t => new UserProfileTag { UserId = dana.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Evan - Frontend specialist
            var evan = specialists.FirstOrDefault(s => s.Username == "SpecialistEvan");
            if (evan != null)
            {
                var evanTags = tags.Where(t => t.Name is "React" or "TypeScript" or "JavaScript" or "UI/UX Design" or "Web Development").ToList();
                userProfileTags.AddRange(evanTags.Select(t => new UserProfileTag { UserId = evan.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Fiona - Full stack specialist
            var fiona = specialists.FirstOrDefault(s => s.Username == "SpecialistFiona");
            if (fiona != null)
            {
                var fionaTags = tags.Where(t => t.Name is "C#" or ".NET" or "React" or "TypeScript" or "Docker" or "Azure").ToList();
                userProfileTags.AddRange(fionaTags.Select(t => new UserProfileTag { UserId = fiona.Id, TagId = t.Id, TagName = t.Name }));
            }

            // George - DevOps specialist
            var george = specialists.FirstOrDefault(s => s.Username == "SpecialistGeorge");
            if (george != null)
            {
                var georgeTags = tags.Where(t => t.Name is "Docker" or "Kubernetes" or "Azure" or "DevOps" or "Cloud Architecture").ToList();
                userProfileTags.AddRange(georgeTags.Select(t => new UserProfileTag { UserId = george.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Hannah - Enterprise Java specialist
            var hannah = specialists.FirstOrDefault(s => s.Username == "SpecialistHannah");
            if (hannah != null)
            {
                var hannahTags = tags.Where(t => t.Name is "Java" or "Spring Boot" or "SQL" or "AWS" or "Database Design").ToList();
                userProfileTags.AddRange(hannahTags.Select(t => new UserProfileTag { UserId = hannah.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Ian - Modern Web specialist
            var ian = specialists.FirstOrDefault(s => s.Username == "SpecialistIan");
            if (ian != null)
            {
                var ianTags = tags.Where(t => t.Name is "Node.js" or "Vue.js" or "GraphQL" or "Web Development" or "JavaScript").ToList();
                userProfileTags.AddRange(ianTags.Select(t => new UserProfileTag { UserId = ian.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Jack - Security & Data specialist
            var jack = specialists.FirstOrDefault(s => s.Username == "SpecialistJack");
            if (jack != null)
            {
                var jackTags = tags.Where(t => t.Name is "Cybersecurity" or "Python" or "Cloud Architecture" or "AWS").ToList();
                userProfileTags.AddRange(jackTags.Select(t => new UserProfileTag { UserId = jack.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Karen - Mobile & UI specialist
            var karen = specialists.FirstOrDefault(s => s.Username == "SpecialistKaren");
            if (karen != null)
            {
                var karenTags = tags.Where(t => t.Name is "Mobile Development" or "UI/UX Design" or "Angular" or "Web Development").ToList();
                userProfileTags.AddRange(karenTags.Select(t => new UserProfileTag { UserId = karen.Id, TagId = t.Id, TagName = t.Name }));
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
            var david = clients.FirstOrDefault(c => c.Username == "ClientDavid");
            var emma = clients.FirstOrDefault(c => c.Username == "ClientEmma");
            var frank = clients.FirstOrDefault(c => c.Username == "ClientFrank");

            var dana = specialists.FirstOrDefault(s => s.Username == "SpecialistDana");
            var evan = specialists.FirstOrDefault(s => s.Username == "SpecialistEvan");
            var fiona = specialists.FirstOrDefault(s => s.Username == "SpecialistFiona");
            var hannah = specialists.FirstOrDefault(s => s.Username == "SpecialistHannah");
            var ian = specialists.FirstOrDefault(s => s.Username == "SpecialistIan");
            var jack = specialists.FirstOrDefault(s => s.Username == "SpecialistJack");
            var karen = specialists.FirstOrDefault(s => s.Username == "SpecialistKaren");

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
                new Project
                {
                    Name = "Blockchain Smart Contracts",
                    Description = "Develop secure smart contracts for a new decentralized finance application.",
                    Deadline = DateTime.UtcNow.AddDays(90),
                    OwnerId = david!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Project
                {
                    Name = "Legacy PHP Migration",
                    Description = "Migrate a legacy PHP application to a modern Laravel framework with an updated database schema.",
                    Deadline = DateTime.UtcNow.AddDays(120),
                    OwnerId = emma!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
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
                new Project
                {
                    Name = "Mobile App Refactoring",
                    Description = "Refactor the existing mobile app to improve performance and update the UI design.",
                    Deadline = DateTime.UtcNow.AddDays(15),
                    OwnerId = frank!.Id,
                    WorkerId = karen!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Project
                {
                    Name = "Cloud Security Audit",
                    Description = "Perform a comprehensive security audit of our AWS infrastructure and implement necessary patches.",
                    Deadline = DateTime.UtcNow.AddDays(5),
                    OwnerId = david.Id,
                    WorkerId = jack!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
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
                new Project
                {
                    Name = "E-learning Portal",
                    Description = "Build a complete e-learning management system with video streaming and quizzes.",
                    Deadline = DateTime.UtcNow.AddDays(-2),
                    OwnerId = emma.Id,
                    WorkerId = ian!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new Project
                {
                    Name = "Inventory Management System",
                    Description = "Develop a robust backend system in Java for managing warehouse inventory in real-time.",
                    Deadline = DateTime.UtcNow.AddDays(-15),
                    OwnerId = frank.Id,
                    WorkerId = hannah!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Project
                {
                    Name = "Corporate Website",
                    Description = "Complete redesign of the corporate website using modern frontend frameworks.",
                    Deadline = DateTime.UtcNow.AddDays(-25),
                    OwnerId = alice.Id,
                    WorkerId = evan!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-70)
                },

                // On Hold projects
                new Project
                {
                    Name = "Machine Learning Model",
                    Description = "Develop a recommendation engine using Python and ML algorithms.",
                    Deadline = DateTime.UtcNow.AddDays(90),
                    OwnerId = bob.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.OnHold,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Project
                {
                    Name = "AI Chatbot Integration",
                    Description = "Integrate an AI-powered customer service chatbot into the main portal.",
                    Deadline = DateTime.UtcNow.AddDays(120),
                    OwnerId = charlie.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.OnHold,
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                }
            };

            database.Projects.AddRange(projects);
            await database.SaveChangesAsync();

            // Add tags to projects
            var projectTags = new List<ProjectTag>();

            // Helper function to add tags easily
            void AttachTagsToProject(string projectName, params string[] tagNames)
            {
                var project = projects.First(p => p.Name == projectName);
                var matchedTags = tags.Where(t => tagNames.Contains(t.Name)).ToList();
                projectTags.AddRange(matchedTags.Select(t => new ProjectTag { ProjectId = project.Id, TagId = t.Id }));
            }

            AttachTagsToProject("E-commerce Platform Backend", "C#", ".NET", "REST API", "SQL", "Azure");
            AttachTagsToProject("Mobile App Dashboard", "React", "TypeScript", "UI/UX Design");
            AttachTagsToProject("CI/CD Pipeline Setup", "DevOps", "Docker", "Kubernetes", "Azure");
            AttachTagsToProject("Blockchain Smart Contracts", "Blockchain", "Cybersecurity");
            AttachTagsToProject("Legacy PHP Migration", "PHP", "Laravel", "Database Design", "SQL");

            AttachTagsToProject("Customer Portal Redesign", "React", "TypeScript", "UI/UX Design", "Web Development");
            AttachTagsToProject("API Integration Service", "C#", ".NET", "REST API");
            AttachTagsToProject("Mobile App Refactoring", "Mobile Development", "UI/UX Design");
            AttachTagsToProject("Cloud Security Audit", "Cybersecurity", "Cloud Architecture", "AWS");

            AttachTagsToProject("Landing Page Development", "JavaScript", "UI/UX Design", "Web Development");
            AttachTagsToProject("Database Migration", "SQL", "Azure", "Database Design");
            AttachTagsToProject("E-learning Portal", "Node.js", "Vue.js", "Web Development");
            AttachTagsToProject("Inventory Management System", "Java", "Spring Boot", "Database Design");
            AttachTagsToProject("Corporate Website", "React", "Web Development", "UI/UX Design");

            AttachTagsToProject("Machine Learning Model", "Python", "Machine Learning", "Data Science");
            AttachTagsToProject("AI Chatbot Integration", "Python", "REST API", "Machine Learning");

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
            var charlie = clients.First(c => c.Username == "ClientCharlie");
            var david = clients.First(c => c.Username == "ClientDavid");
            var emma = clients.First(c => c.Username == "ClientEmma");
            var frank = clients.First(c => c.Username == "ClientFrank");

            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var evan = specialists.First(s => s.Username == "SpecialistEvan");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");
            var george = specialists.First(s => s.Username == "SpecialistGeorge");
            var hannah = specialists.First(s => s.Username == "SpecialistHannah");
            var ian = specialists.First(s => s.Username == "SpecialistIan");
            var jack = specialists.First(s => s.Username == "SpecialistJack");
            var karen = specialists.First(s => s.Username == "SpecialistKaren");

            var ecommerceProject = projects.First(p => p.Name == "E-commerce Platform Backend");
            var dashboardProject = projects.First(p => p.Name == "Mobile App Dashboard");
            var cicdProject = projects.First(p => p.Name == "CI/CD Pipeline Setup");
            var blockchainProject = projects.First(p => p.Name == "Blockchain Smart Contracts");
            var phpProject = projects.First(p => p.Name == "Legacy PHP Migration");

            var requests = new List<Request>
            {
                // Applications from specialists to hiring projects (pending)
                new Request { Message = "I have extensive experience with .NET and REST APIs. I'd love to work on this e-commerce platform!", SenderId = dana.Id, ReceiverId = alice.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Request { Message = "Full stack developer here. I can handle both the API and any frontend needs for this project.", SenderId = fiona.Id, ReceiverId = alice.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Request { Message = "React and TypeScript are my specialties. I can create beautiful, responsive dashboards.", SenderId = evan.Id, ReceiverId = alice.Id, ProjectId = dashboardProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-12) },
                new Request { Message = "DevOps is my passion. I've set up pipelines for multiple enterprise clients using Azure DevOps.", SenderId = george.Id, ReceiverId = bob.Id, ProjectId = cicdProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new Request { Message = "I have a strong background in smart contract development and security best practices.", SenderId = ian.Id, ReceiverId = david.Id, ProjectId = blockchainProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-24) },
                new Request { Message = "I've successfully migrated several PHP monoliths to modern frameworks. Let's talk.", SenderId = hannah.Id, ReceiverId = emma.Id, ProjectId = phpProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-3) },

                // Invitations from clients to specialists (pending)
                new Request { Message = "Hi Dana, I saw your profile and think you'd be perfect for our e-commerce project. Would you like to join?", SenderId = alice.Id, ReceiverId = dana.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Request { Message = "We need someone with strong Python background for our ML project, would you be available later this year?", SenderId = bob.Id, ReceiverId = jack.Id, ProjectId = projects.First(p => p.Name == "Machine Learning Model").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-5) },

                // Rejected applications
                new Request { Message = "I'm interested in the CI/CD pipeline project.", SenderId = evan.Id, ReceiverId = bob.Id, ProjectId = cicdProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = false, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Request { Message = "I can do this mobile app refactoring quickly.", SenderId = dana.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Mobile App Refactoring").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = false, CreatedAt = DateTime.UtcNow.AddDays(-22) },

                // Accepted applications/invitations (these led to in-progress or completed projects)
                new Request { Message = "I'd like to work on the customer portal redesign.", SenderId = fiona.Id, ReceiverId = bob.Id, ProjectId = projects.First(p => p.Name == "Customer Portal Redesign").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-16) },
                new Request { Message = "I specialize in AWS architecture and security auditing.", SenderId = jack.Id, ReceiverId = david.Id, ProjectId = projects.First(p => p.Name == "Cloud Security Audit").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-26) },
                new Request { Message = "Happy to help you with the mobile app refactoring!", SenderId = karen.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Mobile App Refactoring").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-21) },
                new Request { Message = "I've reviewed your requirements and can deliver this portal.", SenderId = ian.Id, ReceiverId = emma.Id, ProjectId = projects.First(p => p.Name == "E-learning Portal").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-85) },
                new Request { Message = "I have the Java Spring Boot experience required for this inventory system.", SenderId = hannah.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Inventory Management System").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-95) },
                new Request { Message = "Hi Evan, loved your portfolio. Can you redesign our corporate site?", SenderId = alice.Id, ReceiverId = evan.Id, ProjectId = projects.First(p => p.Name == "Corporate Website").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-75) }
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
            var emma = clients.First(c => c.Username == "ClientEmma");
            var frank = clients.First(c => c.Username == "ClientFrank");

            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var evan = specialists.First(s => s.Username == "SpecialistEvan");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");
            var hannah = specialists.First(s => s.Username == "SpecialistHannah");
            var ian = specialists.First(s => s.Username == "SpecialistIan");

            var reviews = new List<Review>
            {
                // Reviews for completed projects
                new Review { Title = "Excellent work on landing page", Comment = "Fiona delivered the landing page ahead of schedule with great attention to detail. Very responsive to feedback and made all requested changes quickly.", Rating = 5, ReviewerId = alice.Id, RevieweeId = fiona.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Review { Title = "Great client to work with", Comment = "Alice provided clear requirements and timely feedback throughout the project. Would work with again!", Rating = 5, ReviewerId = fiona.Id, RevieweeId = alice.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Review { Title = "Professional database migration", Comment = "Dana handled our complex database migration flawlessly. Zero data loss and minimal downtime. Highly recommended for database work.", Rating = 5, ReviewerId = charlie.Id, RevieweeId = dana.Id, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                new Review { Title = "Good collaboration", Comment = "Charlie was helpful in providing access to legacy systems and answering questions about the data structure.", Rating = 4, ReviewerId = dana.Id, RevieweeId = charlie.Id, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                new Review { Title = "Solid work overall", Comment = "Good technical skills but communication could have been better. Final result was what we needed.", Rating = 4, ReviewerId = alice.Id, RevieweeId = dana.Id, CreatedAt = DateTime.UtcNow.AddDays(-60) },
                
                // New Reviews
                new Review { Title = "Fantastic E-learning platform", Comment = "Ian built an incredible product. His knowledge of Node.js and modern web frameworks is impressive.", Rating = 5, ReviewerId = emma.Id, RevieweeId = ian.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Review { Title = "A pleasure to work for", Comment = "Emma is a great communicator and pays on time. The project scope was very clear.", Rating = 5, ReviewerId = ian.Id, RevieweeId = emma.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Review { Title = "Robust Inventory System", Comment = "Hannah delivered a very solid Java backend. We had some minor bugs in the testing phase but she fixed them very fast.", Rating = 4, ReviewerId = frank.Id, RevieweeId = hannah.Id, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new Review { Title = "Responsive and talented", Comment = "Evan gave our corporate website the exact facelift it needed. Highly recommend for any front-end UI work.", Rating = 5, ReviewerId = alice.Id, RevieweeId = evan.Id, CreatedAt = DateTime.UtcNow.AddDays(-20) }
            };

            database.Reviews.AddRange(reviews);
            await database.SaveChangesAsync();
        }
    }
}