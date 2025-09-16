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

    public bool TrySendFile(FileDesc file)
    {
        return _storageLogic.TrySendFile(file);
    }
}