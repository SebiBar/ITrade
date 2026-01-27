namespace ITrade.Services.Interfaces
{
    public interface ISearchingService
    {
        Task<ICollection<T>> SearchAsync<T>(string query);
    }
}
