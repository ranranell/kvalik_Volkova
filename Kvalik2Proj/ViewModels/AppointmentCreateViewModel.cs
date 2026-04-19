using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kvalik2Proj.ViewModels;

public partial class AppointmentCreateViewModel : ViewModelBase
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
    private DateTimeOffset selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private string queueInfo = string.Empty;

    public AppointmentCreateViewModel()
    {
        LoadServices();
    }

    private void LoadServices()
    {
        using var db = new AppDbContext();
        var services = db.Services.OrderBy(s => s.Name).ToList();
        ServicesList = new ObservableCollection<Service>(services);
    }

    partial void OnSelectedServiceChanged(Service? value)
    {
        MastersList.Clear();
        SelectedMaster = null;
        QueueInfo = string.Empty;

        if (value == null) return;

        using var db = new AppDbContext();
        var masters = db.MastersServices
            .Where(ms => ms.ServiceId == value.Id)
            .Include(ms => ms.Master)
            .Select(ms => ms.Master!)
            .Where(m => m != null)
            .ToList();

        MastersList = new ObservableCollection<User>(masters);
    }

    partial void OnSelectedMasterChanged(User? value)
    {
        UpdateQueueInfo();
    }

    private void UpdateQueueInfo()
    {
        if (SelectedMaster == null || SelectedService == null)
        {
            QueueInfo = string.Empty;
            return;
        }

        using var db = new AppDbContext();
        var date = SelectedDate.Date;
        var nextDay = date.AddDays(1);
        var count = db.Appointments.Count(a =>
            a.MasterId == SelectedMaster.Id &&
            a.AppointmentDate.HasValue &&
            a.AppointmentDate.Value >= date &&
            a.AppointmentDate.Value < nextDay);

        QueueInfo = $"Записей на эту дату: {count}. Ваш номер: {count + 1}";
    }

    [RelayCommand]
    private void CreateAppointment()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedService == null)
        {
            ErrorMessage = "Выберите услугу";
            return;
        }

        if (SelectedMaster == null)
        {
            ErrorMessage = "Выберите мастера";
            return;
        }

        var currentUser = SessionService.Instance.CurrentUser;
        if (currentUser == null)
        {
            ErrorMessage = "Необходимо авторизоваться";
            return;
        }

        using var db = new AppDbContext();
        var date = SelectedDate.Date;
        var nextDay = date.AddDays(1);

        var maxQueue = db.Appointments
            .Where(a => a.MasterId == SelectedMaster.Id &&
                        a.AppointmentDate.HasValue &&
                        a.AppointmentDate.Value >= date &&
                        a.AppointmentDate.Value < nextDay)
            .Select(a => a.QueueNumber)
            .ToList();

        var queueNumber = maxQueue.Count > 0 ? (maxQueue.Max() ?? 0) + 1 : 1;

        var appointment = new Appointment
        {
            UserId = currentUser.Id,
            MasterId = SelectedMaster.Id,
            ServiceId = SelectedService.Id,
            AppointmentDate = SelectedDate.DateTime,
            QueueNumber = queueNumber,
            Status = "Ожидание"
        };

        db.Appointments.Add(appointment);
        db.SaveChanges();

        NavigationService.Instance.NavigateTo(new MyAppointmentsViewModel());
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}
