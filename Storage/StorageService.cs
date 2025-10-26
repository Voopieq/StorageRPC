namespace Servers;

using Grpc.Core;

using Services;

public class StorageService : Services.Storage.StorageBase
{
    private readonly StorageLogic _storageLogic = new StorageLogic();

    /// <summary>
    /// Gets total file count in the storage.
    /// </summary>
    /// <returns>File count in storage.</returns>
    public override Task<IntMsg> GetFileCount(Empty input, ServerCallContext context)
    {
        var result = new IntMsg { Value = _storageLogic.GetFileCount() };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Allows to request a file to be received from the server.
    /// </summary>
    /// <returns>File descriptor.</returns>
    public override Task<Services.FileDesc> TryGetFile(IntMsg input, ServerCallContext context)
    {
        var file = _storageLogic.TryGetFile(input.Value);
        var result = new Services.FileDesc
        {
            FileName = file.FileName,
            FileSize = file.FileSize
        };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Gets the oldest file from storage.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
    /// <returns>True if file has been removed. False otherwise.</returns>
    public override Task<BoolMsg> TryRemoveOldestFile(StringMsg input, ServerCallContext context)
    {
        var result = new BoolMsg { Value = _storageLogic.TryRemoveOldestFile(input.Value) };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Tells if cleaner is done cleaning or not.
    /// </summary>
    /// <param name="cleanerID">Cleaner's ID.</param>
    /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
    public override Task<BoolMsg> GetCleanerState(StringMsg input, ServerCallContext context)
    {
        var result = new BoolMsg { Value = _storageLogic.GetCleanerState(input.Value) };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Allows to send the file to storage if there is enough space.
    /// </summary>
    /// <param name="file">File to store.</param>
    /// <returns>True if file successfully stored, false otherwise.</returns>
    public override Task<BoolMsg> TrySendFile(Services.FileDesc input, ServerCallContext context)
    {
        //convert input to the format expected by logic
        var file = new FileDesc
        {
            FileName = input.FileName,
            FileSize = input.FileSize
        };

        var logicResult = _storageLogic.TrySendFile(file);
        var result = new BoolMsg { Value = logicResult };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Tells if cleaning mode has been activated
    /// </summary>
    /// <returns>True if cleaning mode is active. False otherwise.</returns>
    public override Task<BoolMsg> IsCleaningMode(Empty input, ServerCallContext context)
    {
        var result = new BoolMsg { Value = _storageLogic.IsCleaningMode() };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Add a cleaner to the list.
    /// </summary>
    /// <param name="cleaner">Cleaner to add.</param>
    /// <returns>True if added successfully. False otherwise.</returns>
    public override Task<BoolMsg> AddToCleanersList(Services.CleanerData cleaner, ServerCallContext context)
    {
        var cleanerData = new CleanerData
        {
            Id = cleaner.Id,
            IsDoneCleaning = cleaner.IsDoneCleaning
        };

        var logicResult = _storageLogic.AddToCleanersList(cleanerData);
        var result = new BoolMsg { Value = logicResult };
        return Task.FromResult(result);
    }
}