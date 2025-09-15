namespace Servers;

using Services;

public class StorageService : IStorageService
{
    private readonly StorageLogic _storageLogic = new StorageLogic();

    public int GetFileCount()
    {
        throw new NotImplementedException();
        // return _storageLogic.GetFileCount();
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

    public FileDesc TryGetFile()
    {
        throw new NotImplementedException();
    }

    public bool TrySendFile(FileDesc file)
    {
        throw new NotImplementedException();
    }
}