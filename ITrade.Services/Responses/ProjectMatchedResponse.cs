namespace ITrade.Services.Responses
{
    public record ProjectMatchedResponse
    (
        ProjectResponse Project,
        double MatchPercentage
    );
}
