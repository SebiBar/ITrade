using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IMatchingService
    {
        Task<ICollection<ProjectMatchedResponse>> RecommandProjectsForSpecialist();
        Task<ICollection<UserMatchedResponse>> RecommandSpecialistsForProject(int projectId);
        Task<IDictionary<int, ICollection<UserMatchedResponse>>> RecommandSpecialistsForProjects(ICollection<int> projectIds);

        /// <summary>
        /// Computes match scores for multiple specialist-project pairs in a batch.
        /// Returns a dictionary keyed by (specialistId, projectId) tuples.
        /// </summary>
        Task<IDictionary<(int SpecialistId, int ProjectId), double>> ComputeMatchScoresAsync(
            ICollection<(int SpecialistId, int ProjectId)> pairs);
    }
}
