using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;

namespace Kvalik2Proj.ViewModels;

public partial class QualificationRequestsListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<QualRequestListItem> requests = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private string pageInfo = "";

    [ObservableProperty]
    private string selectedFilter = "Все";

    public string[] FilterOptions { get; } = new[] { "Все", "Ожидание", "Одобрена", "Отклонена" };

    private const int PageSize = 10;
    private int _totalCount;

    public QualificationRequestsListViewModel()
    {
        LoadRequests();
    }

    partial void OnSelectedFilterChanged(string value)
    {
        CurrentPage = 1;
        LoadRequests();
    }

    private void LoadRequests()
    {
        using var db = new AppDbContext();
        var query = db.QualificationRequests
            .Include(r => r.Master)
            .Include(r => r.ReviewedByUser)
            .AsQueryable();

        if (SelectedFilter != "Все")
            query = query.Where(r => r.Status == SelectedFilter);

        query = query.OrderByDescending(r => r.CreatedAt);

        _totalCount = query.Count();

        var list = query
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Requests.Clear();
        foreach (var r in list)
        {
            Requests.Add(new QualRequestListItem
            {
                Id = r.Id,
                MasterName = r.Master?.Name ?? "—",
                Description = r.Description ?? "",
                Status = r.Status ?? "Ожидание",
                CreatedAt = r.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—",
                ReviewComment = r.ReviewComment ?? "",
                ReviewedBy = r.ReviewedByUser?.Name ?? "",
                IsPending = r.Status == "Ожидание",
                Parent = this
            });
        }

        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        PageInfo = $"Стр. {CurrentPage} из {(totalPages == 0 ? 1 : totalPages)} (всего: {_totalCount})";
    }

    public void ApproveRequest(QualRequestListItem item)
    {
        using var db = new AppDbContext();
        var request = db.QualificationRequests.Find(item.Id);
        if (request == null) return;

        request.Status = "Одобрена";
        request.ReviewedAt = DateTime.Now;
        request.ReviewedByUserId = SessionService.Instance.CurrentUser?.Id;
        db.SaveChanges();

        LoadRequests();
    }

    public void RejectRequest(QualRequestListItem item)
    {
        if (string.IsNullOrWhiteSpace(item.RejectComment))
            return;

        using var db = new AppDbContext();
        var request = db.QualificationRequests.Find(item.Id);
        if (request == null) return;

        request.Status = "Отклонена";
        request.ReviewComment = item.RejectComment.Trim();
        request.ReviewedAt = DateTime.Now;
        request.ReviewedByUserId = SessionService.Instance.CurrentUser?.Id;
        db.SaveChanges();

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
}

public partial class QualRequestListItem : ObservableObject
{
    public int Id { get; set; }
    public string MasterName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string ReviewComment { get; set; } = "";
    public string ReviewedBy { get; set; } = "";
    public bool IsPending { get; set; }
    public QualificationRequestsListViewModel? Parent { get; set; }

    [ObservableProperty]
    private string rejectComment = "";

    [RelayCommand]
    private void Approve()
    {
        Parent?.ApproveRequest(this);
    }

    [RelayCommand]
    private void Reject()
    {
        Parent?.RejectRequest(this);
    }
}
