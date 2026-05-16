using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace WebApplication1.Application.Features.Kanban.CreateKanban;

public class CreateKanbanHandler(
    IProjectRepository projectRepository,
    IKanbanRepository kanbanRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateKanbanCommand, CreateKanbanResponse>
{
    public Task<CreateKanbanResponse> Handle(CreateKanbanCommand request, CancellationToken cancellationToken)
    {
        // TODO: сделать, когда будут сущности таблиц

        // var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken);
        //
        // if (project is null)
        //     throw new Exception("Project not found");

        // var kanban = new Kanban
        // {
        //     
        // }

        // var columns = new List<KanbanColumn>
        // {
        //     
        // }

        // kanban.columns = columns;

        // dbContext.Kanbans.Add(kanban);

        // TODO: гпт сказал, что не круто для каждого репозитория сохранять, а проще сделать все, а потом одной штукой отправить SQL запрос уже (надо думать)
        // await unitOfWork.SaveChangesAsync(cancellationToken);

        // return new CreateKanbanResponse
        // {
        //     Id = kanban.Id,
        //     ProjectId = kanban.ProjectId,
        //     Name = kanban.Name,
        //     Columns = columns
        //         .OrderBy(x => x.Order)
        //         .Select(x => new KanbanColumnResponse
        //         {
        //             Id = x.Id,
        //             Name = x.Name,
        //             Order = x.Order
        //         })
        //         .ToList()
        // };
        return Task.FromResult(new CreateKanbanResponse());
    }
}