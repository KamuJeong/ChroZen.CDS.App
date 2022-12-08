using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDS.Core;
using CDS.InstrumentModel;

namespace ChroZen.CDS.App.Contracts.PlugIns;
public class DeviceDriver
{
    public string Location
    {
        get; init;
    }
    public string? Model
    {
        get; init;
    }
    public string? Maker
    {
        get; init;
    }
    public string? Author
    {
        get; init;
    }
    public string? Version
    {
        get; init;
    }
    public string? Type
    {
        get; init;
    }

    public DeviceDriver(string location, string? model, string? maker, string? author, string? version, string? type)
    {
        Location = location;
        Model = model;
        Maker = maker;
        Author = author;
        Version = version;
        Type = type;
    }
}

public interface IDeviceProvider
{
    void Configure(string assembly);
    IEnumerable<DeviceDriver> AvailableDevices
    {
        get;
    }
    Device? Get(Instrument instrument, DeviceDriver device, string? name);
}