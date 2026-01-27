using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IMatchingService
    {
        Task<ICollection<ProjectMatchedResponse>> RecommandSpecialistProjects();
        Task<ICollection<UserMatchedResponse>> RecommandSpecialistsForProject(int projectId);
    }
}
