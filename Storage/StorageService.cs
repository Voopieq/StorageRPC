namespace Servers;

using Services;

public class StorageService : IStorageService
{
    private readonly StorageLogic _storageLogic = new StorageLogic();

    public int GetFileCount()
    {
        return _storageLogic.GetFileCount();
    }

    public FileDesc GetFile()
    {
        throw new NotImplementedException();
    }

    public bool IsFileInStorage(FileDesc file)
    {
        throw new NotImplementedException();
    }

    public bool IsStorageFull()
    {
        throw new NotImplementedException();
    }

    public FileDesc? TryGetFile(int fileNumber)
    {
        return _storageLogic.TryGetFile(fileNumber);
    }

    public bool TryGetOldestFile()
    {
        return _storageLogic.TryGetOldestFile();
    }

    public bool DeleteFile(FileDesc file)
    {
        return _storageLogic.DeleteFile(file);
    }

    public bool GetCleanerState(string cleanerID)
    {
        return _storageLogic.GetCleanerState(cleanerID);
    }

    public bool TrySendFile(FileDesc file)
    {
        return _storageLogic.TrySendFile(file);
    }

    public bool IsCleaningMode()
    {
        return _storageLogic.IsCleaningMode();
    }

    public void AddToCleanersList(CleanerData cleaner)
    {
        _storageLogic.AddToCleanersList(cleaner);
    }

    public void ChangeCleanerState(string cleanerID, bool state)
    {
        _storageLogic.ChangeCleanerState(cleanerID, state);
    }
}