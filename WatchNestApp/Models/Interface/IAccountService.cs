namespace WatchNestApp.Models.Interface
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string? ErrorMessage, string? JwtCookie)> LoginAsync(string username, string password);
        Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(string username, string password, string email);
        Task<bool> LogoutAsync();
    }
}
