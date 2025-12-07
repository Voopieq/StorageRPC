
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

        public int GetFileCount()
        {
            return _http.GetFromJsonAsync<int>("getFileCount").GetAwaiter().GetResult();
        }

        public FileDesc TryGetFile(int fileNumber)
        {
            return _http.GetFromJsonAsync<FileDesc>($"tryGetFile?fileNumber={fileNumber}")
                        .GetAwaiter()
                        .GetResult();
        }

        public bool TryRemoveOldestFile(string cleanerID)
        {
            var response = _http.PostAsync($"tryRemoveOldestFile?cleanerID={cleanerID}", null)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }

        public bool GetCleanerState(string cleanerID)
        {
            return _http.GetFromJsonAsync<bool>($"getCleanerState?cleanerID={cleanerID}")
                        .GetAwaiter()
                        .GetResult();
        }

        public bool TrySendFile(FileDesc file)
        {
            var response = _http.PostAsJsonAsync("trySendFile", file)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }

        public bool IsCleaningMode()
        {
            return _http.GetFromJsonAsync<bool>("isCleaningMode")
                        .GetAwaiter()
                        .GetResult();
        }

        public bool AddToCleanersList(CleanerData cleaner)
        {
            var response = _http.PostAsJsonAsync("addToCleanersList", cleaner)
                                .GetAwaiter()
                                .GetResult();
            return response.IsSuccessStatusCode;
        }
    }
}
