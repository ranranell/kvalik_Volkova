using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using System;
using System.Linq;

namespace Kvalik2Proj.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [RelayCommand]
    private void Register()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        using var db = new AppDbContext();

        if (db.Users.Any(u => u.Email == Email))
        {
            ErrorMessage = "Пользователь с таким email уже существует";
            return;
        }

        var userRole = db.Roles.FirstOrDefault(r => r.Name == "Пользователь");

        var user = new User
        {
            Name = Name,
            Email = Email,
            Password = Password,
            Balance = 0,
            RoleId = userRole?.Id,
            CreatedAt = DateTime.Now
        };

        db.Users.Add(user);
        db.SaveChanges();

        NavigationService.Instance.NavigateTo(new LoginViewModel());
    }

    [RelayCommand]
    private void GoToLogin()
    {
        NavigationService.Instance.NavigateTo(new LoginViewModel());
    }
}
