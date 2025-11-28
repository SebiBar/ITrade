namespace ITrade.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string UserRole { get; }
    }
}
