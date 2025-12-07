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

        public override async Task<BoolMsg> GetCleanerState(StringMsg request, ServerCallContext context)
        {
            var state = await _restClient.GetCleanerStateAsync(request.Value);
            return new BoolMsg { Value = state };
        }

        public override async Task<IntMsg> GetFileCount(Empty request, ServerCallContext context)
        {
            var count = await _restClient.GetFileCountAsync();
            return new IntMsg { Value = count };
        }

        public override async Task<FileDesc> TryGetFile(IntMsg request, ServerCallContext context)
        {
            var file = await _restClient.TryGetFileAsync(request.Value);
            return file ?? new FileDesc();
        }

        public override async Task<BoolMsg> TrySendFile(FileDesc request, ServerCallContext context)
        {
            var success = await _restClient.TrySendFileAsync(request);
            return new BoolMsg { Value = success };
        }

        public override async Task<BoolMsg> TryRemoveOldestFile(StringMsg request, ServerCallContext context)
        {
            var success = await _restClient.TryRemoveOldestFileAsync(request.Value);
            return new BoolMsg { Value = success };
        }

        public override async Task<BoolMsg> AddToCleanersList(CleanerData request, ServerCallContext context)
        {
            var success = await _restClient.AddToCleanersListAsync(request);
            return new BoolMsg { Value = success };
        }

        public override async Task<BoolMsg> IsCleaningMode(Empty request, ServerCallContext context)
        {
            var mode = await _restClient.IsCleaningModeAsync();
            return new BoolMsg { Value = mode };
        }
    }
}
