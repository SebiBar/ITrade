namespace ITrade.Services.Responses
{
    public record UserMatchedResponse
    (
        UserResponse User,
        double MatchPercentage
    );
}
