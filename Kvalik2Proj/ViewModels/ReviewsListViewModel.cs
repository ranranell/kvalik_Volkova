using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kvalik2Proj.ViewModels;

public partial class ReviewsListViewModel : ViewModelBase
{
    private const int pageSize = 5;
    private List<ReviewItemViewModel> allReviews = new();

    [ObservableProperty]
    private ObservableCollection<ReviewItemViewModel> reviews = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string paginationText = string.Empty;

    public ReviewsListViewModel()
    {
        LoadReviews();
    }

    private void LoadReviews()
    {
        using var db = new AppDbContext();
        var list = db.Reviews
            .Include(r => r.User)
            .Include(r => r.Service)
            .Include(r => r.Master)
            .OrderByDescending(r => r.CreatedAt)
            .ToList()
            .Select(r => new ReviewItemViewModel(r))
            .ToList();

        allReviews = list;
        TotalPages = Math.Max(1, (int)Math.Ceiling(allReviews.Count / (double)pageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        UpdatePage();
    }

    private void UpdatePage()
    {
        var pageItems = allReviews
            .Skip((CurrentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Reviews = new ObservableCollection<ReviewItemViewModel>(pageItems);

        var start = (CurrentPage - 1) * pageSize + 1;
        var end = Math.Min(CurrentPage * pageSize, allReviews.Count);
        PaginationText = allReviews.Count > 0
            ? $"{start}-{end} из {allReviews.Count}"
            : "Нет отзывов";
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages) { CurrentPage++; UpdatePage(); }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1) { CurrentPage--; UpdatePage(); }
    }

    [RelayCommand]
    private void WriteReview()
    {
        NavigationService.Instance.NavigateTo(new ReviewCreateViewModel());
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}

public class ReviewItemViewModel
{
    public string AuthorName { get; }
    public string ServiceName { get; }
    public string MasterName { get; }
    public string RatingStars { get; }
    public string Comment { get; }
    public string Date { get; }

    public ReviewItemViewModel(Review review)
    {
        AuthorName = review.User?.Name ?? "Аноним";
        ServiceName = review.Service?.Name ?? "—";
        MasterName = review.Master?.Name ?? "—";
        RatingStars = new string('★', review.Rating ?? 0) + new string('☆', 5 - (review.Rating ?? 0));
        Comment = review.Comment ?? "";
        Date = review.CreatedAt?.ToString("dd.MM.yyyy") ?? "";
    }
}
