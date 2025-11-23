namespace ITrade.Common.Helpers
{
    public class TokenSettings
    {
        public required int TokenLength { get; set; }
        public required int RefreshExpiresInDays { get; set; }
        public required int VerifyEmailExpiresInDays { get; set; }
        public required int PasswordResetExpiresInHours { get; set; }

    }
}
