// Taken from: https://github.com/fbeltrao/ConfigMapFileProvider. What a saviour!

using System.Security.Cryptography;
using Microsoft.Extensions.Primitives;
using Timer = System.Threading.Timer;

namespace Kurrent.Utils;

public class ConfigMapFileProviderChangeToken: IChangeToken, IDisposable
{
    List<CallbackRegistration>? _registeredCallbacks;
    private readonly string _rootPath;
    private readonly string _filter;
    private readonly int _detectChangeIntervalMs;
    private Timer? _timer;
    private bool _hasChanged;
    private string? _lastChecksum;
    readonly object _timerLock = new object();

    class CallbackRegistration : IDisposable
    {
        private Action<object>? _callback;
        private object? _state;
        private Action<CallbackRegistration> _unregister;


        public CallbackRegistration(
            Action<object> callback, 
            object state, 
            Action<CallbackRegistration> unregister)
        {
            _callback = callback;
            _state = state;
            _unregister = unregister;
        }

        public void Notify()
        {
            var localState = _state;
            var localCallback = _callback;
            if (localState != null && localCallback != null)
            {
                localCallback.Invoke(localState);
            }
        }


        public void Dispose()
        {
            Action<CallbackRegistration> localUnregister = Interlocked.Exchange(ref _unregister, null);
            if (localUnregister != null)
            {
                localUnregister(this);
                _callback = null;
                _state = null;
            }
        }
    }

    public ConfigMapFileProviderChangeToken(string rootPath, string filter, int detectChangeIntervalMs = 30_000)
    {
        _registeredCallbacks = new List<CallbackRegistration>();
        _rootPath = rootPath;
        _filter = filter;
        _detectChangeIntervalMs = detectChangeIntervalMs;
    }

    internal void EnsureStarted()
    {
        lock (_timerLock)
        {
            if (_timer == null)
            {
                var fullPath = Path.Combine(_rootPath, _filter);
                if (File.Exists(fullPath))
                {
                    _timer = new Timer(CheckForChanges);
                    _timer.Change(0, _detectChangeIntervalMs);
                }
            }
        }
    }

    private void CheckForChanges(object? state)
    {
        var fullPath = Path.Combine(_rootPath, _filter);

        var newCheckSum = GetFileChecksum(fullPath);
        var newHasChangesValue = false;
        if (_lastChecksum != null && _lastChecksum != newCheckSum)
        {
            // changed
            NotifyChanges();

            newHasChangesValue = true;
        }

        _hasChanged = newHasChangesValue;

        _lastChecksum = newCheckSum;
        
    }

    private void NotifyChanges()
    {
        var localRegisteredCallbacks = _registeredCallbacks;
        if (localRegisteredCallbacks != null)
        {
            var count = localRegisteredCallbacks.Count;
            for (int i = 0; i < count; i++)
            {
                localRegisteredCallbacks[i].Notify();
            }
        }
    }

    string GetFileChecksum(string filename)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filename);
        return BitConverter.ToString(md5.ComputeHash(stream));
    }

    public bool HasChanged => _hasChanged;

    public bool ActiveChangeCallbacks => true;

    public IDisposable RegisterChangeCallback(Action<object> callback, object state)
    {
        var localRegisteredCallbacks = _registeredCallbacks;
        if (localRegisteredCallbacks == null)
            throw new ObjectDisposedException(nameof(_registeredCallbacks));

        var cbRegistration = new CallbackRegistration(callback, state, (cb) => localRegisteredCallbacks.Remove(cb));
        localRegisteredCallbacks.Add(cbRegistration);

        return cbRegistration;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _registeredCallbacks, null);

        Timer? localTimer;
        lock (_timerLock)
        {
            localTimer = Interlocked.Exchange(ref _timer, null);
        }

        localTimer?.Dispose();
    }
}
