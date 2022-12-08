using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CDS.Core;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.PlugIns;
using ChroZen.CDS.App.Contracts.Services;
using ChroZen.CDS.App.Contracts.ViewModels;
using ChroZen.CDS.App.Services;
using ChroZen.CDS.App.Views;
using ChroZen.CDS.App.Views.Dialog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChroZen.CDS.App.ViewModels;

public class ObservableDevice : ObservableObject
{
    public Device Device
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
    public SignalSet Signal
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

    public string? Channel => string.Join("-", Signal.Device?.Name, Signal.Channel);

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
        AddDevice = new RelayCommand<XamlRoot>(AddDeviceExecute);
        DeleteDevice = new RelayCommand<IList<object>>(DeleteDeviceExecute, DeleteDeviceCanExecute);
        AddSignal = new RelayCommand<XamlRoot>(AddSignalExecute);

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

    public ICommand DeleteDevice
    {
        get;
    }

    private void DeleteDeviceExecute(IList<object>? selectedItems)
    {
        if (selectedItems != null)
        {
            foreach (var device in selectedItems.Cast<ObservableDevice>().ToArray())
            {
                if (Devices.Remove(device))
                {
                    foreach (var signal in Signals.Where(s => s.Signal.Device == device.Device).ToArray())
                    {
                        if (Signals.Remove(signal))
                        {
                            signal.Signal.Delete();
                        }
                    }
                    device.Device.Delete();
                }
            }
            PopulateSignalIndexes();
        }

    }

    private bool DeleteDeviceCanExecute(IList<object>? seletedItems)
    {
        return seletedItems?.Any() ?? false;
    }

    public void DeviceListSelectionChanged() => ((RelayCommand<IList<object>>)DeleteDevice).NotifyCanExecuteChanged();


    public ICommand AddDevice
    {
        get;
    }

    private async void AddDeviceExecute(XamlRoot? root)
    {
        if (Instrument != null)
        {
            var dialog = new AddDeviceDialog();
            dialog.XamlRoot = root;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (dialog.DriverList.SelectedItem is DeviceDriver drv)
                {
                    var device = dialog.Provider.Get(Instrument, drv, dialog.DeviceName.Text);
                    if(device != null)
                        Devices.Add(new ObservableDevice(device));
                }
            }
        }
    }

    public ICommand AddSignal
    {
        get;
    }

    private async void AddSignalExecute(XamlRoot? root)
    {
        if (Instrument != null)
        {
            var dialog = new AddSignalDialog(Instrument);
            dialog.XamlRoot = root;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (dialog.ChannelList.SelectedItem is PairedDeviceChannel dc)
                {
                    var signal = new SignalSet(dc.Device, dc.Channel.Name, dialog.SignalName.Text);
                    signal.Index = Signals.Count;
                    Signals.Add(new ObservableSignal(signal));
                }
            }
        }
    }

    private void PopulateSignalIndexes()
    {
        foreach (var si in Signals.Select((s, i) => (i, s.Signal)))
        {
            si.Signal.Index = si.i;
        }
    }
}
