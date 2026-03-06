using Avalonia.Controls;
using Avalonia.Interactivity;
using FurnitureApp.ViewModels;

namespace FurnitureApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenAddProductWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        var addProductViewModel = new AddProductWindowViewModel();
        await addProductViewModel.LoadDataAsync();
        
        var addProductWindow = new AddProductWindow
        {
            DataContext = addProductViewModel
        };
        
        await addProductWindow.ShowDialog(this);

        if (addProductWindow.IsSaved && DataContext is MainWindowViewModel mainWindowViewModel)
        {
            await mainWindowViewModel.ReloadProductsAsync();
        }
    }
}