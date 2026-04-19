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

public partial class MyAppointmentsViewModel : ViewModelBase
{
    private const int pageSize = 5;
    private List<AppointmentItemViewModel> allAppointments = new();

    [ObservableProperty]
    private ObservableCollection<AppointmentItemViewModel> appointments = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string paginationText = string.Empty;

    public MyAppointmentsViewModel()
    {
        LoadAppointments();
    }

    private void LoadAppointments()
    {
        var currentUser = SessionService.Instance.CurrentUser;
        if (currentUser == null) return;

        using var db = new AppDbContext();
        var list = db.Appointments
            .Where(a => a.UserId == currentUser.Id)
            .Include(a => a.Service)
            .Include(a => a.Master)
            .OrderByDescending(a => a.AppointmentDate)
            .ToList()
            .Select(a => new AppointmentItemViewModel(a))
            .ToList();

        allAppointments = list;
        TotalPages = Math.Max(1, (int)Math.Ceiling(allAppointments.Count / (double)pageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        UpdatePage();
    }

    private void UpdatePage()
    {
        var pageItems = allAppointments
            .Skip((CurrentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Appointments = new ObservableCollection<AppointmentItemViewModel>(pageItems);

        var start = (CurrentPage - 1) * pageSize + 1;
        var end = Math.Min(CurrentPage * pageSize, allAppointments.Count);
        PaginationText = allAppointments.Count > 0
            ? $"{start}-{end} из {allAppointments.Count}"
            : "Нет записей";
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
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}

public class AppointmentItemViewModel
{
    public int Id { get; }
    public string ServiceName { get; }
    public string MasterName { get; }
    public string Date { get; }
    public string QueueNumber { get; }
    public string Status { get; }

    public AppointmentItemViewModel(Appointment appointment)
    {
        Id = appointment.Id;
        ServiceName = appointment.Service?.Name ?? "—";
        MasterName = appointment.Master?.Name ?? "—";
        Date = appointment.AppointmentDate?.ToString("dd.MM.yyyy") ?? "—";
        QueueNumber = appointment.QueueNumber?.ToString() ?? "—";
        Status = appointment.Status ?? "—";
    }
}
