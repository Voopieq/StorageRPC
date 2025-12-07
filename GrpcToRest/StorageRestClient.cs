using System.Net.Http.Json;
using Services;

namespace GrpcToRestAdapter.Services
{
    public class StorageRestClient
    {
        private readonly HttpClient _http;

        public StorageRestClient(HttpClient http)
        {
            _http = http;
        }

        public Task<int> GetFileCountAsync() =>
            _http.GetFromJsonAsync<int>("getFileCount")!;

        public Task<FileDesc?> TryGetFileAsync(int fileNumber) =>
            _http.GetFromJsonAsync<FileDesc?>($"tryGetFile?fileNumber={fileNumber}");

        public async Task<bool> TrySendFileAsync(FileDesc file)
        {
            var res = await _http.PostAsJsonAsync("trySendFile", file);
            return res.IsSuccessStatusCode;
        }

        public Task<bool> TryRemoveOldestFileAsync(string cleanerID) =>
            _http.PostAsync($"tryRemoveOldestFile?cleanerID={cleanerID}", null)
                 .ContinueWith(t => t.Result.IsSuccessStatusCode);

        public Task<bool> GetCleanerStateAsync(string cleanerID) =>
            _http.GetFromJsonAsync<bool>($"getCleanerState?cleanerID={cleanerID}");

        public async Task<bool> AddToCleanersListAsync(CleanerData cleaner)
        {
            var res = await _http.PostAsJsonAsync("addToCleanersList", cleaner);
            return res.IsSuccessStatusCode;
        }

        public Task<bool> IsCleaningModeAsync() =>
            _http.GetFromJsonAsync<bool>("isCleaningMode")!;
    }
}
