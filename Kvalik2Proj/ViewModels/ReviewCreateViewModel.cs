using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kvalik2Proj.ViewModels;

public partial class ReviewCreateViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Service> servicesList = new();

    [ObservableProperty]
    private Service? selectedService;

    [ObservableProperty]
    private ObservableCollection<User> mastersList = new();

    [ObservableProperty]
    private User? selectedMaster;

    [ObservableProperty]
    private int rating = 5;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public ObservableCollection<int> RatingOptions { get; } = new(new[] { 1, 2, 3, 4, 5 });

    public ReviewCreateViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();
        ServicesList = new ObservableCollection<Service>(db.Services.OrderBy(s => s.Name).ToList());
        MastersList = new ObservableCollection<User>();
    }

    partial void OnSelectedServiceChanged(Service? value)
    {
        LoadMastersForService();
    }

    private void LoadMastersForService()
    {
        SelectedMaster = null;
        MastersList.Clear();

        if (SelectedService == null) return;

        using var db = new AppDbContext();
        var masters = db.MastersServices
            .Where(ms => ms.ServiceId == SelectedService.Id)
            .Include(ms => ms.Master)
            .Where(ms => ms.Master != null)
            .Select(ms => ms.Master!)
            .OrderBy(m => m.Name)
            .ToList();

        MastersList = new ObservableCollection<User>(masters);
    }

    [RelayCommand]
    private void Submit()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedService == null && SelectedMaster == null)
        {
            ErrorMessage = "Выберите услугу или мастера";
            return;
        }

        if (string.IsNullOrWhiteSpace(Comment))
        {
            ErrorMessage = "Напишите комментарий";
            return;
        }

        var currentUser = SessionService.Instance.CurrentUser;
        if (currentUser == null)
        {
            ErrorMessage = "Необходимо авторизоваться";
            return;
        }

        using var db = new AppDbContext();
        var review = new Review
        {
            UserId = currentUser.Id,
            ServiceId = SelectedService?.Id,
            MasterId = SelectedMaster?.Id,
            Rating = Rating,
            Comment = Comment,
            CreatedAt = DateTime.Now
        };

        db.Reviews.Add(review);
        db.SaveChanges();

        NavigationService.Instance.NavigateTo(new ReviewsListViewModel());
    }

    [RelayCommand]
    private void GoToList()
    {
        NavigationService.Instance.NavigateTo(new ReviewsListViewModel());
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}
