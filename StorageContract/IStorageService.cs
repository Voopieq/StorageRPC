namespace Services;


public class FileDesc
{
    /// <summary>
    /// File number.
    /// </summary>
    public int FileNumber { get; set; }

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// File size.
    /// </summary>
    public int FileSize { get; set; }
}

public interface IStorageService
{
    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    bool TrySendFile(FileDesc file);

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor</returns>
    FileDesc TryGetFile(int fileNumber);

    /// <summary>
    /// Gets file from the storage.
    /// </summary>
    /// <returns>File.</returns>
    FileDesc GetFile();

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    int GetFileCount();

    /// <summary>
    /// Tells if the storage is full or not.
    /// </summary>
    /// <returns>True if storage is full. False otherwise.</returns> 
    bool IsStorageFull();

    /// <summary>
    /// Tells if provided file is in storage.
    /// </summary>
    /// <param name="file">User's generated file.</param>
    /// <returns>True if file exists in storage. False otherwise.</returns>
    bool IsFileInStorage(FileDesc file);
}

