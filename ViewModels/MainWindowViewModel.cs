using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace CTRExploreSD.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _statusMessage = "Ready for FTP connection. Chicken Time !";
    [ObservableProperty] private string _consoleIp = "192.168.1.";

    [RelayCommand]
    private void OpenFtpExplorer()
    {
        var ftpWindow = new Views.FtpExplorerWindow();

        var ftpViewModel = new FtpExplorerViewModel();
        ftpWindow.DataContext = ftpViewModel;

        ftpWindow.Show();

        _ = ftpViewModel.ConnectAndLoadAsync(ConsoleIp);
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        string url = "https://github.com/saysaa/CTRExplorerSD";

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            StatusMessage = "Could not open browser.";
        }
    }
}