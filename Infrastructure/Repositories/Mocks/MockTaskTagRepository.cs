using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockTaskTagRepository(MockDataStore store) : ITaskTagRepository
{
    public Task<List<Tag>> GetTagsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var tags = store.TaskTags
            .Where(x => x.TaskId == taskId)
            .Select(x => x.Tag ?? store.Tags.First(tag => tag.Id == x.TagId))
            .OrderBy(x => x.Name)
            .ToList();

        return Task.FromResult(tags);
    }

    public Task<TaskTag> AttachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var existing = store.TaskTags
            .FirstOrDefault(x => x.TaskId == taskId && x.TagId == tagId);

        if (existing is not null)
            return Task.FromResult(existing);

        var task = store.Tasks.First(x => x.Id == taskId);
        var tag = store.Tags.First(x => x.Id == tagId);
        var taskTag = new TaskTag
        {
            TaskId = task.Id,
            Task = task,
            TagId = tag.Id,
            Tag = tag
        };

        store.TaskTags.Add(taskTag);
        task.TaskTags.Add(taskTag);
        tag.TaskTags.Add(taskTag);

        return Task.FromResult(taskTag);
    }

    public Task<bool> DetachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken)
    {
        var taskTag = store.TaskTags
            .FirstOrDefault(x => x.TaskId == taskId && x.TagId == tagId);

        if (taskTag is null)
            return Task.FromResult(false);

        store.TaskTags.Remove(taskTag);
        taskTag.Task?.TaskTags.Remove(taskTag);
        taskTag.Tag?.TaskTags.Remove(taskTag);

        return Task.FromResult(true);
    }

    public Task ReplaceAsync(
        Guid taskId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var distinctTagIds = tagIds.Distinct().ToHashSet();
        var currentTaskTags = store.TaskTags
            .Where(x => x.TaskId == taskId)
            .ToList();

        foreach (var taskTag in currentTaskTags.Where(x => !distinctTagIds.Contains(x.TagId)).ToList())
        {
            store.TaskTags.Remove(taskTag);
            taskTag.Task?.TaskTags.Remove(taskTag);
            taskTag.Tag?.TaskTags.Remove(taskTag);
        }

        var existingTagIds = store.TaskTags
            .Where(x => x.TaskId == taskId)
            .Select(x => x.TagId)
            .ToHashSet();

        foreach (var tagId in distinctTagIds.Where(tagId => !existingTagIds.Contains(tagId)))
        {
            var task = store.Tasks.First(x => x.Id == taskId);
            var tag = store.Tags.First(x => x.Id == tagId);
            var taskTag = new TaskTag
            {
                TaskId = task.Id,
                Task = task,
                TagId = tag.Id,
                Tag = tag
            };

            store.TaskTags.Add(taskTag);
            task.TaskTags.Add(taskTag);
            tag.TaskTags.Add(taskTag);
        }

        return Task.CompletedTask;
    }
}
