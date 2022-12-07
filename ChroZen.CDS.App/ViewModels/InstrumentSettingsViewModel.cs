using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.Services;
using ChroZen.CDS.App.Contracts.ViewModels;
using ChroZen.CDS.App.Services;
using ChroZen.CDS.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChroZen.CDS.App.ViewModels;

public class ObserableDevice : ObservableObject
{
    private Device Device
    {
        get;
    }

    public ObserableDevice(Device device)
    {
        Device = device;
    }

    public string? Name
    {
        get => Device.Name;
        set => SetProperty(Device.Name, value, Device, (d, v) => d.Name = v);
    }

    public string? Model
    {
        get => Device.Model;
        set => SetProperty(Device.Model, value, Device, (d, v) => d.Model = v);
    }

    public string? SerialNumber => Device.SerialNumber;
}

public class InstrumentSettingsViewModel : ObservableRecipient, INavigationAware
{
    private Instrument? instrument;
    public Instrument? Instrument
    {
        get => instrument;
        set => SetProperty(ref instrument, value);
    }

    public ObservableCollection<ObserableDevice> Devices
    {
        get;
    }

    public InstrumentSettingsViewModel()
    {
        Devices = new ObservableCollection<ObserableDevice>();
    }

    public void OnNavigatedFrom()
    {
    
    }

    public void OnNavigatedTo(object parameter)
    {
        Devices.Clear();

        if (parameter is Instrument inst)
        {
            Instrument = inst;

            foreach (var device in Instrument.Devices)
            {
                Devices.Add(new ObserableDevice(device));
            }
        }
    }

    public string? Name
    {
        get => Instrument?.Name;
        set
        {
            if (Instrument != null)
            {
                SetProperty(Instrument.Name, value, Instrument, (i, v) => i.Name = v);
            }
        }
    }
}
