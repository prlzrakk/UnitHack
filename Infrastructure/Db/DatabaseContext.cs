using System.Diagnostics;
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
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.ApplyConfiguration(new TeamMemberConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanColumnConfiguration());
        modelBuilder.ApplyConfiguration(new KanbanTaskConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new TaskTagConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TaskEventConfiguration());
        modelBuilder.ApplyConfiguration(new AutomationRuleConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEventConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.HashPassword)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Team)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Projects)
            .WithOne(x => x.Team)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(x => new { x.TeamId, x.UserId });
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany(x => x.TeamMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.TeamId });
        builder.HasIndex(x => new { x.TeamId, x.Role });
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Kanbans)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TeamId, x.Name })
            .IsUnique();
    }
}

public class KanbanConfiguration : IEntityTypeConfiguration<Kanban>
{
    public void Configure(EntityTypeBuilder<Kanban> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasOne(x => x.Project)
            .WithMany(x => x.Kanbans)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Columns)
            .WithOne(x => x.Kanban)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Tasks)
            .WithOne(x => x.Kanban)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Tags)
            .WithOne(x => x.Kanban)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ProjectId, x.Name })
            .IsUnique();
    }
}

public class KanbanColumnConfiguration : IEntityTypeConfiguration<KanbanColumn>
{
    public void Configure(EntityTypeBuilder<KanbanColumn> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Kanban)
            .WithMany(x => x.Columns)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasIndex(x => new { x.KanbanId, x.Name })
            .IsUnique();
        builder.HasIndex(x => new { x.KanbanId, x.Order });
    }
}

public class KanbanTaskConfiguration : IEntityTypeConfiguration<KanbanTask>
{
    public void Configure(EntityTypeBuilder<KanbanTask> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Description)
            .HasMaxLength(200);
        builder.Property(x => x.Priority)
            .IsRequired();
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.Deadline);
        builder.Property(x => x.Order);

        builder.HasOne(x => x.Kanban)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Column)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TaskTags)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ColumnId, x.Order });
        builder.HasIndex(x => new { x.KanbanId, x.ColumnId, x.Order });
        builder.HasIndex(x => new { x.KanbanId, x.UserId });
        builder.HasIndex(x => new { x.KanbanId, x.Priority });
        builder.HasIndex(x => new { x.KanbanId, x.Deadline });
        builder.HasIndex(x => new { x.UserId, x.Deadline });
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.Kanban)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TaskTags)
            .WithOne(x => x.Tag)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.KanbanId, x.Name })
            .IsUnique();
    }
}

public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.HasKey(x => new { x.TaskId, x.TagId });

        builder.HasOne(x => x.Task)
            .WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.TaskTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TagId, x.TaskId });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Message)
            .HasMaxLength(200);
        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
        builder.Property(x => x.ReadAt);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.KanbanTask)
            .WithMany()
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Kanban)
            .WithMany()
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => x.TaskId);
        builder.HasIndex(x => x.KanbanId);
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
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Kanban)
            .WithMany()
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromColumn)
            .WithMany()
            .HasForeignKey(x => x.FromColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ToColumn)
            .WithMany()
            .HasForeignKey(x => x.ToColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TaskId, x.CreatedAt });
        builder.HasIndex(x => new { x.KanbanId, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

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
            .HasForeignKey(x => x.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.KanbanId, x.IsEnabled, x.TriggerType });
    }
}


public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.LastError);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.PublishedAt);
    }
}

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new {x.MessageId, x.Handler});
        builder.Property(x => x.Handler)
            .IsRequired();
        builder.Property(x => x.ProcessedAt)
            .IsRequired();
    }
}  