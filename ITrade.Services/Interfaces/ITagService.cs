using ITrade.DB.Entities;

namespace ITrade.Services.Interfaces
{
    public interface ITagService
    {
        Task<int> CreateTagAsync(string tagName);
        Task DeleteTagAsync(int tagId);
        Task<ICollection<Tag>> SearchTagsAsync(string tagName);
        Task<int> AddProjectTagAsync(int projectId, int tagId);
        Task RemoveProjectTagAsync(int projectId, int tagId);
        Task<int> AddProfileTagAsync(int tagId);
        Task RemoveProfileTagAsync(int tagId);
    }
}
