namespace ITrade.Services.Interfaces
{
    public interface ITagService
    {
        Task<int> AddProjectTagAsync(int projectId, int tagId);
        Task RemoveProjectTagAsync(int projectId, int tagId);
        Task<int> AddProfileTagAsync(int tagId);
        Task RemoveProfileTagAsync(int tagId);
    }
}
