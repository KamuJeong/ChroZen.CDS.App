using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDS.Core;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.Services;
using ChroZen.CDS.App.Contracts.ViewModels;
using ChroZen.CDS.App.Services;
using ChroZen.CDS.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChroZen.CDS.App.ViewModels;

public class ObservableDevice : ObservableObject
{
    private Device Device
    {
        get;
    }

    public ObservableDevice(Device device)
    {
        Device = device;
    }

    public string? Name
    {
        get => Device.Name;
        set => SetProperty(Device.Name, value, Device, (d, v) => d.Name = v);
    }

    public string? Model => Device.Model;
 
    public string? SerialNumber => Device.SerialNumber;

    public string? Maker => (string?)Device.CreateReferInstance("Maker");

    public DeviceChannel[]? Channels => (DeviceChannel[]?)Device.CreateReferInstance("Channels");
}


public class ObservableSignal : ObservableObject
{
    private SignalSet Signal
    {
        get;
    }

    public ObservableSignal(SignalSet signal)
    {
        Signal = signal;
    }

    public string? Name
    {
        get => Signal.Name;
        set => SetProperty(Signal.Name, value, Signal, (s, v) => s.Name = v);
    }

    public string? Channel => string.Join(" ", Signal.Device?.Name, Signal.Channel+1);

    public string? Unit => Signal.Unit;
}

public class InstrumentSettingsViewModel : ObservableRecipient, INavigationAware
{
    private Instrument? instrument;
    public Instrument? Instrument
    {
        get => instrument;
        set => SetProperty(ref instrument, value);
    }

    public ObservableCollection<ObservableDevice> Devices
    {
        get;
    }

    public ObservableCollection<ObservableSignal> Signals
    {
        get;
    }

    public InstrumentSettingsViewModel()
    {
        Devices = new();
        Signals = new();
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
                Devices.Add(new ObservableDevice(device));
            }

            foreach (var signal in Instrument.Signals)
            {
                Signals.Add(new ObservableSignal(signal));
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
