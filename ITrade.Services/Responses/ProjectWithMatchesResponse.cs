namespace ITrade.Services.Responses
{
    public record ProjectWithMatchesResponse
    (
        ProjectResponse Project,
        ICollection<UserMatchedResponse> RecommendedSpecialists
    );
}
