using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace ChroZen.CDS.App.Core.Services;
public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    private string _pluginDirectory;

    public PluginLoadContext(string pluginPath)
    {
        _pluginDirectory = Path.GetFullPath(Path.GetDirectoryName(pluginPath));
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (new string[] 
            { 
                "CDS.Core", 
                "CDS.InstrumentModel", 
            }.Contains(assemblyName.Name))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
 //           if (Path.GetFullPath(assemblyPath).StartsWith(_pluginDirectory, true, CultureInfo.CurrentCulture))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
