using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentFTP;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CTRExploreSD.ViewModels;

public partial class FtpExplorerViewModel : ViewModelBase, IDisposable
{
    private AsyncFtpClient? _client;

    [ObservableProperty] private string _currentPath = "/";
    [ObservableProperty] private string _statusMessage = "Connecting...";
    [ObservableProperty] private FtpItem? _selectedItem;

    [ObservableProperty] private bool _isPropertiesVisible = false;
    [ObservableProperty] private string _propertiesText = "";

    public ObservableCollection<FtpItem> Files { get; } = new();

    public async Task ConnectAndLoadAsync(string ip)
    {
        try
        {
            var config = new FtpConfig { ConnectTimeout = 5000, DataConnectionType = FtpDataConnectionType.AutoPassive };
            _client = new AsyncFtpClient(ip, "anonymous", "anonymous", 5000, config);

            _client.Encoding = System.Text.Encoding.UTF8;

            await _client.Connect();
            await LoadDirectoryAsync("/");
        }
        catch (Exception ex) { StatusMessage = $"Connection error: {ex.Message}"; }
    }

    private async Task LoadDirectoryAsync(string path)
    {
        if (_client == null || !_client.IsConnected) return;

        StatusMessage = $"Loading {path}...";
        Files.Clear();
        CurrentPath = path;

        var items = await _client.GetListing(path);

        if (path != "/")
        {
            string parentPath = path.LastIndexOf('/') > 0 ? path.Substring(0, path.LastIndexOf('/')) : "/";
            Files.Add(new FtpItem { Name = "..", FullPath = parentPath, IsDirectory = true });
        }

        foreach (var item in items)
        {
            var riskInfo = DetermineRisk(item.Name);
            Files.Add(new FtpItem
            {
                Name = item.Name,
                FullPath = item.FullName,
                IsDirectory = item.Type == FtpObjectType.Directory,
                Size = item.Size,
                Risk = riskInfo.Risk,
                Color = riskInfo.Color
            });
        }
        StatusMessage = "Ready.";
    }

    [RelayCommand]
    private async Task OpenItemAsync(FtpItem? item)
    {
        if (item == null) return;

        if (item.IsDirectory)
        {
            await LoadDirectoryAsync(item.FullPath);
        }
        else
        {
            try
            {
                StatusMessage = $"Opening {item.Name}...";
                string tempFilePath = Path.Combine(Path.GetTempPath(), item.Name);
                await _client!.DownloadFile(tempFilePath, item.FullPath);
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
                StatusMessage = "Ready.";
            }
            catch (Exception ex) { StatusMessage = $"Unable to open file: {ex.Message}"; }
        }
    }

    public async Task DownloadItemAsync(FtpItem item, string localPath)
    {
        if (_client == null || !_client.IsConnected) return;

        try
        {
            StatusMessage = $"Downloading {item.Name}...";
            if (item.IsDirectory)
                await _client.DownloadDirectory(localPath, item.FullPath, FtpFolderSyncMode.Update);
            else
                await _client.DownloadFile(localPath, item.FullPath);

            StatusMessage = $"Download complete for {item.Name}";
        }
        catch (Exception ex) { StatusMessage = $"Download error ! - {ex.Message}"; }
    }

    [RelayCommand]
    private async Task ShowPropertiesAsync(FtpItem? item)
    {
        if (item == null || item.Name == "..") return;

        try
        {
            StatusMessage = "Fetching properties...";
            var modified = await _client!.GetModifiedTime(item.FullPath);

            PropertiesText = $"Name: {item.Name}\n" +
                             $"Path: {item.FullPath}\n" +
                             $"Type: {(item.IsDirectory ? "Folder" : "File")}\n" +
                             $"Size: {(item.IsDirectory ? "Calculating..." : item.SizeDisplay)}\n" +
                             $"Modified: {modified:yyyy-MM-dd HH:mm}\n" +
                             $"Risk Level: {(string.IsNullOrEmpty(item.Risk) ? "None" : item.Risk)}";

            IsPropertiesVisible = true;
            StatusMessage = "Ready.";
        }
        catch (Exception ex) { StatusMessage = $"Properties error: {ex.Message}"; }
    }

    [RelayCommand]
    private void CloseProperties() { IsPropertiesVisible = false; }

    [RelayCommand]
    private async Task DeleteSelectedItemAsync()
    {
        if (SelectedItem == null || SelectedItem.Name == "..") return;
        try
        {
            StatusMessage = $"Deleting {SelectedItem.Name}...";
            if (SelectedItem.IsDirectory) await _client!.DeleteDirectory(SelectedItem.FullPath);
            else await _client!.DeleteFile(SelectedItem.FullPath);
            await LoadDirectoryAsync(CurrentPath);
        }
        catch (Exception ex) { StatusMessage = $"Delete error: {ex.Message}"; }
    }

    public void Dispose() { _client?.Dispose(); }

    private (string Risk, string Color) DetermineRisk(string itemName)
    {
        string name = itemName.ToLower();

        if (name == "boot.firm") return ("CRITICAL (BOOT)", "#FF0044");
        if (name == "boot.3dsx") return ("ENTRYPOINT (HBL)", "#FF8C00");

        if (name.EndsWith(".cia")) return ("CIA INSTALLER", "#00FA9A");
        if (name.EndsWith(".3dsx")) return ("HOMEBREW", "#FFD700");
        if (name.EndsWith(".nds") || name.EndsWith(".gba")) return ("ROM", "#00FA9A");
        if (name.EndsWith(".firm")) return ("PAYLOAD", "#FF0044");

        return name switch
        {
            // CRITICAL
            "luma" or "arm9loaderhax.bin" or "essential.exefs" or "arm9.bin" or "arm11.bin"
                => ("CRITICAL", "#FF0044"),

            // SYSTEM
            "nintendo 3ds" or "private" or "dbs" or "extdata" or "title" or "ticket" or "nand" or "rw" or "sys"
                => ("SYSTEM", "#FF8C00"),

            "gm9" or "godmode9" or "aeskeydb.bin" or "seeddb.bin" or "encTitleKeys.bin"
                => ("KEYS/CORE", "#FF8C00"),

            // TOOLS, APPS
            "3ds" or "checkpoint" or "jksm" or "anemone" or "fbi" or "universal-db" or "ftpd"
                => ("TOOL", "#FFD700"),

            "ctgp-7" or "luma3ds" or "salt" or "plugin" or "cheats" or "patches" or "rehid"
                => ("MOD/PLUGIN", "#FFD700"),

            "payloads" or "scripts" or "gm9out" or "backups"
                => ("GM9 DATA", "#FFD700"),

            "retroarch" or "mgba" or "twiilight" or "_nds" or "luma-updater"
                => ("APPS", "#FFD700"),

            // PERSONAL STORAGE
            "cias" or "roms" or "themes" or "splashes" or "badges" or "screenshots" or "dcim"
                => ("STORAGE", "#00FA9A"),

            "music" or "videos" or "files9" or "install"
                => ("SAFE", "#00FA9A"),

            // TRASH
            "system volume information" or ".trashes" or ".ds_store" or "thumbs.db" or "desktop.ini"
                => ("TRASH", "#666666"),

            // IDK
            _ => ("idk :(", "#00ffea")
        };
    }
}