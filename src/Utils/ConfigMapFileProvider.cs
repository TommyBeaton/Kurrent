// Taken from: https://github.com/fbeltrao/ConfigMapFileProvider. What a saviour!

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Lighthouse.Utils;

/// <summary>
/// Simple <see cref="IFileProvider"/> implementation using config maps as source
/// Config maps volumes in Linux/Kubernetes are implemented as symlink files.
/// Once reloaded their Last modified date does not change. This implementation uses a check sum to verify
/// </summary>
public class ConfigMapFileProvider : IFileProvider
{
    readonly ConcurrentDictionary<string, ConfigMapFileProviderChangeToken> _watchers;

    public static IFileProvider? FromRelativePath(string subPath)
    {
        var executableLocation = Assembly.GetEntryAssembly().Location;
        var executablePath = Path.GetDirectoryName(executableLocation);
        var configPath = Path.Combine(executablePath, subPath);
        if (Directory.Exists(configPath))
        {
            return new ConfigMapFileProvider(configPath);
        }

        return null;
    }

    public ConfigMapFileProvider(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Invalid root path", nameof(rootPath));
        }

        RootPath = rootPath;
        _watchers = new ConcurrentDictionary<string, ConfigMapFileProviderChangeToken>();
    }

    public string RootPath { get; }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return new PhysicalDirectoryContents(Path.Combine(RootPath, subpath));
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var fi = new FileInfo(Path.Combine(RootPath, subpath));
        return new PhysicalFileInfo(fi);
    }

    public IChangeToken Watch(string filter)
    {
        var watcher = _watchers.AddOrUpdate(filter, 
            addValueFactory: (f) =>
            {
                return new ConfigMapFileProviderChangeToken(RootPath, filter);
            },
            updateValueFactory: (f, e) =>
            {
                e.Dispose();
                return new ConfigMapFileProviderChangeToken(RootPath, filter);
            });

        watcher.EnsureStarted();
        return watcher;
    }
}
