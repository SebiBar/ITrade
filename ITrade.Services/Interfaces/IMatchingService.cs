using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IMatchingService
    {
        Task<ICollection<MatchedProjectResponse>> RecommandSpecialistProjects();
        Task<ICollection<MatchedUserResponse>> RecommandSpecialistsForProject(int projectId);
    }
}
