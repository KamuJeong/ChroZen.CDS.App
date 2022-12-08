using ChroZen.CDS.App.Activation;
using ChroZen.CDS.App.Contracts.Services;
using ChroZen.CDS.App.Core.Contracts.Services;
using ChroZen.CDS.App.Core.Services;
using ChroZen.CDS.App.Helpers;
using ChroZen.CDS.App.Models;
using ChroZen.CDS.App.Services;
using ChroZen.CDS.App.ViewModels;
using ChroZen.CDS.App.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using CDS.InstrumentModel;
using System.Reflection;
using CDS.Core;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Runtime.Loader;
using ChroZen.CDS.App.Contracts.PlugIns;

namespace ChroZen.CDS.App;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IDeviceProvider, DeviceProvider>();
            services.AddSingleton<Instrument>(new Instrument(null, "HPLC"));

            // Views and ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<InstrumentViewModel>();
            services.AddTransient<InstrumentSettingsViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);

        InitializeDeviceProvier();
        InitializeInstrumentModel();

    }

    private void InitializeDeviceProvier()
    {
        var provider = App.GetService<IDeviceProvider>();
        provider.Configure(typeof(global::CDS.Chromass.ChroZenPump.ChroZenPumpDevice).Assembly.Location);
    }

    private void InitializeInstrumentModel()
    {
        var instrument = App.GetService<Instrument>();
        new global::CDS.Chromass.ChroZenPump.ChroZenPumpDevice(instrument, "ChroZen Pump 1");
        new global::CDS.Chromass.ChroZenPump.ChroZenPumpDevice(instrument, "ChroZen Pump 2");
    }
}
