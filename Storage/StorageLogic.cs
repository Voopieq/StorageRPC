namespace Servers;

using NLog;

using Services;

public class StorageState
{
    public readonly object AccessLock = new();
    public int StorageCapacity = 5000; // Total storage size
    public int CurrentSize = 0;   // How much space occupied
    public bool IsStorageFull = false;
    public Dictionary<string, int> FilesDict = new Dictionary<string, int>(); // Actual files. Key = file name, Value = file size.
}


class StorageLogic
{
    private Logger _log = LogManager.GetCurrentClassLogger();
    private Thread _bgTaskThread;
    private StorageState _state = new StorageState();

    public StorageLogic()
    {
        // CHECK PERIODICALLY IN THIS THREAD IF STORAGE IS FULL
        _bgTaskThread = new Thread(BackgroundTask);
        _bgTaskThread.Start();
    }

    public int GetFileCount()
    {
        lock (_state.AccessLock)
        {
            return _state.FilesDict.Count;
        }
    }

    public bool IsStorageFull()
    {
        lock (_state.AccessLock)
        {
            return _state.CurrentSize < _state.StorageCapacity;
        }
    }

    public bool TrySendFile(FileDesc file)
    {
        lock (_state.AccessLock)
        {
            // Storage is full. Return an error.
            if (_state.CurrentSize > _state.StorageCapacity)
            {
                _log.Error("Storage is full. Cannot send file!");
                return false;
            }

            // Storage is not full. We can store the file.
            _state.FilesDict.Add(file.FileName, file.FileSize);
            _state.CurrentSize += file.FileSize;
            _log.Info("File added to storage! Total storage size: " + _state.CurrentSize);
            return true;

        }
    }

    // public FileDesc TryGetFile(int fileNumber)
    // {
    //     lock (_state.AccessLock)
    //     {
    //         // File does not exist in the storage
    //         // if(_state.FilesDict.)
    //     }
    // }

    /// <summary>
    /// Periodically checks if storage is full
    /// </summary>
    public void BackgroundTask()
    {
        while (true)
        {
            int counter = 0;

            lock (_state.AccessLock)
            {
                if (_state.CurrentSize > _state.StorageCapacity)
                {
                    counter++;
                    if (counter == 3)
                    {
                        // Activate cleaners
                        _log.Info("Activating cleaners!");
                        return;
                    }
                }
                counter = 0;
                //_log.Info("Storage is currently not full. Checking again...");

            }
        }
    }
}