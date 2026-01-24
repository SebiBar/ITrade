namespace ITrade.Services.Requests
{
    public record ResolveForgotPasswordRequest
    (
        string EmailedToken,
        string NewPassword
    );
}
