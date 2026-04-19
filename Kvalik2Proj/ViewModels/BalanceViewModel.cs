using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kvalik2Proj.ViewModels;

public partial class BalanceViewModel : ViewModelBase
{
    [ObservableProperty]
    private string currentBalance = "0.00 ₽";

    [ObservableProperty]
    private string amount = string.Empty;

    [ObservableProperty]
    private string cardNumber = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public BalanceViewModel()
    {
        RefreshBalance();
    }

    private void RefreshBalance()
    {
        var user = SessionService.Instance.CurrentUser;
        if (user == null) return;

        using var db = new AppDbContext();
        var dbUser = db.Users.Find(user.Id);
        if (dbUser != null)
        {
            user.Balance = dbUser.Balance;
            CurrentBalance = (dbUser.Balance ?? 0).ToString("N2") + " ₽";
        }
    }

    [RelayCommand]
    private void TopUp()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!decimal.TryParse(Amount, out var amountValue) || amountValue <= 0)
        {
            ErrorMessage = "Введите корректную сумму больше 0";
            return;
        }

        var digitsOnly = Regex.Replace(CardNumber ?? "", @"\D", "");
        if (digitsOnly.Length != 16)
        {
            ErrorMessage = "Номер карты должен содержать 16 цифр";
            return;
        }

        var user = SessionService.Instance.CurrentUser;
        if (user == null)
        {
            ErrorMessage = "Необходимо авторизоваться";
            return;
        }

        using var db = new AppDbContext();
        var dbUser = db.Users.Find(user.Id);
        if (dbUser == null)
        {
            ErrorMessage = "Пользователь не найден";
            return;
        }

        dbUser.Balance = (dbUser.Balance ?? 0) + amountValue;

        var payment = new Payment
        {
            UserId = user.Id,
            Amount = amountValue,
            CreatedAt = DateTime.Now
        };

        db.Payments.Add(payment);
        db.SaveChanges();

        user.Balance = dbUser.Balance;
        CurrentBalance = dbUser.Balance.Value.ToString("N2") + " ₽";
        Amount = string.Empty;
        CardNumber = string.Empty;
        SuccessMessage = $"Баланс пополнен на {amountValue:N2} ₽";
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}
