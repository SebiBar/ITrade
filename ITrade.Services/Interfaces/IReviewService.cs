using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ICollection<ReviewResponse>> GetSentReviewsAsync();
        Task<int> CreateReviewAsync(ReviewCreateRequest createReviewRequest);
        Task UpdateReviewAsync(ReviewUpdateRequest updateReviewRequest);
        Task DeleteReviewAsync(int reviewId);
    }
}