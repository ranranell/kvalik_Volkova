using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Data;
using Kvalik2Proj.Services;
using Microsoft.EntityFrameworkCore;

namespace Kvalik2Proj.ViewModels;

public partial class UsersListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<UserItemViewModel> users = new();

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private ObservableCollection<Role> roles = new();

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private string pageInfo = "";

    private const int PageSize = 10;
    private int _totalCount;

    public UsersListViewModel()
    {
        LoadRoles();
        LoadUsers();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        LoadUsers();
    }

    private void LoadRoles()
    {
        using var db = new AppDbContext();
        Roles = new ObservableCollection<Role>(db.Roles.OrderBy(r => r.Id).ToList());
    }

    private void LoadUsers()
    {
        using var db = new AppDbContext();
        var query = db.Users.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.ToLower();
            query = query.Where(u =>
                (u.Name != null && u.Name.ToLower().Contains(s)) ||
                (u.Email != null && u.Email.ToLower().Contains(s)));
        }

        _totalCount = query.Count();
        var list = query
            .OrderBy(u => u.Id)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Users.Clear();
        foreach (var u in list)
        {
            var item = new UserItemViewModel
            {
                Id = u.Id,
                Name = u.Name ?? "",
                Email = u.Email ?? "",
                RoleName = u.Role?.Name ?? "",
                RoleId = u.RoleId ?? 0,
                Balance = u.Balance ?? 0,
                AvailableRoles = Roles,
                Parent = this
            };
            item.SelectedRole = Roles.FirstOrDefault(r => r.Id == item.RoleId);
            Users.Add(item);
        }

        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        PageInfo = $"Стр. {CurrentPage} из {(totalPages == 0 ? 1 : totalPages)} (всего: {_totalCount})";
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            LoadUsers();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        var totalPages = (_totalCount + PageSize - 1) / PageSize;
        if (CurrentPage < totalPages)
        {
            CurrentPage++;
            LoadUsers();
        }
    }

    public void ChangeUserRole(UserItemViewModel item, int newRoleId)
    {
        using var db = new AppDbContext();
        var user = db.Users.Find(item.Id);
        if (user == null) return;
        user.RoleId = newRoleId;
        db.SaveChanges();
        item.RoleName = Roles.FirstOrDefault(r => r.Id == newRoleId)?.Name ?? "";
    }

    [RelayCommand]
    private void AddEmployee()
    {
        NavigationService.Instance.NavigateTo(new AddEmployeeViewModel());
    }

    public void DeleteUser(int userId)
    {
        if (userId == SessionService.Instance.CurrentUser?.Id) return;

        using var db = new AppDbContext();
        var user = db.Users.Find(userId);
        if (user == null) return;

        db.MastersServices.RemoveRange(db.MastersServices.Where(ms => ms.MasterId == userId));
        db.Reviews.RemoveRange(db.Reviews.Where(r => r.UserId == userId || r.MasterId == userId));
        db.Appointments.RemoveRange(db.Appointments.Where(a => a.UserId == userId || a.MasterId == userId));
        db.Payments.RemoveRange(db.Payments.Where(p => p.UserId == userId));
        db.Users.Remove(user);
        db.SaveChanges();

        LoadUsers();
    }
}

public partial class UserItemViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Balance { get; set; }
    public int RoleId { get; set; }
    public ObservableCollection<Role>? AvailableRoles { get; set; }
    public UsersListViewModel? Parent { get; set; }

    [ObservableProperty]
    private string roleName = "";

    [ObservableProperty]
    private Role? selectedRole;

    partial void OnSelectedRoleChanged(Role? value)
    {
        if (value != null && value.Id != RoleId)
        {
            Parent?.ChangeUserRole(this, value.Id);
            RoleId = value.Id;
        }
    }

    [RelayCommand]
    private void Delete()
    {
        Parent?.DeleteUser(this.Id);
    }
}
