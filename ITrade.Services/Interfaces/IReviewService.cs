using ITrade.Services.Requests;

namespace ITrade.Services.Interfaces
{
    public interface IReviewService
    {
        Task<int> CreateReviewAsync(ReviewCreateRequest createReviewRequest);
        Task UpdateReviewAsync(ReviewUpdateRequest updateReviewRequest);
        Task DeleteReviewAsync(int reviewId);
    }
}