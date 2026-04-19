using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;

namespace Kvalik2Proj.ViewModels;

public partial class QualificationRequestViewModel : ViewModelBase
{
    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private string successMessage = "";

    [ObservableProperty]
    private ObservableCollection<QualRequestItem> requests = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private string pageInfo = "";

    private const int PageSize = 5;
    private int _totalCount;

    public QualificationRequestViewModel()
    {
        LoadRequests();
    }

    private void LoadRequests()
    {
        var userId = SessionService.Instance.CurrentUser?.Id;
        if (userId == null) return;

        using var db = new AppDbContext();
        var query = db.QualificationRequests
            .Where(r => r.MasterId == userId)
            .Include(r => r.ReviewedByUser)
            .OrderByDescending(r => r.CreatedAt);

        _totalCount = query.Count();

        var list = query
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Requests.Clear();
        foreach (var r in list)
        {
            Requests.Add(new QualRequestItem
            {
                Id = r.Id,
                Description = r.Description ?? "",
                Status = r.Status ?? "Ожидание",
                CreatedAt = r.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—",
                ReviewedAt = r.ReviewedAt?.ToString("dd.MM.yyyy HH:mm") ?? "",
                ReviewComment = r.ReviewComment ?? "",
                ReviewedBy = r.ReviewedByUser?.Name ?? ""
            });
        }

        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        PageInfo = $"Стр. {CurrentPage} из {(totalPages == 0 ? 1 : totalPages)} (всего: {_totalCount})";
    }

    [RelayCommand]
    private void Submit()
    {
        ErrorMessage = "";
        SuccessMessage = "";

        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Введите описание заявки";
            return;
        }

        var userId = SessionService.Instance.CurrentUser?.Id;
        if (userId == null)
        {
            ErrorMessage = "Ошибка сессии";
            return;
        }

        using var db = new AppDbContext();
        var request = new QualificationRequest
        {
            MasterId = userId,
            Description = Description.Trim(),
            Status = "Ожидание",
            CreatedAt = DateTime.Now
        };
        db.QualificationRequests.Add(request);
        db.SaveChanges();

        Description = "";
        SuccessMessage = "Заявка успешно подана";
        CurrentPage = 1;
        LoadRequests();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1) { CurrentPage--; LoadRequests(); }
    }

    [RelayCommand]
    private void NextPage()
    {
        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        if (CurrentPage < totalPages) { CurrentPage++; LoadRequests(); }
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new MasterPanelViewModel());
    }
}

public class QualRequestItem
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string ReviewedAt { get; set; } = "";
    public string ReviewComment { get; set; } = "";
    public string ReviewedBy { get; set; } = "";
}
