namespace Servers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Service.
/// </summary>
[Route("/storage")]
[ApiController]
public class StorageController : ControllerBase
{
    /// <summary>
    /// Service logic. This is created in Server.StartServer() and received through DI in constructor.
    /// </summary>
    private readonly StorageLogic _storageLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logic">Logic to use. This will get passed through DI.</param>
    public StorageController(StorageLogic logic)
    {
        _storageLogic = logic;
    }

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    [HttpGet("/getFileCount")]
    public ActionResult<int> GetFileCount()
    {
        return _storageLogic.GetFileCount();
    }

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor.</returns>
    [HttpPost("/tryGetFile")]
    public ActionResult<FileDesc> TryGetFile(int fileNumber)
    {
        return _storageLogic.TryGetFile(fileNumber);
    }

    // /// <summary>
    // /// Gets the oldest file from storage.
    // /// </summary>
    // /// <returns>Oldest file.</returns>
    // [HttpGet("/tryRemoveOldestFile")]
    // public ActionResult<bool> TryRemoveOldestFile()
    // {
    //     return _storageLogic.TryRemoveOldestFile();
    // }

    /// <summary>
    /// Gets the oldest file from storage.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
    /// <returns>True if file has been removed. False otherwise.</returns>
    [HttpPost("/tryRemoveOldestFile")]
    public ActionResult<bool> TryRemoveOldestFile(string cleanerID)
    {
        return _storageLogic.TryRemoveOldestFile(cleanerID);
    }

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    [HttpPost("/getCleanerState")]
    public ActionResult<bool> GetCleanerState(string cleanerID)
    {
        return _storageLogic.GetCleanerState(cleanerID);
    }

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    [HttpPost("/trySendFile")]
    public ActionResult<bool> TrySendFile(FileDesc file)
    {
        return _storageLogic.TrySendFile(file);
    }

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    [HttpGet("/isCleaningMode")]
    public ActionResult<bool> IsCleaningMode()
    {
        return _storageLogic.IsCleaningMode();
    }

    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    [HttpPost("/addToCleanersList")]
    public ActionResult<bool> AddToCleanersList(CleanerData cleaner)
    {
        return _storageLogic.AddToCleanersList(cleaner);
    }
}