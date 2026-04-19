using Kvalik2Proj.Data;

namespace Kvalik2Proj.Services;

public class SessionService
{
    private static SessionService? _instance;
    public static SessionService Instance => _instance ??= new SessionService();

    public User? CurrentUser { get; private set; }
    public string? RoleName => CurrentUser?.Role?.Name;

    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => RoleName == "Администратор";
    public bool IsModerator => RoleName == "Модератор";
    public bool IsMaster => RoleName == "Мастер";
    public bool IsUser => RoleName == "Пользователь";

    public void Login(User user)
    {
        CurrentUser = user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
