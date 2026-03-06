using Avalonia.Controls;
using Avalonia.Interactivity;
using FurnitureApp.DTOs;
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

    private async void DeleteProduct_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        
        if(button.Tag is not ProductCardItem product)
            return;
        
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        await viewModel.DeleteProductAsync(product.Id);
    }

    private async void EditProduct_OnClick(object? sender, RoutedEventArgs e)
    {
        if(sender is not Button button)
            return;
        
        if (button.Tag is not ProductCardItem product)
            return;
        
        var editProductViewModel = new EditProductWindowViewModel(product.Id);
        await editProductViewModel.LoadDataAsync();

        var editProductWindow = new EditProductWindow
        {
            DataContext = editProductViewModel
        };
        
        await editProductWindow.ShowDialog(this);

        if (editProductWindow.IsSaved && DataContext is MainWindowViewModel mainWindowViewModel)
        {
            await mainWindowViewModel.ReloadProductsAsync();
        }
    }
}