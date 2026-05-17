using Infrastructure.Constants;
using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Security.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DatabaseInitializerExtensions
{
    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (context.Database.GetMigrations().Any())
            context.Database.Migrate();
        else
            context.Database.EnsureCreated();

        EnsureOutboxSchema(context);
        EnsureNotificationSchema(context);

        var userId = SeedDevUser(context, hasher);
        SeedDevWorkspace(context, userId);

        context.SaveChanges();

        return app;
    }

    private static void EnsureOutboxSchema(DatabaseContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "OutboxEvents" (
                "Id" uuid NOT NULL,
                "EventType" text NOT NULL,
                "Payload" jsonb NOT NULL,
                "Status" character varying(50) NOT NULL,
                "RetryCount" integer NOT NULL,
                "LastError" text NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "PublishedAt" timestamp with time zone NULL,
                CONSTRAINT "PK_OutboxEvents" PRIMARY KEY ("Id")
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_OutboxEvents_Status_CreatedAt"
            ON "OutboxEvents" ("Status", "CreatedAt");
            """);
    }

    private static void EnsureNotificationSchema(DatabaseContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Notifications" (
                "Id" uuid NOT NULL,
                "Name" character varying(100) NOT NULL,
                "UserId" uuid NOT NULL,
                "TaskId" uuid NOT NULL,
                "KanbanId" uuid NOT NULL,
                "Message" character varying(200) NOT NULL,
                "IsRead" boolean NOT NULL DEFAULT false,
                "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                "ReadAt" timestamp with time zone NULL,
                CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Notifications_Kanbans_KanbanId" FOREIGN KEY ("KanbanId") REFERENCES "Kanbans" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Notifications_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
            );
            """);

        context.Database.ExecuteSqlRaw("""
            ALTER TABLE "Notifications"
            ADD COLUMN IF NOT EXISTS "IsRead" boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
            ADD COLUMN IF NOT EXISTS "ReadAt" timestamp with time zone NULL;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId"
            ON "Notifications" ("UserId");

            CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId_IsRead"
            ON "Notifications" ("UserId", "IsRead");

            CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId_CreatedAt"
            ON "Notifications" ("UserId", "CreatedAt");

            CREATE INDEX IF NOT EXISTS "IX_Notifications_TaskId"
            ON "Notifications" ("TaskId");

            CREATE INDEX IF NOT EXISTS "IX_Notifications_KanbanId"
            ON "Notifications" ("KanbanId");
            """);
    }

    private static Guid SeedDevUser(DatabaseContext context, IPasswordHasher hasher)
    {
        var user = context.Users.FirstOrDefault(x =>
            x.Id == DevSeedDefaults.UserId ||
            x.Email == DevSeedDefaults.UserEmail);

        if (user is null)
        {
            context.Users.Add(new User
            {
                Id = DevSeedDefaults.UserId,
                Email = DevSeedDefaults.UserEmail,
                Name = DevSeedDefaults.UserName,
                HashPassword = hasher.Hash(DevSeedDefaults.UserPassword)
            });

            return DevSeedDefaults.UserId;
        }

        user.Email = DevSeedDefaults.UserEmail;
        user.Name = DevSeedDefaults.UserName;
        user.HashPassword = hasher.Hash(DevSeedDefaults.UserPassword);

        return user.Id;
    }

    private static void SeedDevWorkspace(DatabaseContext context, Guid userId)
    {
        if (!context.Teams.Any(x => x.Id == DevSeedDefaults.TeamId))
        {
            context.Teams.Add(new Team
            {
                Id = DevSeedDefaults.TeamId,
                Name = DevSeedDefaults.TeamName
            });
        }

        if (!context.TeamMembers.Any(x =>
                x.TeamId == DevSeedDefaults.TeamId &&
                x.UserId == userId))
        {
            context.TeamMembers.Add(new TeamMember
            {
                TeamId = DevSeedDefaults.TeamId,
                UserId = userId,
                Role = TeamRole.Admin
            });
        }

        if (!context.Projects.Any(x => x.Id == DevSeedDefaults.ProjectId))
        {
            context.Projects.Add(new Project
            {
                Id = DevSeedDefaults.ProjectId,
                TeamId = DevSeedDefaults.TeamId,
                Name = DevSeedDefaults.ProjectName
            });
        }

        if (context.Kanbans.Any(x => x.Id == DevSeedDefaults.KanbanId))
            return;

        context.Kanbans.Add(new Kanban
        {
            Id = DevSeedDefaults.KanbanId,
            ProjectId = DevSeedDefaults.ProjectId,
            Name = DevSeedDefaults.KanbanName,
            Columns = KanbanDefaults.BasicColumns
                .Select(column => new KanbanColumn
                {
                    Id = Guid.NewGuid(),
                    KanbanId = DevSeedDefaults.KanbanId,
                    Name = column.Name,
                    Order = column.Order
                })
                .ToList()
        });
    }
}
