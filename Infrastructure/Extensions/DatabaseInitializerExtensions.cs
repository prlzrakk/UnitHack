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

        var userId = SeedDevUser(context, hasher);
        SeedDevWorkspace(context, userId);

        context.SaveChanges();

        return app;
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
