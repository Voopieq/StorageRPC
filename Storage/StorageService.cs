namespace Servers;

using Services;

public class StorageService : IStorageService
{
    private readonly StorageLogic _storageLogic = new StorageLogic();

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    public int GetFileCount()
    {
        return _storageLogic.GetFileCount();
    }

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor.</returns>
    public FileDesc? TryGetFile(int fileNumber)
    {
        return _storageLogic.TryGetFile(fileNumber);
    }


    /// <summary>
    /// Gets the oldest file from storage.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
    /// <returns>True if file has been removed. False otherwise.</returns>
    public bool TryRemoveOldestFile(string cleanerID)
    {
        return _storageLogic.TryRemoveOldestFile(cleanerID);
    }

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    public bool GetCleanerState(string cleanerID)
    {
        return _storageLogic.GetCleanerState(cleanerID);
    }

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    public bool TrySendFile(FileDesc file)
    {
        return _storageLogic.TrySendFile(file);
    }

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    public bool IsCleaningMode()
    {
        return _storageLogic.IsCleaningMode();
    }

    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    public bool AddToCleanersList(CleanerData cleaner)
    {
        return _storageLogic.AddToCleanersList(cleaner);
    }
}