namespace ITrade.Services.Responses
{
    public record UserProfileResponse
    (
        int Id,
        string Username,
        string Email,
        string Role,
        ICollection<UserProfileLinkResponse> ProfileLinks,
        ICollection<UserProfileTagResponse> ProfileTags,
        ICollection<ProjectResponse> Projects,
        ICollection<ReviewResponse> Reviews
    );
}
