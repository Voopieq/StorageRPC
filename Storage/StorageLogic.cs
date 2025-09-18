namespace Servers;

using NLog;

using Services;

public class StorageState
{
    public readonly object AccessLock = new();
    public int StorageCapacity = 200; // Total storage size, MB
    public int CurrentSize = 0;   // How much space occupied
    public bool IsCleaningMode = false;
    public List<CleanerData> Cleaners = new List<CleanerData>(); // Cleaners. Key = cleanerID, Value = bool (done cleaning or not)
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

    public void AddToCleanersList(CleanerData cleaner)
    {
        lock (_state.AccessLock)
        {
            _state.Cleaners.Add(cleaner);
        }
    }

    public bool RemoveCleaner(CleanerData cleaner)
    {
        lock (_state.AccessLock)
        {
            if (_state.Cleaners.Remove(cleaner))
            {
                return true;
            }
            return false;
        }
    }

    public bool GetCleanerState(string cleanerID)
    {
        lock (_state.AccessLock)
        {
            return _state.Cleaners.Find(cleaner => cleaner.Id == cleanerID).IsDoneCleaning;
        }
    }

    public void ChangeCleanerState(string cleanerID, bool state)
    {
        lock (_state.AccessLock)
        {
            CleanerData cleaner = _state.Cleaners.Find(cleaner => cleaner.Id == cleanerID);
            if (cleaner != null)
            {
                cleaner.IsDoneCleaning = state;
            }
        }
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

    public bool IsCleaningMode()
    {
        lock (_state.AccessLock)
        {
            return _state.IsCleaningMode;
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

    public bool TryGetOldestFile()
    {
        lock (_state.AccessLock)
        {
            if (_state.FilesList.Count <= 0)
            {
                _log.Error("There are no files in the storage to return!");
                return false;
            }

            // TODO: Maybe delete the file here?
            FileDesc file = _state.FilesList[0];
            _state.FilesList.Remove(file);
            _state.CurrentSize -= file.FileSize;
            _log.Info("New storage size: " + _state.CurrentSize);
            return true;
        }
    }

    public bool DeleteFile(FileDesc file)
    {
        lock (_state.AccessLock)
        {
            if (file is null)
            {
                _log.Error("Trying to delete a null file!");
                return false;
            }

            if (_state.FilesList.RemoveAll(f => f.FileName == file.FileName) == 0)
            {
                _log.Error("File doesn't exist in storage!");
                return false;
            }

            _state.CurrentSize -= file.FileSize;
            return true;
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
            Thread.Sleep(1500);

            //_log.Info("Sleeping some more");


            lock (_state.AccessLock)
            {
                if (_state.IsCleaningMode)
                {
                    // Storage is still not empty. Continue
                    if (_state.CurrentSize == 0 || !_state.Cleaners.TrueForAll(cleaner => cleaner.IsDoneCleaning))
                    {
                        // Storage is empty. Change state
                        _log.Info("Cleaning finished. Switching off cleaning mode.");
                        _state.IsCleaningMode = false;
                        continue;
                    }
                }

                if (_state.CurrentSize <= _state.StorageCapacity)
                {
                    counter = 0;
                    _state.IsCleaningMode = false;
                    //_log.Info("Storage is not full");
                    continue;
                }

                counter++;
                _log.Warn($"Storage is full! ({counter}/3");

                if (counter >= 3)
                {
                    // Activate cleaners
                    _state.IsCleaningMode = true;
                    _log.Info("Storage is in cleaning mode...");
                    // if (_state.CurrentSize <= 0)
                    // {
                    //     _log.Info("Storage has been fully cleaned!");
                    //     _state.IsCleaningMode = false;
                    // }
                }
            }
        }
    }
}