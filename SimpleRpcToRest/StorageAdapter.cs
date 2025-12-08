
using System.Net.Http.Json;
using Services;

namespace Servers
{
    public class StorageAdapter : IStorageService
    {
        private readonly HttpClient _http;

        public StorageAdapter(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Gets total file count in the storage.
        /// </summary>
        /// <returns>File count in storage.</returns>
        public int GetFileCount()
        {
            return _http.GetFromJsonAsync<int>("getFileCount").GetAwaiter().GetResult();
        }

        /// <summary>
        /// Allows to request a file to be received from the server.
        /// </summary>
        /// <returns>File descriptor.</returns>
        public FileDesc TryGetFile(int fileNumber)
        {
            return _http.GetFromJsonAsync<FileDesc>($"tryGetFile?fileNumber={fileNumber}")
                        .GetAwaiter()
                        .GetResult();
        }

        /// <summary>
        /// Gets the oldest file from storage.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID who wants to clean.</param>
        /// <returns>True if file has been removed. False otherwise.</returns>
        public bool TryRemoveOldestFile(string cleanerID)
        {
            var response = _http.PostAsync($"tryRemoveOldestFile?cleanerID={cleanerID}", null)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Tells if cleaner is done cleaning or not.
        /// </summary>
        /// <param name="cleanerID">Cleaner's ID.</param>
        /// <returns>True, if cleaner is done cleaning. False otherwise.</returns>
        public bool GetCleanerState(string cleanerID)
        {
            return _http.GetFromJsonAsync<bool>($"getCleanerState?cleanerID={cleanerID}")
                        .GetAwaiter()
                        .GetResult();
        }

        /// <summary>
        /// Allows to send the file to storage if there is enough space.
        /// </summary>
        /// <param name="file">File to store.</param>
        /// <returns>True if file successfully stored, false otherwise.</returns>
        public bool TrySendFile(FileDesc file)
        {
            var response = _http.PostAsJsonAsync("trySendFile", file)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Tells if cleaning mode has been activated
        /// </summary>
        /// <returns>True if cleaning mode is active. False otherwise.</returns>
        public bool IsCleaningMode()
        {
            return _http.GetFromJsonAsync<bool>("isCleaningMode")
                        .GetAwaiter()
                        .GetResult();
        }

        /// <summary>
        /// Add a cleaner to the list.
        /// </summary>
        /// <param name="cleaner">Cleaner to add.</param>
        /// <returns>True if added successfully. False otherwise.</returns>
        public bool AddToCleanersList(CleanerData cleaner)
        {
            var response = _http.PostAsJsonAsync("addToCleanersList", cleaner)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }
    }
}
