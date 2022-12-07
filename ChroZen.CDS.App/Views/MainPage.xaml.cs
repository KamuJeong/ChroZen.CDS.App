using ChroZen.CDS.App.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace ChroZen.CDS.App.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
