namespace ITrade.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string title, string textBody, string htmlBody);
    }
}
