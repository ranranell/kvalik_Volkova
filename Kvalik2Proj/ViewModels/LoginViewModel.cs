using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kvalik2Proj.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        using var db = new AppDbContext();
        var user = db.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Email == Email);

        if (user == null || user.Password != Password)
        {
            ErrorMessage = "Неверный email или пароль";
            return;
        }

        SessionService.Instance.Login(user);
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }

    [RelayCommand]
    private void GoToRegister()
    {
        NavigationService.Instance.NavigateTo(new RegisterViewModel());
    }
}
