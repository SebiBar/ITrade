using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IMatchingService
    {
        Task<ICollection<ProjectMatchedResponse>> RecommandProjectsForSpecialist();
        Task<ICollection<UserMatchedResponse>> RecommandSpecialistsForProject(int projectId);
        Task<IDictionary<int, ICollection<UserMatchedResponse>>> RecommandSpecialistsForProjects(ICollection<int> projectIds);
        Task<IDictionary<(int SpecialistId, int ProjectId), double>> ComputeMatchScoresAsync(
            ICollection<(int SpecialistId, int ProjectId)> pairs);
    }
}
