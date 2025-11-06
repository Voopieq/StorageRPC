namespace Clients;

using NLog;

using Services;

class Client
{
    /// <summary>
    /// Enum to determine if the user wants to download, or upload the file.
    /// </summary>
    private enum OperationType
    {
        Download,
        Upload
    }

    /// <summary>
    /// Logger for this class
    /// </summary>
    Logger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Enum to keep track of current operation type.
    /// </summary>
    OperationType operationType;

    /// <summary>
    /// Configure Logging subsystem
    /// </summary>
    private void ConfigureLogging()
    {
        var config = new NLog.Config.LoggingConfiguration();

        var console = new NLog.Targets.ConsoleTarget("console")
        {
            Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
        };
        config.AddTarget(console);
        config.AddRuleForAllLevels(console);

        LogManager.Configuration = config;
    }

    /// <summary>
    /// Program body.
    /// </summary>
    private void Run()
    {
        ConfigureLogging();

        // Does the user want to download or upload the file. 0 for download, 1 for upload.
        Random rng = new Random();

        // Run everything in a loop to recover from connection errors
        while (true)
        {
            try
            {
                //connect to the server, get service client proxy
                var storage = new();

                // Connection to the server
                sc.AddSimpleRpcClient("storageService", new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/filestoragerpc",
                    Serializer = "HyperionMessageSerializer"
                }).AddSimpleRpcHyperionSerializer();

                sc.AddSimpleRpcProxy<IStorageService>("storageService");

                var sp = sc.BuildServiceProvider();

                var storageService = sp.GetService<IStorageService>();

                // Initialize file descriptor.
                FileDesc file = new FileDesc();

                // User cycle
                while (true)
                {
                    Thread.Sleep(2000 + rng.Next(1000));

                    // Storage is in cleaning mode. Pause the user.
                    if (storageService.IsCleaningMode())
                    {
                        _log.Warn("Storage is in cleaning mode. Waiting for it to finish...\n");
                        continue;
                    }

                    // Determine what to do: upload or download.
                    operationType = (OperationType)rng.Next(0, 2);
                    _log.Info("I decided to " + operationType + " the file.");

                    switch (operationType)
                    {
                        case OperationType.Upload:
                            // Generate file info
                            int fileSize = rng.Next(20, 50);
                            string fileName = Guid.NewGuid().ToString();

                            file.FileName = fileName;
                            file.FileSize = fileSize;

                            // Try to upload the file.
                            if (!storageService.TrySendFile(file))
                            {
                                _log.Warn("Can't upload the file. Storage is full!\n");
                            }
                            else
                            {
                                _log.Info("File uploaded successfully!\n");
                            }
                            break;
                        case OperationType.Download:
                            // Generate file number
                            int fileCount = storageService.GetFileCount();
                            int rngFileNumber = rng.Next(fileCount);

                            // Try to download the file.
                            if (storageService.TryGetFile(rngFileNumber) is null)
                            {
                                // File does not exist
                                _log.Warn($"File with index {rngFileNumber} doesn't exist!\n");
                            }
                            else
                            {
                                _log.Info($"File with index {rngFileNumber} downloaded successfully!\n");
                            }
                            break;
                        default:
                            _log.Error("Operation type doesn't exist! Type: " + operationType);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Warn(e, "Unhandled exception caught. Will restart main loop...");

                Thread.Sleep(2000);
            }
        }
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var self = new Client();
        self.Run();
    }
}
