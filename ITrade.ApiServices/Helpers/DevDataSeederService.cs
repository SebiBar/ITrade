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
                CreateUser("client7@test.com", "ClientGrace", UserRoleEnum.Client, -42),
                CreateUser("client8@test.com", "ClientHenry", UserRoleEnum.Client, -38),
                CreateUser("client9@test.com", "ClientIsla", UserRoleEnum.Client, -33),
                CreateUser("client10@test.com", "ClientJason", UserRoleEnum.Client, -29),

                // Specialists
                CreateUser("specialist1@test.com", "SpecialistDana", UserRoleEnum.Specialist, -58),
                CreateUser("specialist2@test.com", "SpecialistEvan", UserRoleEnum.Specialist, -52),
                CreateUser("specialist3@test.com", "SpecialistFiona", UserRoleEnum.Specialist, -45),
                CreateUser("specialist4@test.com", "SpecialistGeorge", UserRoleEnum.Specialist, -40),
                CreateUser("specialist5@test.com", "SpecialistHannah", UserRoleEnum.Specialist, -30),
                CreateUser("specialist6@test.com", "SpecialistIan", UserRoleEnum.Specialist, -25),
                CreateUser("specialist7@test.com", "SpecialistJack", UserRoleEnum.Specialist, -15),
                CreateUser("specialist8@test.com", "SpecialistKaren", UserRoleEnum.Specialist, -10),
                CreateUser("specialist9@test.com", "SpecialistLiam", UserRoleEnum.Specialist, -120),
                CreateUser("specialist10@test.com", "SpecialistMia", UserRoleEnum.Specialist, -95),
                CreateUser("specialist11@test.com", "SpecialistNoah", UserRoleEnum.Specialist, -70),
                CreateUser("specialist12@test.com", "SpecialistOlivia", UserRoleEnum.Specialist, -85),
                CreateUser("specialist13@test.com", "SpecialistPeter", UserRoleEnum.Specialist, -12),
                CreateUser("specialist14@test.com", "SpecialistQuinn", UserRoleEnum.Specialist, -200),
                CreateUser("specialist15@test.com", "SpecialistRiley", UserRoleEnum.Specialist, -35),
                CreateUser("specialist16@test.com", "SpecialistSophia", UserRoleEnum.Specialist, -55),

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

            // Liam - Distributed systems backend specialist
            var liam = specialists.FirstOrDefault(s => s.Username == "SpecialistLiam");
            if (liam != null)
            {
                var liamTags = tags.Where(t => t.Name is "C#" or ".NET" or "REST API" or "SQL" or "Cloud Architecture").ToList();
                userProfileTags.AddRange(liamTags.Select(t => new UserProfileTag { UserId = liam.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Mia - Data/AI specialist
            var mia = specialists.FirstOrDefault(s => s.Username == "SpecialistMia");
            if (mia != null)
            {
                var miaTags = tags.Where(t => t.Name is "Python" or "Machine Learning" or "Data Science" or "SQL" or "Azure").ToList();
                userProfileTags.AddRange(miaTags.Select(t => new UserProfileTag { UserId = mia.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Noah - Mobile product specialist
            var noah = specialists.FirstOrDefault(s => s.Username == "SpecialistNoah");
            if (noah != null)
            {
                var noahTags = tags.Where(t => t.Name is "Mobile Development" or "UI/UX Design" or "TypeScript" or "React" or "Web Development").ToList();
                userProfileTags.AddRange(noahTags.Select(t => new UserProfileTag { UserId = noah.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Olivia - Security and platform specialist
            var olivia = specialists.FirstOrDefault(s => s.Username == "SpecialistOlivia");
            if (olivia != null)
            {
                var oliviaTags = tags.Where(t => t.Name is "Cybersecurity" or "DevOps" or "Docker" or "Kubernetes" or "AWS").ToList();
                userProfileTags.AddRange(oliviaTags.Select(t => new UserProfileTag { UserId = olivia.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Peter - Junior frontend specialist
            var peter = specialists.FirstOrDefault(s => s.Username == "SpecialistPeter");
            if (peter != null)
            {
                var peterTags = tags.Where(t => t.Name is "JavaScript" or "TypeScript" or "React" or "Web Development").ToList();
                userProfileTags.AddRange(peterTags.Select(t => new UserProfileTag { UserId = peter.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Quinn - Enterprise architecture specialist
            var quinn = specialists.FirstOrDefault(s => s.Username == "SpecialistQuinn");
            if (quinn != null)
            {
                var quinnTags = tags.Where(t => t.Name is "Java" or "Spring Boot" or "Cloud Architecture" or "Database Design" or "DevOps").ToList();
                userProfileTags.AddRange(quinnTags.Select(t => new UserProfileTag { UserId = quinn.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Riley - API and data integration specialist
            var riley = specialists.FirstOrDefault(s => s.Username == "SpecialistRiley");
            if (riley != null)
            {
                var rileyTags = tags.Where(t => t.Name is "REST API" or "GraphQL" or "Node.js" or "SQL" or "Database Design").ToList();
                userProfileTags.AddRange(rileyTags.Select(t => new UserProfileTag { UserId = riley.Id, TagId = t.Id, TagName = t.Name }));
            }

            // Sophia - Blockchain and full stack specialist
            var sophia = specialists.FirstOrDefault(s => s.Username == "SpecialistSophia");
            if (sophia != null)
            {
                var sophiaTags = tags.Where(t => t.Name is "Blockchain" or "Cybersecurity" or "TypeScript" or "React" or "REST API").ToList();
                userProfileTags.AddRange(sophiaTags.Select(t => new UserProfileTag { UserId = sophia.Id, TagId = t.Id, TagName = t.Name }));
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
            var grace = clients.FirstOrDefault(c => c.Username == "ClientGrace");
            var henry = clients.FirstOrDefault(c => c.Username == "ClientHenry");
            var isla = clients.FirstOrDefault(c => c.Username == "ClientIsla");
            var jason = clients.FirstOrDefault(c => c.Username == "ClientJason");

            var dana = specialists.FirstOrDefault(s => s.Username == "SpecialistDana");
            var evan = specialists.FirstOrDefault(s => s.Username == "SpecialistEvan");
            var fiona = specialists.FirstOrDefault(s => s.Username == "SpecialistFiona");
            var hannah = specialists.FirstOrDefault(s => s.Username == "SpecialistHannah");
            var ian = specialists.FirstOrDefault(s => s.Username == "SpecialistIan");
            var jack = specialists.FirstOrDefault(s => s.Username == "SpecialistJack");
            var karen = specialists.FirstOrDefault(s => s.Username == "SpecialistKaren");
            var liam = specialists.FirstOrDefault(s => s.Username == "SpecialistLiam");
            var mia = specialists.FirstOrDefault(s => s.Username == "SpecialistMia");
            var noah = specialists.FirstOrDefault(s => s.Username == "SpecialistNoah");
            var olivia = specialists.FirstOrDefault(s => s.Username == "SpecialistOlivia");
            var peter = specialists.FirstOrDefault(s => s.Username == "SpecialistPeter");
            var quinn = specialists.FirstOrDefault(s => s.Username == "SpecialistQuinn");
            var riley = specialists.FirstOrDefault(s => s.Username == "SpecialistRiley");
            var sophia = specialists.FirstOrDefault(s => s.Username == "SpecialistSophia");

            var projects = new List<Project>
            {
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
                new Project
                {
                    Name = "Real-Time Alert Center",
                    Description = "Implement a real-time incident alert center with triage workflows and audit-ready event trails.",
                    Deadline = DateTime.UtcNow.AddDays(20),
                    OwnerId = grace!.Id,
                    WorkerId = olivia!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                },
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
                new Project
                {
                    Name = "B2B Integration Hub",
                    Description = "Deliver a robust integration hub for partners with webhook orchestration and SLA monitoring.",
                    Deadline = DateTime.UtcNow.AddDays(-6),
                    OwnerId = henry!.Id,
                    WorkerId = liam!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-55)
                },
                new Project
                {
                    Name = "ETL Quality Stabilization",
                    Description = "Stabilize ETL pipelines with validation checks, lineage visibility, and failure recovery improvements.",
                    Deadline = DateTime.UtcNow.AddDays(-4),
                    OwnerId = isla!.Id,
                    WorkerId = mia!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-62)
                },
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
                },
                new Project
                {
                    Name = "Release Governance Automation",
                    Description = "Automate release governance policies and exception handling for enterprise change management.",
                    Deadline = DateTime.UtcNow.AddDays(95),
                    OwnerId = jason!.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.OnHold,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Project
                {
                    Name = "Event-Driven Order Processing",
                    Description = "Design a high-throughput event-driven backend for processing orders and notifications in near real-time.",
                    Deadline = DateTime.UtcNow.AddDays(70),
                    OwnerId = alice.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Project
                {
                    Name = "API Gateway Hardening",
                    Description = "Implement API gateway policies, rate limiting, and observability for a microservices ecosystem.",
                    Deadline = DateTime.UtcNow.AddDays(50),
                    OwnerId = charlie.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Project
                {
                    Name = "Fraud Detection Pipeline",
                    Description = "Build an ML-driven fraud detection pipeline with feature engineering and model monitoring.",
                    Deadline = DateTime.UtcNow.AddDays(80),
                    OwnerId = bob.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Project
                {
                    Name = "Demand Forecasting Engine",
                    Description = "Create predictive models for sales demand forecasting and integrate them into reporting workflows.",
                    Deadline = DateTime.UtcNow.AddDays(95),
                    OwnerId = emma.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Project
                {
                    Name = "Cross-Platform Wellness App",
                    Description = "Develop a modern cross-platform wellness app with clean UI and engagement tracking.",
                    Deadline = DateTime.UtcNow.AddDays(65),
                    OwnerId = frank.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Project
                {
                    Name = "Mobile UX Modernization",
                    Description = "Redesign and modernize the mobile product experience to improve retention and accessibility.",
                    Deadline = DateTime.UtcNow.AddDays(55),
                    OwnerId = david.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Project
                {
                    Name = "SOC Automation Platform",
                    Description = "Build security automation workflows for incident triage and response orchestration.",
                    Deadline = DateTime.UtcNow.AddDays(75),
                    OwnerId = bob.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Project
                {
                    Name = "Container Security Baseline",
                    Description = "Define and implement secure container baselines, image scanning, and runtime controls.",
                    Deadline = DateTime.UtcNow.AddDays(45),
                    OwnerId = alice.Id,
                    ProjectStatusTypeId = (int)ProjectStatusTypeEnum.Hiring,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
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
            AttachTagsToProject("Real-Time Alert Center", "Cybersecurity", "DevOps", "Cloud Architecture", "REST API");
            AttachTagsToProject("Landing Page Development", "JavaScript", "UI/UX Design", "Web Development");
            AttachTagsToProject("Database Migration", "SQL", "Azure", "Database Design");
            AttachTagsToProject("E-learning Portal", "Node.js", "Vue.js", "Web Development");
            AttachTagsToProject("Inventory Management System", "Java", "Spring Boot", "Database Design");
            AttachTagsToProject("Corporate Website", "React", "Web Development", "UI/UX Design");
            AttachTagsToProject("Machine Learning Model", "Python", "Machine Learning", "Data Science");
            AttachTagsToProject("AI Chatbot Integration", "Python", "REST API", "Machine Learning");
            AttachTagsToProject("Event-Driven Order Processing", "C#", ".NET", "REST API", "Cloud Architecture", "SQL");
            AttachTagsToProject("API Gateway Hardening", "C#", ".NET", "DevOps", "REST API", "Cybersecurity");
            AttachTagsToProject("Fraud Detection Pipeline", "Python", "Machine Learning", "Data Science", "SQL");
            AttachTagsToProject("Demand Forecasting Engine", "Python", "Machine Learning", "Data Science", "Azure");
            AttachTagsToProject("Cross-Platform Wellness App", "Mobile Development", "UI/UX Design", "React", "TypeScript");
            AttachTagsToProject("Mobile UX Modernization", "Mobile Development", "UI/UX Design", "Web Development");
            AttachTagsToProject("SOC Automation Platform", "Cybersecurity", "DevOps", "AWS", "Cloud Architecture");
            AttachTagsToProject("Container Security Baseline", "Cybersecurity", "Docker", "Kubernetes", "DevOps");
            AttachTagsToProject("Compliance Knowledge Base", "C#", ".NET", "Database Design", "Web Development");
            AttachTagsToProject("Real-Time Alert Center", "Cybersecurity", "DevOps", "Cloud Architecture", "REST API");
            AttachTagsToProject("B2B Integration Hub", "C#", ".NET", "REST API", "Cloud Architecture");
            AttachTagsToProject("Subscription Analytics Portal", "Data Science", "SQL", "Web Development", "React");
            AttachTagsToProject("Patient Booking Experience", "Mobile Development", "UI/UX Design", "React", "TypeScript");
            AttachTagsToProject("Clinical Notes AI Assistant", "Python", "Machine Learning", "Data Science", "REST API");
            AttachTagsToProject("ETL Quality Stabilization", "Python", "SQL", "Data Science", "Azure");
            AttachTagsToProject("Vendor Risk Scoring", "Cybersecurity", "Data Science", "Python", "Cloud Architecture");
            AttachTagsToProject("Supplier Portal Revamp", "Web Development", "React", "TypeScript", "REST API");
            AttachTagsToProject("Release Governance Automation", "DevOps", "Cloud Architecture", "Cybersecurity", "Docker");

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
            var grace = clients.First(c => c.Username == "ClientGrace");
            var henry = clients.First(c => c.Username == "ClientHenry");
            var isla = clients.First(c => c.Username == "ClientIsla");
            var jason = clients.First(c => c.Username == "ClientJason");

            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var evan = specialists.First(s => s.Username == "SpecialistEvan");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");
            var george = specialists.First(s => s.Username == "SpecialistGeorge");
            var hannah = specialists.First(s => s.Username == "SpecialistHannah");
            var ian = specialists.First(s => s.Username == "SpecialistIan");
            var jack = specialists.First(s => s.Username == "SpecialistJack");
            var karen = specialists.First(s => s.Username == "SpecialistKaren");
            var liam = specialists.First(s => s.Username == "SpecialistLiam");
            var mia = specialists.First(s => s.Username == "SpecialistMia");
            var noah = specialists.First(s => s.Username == "SpecialistNoah");
            var olivia = specialists.First(s => s.Username == "SpecialistOlivia");
            var peter = specialists.First(s => s.Username == "SpecialistPeter");
            var quinn = specialists.First(s => s.Username == "SpecialistQuinn");
            var riley = specialists.First(s => s.Username == "SpecialistRiley");
            var sophia = specialists.First(s => s.Username == "SpecialistSophia");

            var ecommerceProject = projects.First(p => p.Name == "E-commerce Platform Backend");
            var dashboardProject = projects.First(p => p.Name == "Mobile App Dashboard");
            var cicdProject = projects.First(p => p.Name == "CI/CD Pipeline Setup");
            var blockchainProject = projects.First(p => p.Name == "Blockchain Smart Contracts");
            var phpProject = projects.First(p => p.Name == "Legacy PHP Migration");

            var eventDrivenProject = projects.First(p => p.Name == "Event-Driven Order Processing");
            var gatewayHardeningProject = projects.First(p => p.Name == "API Gateway Hardening");
            var fraudPipelineProject = projects.First(p => p.Name == "Fraud Detection Pipeline");
            var forecastingProject = projects.First(p => p.Name == "Demand Forecasting Engine");
            var wellnessProject = projects.First(p => p.Name == "Cross-Platform Wellness App");
            var mobileUxProject = projects.First(p => p.Name == "Mobile UX Modernization");
            var socAutomationProject = projects.First(p => p.Name == "SOC Automation Platform");
            var containerSecurityProject = projects.First(p => p.Name == "Container Security Baseline");

            var complianceProject = projects.First(p => p.Name == "Compliance Knowledge Base");
            var b2bIntegrationProject = projects.First(p => p.Name == "B2B Integration Hub");
            var patientBookingProject = projects.First(p => p.Name == "Patient Booking Experience");
            var clinicalNotesProject = projects.First(p => p.Name == "Clinical Notes AI Assistant");
            var etlStabilizationProject = projects.First(p => p.Name == "ETL Quality Stabilization");
            var vendorRiskProject = projects.First(p => p.Name == "Vendor Risk Scoring");
            var supplierPortalProject = projects.First(p => p.Name == "Supplier Portal Revamp");

            var requests = new List<Request>
            {
                new Request { Message = "I have extensive experience with .NET and REST APIs. I'd love to work on this e-commerce platform!", SenderId = dana.Id, ReceiverId = alice.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Request { Message = "Full stack developer here. I can handle both the API and any frontend needs for this project.", SenderId = fiona.Id, ReceiverId = alice.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Request { Message = "React and TypeScript are my specialties. I can create beautiful, responsive dashboards.", SenderId = evan.Id, ReceiverId = alice.Id, ProjectId = dashboardProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-12) },
                new Request { Message = "DevOps is my passion. I've set up pipelines for multiple enterprise clients using Azure DevOps.", SenderId = george.Id, ReceiverId = bob.Id, ProjectId = cicdProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new Request { Message = "I have a strong background in smart contract development and security best practices.", SenderId = ian.Id, ReceiverId = david.Id, ProjectId = blockchainProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-24) },
                new Request { Message = "I've successfully migrated several PHP monoliths to modern frameworks. Let's talk.", SenderId = hannah.Id, ReceiverId = emma.Id, ProjectId = phpProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Request { Message = "Hi Dana, I saw your profile and think you'd be perfect for our e-commerce project. Would you like to join?", SenderId = alice.Id, ReceiverId = dana.Id, ProjectId = ecommerceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Request { Message = "We need someone with strong Python background for our ML project, would you be available later this year?", SenderId = bob.Id, ReceiverId = jack.Id, ProjectId = projects.First(p => p.Name == "Machine Learning Model").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = null, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Request { Message = "I'm interested in the CI/CD pipeline project.", SenderId = evan.Id, ReceiverId = bob.Id, ProjectId = cicdProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = false, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Request { Message = "I can do this mobile app refactoring quickly.", SenderId = dana.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Mobile App Refactoring").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = false, CreatedAt = DateTime.UtcNow.AddDays(-22) },
                new Request { Message = "I'd like to work on the customer portal redesign.", SenderId = fiona.Id, ReceiverId = bob.Id, ProjectId = projects.First(p => p.Name == "Customer Portal Redesign").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-16) },
                new Request { Message = "I specialize in AWS architecture and security auditing.", SenderId = jack.Id, ReceiverId = david.Id, ProjectId = projects.First(p => p.Name == "Cloud Security Audit").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-26) },
                new Request { Message = "Happy to help you with the mobile app refactoring!", SenderId = karen.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Mobile App Refactoring").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-21) },
                new Request { Message = "I've reviewed your requirements and can deliver this portal.", SenderId = ian.Id, ReceiverId = emma.Id, ProjectId = projects.First(p => p.Name == "E-learning Portal").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-85) },
                new Request { Message = "I have the Java Spring Boot experience required for this inventory system.", SenderId = hannah.Id, ReceiverId = frank.Id, ProjectId = projects.First(p => p.Name == "Inventory Management System").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-95) },
                new Request { Message = "Hi Evan, loved your portfolio. Can you redesign our corporate site?", SenderId = alice.Id, ReceiverId = evan.Id, ProjectId = projects.First(p => p.Name == "Corporate Website").Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-75) },
                new Request { Message = "I can architect and implement this event-driven backend with robust API boundaries.", SenderId = liam.Id, ReceiverId = alice.Id, ProjectId = eventDrivenProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-9) },
                new Request { Message = "I have delivered API gateway hardening projects focused on resiliency and security.", SenderId = liam.Id, ReceiverId = charlie.Id, ProjectId = gatewayHardeningProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-7) },
                new Request { Message = "I can design and productionize the fraud detection pipeline end-to-end.", SenderId = mia.Id, ReceiverId = bob.Id, ProjectId = fraudPipelineProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-10) },
                new Request { Message = "Forecasting models and data quality checks are my strengths; I'd love to contribute.", SenderId = mia.Id, ReceiverId = emma.Id, ProjectId = forecastingProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-8) },
                new Request { Message = "I specialize in high-quality mobile UX and cross-platform delivery.", SenderId = noah.Id, ReceiverId = frank.Id, ProjectId = wellnessProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-11) },
                new Request { Message = "I can modernize your mobile UX while keeping performance and accessibility central.", SenderId = noah.Id, ReceiverId = david.Id, ProjectId = mobileUxProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new Request { Message = "I can build practical SOC automation workflows and improve incident response speed.", SenderId = olivia.Id, ReceiverId = bob.Id, ProjectId = socAutomationProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Request { Message = "I have deep container security experience and can define enforceable baselines.", SenderId = olivia.Id, ReceiverId = alice.Id, ProjectId = containerSecurityProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-4) },
                new Request { Message = "I can craft a clean knowledge base UX and implement performant search behavior.", SenderId = peter.Id, ReceiverId = grace.Id, ProjectId = complianceProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-14) },
                new Request { Message = "I can support patient booking flows with polished frontend delivery and accessibility in mind.", SenderId = sophia.Id, ReceiverId = isla.Id, ProjectId = patientBookingProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-9) },
                new Request { Message = "I can build the API layer and integration model for this supplier portal revamp.", SenderId = riley.Id, ReceiverId = jason.Id, ProjectId = supplierPortalProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new Request { Message = "My background in cloud architecture and risk controls aligns well with this vendor scoring initiative.", SenderId = quinn.Id, ReceiverId = jason.Id, ProjectId = vendorRiskProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = null, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Request { Message = "I can deliver the partner integration hub with reliable webhook orchestration and monitoring.", SenderId = liam.Id, ReceiverId = henry.Id, ProjectId = b2bIntegrationProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-56) },
                new Request { Message = "I can stabilize your ETL quality process and ship measurable reliability improvements.", SenderId = mia.Id, ReceiverId = isla.Id, ProjectId = etlStabilizationProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Application, Accepted = true, CreatedAt = DateTime.UtcNow.AddDays(-63) },
                new Request { Message = "I can prototype and integrate the clinical notes assistant while respecting accuracy and review workflows.", SenderId = mia.Id, ReceiverId = isla.Id, ProjectId = clinicalNotesProject.Id, RequestTypeId = (int)ProjectRequestTypeEnum.Invitation, Accepted = false, CreatedAt = DateTime.UtcNow.AddDays(-2) }
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
            var grace = clients.First(c => c.Username == "ClientGrace");
            var henry = clients.First(c => c.Username == "ClientHenry");
            var isla = clients.First(c => c.Username == "ClientIsla");

            var dana = specialists.First(s => s.Username == "SpecialistDana");
            var evan = specialists.First(s => s.Username == "SpecialistEvan");
            var fiona = specialists.First(s => s.Username == "SpecialistFiona");
            var hannah = specialists.First(s => s.Username == "SpecialistHannah");
            var ian = specialists.First(s => s.Username == "SpecialistIan");
            var liam = specialists.First(s => s.Username == "SpecialistLiam");
            var mia = specialists.First(s => s.Username == "SpecialistMia");
            var olivia = specialists.First(s => s.Username == "SpecialistOlivia");

            var reviews = new List<Review>
            {
                new Review { Title = "Excellent work on landing page", Comment = "Fiona delivered the landing page ahead of schedule with great attention to detail. Very responsive to feedback and made all requested changes quickly.", Rating = 5, ReviewerId = alice.Id, RevieweeId = fiona.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Review { Title = "Great client to work with", Comment = "Alice provided clear requirements and timely feedback throughout the project. Would work with again!", Rating = 5, ReviewerId = fiona.Id, RevieweeId = alice.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Review { Title = "Professional database migration", Comment = "Dana handled our complex database migration flawlessly. Zero data loss and minimal downtime. Highly recommended for database work.", Rating = 5, ReviewerId = charlie.Id, RevieweeId = dana.Id, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                new Review { Title = "Good collaboration", Comment = "Charlie was helpful in providing access to legacy systems and answering questions about the data structure.", Rating = 4, ReviewerId = dana.Id, RevieweeId = charlie.Id, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                new Review { Title = "Solid work overall", Comment = "Good technical skills but communication could have been better. Final result was what we needed.", Rating = 4, ReviewerId = charlie.Id, RevieweeId = dana.Id, CreatedAt = DateTime.UtcNow.AddDays(-60) },
                new Review { Title = "Fantastic E-learning platform", Comment = "Ian built an incredible product. His knowledge of Node.js and modern web frameworks is impressive.", Rating = 5, ReviewerId = emma.Id, RevieweeId = ian.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Review { Title = "A pleasure to work for", Comment = "Emma is a great communicator and pays on time. The project scope was very clear.", Rating = 5, ReviewerId = ian.Id, RevieweeId = emma.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Review { Title = "Robust Inventory System", Comment = "Hannah delivered a very solid Java backend. We had some minor bugs in the testing phase but she fixed them very fast.", Rating = 4, ReviewerId = frank.Id, RevieweeId = hannah.Id, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new Review { Title = "Responsive and talented", Comment = "Evan gave our corporate website the exact facelift it needed. Highly recommend for any front-end UI work.", Rating = 5, ReviewerId = alice.Id, RevieweeId = evan.Id, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Review { Title = "Excellent backend architecture guidance", Comment = "Liam quickly identified scaling bottlenecks and proposed a clean, reliable API architecture.", Rating = 5, ReviewerId = henry.Id, RevieweeId = liam.Id, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new Review { Title = "Focused and fair stakeholder", Comment = "Henry was clear about priorities and quick with decisions, which kept delivery smooth.", Rating = 5, ReviewerId = liam.Id, RevieweeId = henry.Id, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new Review { Title = "Strong data reliability delivery", Comment = "Mia brought order to our ETL pipeline and significantly reduced data quality incidents.", Rating = 5, ReviewerId = isla.Id, RevieweeId = mia.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Review { Title = "Clear requirements and fast feedback", Comment = "Isla gave precise requirements and quick validation throughout the engagement.", Rating = 5, ReviewerId = mia.Id, RevieweeId = isla.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Review { Title = "Great security mindset", Comment = "Olivia was proactive about threat modeling and gave us actionable hardening steps.", Rating = 4, ReviewerId = grace.Id, RevieweeId = olivia.Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Review { Title = "Collaborative and decisive client", Comment = "Grace communicates clearly and keeps scope aligned with project goals.", Rating = 5, ReviewerId = olivia.Id, RevieweeId = grace.Id, CreatedAt = DateTime.UtcNow.AddDays(-2) }
            };

            database.Reviews.AddRange(reviews);
            await database.SaveChangesAsync();
        }
    }
}