using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("reviews")]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSentReviews()
        {
            return Ok(await reviewService.GetSentReviewsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateRequest createReviewRequest)
        {
            var reviewId = await reviewService.CreateReviewAsync(createReviewRequest);
            return Ok(reviewId);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewUpdateRequest updateReviewRequest)
        {
            await reviewService.UpdateReviewAsync(updateReviewRequest);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReview([FromQuery] int reviewId)
        {
            await reviewService.DeleteReviewAsync(reviewId);
            return Ok();
        }
    }
}
