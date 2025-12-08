using Grpc.Core;
using Services; // Your generated gRPC proto classes

namespace GrpcToRestAdapter.Services
{
    public class StorageGrpcService : Storage.StorageBase
    {
        private readonly StorageRestClient _restClient;



        public StorageGrpcService(StorageRestClient restClient)
        {
            _restClient = restClient;
        }

        /// <summary>
        /// Tells if cleaner is done cleaning or not.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID.</param>
        /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
        public override async Task<BoolMsg> GetCleanerState(StringMsg request, ServerCallContext context)
        {
            var state = await _restClient.GetCleanerStateAsync(request.Value);
            return new BoolMsg { Value = state };
        }

        /// <summary>
        /// Gets total file count in the storage.
        /// </summary>
        /// <returns>File count in storage.</returns>
        public override async Task<IntMsg> GetFileCount(Empty request, ServerCallContext context)
        {
            var count = await _restClient.GetFileCountAsync();
            return new IntMsg { Value = count };
        }

        /// <summary>
        /// Allows to request a file to be received from the server.
        /// </summary>
        /// <returns>File descriptor.</returns>
        public override async Task<FileDesc> TryGetFile(IntMsg request, ServerCallContext context)
        {
            var file = await _restClient.TryGetFileAsync(request.Value);
            return file ?? new FileDesc();
        }

        /// <summary>
        /// Allows to send the file to storage if there is enough space.
        /// </summary>
        /// <param name="file">File to store.</param>
        /// <returns>True if file successfully stored, false otherwise.</returns>
        public override async Task<BoolMsg> TrySendFile(FileDesc request, ServerCallContext context)
        {
            var success = await _restClient.TrySendFileAsync(request);
            return new BoolMsg { Value = success };
        }

        /// <summary>
        /// Gets the oldest file from storage.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
        /// <returns>True if file has been removed. False otherwise.</returns>
        public override async Task<BoolMsg> TryRemoveOldestFile(StringMsg request, ServerCallContext context)
        {
            var success = await _restClient.TryRemoveOldestFileAsync(request.Value);
            return new BoolMsg { Value = success };
        }

        /// <summary>
        /// Add a cleaner to the list.
        /// </summary>
        /// <param name="cleaner">Cleaner to add.</param>
        /// <returns>True if added successfully. False otherwise.</returns>
        public override async Task<BoolMsg> AddToCleanersList(CleanerData request, ServerCallContext context)
        {
            var success = await _restClient.AddToCleanersListAsync(request);
            return new BoolMsg { Value = success };
        }

        /// <summary>
        /// Tells if cleaning mode has been activated
        /// </summary>
        /// <returns>True if cleaning mode is active. False otherwise.</returns>
        public override async Task<BoolMsg> IsCleaningMode(Empty request, ServerCallContext context)
        {
            var mode = await _restClient.IsCleaningModeAsync();
            return new BoolMsg { Value = mode };
        }
    }
}
