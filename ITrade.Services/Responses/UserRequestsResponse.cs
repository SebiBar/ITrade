namespace ITrade.Services.Responses
{
    public record UserRequestsResponse
    (
        ICollection<ProjectRequestResponse> Invitations,
        ICollection<ProjectRequestResponse> Applications
    );
}
