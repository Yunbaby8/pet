using System.Threading;

namespace DesktopPet.App.SystemIntegration;

public sealed class SingleInstanceGuard : IDisposable
{
    private readonly string _mutexName;
    private Mutex? _mutex;
    private bool _ownsMutex;

    public SingleInstanceGuard(string mutexName)
    {
        _mutexName = mutexName;
    }

    public bool TryAcquire()
    {
        if (_mutex is not null)
        {
            return _ownsMutex;
        }

        _mutex = new Mutex(initiallyOwned: true, _mutexName, out bool createdNew);
        _ownsMutex = createdNew;

        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
        }

        return createdNew;
    }

    public void Dispose()
    {
        if (_mutex is null)
        {
            return;
        }

        if (_ownsMutex)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The process is already shutting down and no longer owns it.
            }
        }

        _mutex.Dispose();
        _mutex = null;
        _ownsMutex = false;
    }
}