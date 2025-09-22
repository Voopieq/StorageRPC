namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;

using NLog;
using Services;
using NLog.Targets;
using SimpleRpc.Transports.Http.Client;
using System.Data.SqlTypes;

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

    OperationType operationType;

    /// <summary>
    /// Configure Logging subsystem
    /// </summary>
    private void ConfigureLogging()
    {
        var config = new NLog.Config.LoggingConfiguration();

        var console = new ConsoleTarget("console")
        {
            Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
        };
        config.AddTarget(console);
        config.AddRuleForAllLevels(console);

        LogManager.Configuration = config;
    }

    private void Run()
    {
        ConfigureLogging();

        // Does the user want to download or upload the file. Use 0 for download, 1 for upload.
        Random rng = new Random();

        while (true)
        {
            try
            {
                var sc = new ServiceCollection();

                sc.AddSimpleRpcClient("storageService", new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/filestoragerpc",
                    Serializer = "HyperionMessageSerializer"
                }).AddSimpleRpcHyperionSerializer();

                sc.AddSimpleRpcProxy<IStorageService>("storageService");

                var sp = sc.BuildServiceProvider();

                var storageService = sp.GetService<IStorageService>();

                FileDesc file = new FileDesc();

                // User cycle
                while (true)
                {
                    Thread.Sleep(2000 + rng.Next(1500));

                    if (storageService.IsCleaningMode())
                    {
                        _log.Warn("Storage is in cleaning mode. Waiting for it to finish...");
                        continue;
                    }

                    operationType = (OperationType)rng.Next(1, 1);
                    _log.Info("I decided to " + operationType + " the file.");

                    switch (operationType)
                    {
                        case OperationType.Upload:
                            // Generate file info
                            int fileSize = rng.Next(300, 300);
                            string fileName = Guid.NewGuid().ToString();

                            file.FileName = fileName;
                            file.FileSize = fileSize;

                            // Storage is full, or is in cleaning mode
                            if (!storageService.TrySendFile(file))
                            {
                                _log.Warn("Storage is full!");
                            }
                            break;
                        case OperationType.Download:
                            // Generate file number
                            int fileCount = storageService.GetFileCount();
                            int rngFileNumber = rng.Next(0, fileCount);

                            if (storageService.TryGetFile(rngFileNumber) is null)
                            {
                                // File does not exist
                                _log.Warn("File with number " + rngFileNumber + " doesn't exist!");
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


    public static void Main(string[] args)
    {
        var self = new Client();
        self.Run();
    }
}
