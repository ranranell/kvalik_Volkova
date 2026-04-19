using System;
using Kvalik2Proj.ViewModels;

namespace Kvalik2Proj.Services;

public class NavigationService
{
    private static NavigationService? _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();

    public event Action<ViewModelBase>? CurrentViewChanged;

    private ViewModelBase? _currentView;
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke(value!);
        }
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentView = viewModel;
    }
}
