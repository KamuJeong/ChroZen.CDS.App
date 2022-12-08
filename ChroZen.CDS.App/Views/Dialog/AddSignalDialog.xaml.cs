using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CDS.Core;
using CDS.InstrumentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ChroZen.CDS.App.Views.Dialog;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
/// 

public class PairedDeviceChannel
{
    public Device Device 
    { 
        get; 
    }

    public DeviceChannel Channel
    {
        get;
    }

    public PairedDeviceChannel(Device device, DeviceChannel channel)
    {
        Device = device;
        Channel = channel;
    }

    public override string ToString() => $"{Device.Name} - {Channel.Name}";
}


public sealed partial class AddSignalDialog : ContentDialog
{
    private Instrument Instrument
    {
        get;
    }

    public AddSignalDialog(Instrument instrument)
    {
        Instrument = instrument;
        this.InitializeComponent();
    }

    private bool IsNotNull(object? o) => o != null;

    private IEnumerable<PairedDeviceChannel> AvailableChannels
        => Instrument.Devices
            .SelectMany(d => d.CreateReferInstance("Channels") as DeviceChannel[] ?? Array.Empty<DeviceChannel>(),
                             (d, c) => new PairedDeviceChannel(d, c))
            .Where(dc => dc.Device.FindChildren<SignalSet>(null).All(s => string.Equals(s.Channel, dc.Channel.Name)));
}
