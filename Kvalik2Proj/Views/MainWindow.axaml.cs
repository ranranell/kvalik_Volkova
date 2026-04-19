using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Kvalik2Proj.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MinimizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private async void CloseClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Подтверждение",
            "Вы уверены, что хотите закрыть приложение?",
            ButtonEnum.YesNo,
            MsBox.Avalonia.Enums.Icon.Question);

        var result = await box.ShowWindowDialogAsync(this);
        if (result == ButtonResult.Yes)
        {
            _forceClose = true;
            Close();
        }
    }

    private bool _forceClose;

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClose)
        {
            e.Cancel = true;

            var box = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение",
                "Вы уверены, что хотите закрыть приложение?",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question);

            var result = await box.ShowWindowDialogAsync(this);
            if (result == ButtonResult.Yes)
            {
                _forceClose = true;
                Close();
            }
        }

        base.OnClosing(e);
    }
}
