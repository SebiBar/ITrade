namespace ITrade.Services.Responses
{
    public record CurrentUserResponse
    (
        UserResponse User,
        string Email,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool IsEmailConfirmed,
        ICollection<UserProfileLinkResponse> ProfileLinks,
        ICollection<UserProfileTagResponse> ProfileTags,
        ICollection<ProjectResponse> Projects,
        ICollection<ReviewResponse> Reviews
    );
}
