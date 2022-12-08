using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using CDS.Core;
using CDS.InstrumentModel;
using ChroZen.CDS.App.Contracts.PlugIns;
using ChroZen.CDS.App.Core.Services;

namespace ChroZen.CDS.App.Services;
public class DeviceProvider : IDeviceProvider
{
    private Dictionary<Type, DeviceDriver> Manifest 
    {
        get; init;
    }

    public IEnumerable<DeviceDriver> AvailableDevices => Manifest.Values;

    private Assembly? GetAssembly(string assembly)
    {
        var dll = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => Path.Equals(a.Location, assembly));
        if (dll == null)
        {
            PluginLoadContext pluginLoadContext = new PluginLoadContext(assembly);
            return pluginLoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(assembly)));
        }
        return dll;
    }

    public void Configure(string assembly)
    {
        var dll = GetAssembly(assembly);

        if (dll != null)
        {
            foreach (var type in dll.ExportedTypes)
            {
                if (type.IsSubclassOf(typeof(Device)))
                {
                    if (Activator.CreateInstance(type, new object[] { null, "" }) is Device device)
                        Manifest[type] = new DeviceDriver(assembly, device.Model,
                                                            device.CreateReferInstance("Maker") as string,
                                                            device.CreateReferInstance("Author") as string,
                                                            dll.GetName().Version?.ToString(),
                                                            type.FullName);
                }
            }
        }
    }

    public Device? Get(Instrument instrument, DeviceDriver drv, string? name)
    {
        var dll = GetAssembly(drv.Location);
        if (dll != null)
        {
            var type = dll.GetType(drv.Type);
            if (type != null)
                return Activator.CreateInstance(type, new object[] { instrument, name }) as Device;
        }
        return null;
    }

    public DeviceProvider()
    {
        Manifest = new();
    }
}
