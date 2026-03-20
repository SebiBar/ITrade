using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class ReviewService(
        Context context,
        ICurrentUserService currentUserService
    ) : IReviewService
    {
        public async Task<ICollection<ReviewResponse>> GetSentReviewsAsync()
        {
            return await context.Reviews
                .Where(r => r.ReviewerId == currentUserService.UserId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                (
                    r.Id,
                    r.ReviewerId,
                    r.Reviewer.Username,
                    r.RevieweeId,
                    r.Reviewee.Username,
                    r.Rating,
                    r.Title,
                    r.Comment,
                    r.CreatedAt
                )).ToListAsync();
        }

        public async Task<int> CreateReviewAsync(ReviewCreateRequest createReviewRequest)
        {
            ValidateReviewCreateRequest(createReviewRequest);

            var workedTogether = await context.Projects.AnyAsync(p => 
                (p.OwnerId == currentUserService.UserId && p.WorkerId == createReviewRequest.RevieweeId) ||
                (p.OwnerId == createReviewRequest.RevieweeId && p.WorkerId == currentUserService.UserId));

            if (!workedTogether)
            {
                throw new ArgumentException("You can only review users you have worked with.", nameof(createReviewRequest.RevieweeId));
            }

            var review = new Review
            {
                RevieweeId = createReviewRequest.RevieweeId,
                ReviewerId = currentUserService.UserId,
                Title = createReviewRequest.Title,
                Comment = createReviewRequest.Comment,
                Rating = createReviewRequest.Rating,
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            return review.Id;
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await context.Reviews
                .Where(r => r.Id == reviewId && r.ReviewerId == currentUserService.UserId)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Review not found.", nameof(reviewId));

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();
        }

        public async Task UpdateReviewAsync(ReviewUpdateRequest updateReviewRequest)
        {
            ValidateReviewUpdateRequest(updateReviewRequest);

            var review = await context.Reviews
                .Where(r => r.Id == updateReviewRequest.ReviewId && r.ReviewerId == currentUserService.UserId)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Review not found.", nameof(updateReviewRequest.ReviewId));

            if (updateReviewRequest.Title != null)
            {
                review.Title = updateReviewRequest.Title;
            }
            if (updateReviewRequest.Comment != null)
            {
                review.Comment = updateReviewRequest.Comment;
            }
            if (updateReviewRequest.Rating != null)
            {
                review.Rating = updateReviewRequest.Rating.Value;
            }

            await context.SaveChangesAsync();
        }

        private void ValidateReviewUpdateRequest(ReviewUpdateRequest request)
        {
            _ = request switch
            {
                { Rating: > 5 or < 1 }
                    => throw new ArgumentException("Invalid rating.", nameof(request.Rating)),

                { Title: not null } when string.IsNullOrWhiteSpace(request.Title)
                    => throw new ArgumentException("Title cannot be empty.", nameof(request.Title)),

                { Comment: not null } when string.IsNullOrWhiteSpace(request.Comment)
                    => throw new ArgumentException("Comment cannot be empty.", nameof(request.Comment)),

                { Title.Length: > 200 }
                    => throw new ArgumentException("Title is too long.", nameof(request.Title)),

                { Comment.Length: > 2000 }
                    => throw new ArgumentException("Comment is too long.", nameof(request.Comment)),

                _ => request
            };
        }

        private void ValidateReviewCreateRequest(ReviewCreateRequest createReviewRequest)
        {
            _ = createReviewRequest switch
            {
                { Rating: > 5 or < 1 }
                    => throw new ArgumentException("Invalid rating.", nameof(createReviewRequest.Rating)),
                { Title: null or { } } when string.IsNullOrWhiteSpace(createReviewRequest.Title)
                    => throw new ArgumentException("Title cannot be empty.", nameof(createReviewRequest.Title)),
                { Comment: not null } when string.IsNullOrWhiteSpace(createReviewRequest.Comment)
                    => throw new ArgumentException("Comment cannot be empty.", nameof(createReviewRequest.Comment)),
                { Title.Length: > 200 }
                    => throw new ArgumentException("Title is too long.", nameof(createReviewRequest.Title)),
                { Comment.Length: > 2000 }
                    => throw new ArgumentException("Comment is too long.", nameof(createReviewRequest.Comment)),
                _ => createReviewRequest
            };
        }
    }
}
