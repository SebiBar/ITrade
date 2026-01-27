using ITrade.DB;
using ITrade.Services.Interfaces;

namespace ITrade.Services.Services
{
    public class SearchingService(
        Context context,
        ICurrentUserService currentUserService
        ) : ISearchingService
    {
        public Task<ICollection<T>> SearchAsync<T>(string query)
        {
            throw new NotImplementedException();
        }
    }
}
