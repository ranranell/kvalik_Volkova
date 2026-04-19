using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;

namespace Kvalik2Proj.ViewModels;

public partial class MasterPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<MasterAppointmentItem> appointments = new();

    [ObservableProperty]
    private ObservableCollection<string> linkedServices = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private string pageInfo = "";

    private const int PageSize = 5;
    private int _totalCount;

    public string MasterName => SessionService.Instance.CurrentUser?.Name ?? "";

    public MasterPanelViewModel()
    {
        LoadLinkedServices();
        LoadAppointments();
    }

    private void LoadLinkedServices()
    {
        var userId = SessionService.Instance.CurrentUser?.Id;
        if (userId == null) return;

        using var db = new AppDbContext();
        var services = db.MastersServices
            .Where(ms => ms.MasterId == userId)
            .Include(ms => ms.Service)
            .Select(ms => ms.Service!.Name ?? "")
            .OrderBy(n => n)
            .ToList();

        LinkedServices = new ObservableCollection<string>(services);
    }

    private void LoadAppointments()
    {
        var userId = SessionService.Instance.CurrentUser?.Id;
        if (userId == null) return;

        using var db = new AppDbContext();
        var query = db.Appointments
            .Where(a => a.MasterId == userId)
            .Include(a => a.User)
            .Include(a => a.Service)
            .OrderByDescending(a => a.AppointmentDate);

        _totalCount = query.Count();

        var list = query
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Appointments.Clear();
        foreach (var a in list)
        {
            var item = new MasterAppointmentItem
            {
                Id = a.Id,
                ClientName = a.User?.Name ?? "—",
                ServiceName = a.Service?.Name ?? "—",
                Date = a.AppointmentDate?.ToString("dd.MM.yyyy") ?? "—",
                QueueNumber = a.QueueNumber ?? 0,
                Parent = this
            };
            item.SetInitialStatus(a.Status ?? "Ожидание");
            Appointments.Add(item);
        }

        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        PageInfo = $"Стр. {CurrentPage} из {(totalPages == 0 ? 1 : totalPages)} (всего: {_totalCount})";
    }

    [RelayCommand]
    private void NavigateToQualificationRequest()
    {
        NavigationService.Instance.NavigateTo(new QualificationRequestViewModel());
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1) { CurrentPage--; LoadAppointments(); }
    }

    [RelayCommand]
    private void NextPage()
    {
        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        if (CurrentPage < totalPages) { CurrentPage++; LoadAppointments(); }
    }

    public void ChangeAppointmentStatus(MasterAppointmentItem item, string newStatus)
    {
        using var db = new AppDbContext();
        var appt = db.Appointments.Find(item.Id);
        if (appt == null) return;

        if (newStatus == "Завершено" && appt.Status != "Завершено")
        {
            var service = db.Services.Find(appt.ServiceId);
            var client = db.Users.Find(appt.UserId);

            if (client != null && service?.Price != null)
            {
                client.Balance = (client.Balance ?? 0) - service.Price.Value;
            }
        }

        appt.Status = newStatus;
        db.SaveChanges();
    }
}

public partial class MasterAppointmentItem : ObservableObject
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Date { get; set; } = "";
    public int QueueNumber { get; set; }
    public MasterPanelViewModel? Parent { get; set; }

    public string[] StatusOptions { get; } = new[] { "Ожидание", "Записан", "Завершено" };

    private bool _initializing;

    [ObservableProperty]
    private string status = "";

    public void SetInitialStatus(string value)
    {
        _initializing = true;
        Status = value;
        _initializing = false;
    }

    partial void OnStatusChanged(string value)
    {
        if (!_initializing)
            Parent?.ChangeAppointmentStatus(this, value);
    }
}
