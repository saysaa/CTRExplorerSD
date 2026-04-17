using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CTRExploreSD.ViewModels;
using System;

namespace CTRExploreSD.Views;

public partial class FtpExplorerWindow : Window
{
    public FtpExplorerWindow()
    {
        InitializeComponent();
    }

    public void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is FtpExplorerViewModel viewModel)
        {
            viewModel.OpenItemCommand.Execute(viewModel.SelectedItem);
        }
    }

    public async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FtpExplorerViewModel viewModel && viewModel.SelectedItem is FtpItem selectedItem)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            if (selectedItem.IsDirectory)
            {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Choose destination folder" });
                if (folders.Count > 0)
                {
                    await viewModel.DownloadItemAsync(selectedItem, folders[0].Path.LocalPath);
                }
            }
            else
            {
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save file as...",
                    SuggestedFileName = selectedItem.Name
                });
                if (file != null)
                {
                    await viewModel.DownloadItemAsync(selectedItem, file.Path.LocalPath);
                }
            }
        }
    }
}