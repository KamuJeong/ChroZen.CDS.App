using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CDS.Core;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using OxyPlot;
using OxyPlot.Axes;

namespace ChroZen.CDS.App.ViewModels;

public class DeviceCollectionItem
{
    public Device Device
    {
        get; init;
    }
    public UIElement? Monitor
    {
        get; init;
    }

    public DeviceCollectionItem(Device device)
    {
        Device = device;
        Monitor = (UIElement?)device.CreateReferInstance("Monitor");
    }
}

public class InstrumentViewModel : ObservableRecipient
{
    public InstrumentViewModel(INavigationService navigationService, Instrument model)
    {
        NavigationService = navigationService;

        Instrument = model;
        Devices = new ObservableCollection<DeviceCollectionItem>
                        (Instrument.Devices.Select(d => new DeviceCollectionItem(d)));

        Settings = new RelayCommand(SettingsExcute, SettingsCanExecute);
    }

    public Instrument Instrument
    {
        get;
    }

    private INavigationService NavigationService
    {
        get;
    }

    public ObservableCollection<DeviceCollectionItem> Devices
    {
        get; init;
    }

    public PlotModel PlotModel
    {
        get; private set;
    } = new PlotModel
    {
        Title = "Signal",
        PlotAreaBorderColor = OxyColors.Transparent,
        Axes =
        {
            new LinearAxis {Position = AxisPosition.Bottom},
            new LinearAxis {Position = AxisPosition.Left},
        }
    };

    public ICommand Settings
    {
        get;
    }

    private void SettingsExcute()
    {
        NavigationService.NavigateTo(typeof(InstrumentSettingsViewModel).FullName!, Instrument);
    }

    private bool SettingsCanExecute()
    {
        return true;
    }
}
