namespace Servers;

using NLog;

using Services;

public class StorageState
{
    public readonly object AccessLock = new();
    public int StorageCapacity = 100; // Total storage size, MB
    public int CurrentSize = 0;   // How much space occupied
    public bool IsCleaningMode = false;
    public List<CleanerData> Cleaners = new List<CleanerData>(); // Cleaners. Key = cleanerID, Value = bool (done cleaning or not)
    public List<FileDesc> FilesList = new List<FileDesc>();
}

/// <summary>
/// Storage logic.
/// </summary>
class StorageLogic
{
    /// <summary>
    /// Logger for this class.
    /// </summary>
    private Logger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Background task thread.
    /// </summary>
    private Thread _bgTaskThread;

    /// <summary>
    /// State descriptor.
    /// </summary>
    private StorageState _state = new StorageState();

    /// <summary>
    /// Constructor.
    /// </summary>
    public StorageLogic()
    {
        // CHECK PERIODICALLY IN THIS THREAD IF STORAGE IS FULL.
        _bgTaskThread = new Thread(BackgroundTask);
        _bgTaskThread.Start();
    }

    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    public void AddToCleanersList(CleanerData cleaner)
    {
        lock (_state.AccessLock)
        {
            _state.Cleaners.Add(cleaner);
        }
    }

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    public bool GetCleanerState(string cleanerID)
    {
        lock (_state.AccessLock)
        {
            return _state.Cleaners.Find(cleaner => cleaner.Id == cleanerID).IsDoneCleaning;
        }
    }

    /// <summary>
    /// Changes cleaner state IsDoneCleaning to 'state' parameter.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <param name="state">State to put the cleaner in. True to make it done cleaning.</param>
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

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    public int GetFileCount()
    {
        lock (_state.AccessLock)
        {
            return _state.FilesList.Count;
        }
    }

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    public bool IsCleaningMode()
    {
        lock (_state.AccessLock)
        {
            return _state.IsCleaningMode;
        }
    }

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    public bool TrySendFile(FileDesc file)
    {
        lock (_state.AccessLock)
        {
            // Storage is full. Display an error.
            if (_state.CurrentSize > _state.StorageCapacity)
            {
                _log.Error("Storage is full. Cannot receive file!\n");
                return false;
            }

            // Storage is not full. We can store the file.
            _state.FilesList.Add(file);
            _state.CurrentSize += file.FileSize;

            _log.Info("File added to storage! Total storage size: " + _state.CurrentSize + "/" + _state.StorageCapacity + "MB\n");
            return true;
        }
    }

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor.</returns>
    public FileDesc? TryGetFile(int idx)
    {
        lock (_state.AccessLock)
        {
            FileDesc? file = _state.FilesList.ElementAtOrDefault(idx);
            if (file != null)
            {
                _state.FilesList.Remove(file);
                _state.CurrentSize -= file.FileSize;
                _log.Info($"Found a file with index {idx}. Passing it to client and removing from storage. New storage size: {_state.CurrentSize}.\n");
                return file;
            }

            // File doesn't exist. Return an empty object and an error.
            _log.Error($"File with index {idx} doesn't exist in the storage!\n");
            return null;
        }
    }

    /// <summary>
    /// Gets the oldest file from storage.
    /// </summary>
    /// <returns>Oldest file.</returns>
    public bool TryRemoveOldestFile()
    {
        lock (_state.AccessLock)
        {
            if (_state.FilesList.Count <= 0)
            {
                _log.Error("There are no files in the storage to return!\n");
                return false;
            }

            FileDesc file = _state.FilesList[0];
            _state.FilesList.Remove(file);
            _state.CurrentSize -= file.FileSize;
            _log.Info("Oldest file removed. New storage size: " + _state.CurrentSize);
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
            Thread.Sleep(1000);

            lock (_state.AccessLock)
            {
                if (_state.IsCleaningMode)
                {
                    if (_state.CurrentSize == 0 || _state.Cleaners.TrueForAll(cleaner => cleaner.IsDoneCleaning))
                    {
                        // Storage is empty, or all cleaners are finished. Reset the state
                        _log.Info("Cleaning finished. Switching off cleaning mode.\n");
                        _state.IsCleaningMode = false;
                        counter = 0;
                    }
                    continue;
                }

                // Storage is not full. Continue the loop.
                if (_state.CurrentSize <= _state.StorageCapacity)
                {
                    counter = 0;
                    _state.IsCleaningMode = false;
                    continue;
                }

                // Storage is full
                counter++;
                _log.Warn($"Storage is full! ({counter}/3)");

                if (counter == 3)
                {
                    // Activate cleaners
                    _state.IsCleaningMode = true;
                    _log.Info("Activating storage cleaning mode...");
                }
            }
        }
    }
}