using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CDS.Core;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.Services;
using ChroZen.CDS.App.OxyPlot.Axes;
using ChroZen.CDS.App.OxyPlot.Legends;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

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


    public DeviceCollectionItem([NotNull] Device device)
    {
        Device = device;
        Monitor = (UIElement?)device.CreateReferInstance("Monitor");
    }
}


public class InstrumentViewModel : ObservableRecipient
{
    public InstrumentViewModel(INavigationService navigationService, [NotNull] Instrument model)
    {
        NavigationService = navigationService;

        Instrument = model;
        new WeakEventSubscriber<Instrument, InstrumentViewModel, SignalUpdatedArgs>(
            model,
            nameof(model.SignalUpdated),
            this,
            (sub, s, e) => sub.OnSignalUpdated(e));

        new WeakEventSubscriber<Instrument, InstrumentViewModel, InstrumentStatusChangedArgs>(
            model,
            nameof(model.StatusChanged),
            this,
            (sub, s, e) =>
            {
                sub.Connect.NotifyCanExecuteChanged();
                sub.Disconnect.NotifyCanExecuteChanged();
                sub.Settings.NotifyCanExecuteChanged();
            });

        Devices = new ObservableCollection<DeviceCollectionItem>
                        (Instrument.Devices.Select(d => new DeviceCollectionItem(d)));

        Connect = new AsyncRelayCommand(ConnectExcuteAsync, ConnectCanExecute);
        Disconnect = new RelayCommand(DisconnectExcute, DisconnectCanExcute);
        Settings = new RelayCommand(SettingsExcute, SettingsCanExecute);

        PopulatePlotModesSeries();
    }

    private void OnSignalUpdated(SignalUpdatedArgs e)
    {
        //try
        //{
        //    if (plotModel.Series.SingleOrDefault(s => s.Title == e.Source.Name) == null)
        //    {
        //        plotModel.Series.Add(
        //            new LineSeries
        //            {
        //                Title = e.Source.Name,
        //                YAxisKey = e.Source.Type.ToString(),
        //                ItemsSource = e.Source.Points,
        //                Mapping = SignalMappingHandler
        //            });
        //    }
        //}
        //catch
        //{
        //    foreach (var series in plotModel.Series.Where(s => s.Title == e.Source.Name).ToArray())
        //        plotModel.Series.Remove(series);
        //}

        plotModel.GetAxis("X").AbsoluteMinimum = 0;
        plotModel.GetAxis("X").AbsoluteMaximum = Instrument.SignalWindow.TotalMinutes;

        plotModel.InvalidatePlot(true);
    }

    private void PopulatePlotModesSeries()
    {
        PlotModel.Series.Clear();

        foreach (var sig in Instrument.Signals)
        {
            var lineSeries = new LineSeries
            {
                Title = $"{sig.Index+1}. {sig.Name}",
                YAxisKey = sig.Type.ToString(),
                ItemsSource = sig.Points,
                Mapping = SignalMappingHandler
            };
            plotModel.Series.Add(lineSeries);

            if (ActiveSignal == -1)
                ActiveSignal = 0;
        }
    }


    private DataPoint SignalMappingHandler(object raw)
    {
        DateTime zero = Instrument.State.Status != InstrumentStatus.Run ?
                            DateTime.Now - Instrument.SignalWindow : Instrument.State.LastRunTime;

        if (raw is SignalPoint point)
        {
            return new DataPoint((point.Time - zero).TotalMinutes, point.Value);
        }

        throw new ArgumentException($"{nameof(raw)} is not a SignalPoint");
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

    private readonly PlotModel plotModel = new()
    {
        Axes =
        {
            new LinearAxis2
            {
                Key = "X",
                Title = "Time",
                Unit = "min",
                MaximumPadding = 0,
                MinimumPadding = 0,
                Position = AxisPosition.Bottom
            },
            new LinearAxis2
            {
                Key = DeviceChannelType.Chromatogram.ToString(),
                Title = "Volt",
                Unit = "mV",
                Position = AxisPosition.Left
            },
            new LinearAxis2
            {
                Key = DeviceChannelType.Pressure.ToString(),
                Title = "Pressure",
                Unit = "psi",
                IsAxisVisible = false,
                Position = AxisPosition.Right
            },
            new LinearAxis2
            {
                Key = DeviceChannelType.Flow.ToString(),
                Title = "Flow",
                Unit = "mL/min",
                IsAxisVisible = false,
                Position = AxisPosition.Right
            },
            new LinearAxis2
            {
                Key = DeviceChannelType.Temperature.ToString(),
                Title = "Temperature",
                Unit = "℃",
                IsAxisVisible = false,
                Position = AxisPosition.Right
            }
        }
        ,
        Legends =
        {
            new Legend2
            {
                LegendPosition = LegendPosition.LeftTop,
            }
        }
    };


    public PlotModel PlotModel => plotModel;

    private int activeSignal = -1;
    private int ActiveSignal
    {
        get => activeSignal;
        set
        {
            if (value != activeSignal)
            {
                SetProperty(ref activeSignal, value);
                if (Instrument.Signals.FirstOrDefault(s => s.Index == value) is SignalSet sig)
                {
                    foreach (var axis in plotModel.Axes)
                    {
                        if (axis.Key == "Chromatogram" || axis.Key == "X")
                            continue;

                        switch (sig.Type)
                        {
                            case DeviceChannelType.Pressure:
                                axis.IsAxisVisible = axis.Key == "Pressure";
                                break;
                            case DeviceChannelType.Flow:
                                axis.IsAxisVisible = axis.Key == "Flow";
                                break;
                            case DeviceChannelType.Temperature:
                                axis.IsAxisVisible = axis.Key == "Temperature";
                                break;
                        }
                    }
                }
                plotModel.InvalidatePlot(false);
            }
        }
    }

    public IRelayCommand Connect
    {
        get;
    }

    private async Task ConnectExcuteAsync()
    {
        await Instrument.ConnectAsync();
    }

    private bool ConnectCanExecute()
    {
        return Instrument.State.Status == InstrumentStatus.None;
    }


    public IRelayCommand Disconnect
    {
        get;
    }

    private void DisconnectExcute()
    {
        Instrument.Disconnect();
    }

    private bool DisconnectCanExcute() => !ConnectCanExecute();


    public IRelayCommand Settings
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
