using CommunityToolkit.Mvvm.ComponentModel;

namespace CTRExploreSD.ViewModels;

public partial class FtpItem : ObservableObject
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public long Size { get; set; }

    public string Risk { get; set; } = "";
    public string Color { get; set; } = "Gray";

    public string Icon => IsDirectory ? "📁" : "📄";
    public string SizeDisplay => IsDirectory ? "" : $"{Size / 1024} KB";
}