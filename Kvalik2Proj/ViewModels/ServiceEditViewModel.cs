using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace Kvalik2Proj.ViewModels;

public partial class ServiceEditViewModel : ViewModelBase
{
    private readonly int? _editingServiceId;

    [ObservableProperty]
    private string title = "Добавление услуги";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string price = string.Empty;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private string selectedType = "Кастом";

    public string[] TypeOptions { get; } = new[] { "Кастом", "Косплей" };

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private string? imagePath;

    [ObservableProperty]
    private Bitmap? imagePreview;

    public ServiceEditViewModel()
    {
        if (!SessionService.Instance.IsModerator)
        {
            NavigationService.Instance.NavigateTo(new ServicesListViewModel());
            return;
        }

        Title = "Добавление услуги";
        LoadCategories();
    }

    public ServiceEditViewModel(int serviceId)
    {
        if (!SessionService.Instance.IsModerator)
        {
            NavigationService.Instance.NavigateTo(new ServicesListViewModel());
            return;
        }

        _editingServiceId = serviceId;
        Title = "Редактирование услуги";
        LoadCategories();
        LoadService(serviceId);
    }

    private void LoadCategories()
    {
        using var db = new AppDbContext();
        var list = db.Categories.ToList();
        Categories = new ObservableCollection<Category>(list);
    }

    private void LoadService(int serviceId)
    {
        using var db = new AppDbContext();
        var service = db.Services.Find(serviceId);
        if (service == null) return;

        Name = service.Name ?? "";
        Description = service.Description ?? "";
        Price = service.Price?.ToString() ?? "";
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == service.CategoryId);
        SelectedType = service.Type ?? "Кастом";
        ImagePath = service.ImagePath;
        LoadImagePreview();
    }

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Введите название услуги";
            return;
        }

        if (!decimal.TryParse(Price, out var priceValue) || priceValue < 0)
        {
            ErrorMessage = "Введите корректную цену";
            return;
        }

        using var db = new AppDbContext();

        if (_editingServiceId.HasValue)
        {
            var service = db.Services.Find(_editingServiceId.Value);
            if (service == null)
            {
                ErrorMessage = "Услуга не найдена";
                return;
            }

            service.Name = Name;
            service.Description = Description;
            service.Price = priceValue;
            service.CategoryId = SelectedCategory?.Id;
            service.Type = SelectedType;
            service.ImagePath = ImagePath;
            service.UpdatedAt = DateTime.Now;

            db.SaveChanges();
            NavigationService.Instance.NavigateTo(new ServicesListViewModel());
        }
        else
        {
            var service = new Service
            {
                Name = Name,
                Description = Description,
                Price = priceValue,
                CategoryId = SelectedCategory?.Id,
                Type = SelectedType,
                ImagePath = ImagePath,
                UpdatedAt = DateTime.Now
            };

            db.Services.Add(service);
            db.SaveChanges();
            NavigationService.Instance.NavigateTo(new ServicesListViewModel());
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickImage()
    {
        var topLevel = TopLevel.GetTopLevel(
            (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображение",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Изображения") { Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" } }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            ImagePath = file.Path.LocalPath;
            LoadImagePreview();
        }
    }

    private void LoadImagePreview()
    {
        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            try
            {
                ImagePreview = new Bitmap(ImagePath);
            }
            catch { ImagePreview = null; }
        }
        else
        {
            ImagePreview = null;
        }
    }
}
