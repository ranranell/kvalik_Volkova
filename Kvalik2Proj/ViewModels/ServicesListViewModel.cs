using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Reflection;
using System.IO;

namespace Kvalik2Proj.ViewModels;

public partial class ServicesListViewModel : ViewModelBase
{
    private const int pageSize = 3;
    private List<Service> allServices = new();

    [ObservableProperty]
    private ObservableCollection<ServiceCardViewModel> services = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string paginationText = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> categories = new();

    [ObservableProperty]
    private bool isCustomTab = true;

    [ObservableProperty]
    private bool isCosplayTab;

    [ObservableProperty]
    private bool sortAscending = true;

    public bool IsModerator => SessionService.Instance.IsModerator;

    public ServicesListViewModel()
    {
        LoadCategories();
        LoadServices();
    }

    private void LoadCategories()
    {
        using var db = new AppDbContext();
        var categoryNames = db.Categories.Select(c => c.Name!).ToList();
        categoryNames.Insert(0, "Все");
        Categories = new ObservableCollection<string>(categoryNames);
    }

    private void LoadServices()
    {
        using var db = new AppDbContext();
        var query = db.Services
            .Include(s => s.Category)
            .Include(s => s.MastersServices)
                .ThenInclude(ms => ms.Master)
            .AsQueryable();

        if (IsCustomTab)
            query = query.Where(s => s.Type == "Кастом" || s.Type == null);
        else if (IsCosplayTab)
            query = query.Where(s => s.Type == "Косплей");

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            query = query.Where(s => s.Name != null && s.Name.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Все")
        {
            query = query.Where(s => s.Category != null && s.Category.Name == SelectedCategory);
        }

        query = SortAscending
            ? query.OrderBy(s => s.Name)
            : query.OrderByDescending(s => s.Name);

        allServices = query.ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(allServices.Count / (double)pageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        UpdatePage();
    }

    private void UpdatePage()
    {
        var pageItems = allServices
            .Skip((CurrentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ServiceCardViewModel(s))
            .ToList();

        Services = new ObservableCollection<ServiceCardViewModel>(pageItems);

        var start = (CurrentPage - 1) * pageSize + 1;
        var end = Math.Min(CurrentPage * pageSize, allServices.Count);
        PaginationText = allServices.Count > 0
            ? $"{start}-{end} из {allServices.Count}"
            : "Нет услуг";
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdatePage();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdatePage();
        }
    }

    [RelayCommand]
    private void Search()
    {
        CurrentPage = 1;
        LoadServices();
    }

    [RelayCommand]
    private void FilterByCategory()
    {
        CurrentPage = 1;
        LoadServices();
    }

    [RelayCommand]
    private void ToggleSort()
    {
        SortAscending = !SortAscending;
        CurrentPage = 1;
        LoadServices();
    }

    [RelayCommand]
    private void ShowCustomTab()
    {
        IsCustomTab = true;
        IsCosplayTab = false;
        CurrentPage = 1;
        LoadServices();
    }

    [RelayCommand]
    private void ShowCosplayTab()
    {
        IsCustomTab = false;
        IsCosplayTab = true;
        CurrentPage = 1;
        LoadServices();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        LoadServices();
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        CurrentPage = 1;
        LoadServices();
    }

    [RelayCommand]
    private void AddService()
    {
        NavigationService.Instance.NavigateTo(new ServiceEditViewModel());
    }

    [RelayCommand]
    private void EditService(int serviceId)
    {
        NavigationService.Instance.NavigateTo(new ServiceEditViewModel(serviceId));
    }

    [RelayCommand]
    private void DeleteService(int serviceId)
    {
        using var db = new AppDbContext();
        var service = db.Services.Find(serviceId);
        if (service == null) return;

        db.MastersServices.RemoveRange(db.MastersServices.Where(ms => ms.ServiceId == serviceId));
        db.Reviews.RemoveRange(db.Reviews.Where(r => r.ServiceId == serviceId));
        db.Appointments.RemoveRange(db.Appointments.Where(a => a.ServiceId == serviceId));
        db.Services.Remove(service);
        db.SaveChanges();

        LoadServices();
    }
}

public partial class ServiceCardViewModel : ViewModelBase
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Price { get; }
    public string CategoryName { get; }
    public string MasterName { get; }
    public string UpdatedAt { get; }
    public string? ImagePath { get; }
    public Bitmap? ImageBitmap { get; }
    public bool IsModerator => SessionService.Instance.IsModerator;

    public ServiceCardViewModel(Service service)
    {
        Id = service.Id;
        Name = service.Name ?? "Без названия";
        Description = service.Description ?? "";
        Price = service.Price?.ToString("N2") + " ₽" ?? "Цена не указана";
        CategoryName = service.Category?.Name ?? "Без категории";
        UpdatedAt = service.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "";

        var master = service.MastersServices?.FirstOrDefault()?.Master;
        MasterName = master != null ? $"Мастер: {master.Name}" : "";

        if (!string.IsNullOrEmpty(service.ImagePath) && File.Exists(service.ImagePath))
        {
            ImagePath = service.ImagePath;
            try
            {
                ImageBitmap = new Bitmap(service.ImagePath);
            }
            catch { ImageBitmap = null; }
        }
        else
        {
            if (service.Type == "Косплей")
            {
                int idx = ((service.Id - 1) % 7) + 1;
                ImagePath = $"avares://Kvalik2Proj/Assets/Services/Косплей/KL{idx}.jpg";
            }
            else
            {
                int idx = ((service.Id - 1) % 12) + 1;
                ImagePath = $"avares://Kvalik2Proj/Assets/Services/Кастом/Pr{idx}.jpg";
            }

            try
            {
                var uri = new Uri(ImagePath);
                using var stream = Avalonia.Platform.AssetLoader.Open(uri);
                ImageBitmap = new Bitmap(stream);
            }
            catch { ImageBitmap = null; }
        }
    }

    [RelayCommand]
    private void Edit()
    {
        NavigationService.Instance.NavigateTo(new ServiceEditViewModel(Id));
    }

    [RelayCommand]
    private void Delete()
    {
        using var db = new AppDbContext();
        var service = db.Services.Find(Id);
        if (service == null) return;

        db.MastersServices.RemoveRange(db.MastersServices.Where(ms => ms.ServiceId == Id));
        db.Reviews.RemoveRange(db.Reviews.Where(r => r.ServiceId == Id));
        db.Appointments.RemoveRange(db.Appointments.Where(a => a.ServiceId == Id));
        db.Services.Remove(service);
        db.SaveChanges();

        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}
