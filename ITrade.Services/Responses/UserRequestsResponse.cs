namespace ITrade.Services.Responses
{
    public record UserRequestsResponse
    (
        ICollection<RequestResponse> Invitations,
        ICollection<RequestResponse> Applications
    );
}
