using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FurnitureApp.ViewModels;

namespace FurnitureApp.Views;

public partial class EditProductWindow : Window
{
    public bool IsSaved { get; private set; }
    
    public EditProductWindow()
    {
        InitializeComponent();
    }

    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if(DataContext is not EditProductWindowViewModel viewModel)
            return;

        var result = await viewModel.SaveAsync();

        if (result)
        {
            IsSaved = true;
            Close();
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}