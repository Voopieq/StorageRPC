using System.Runtime.CompilerServices;

namespace Services;

/// <summary>
/// Contains information about file.
/// </summary>
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

/// <summary>
/// Contains information about cleaner.
/// </summary>
public class CleanerData
{
    /// <summary>
    /// Cleaner's unique ID.
    /// </summary>
    public string Id;

    /// <summary>
    /// Cleaner's status, whether he is done cleaning or not.
    /// </summary>
    public bool IsDoneCleaning = true;
}


public interface IStorageService
{
    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    bool AddToCleanersList(CleanerData cleaner);

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
    /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
    /// <returns>True if file has been removed. False otherwise.</returns>
    bool TryRemoveOldestFile(string cleanerID);

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    bool GetCleanerState(string cleanerID);

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    int GetFileCount();

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    bool IsCleaningMode();
}

