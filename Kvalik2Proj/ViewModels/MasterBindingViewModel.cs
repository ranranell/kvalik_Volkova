using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;

namespace Kvalik2Proj.ViewModels;

public partial class MasterBindingViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<MasterItemViewModel> masters = new();

    [ObservableProperty]
    private MasterItemViewModel? selectedMaster;

    [ObservableProperty]
    private ObservableCollection<ServiceBindingItem> serviceBindings = new();

    public MasterBindingViewModel()
    {
        LoadMasters();
    }

    partial void OnSelectedMasterChanged(MasterItemViewModel? value)
    {
        LoadServiceBindings();
    }

    private void LoadMasters()
    {
        using var db = new AppDbContext();
        var masterRole = db.Roles.FirstOrDefault(r => r.Name == "Мастер");
        if (masterRole == null) return;

        var masterUsers = db.Users
            .Where(u => u.RoleId == masterRole.Id)
            .OrderBy(u => u.Name)
            .ToList();

        Masters.Clear();
        foreach (var m in masterUsers)
            Masters.Add(new MasterItemViewModel { Id = m.Id, Name = m.Name ?? "Без имени" });

        if (Masters.Count > 0)
            SelectedMaster = Masters[0];
    }

    private void LoadServiceBindings()
    {
        ServiceBindings.Clear();
        if (SelectedMaster == null) return;

        using var db = new AppDbContext();
        var allServices = db.Services.OrderBy(s => s.Name).ToList();
        var linkedIds = db.MastersServices
            .Where(ms => ms.MasterId == SelectedMaster.Id)
            .Select(ms => ms.ServiceId)
            .ToHashSet();

        foreach (var s in allServices)
        {
            ServiceBindings.Add(new ServiceBindingItem
            {
                ServiceId = s.Id,
                ServiceName = s.Name ?? "",
                IsLinked = linkedIds.Contains(s.Id),
                MasterId = SelectedMaster.Id,
                Parent = this
            });
        }
    }

    public void ToggleBinding(ServiceBindingItem item)
    {
        using var db = new AppDbContext();
        var existing = db.MastersServices
            .FirstOrDefault(ms => ms.MasterId == item.MasterId && ms.ServiceId == item.ServiceId);

        if (item.IsLinked)
        {
            if (existing == null)
            {
                db.MastersServices.Add(new MastersService
                {
                    MasterId = item.MasterId,
                    ServiceId = item.ServiceId
                });
                db.SaveChanges();
            }
        }
        else
        {
            if (existing != null)
            {
                db.MastersServices.Remove(existing);
                db.SaveChanges();
            }
        }
    }

    [RelayCommand]
    private void Back()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }
}

public partial class MasterItemViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}

public partial class ServiceBindingItem : ObservableObject
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = "";
    public int MasterId { get; set; }
    public MasterBindingViewModel? Parent { get; set; }

    [ObservableProperty]
    private bool isLinked;

    partial void OnIsLinkedChanged(bool value)
    {
        Parent?.ToggleBinding(this);
    }
}
