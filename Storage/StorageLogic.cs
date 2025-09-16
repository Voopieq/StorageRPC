namespace Servers;

using NLog;

using Services;

public class StorageState
{
    public readonly object AccessLock = new();
    public int StorageCapacity = 200; // Total storage size, MB
    public int CurrentSize = 0;   // How much space occupied
    public bool IsStorageFull = false;
    public Dictionary<string, int> FilesDict = new Dictionary<string, int>(); // Actual files. Key = file name, Value = file size.
    public List<FileDesc> FilesList = new List<FileDesc>();
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
            return _state.FilesList.Count;
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
            // Storage is full. Display an error.
            if (_state.CurrentSize > _state.StorageCapacity)
            {
                _log.Error("Storage is full. Cannot receive file!");
                return false;
            }

            // Storage is not full. We can store the file.
            //_state.FilesDict.Add(file.FileName, file.FileSize);
            _state.FilesList.Add(file);
            _state.CurrentSize += file.FileSize;
            _log.Info("File added to storage! Total storage size: " + _state.CurrentSize + "/" + _state.StorageCapacity + "MB");
            return true;

        }
    }

    public FileDesc? TryGetFile(int fileNumber)
    {
        lock (_state.AccessLock)
        {
            // File does not exist in the storage
            foreach (FileDesc file in _state.FilesList)
            {
                if (file.FileNumber == fileNumber)
                {
                    _log.Info("Found a file with number " + fileNumber);
                    _state.FilesList.Remove(file);
                    _state.CurrentSize -= file.FileSize;
                    return file;
                }

            }

            // File doesn't exist. Return an empty object and an error.
            _log.Error("File with number " + fileNumber + " doesn't exist in the storage!");
            return null;
        }
    }

    /// <summary>
    /// Periodically checks if storage is full
    /// </summary>
    public void BackgroundTask()
    {
        Random rng = new Random();
        int counter = 0;

        while (true)
        {
            Thread.Sleep(500 + rng.Next(1500));

            lock (_state.AccessLock)
            {
                if (_state.CurrentSize > _state.StorageCapacity)
                {
                    while (true)
                    {
                        counter++;
                        _log.Warn("Storage is full! (" + counter + "/3");
                        if (counter == 3)
                        {
                            // Activate cleaners
                            _log.Info("Activating cleaners!");
                            return;
                        }
                    }
                }
                counter = 0;
            }
        }
    }
}