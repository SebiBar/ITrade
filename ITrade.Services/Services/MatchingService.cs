using ITrade.DB;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;

namespace ITrade.Services.Services
{
    public class MatchingService(
        Context context,
        ICurrentUserService currentUserService
    ) : IMatchingService
    {
        public Task<ICollection<ProjectMatchedResponse>> RecommandSpecialistProjects()
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<UserMatchedResponse>> RecommandSpecialistsForProject(int projectId)
        {
            throw new NotImplementedException();
        }
    }
}
