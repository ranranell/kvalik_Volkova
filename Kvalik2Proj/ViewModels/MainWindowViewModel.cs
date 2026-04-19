using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvalik2Proj.Services;

namespace Kvalik2Proj.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? currentView;

    [ObservableProperty]
    private bool isLoggedIn;

    [ObservableProperty]
    private bool isModerator;

    [ObservableProperty]
    private bool isAdmin;

    [ObservableProperty]
    private bool isMaster;

    public MainWindowViewModel()
    {
        NavigationService.Instance.CurrentViewChanged += OnViewChanged;
        NavigationService.Instance.NavigateTo(new LoginViewModel());
    }

    private void OnViewChanged(ViewModelBase viewModel)
    {
        CurrentView = viewModel;
        IsLoggedIn = SessionService.Instance.IsLoggedIn;
        IsModerator = SessionService.Instance.IsModerator;
        IsAdmin = SessionService.Instance.IsAdmin;
        IsMaster = SessionService.Instance.IsMaster;
    }

    [RelayCommand]
    private void NavigateToServices()
    {
        NavigationService.Instance.NavigateTo(new ServicesListViewModel());
    }

    [RelayCommand]
    private void NavigateToAddService()
    {
        NavigationService.Instance.NavigateTo(new ServiceEditViewModel());
    }

    [RelayCommand]
    private void NavigateToAppointment()
    {
        NavigationService.Instance.NavigateTo(new AppointmentCreateViewModel());
    }

    [RelayCommand]
    private void NavigateToMyAppointments()
    {
        NavigationService.Instance.NavigateTo(new MyAppointmentsViewModel());
    }

    [RelayCommand]
    private void NavigateToBalance()
    {
        NavigationService.Instance.NavigateTo(new BalanceViewModel());
    }

    [RelayCommand]
    private void NavigateToReviews()
    {
        NavigationService.Instance.NavigateTo(new ReviewsListViewModel());
    }

    [RelayCommand]
    private void NavigateToMasterBinding()
    {
        NavigationService.Instance.NavigateTo(new MasterBindingViewModel());
    }

    [RelayCommand]
    private void NavigateToUsers()
    {
        NavigationService.Instance.NavigateTo(new UsersListViewModel());
    }

    [RelayCommand]
    private void NavigateToMasterPanel()
    {
        NavigationService.Instance.NavigateTo(new MasterPanelViewModel());
    }

    [RelayCommand]
    private void NavigateToQualificationRequests()
    {
        NavigationService.Instance.NavigateTo(new QualificationRequestsListViewModel());
    }

    [RelayCommand]
    private void Logout()
    {
        SessionService.Instance.Logout();
        NavigationService.Instance.NavigateTo(new LoginViewModel());
    }
}
