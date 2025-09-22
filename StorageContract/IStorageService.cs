using System.Runtime.CompilerServices;

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

public class CleanerData
{
    public string Id;
    public bool IsDoneCleaning = true;
}


public interface IStorageService
{
    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    void AddToCleanersList(CleanerData cleaner);

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    bool TrySendFile(FileDesc file);

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor.</returns>
    FileDesc? TryGetFile(int fileNumber);

    /// <summary>
    /// Gets the oldest file from storage.
    /// </summary>
    /// <returns>Oldest file.</returns>
    bool TryRemoveOldestFile();

    /// <summary>
    /// Deletes provided file from the storage.
    /// </summary>
    /// <param name="file">File to delete.</param>
    /// <returns>True if file has been deleted. False otherwise.</returns>
    bool DeleteFile(FileDesc file);

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    bool GetCleanerState(string cleanerID);

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

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    bool IsCleaningMode();

    /// <summary>
    /// Changes cleaner state IsDoneCleaning to 'state' parameter.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <param name="state">State to put the cleaner in. True to make it done cleaning.</param>
    void ChangeCleanerState(string cleanerID, bool state);
}

