using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Db;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Kanban> Kanbans { get; set; }
    public DbSet<KanbanColumn> KanbanColumns { get; set; }
    public DbSet<KanbanTask> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<TaskEvent> TaskEvents { get; set; }
    public DbSet<AutomationRule> AutomationRules { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.ApplyConfiguration(new TeamMemberConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanColumnConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanTaskConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TaskEventConfiguration());
        modelBuilder.ApplyConfiguration(new AutomationRuleConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(50);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Team)
            .HasForeignKey(x => x.TeamId);
    }
}

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(x => new { x.TeamId, x.UserId });
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TeamId);
        builder.HasOne(x => x.User)
            .WithMany(x => x.TeamMemberships)
            .HasForeignKey(x => x.UserId);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasOne(x => x.Team);
    }
}

public class KanbanConfiguration : IEntityTypeConfiguration<Kanban>
{
    public void Configure(EntityTypeBuilder<Kanban> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasOne(x => x.Project);
    }
}

public class KanbanColumnConfiguration : IEntityTypeConfiguration<KanbanColumn>
{
    public void Configure(EntityTypeBuilder<KanbanColumn> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Kanban);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Order)
            .IsRequired();
    }
}

public class KanbanTaskConfiguration : IEntityTypeConfiguration<KanbanTask>
{
    public void Configure(EntityTypeBuilder<KanbanTask> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Description)
            .HasMaxLength(200);
        builder.Property(x => x.Priority)
            .IsRequired();
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.Deadline);
        builder.Property(x => x.Order);

        builder.HasOne(x => x.Kanban)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.KanbanId);

        builder.HasOne(x => x.Column)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ColumnId);

        builder.HasMany(x => x.TaskTags)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}

public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.HasKey(x => new { x.TaskId, x.TagId });

        builder.HasOne(x => x.Task)
            .WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TaskId);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TagId);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Message)
            .HasMaxLength(200);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.KanbanTask)
            .WithMany()
            .HasForeignKey(x => x.TaskId);

        builder.HasOne(x => x.Kanban)
            .WithMany()
            .HasForeignKey(x => x.KanbanId);
    }
}

public class TaskEventConfiguration : IEntityTypeConfiguration<TaskEvent>
{
    public void Configure(EntityTypeBuilder<TaskEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        builder.Property(x => x.EventType)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(x => x.OldOrder);
        builder.Property(x => x.NewOrder);

        builder.HasOne(x => x.Task)
            .WithMany()
            .HasForeignKey(x => x.TaskId);

        builder.HasOne(x => x.Kanban)
            .WithMany()
            .HasForeignKey(x => x.KanbanId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.FromColumn)
            .WithMany()
            .HasForeignKey(x => x.FromColumnId);

        builder.HasOne(x => x.ToColumn)
            .WithMany()
            .HasForeignKey(x => x.ToColumnId);
    }
}

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TriggerType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Condition)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasOne(x => x.Kanban)
            .WithMany()
            .HasForeignKey(x => x.KanbanId);
    }
}