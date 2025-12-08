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

        /// <summary>
        /// Gets total file count in the storage.
        /// </summary>
        /// <returns>File count in storage.</returns>
        public Task<int> GetFileCountAsync() =>
            _http.GetFromJsonAsync<int>("getFileCount")!;

        /// <summary>
        /// Allows to request a file to be received from the server.
        /// </summary>
        /// <returns>File descriptor.</returns>
        public Task<FileDesc?> TryGetFileAsync(int fileNumber) =>
            _http.GetFromJsonAsync<FileDesc?>($"tryGetFile?fileNumber={fileNumber}");

        /// <summary>
        /// Allows to send the file to storage if there is enough space.
        /// </summary>
        /// <param name="file">File to store.</param>
        /// <returns>True if file successfully stored, false otherwise.</returns>
        public async Task<bool> TrySendFileAsync(FileDesc file)
        {
            var res = await _http.PostAsJsonAsync("trySendFile", file);
            return res.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the oldest file from storage.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
        /// <returns>True if file has been removed. False otherwise.</returns>
        public Task<bool> TryRemoveOldestFileAsync(string cleanerID) =>
            _http.PostAsync($"tryRemoveOldestFile?cleanerID={cleanerID}", null)
                 .ContinueWith(t => t.Result.IsSuccessStatusCode);

        /// <summary>
        /// Tells if cleaner is done cleaning or not.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID.</param>
        /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
        public Task<bool> GetCleanerStateAsync(string cleanerID) =>
            _http.GetFromJsonAsync<bool>($"getCleanerState?cleanerID={cleanerID}");

        /// <summary>
        /// Add a cleaner to the list.
        /// </summary>
        /// <param name="cleaner">Cleaner to add.</param>
        /// <returns>True if added successfully. False otherwise.</returns>
        public async Task<bool> AddToCleanersListAsync(CleanerData cleaner)
        {
            var res = await _http.PostAsJsonAsync("addToCleanersList", cleaner);
            return res.IsSuccessStatusCode;
        }

        /// <summary>
        /// Tells if cleaning mode has been activated
        /// </summary>
        /// <returns>True if cleaning mode is active. False otherwise.</returns>
        public Task<bool> IsCleaningModeAsync() =>
            _http.GetFromJsonAsync<bool>("isCleaningMode")!;
    }
}
