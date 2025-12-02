namespace ITrade.Services.Responses
{
    public record ProjectTagResponse
    (
        int Id,
        int TagId,
        string TagName,
        int ProjectId
    );
}
