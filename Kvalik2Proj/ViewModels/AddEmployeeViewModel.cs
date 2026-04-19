using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;

namespace Kvalik2Proj.ViewModels;

public partial class AddEmployeeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string email = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string selectedRoleName = "Мастер";

    [ObservableProperty]
    private string[] roleOptions = new[] { "Мастер", "Модератор" };

    [ObservableProperty]
    private string errorMessage = "";

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        using var db = new AppDbContext();

        if (db.Users.Any(u => u.Email == Email))
        {
            ErrorMessage = "Пользователь с таким email уже существует";
            return;
        }

        var role = db.Roles.FirstOrDefault(r => r.Name == SelectedRoleName);
        if (role == null)
        {
            ErrorMessage = "Роль не найдена";
            return;
        }

        var user = new User
        {
            Name = Name,
            Email = Email,
            Password = Password,
            RoleId = role.Id,
            Balance = 0,
            CreatedAt = DateTime.Now
        };

        db.Users.Add(user);
        db.SaveChanges();

        NavigationService.Instance.NavigateTo(new UsersListViewModel());
    }

    [RelayCommand]
    private void Cancel()
    {
        NavigationService.Instance.NavigateTo(new UsersListViewModel());
    }
}
