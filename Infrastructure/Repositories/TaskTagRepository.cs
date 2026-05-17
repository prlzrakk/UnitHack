using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskTagRepository(DatabaseContext context) : ITaskTagRepository
{
    public async Task<List<Tag>> GetTagsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return await context.TaskTags
            .Where(x => x.TaskId == taskId)
            .Select(x => x.Tag)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskTag> AttachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var existing = await context.TaskTags
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.TagId == tagId, cancellationToken);

        if (existing is not null)
            return existing;

        var taskTag = new TaskTag
        {
            TaskId = taskId,
            TagId = tagId
        };

        await context.TaskTags.AddAsync(taskTag, cancellationToken);
        return taskTag;
    }

    public async Task<bool> DetachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var taskTag = await context.TaskTags
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.TagId == tagId, cancellationToken);

        if (taskTag is null)
            return false;

        context.TaskTags.Remove(taskTag);
        return true;
    }

    public async Task ReplaceAsync(
        Guid taskId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var distinctTagIds = tagIds.Distinct().ToHashSet();
        var existingTaskTags = await context.TaskTags
            .Where(x => x.TaskId == taskId)
            .ToListAsync(cancellationToken);

        var taskTagsToRemove = existingTaskTags
            .Where(x => !distinctTagIds.Contains(x.TagId))
            .ToList();
        context.TaskTags.RemoveRange(taskTagsToRemove);

        var existingTagIds = existingTaskTags
            .Select(x => x.TagId)
            .ToHashSet();

        foreach (var tagId in distinctTagIds.Where(tagId => !existingTagIds.Contains(tagId)))
        {
            await context.TaskTags.AddAsync(new TaskTag
            {
                TaskId = taskId,
                TagId = tagId
            }, cancellationToken);
        }
    }
}
