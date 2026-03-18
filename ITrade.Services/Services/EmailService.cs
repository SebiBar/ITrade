using ITrade.Common.Helpers;
using ITrade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ITrade.Services.Services
{
    public class EmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<MailJetSettings> mailJetSettings,
        ILogger<EmailService> logger
    ) : IEmailService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("Mailjet");
        private readonly string fromEmail = mailJetSettings.Value.SenderEmail;
        private readonly string fromEmailName = mailJetSettings.Value.SenderName;

        public async Task SendEmailAsync(string toEmail, string title, string textBody, string htmlBody)
        {
            ValidateEmailContent(toEmail, title, textBody, htmlBody);

            var payload = new
            {
                Messages = new[]
                {
                    new {
                        From = new { Email = fromEmail, Name = fromEmailName },
                        To = new[] { new { Email = toEmail, Name = GetNameFromEmail(toEmail) } },
                        Subject = title,
                        TextPart = textBody,
                        HTMLPart = htmlBody
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(mailJetSettings.Value.Endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Mailjet send failed with HTTP status {StatusCode}. Response: {ResponseBody}",
                    (int)response.StatusCode,
                    responseBody);

                throw new InvalidOperationException(
                    $"Failed to send email. Status: {(int)response.StatusCode} {response.StatusCode}. " +
                    $"Body: {responseBody}"
                );
            }

            ValidateMailjetMessageStatus(responseBody);
        }

        private static void ValidateMailjetMessageStatus(string responseBody)
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (!document.RootElement.TryGetProperty("Messages", out var messages)
                    || messages.ValueKind != JsonValueKind.Array
                    || messages.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("Mailjet returned an unexpected response format.");
                }

                foreach (var message in messages.EnumerateArray())
                {
                    if (!message.TryGetProperty("Status", out var statusElement))
                    {
                        throw new InvalidOperationException($"Mailjet response missing message status. Body: {responseBody}");
                    }

                    var status = statusElement.GetString();
                    if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Mailjet rejected email with status '{status}'. Body: {responseBody}");
                    }
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException($"Mailjet returned non-JSON response. Body: {responseBody}");
            }
        }

        private string GetNameFromEmail(string email) => email.Split('@')[0];

        private void ValidateEmailContent(string toEmail, string title, string textBody, string htmlBody)
        {
            _ = (toEmail, title, textBody, htmlBody) switch
            {
                var (email, _, _, _) when string.IsNullOrWhiteSpace(email)
                    => throw new ArgumentException("Recipient email cannot be empty or whitespace.", nameof(toEmail)),

                var (email, _, _, _) when !System.Net.Mail.MailAddress.TryCreate(email, out _)
                    => throw new ArgumentException("Invalid email format.", nameof(toEmail)),

                var (_, t, _, _) when string.IsNullOrWhiteSpace(t)
                    => throw new ArgumentException("Email title cannot be empty or whitespace.", nameof(title)),

                var (_, t, _, _) when t.Length > 1000
                    => throw new ArgumentException("Email title cannot exceed 1000 characters.", nameof(title)),

                var (_, _, tBody, hBody) when string.IsNullOrWhiteSpace(tBody) && string.IsNullOrWhiteSpace(hBody)
                    => throw new ArgumentException("Email must have either a text body or an HTML body."),

                var (_, _, tBody, _) when !string.IsNullOrWhiteSpace(tBody) && tBody.Length > 50_000
                    => throw new ArgumentException("Text body is too long (max 50k characters)."),

                var (_, _, _, hBody) when !string.IsNullOrWhiteSpace(hBody) && hBody.Length > 100_000
                    => throw new ArgumentException("HTML body is too long (max 100k characters)."),

                _ => (toEmail, title, textBody, htmlBody)
            };
        }
    }
}
