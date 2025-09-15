namespace Services;


public class FileDesc()
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
    /// Tells if the storage is full or not.
    /// </summary>
    /// <returns>True if storage is full. False otherwise.</returns>
    // TODO: Should 
    public bool IsStorageFull();

    /// <summary>
    /// Tells if provided file is in storage.
    /// </summary>
    /// <param name="file">User's generated file.</param>
    /// <returns>True if file exists in storage. False otherwise.</returns>
    public bool IsFileInStorage(FileDesc file);

    /// <summary>
    /// Gets total files count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    public int GetFileCount();

    /// <summary>
    /// Gets file from the storage.
    /// </summary>
    /// <returns>File.</returns>
    public FileDesc GetFile();

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    public bool TrySendFile(FileDesc file);

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor</returns>
    public FileDesc TryGetFile();
}

