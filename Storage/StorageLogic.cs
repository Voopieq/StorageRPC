namespace Servers;

using NLog;

using Services;

class StorageLogic
{
    private Logger _log = LogManager.GetCurrentClassLogger();
    private Thread _bgTaskThread;
    private readonly object AccessLock = new();


    private int _storageCapacity = 5000; // Total storage size
    private int _currentSize = 0;   // How much space occupied
    private Dictionary<string, int> _filesDict = new Dictionary<string, int>(); // Actual files. Key = file name, Value = file size.

    public StorageLogic()
    {
        //_bgTaskThread = new Thread(BackgroundTask)
    }

    public int GetFileCount()
    {
        lock (AccessLock)
        {
            return _filesDict.Count;
        }
    }

    public bool IsStorageFull()
    {
        lock (AccessLock)
        {
            return _currentSize < _storageCapacity;
        }
    }
}