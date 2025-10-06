namespace Cleaner;

using Microsoft.Extensions.DependencyInjection;

using NLog;
using Services;
using NLog.Targets;
using Grpc.Net.Client;


class Cleaner
{
    /// <summary>
    /// Logger for this class
    /// </summary>
    Logger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// New empty instance of Cleaner data.
    /// </summary>
    CleanerData cleanerData = new CleanerData();

    /// <summary>
    ///  Has the user already cleaned this cycle?
    /// </summary>
    bool hasCleanedThisCycle = false;

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

    /// <summary>
    /// Program body.
    /// </summary>
    private void Run()
    {
        ConfigureLogging();

        _log.Info("It's a new day to start cleaning bits!\n");

        // Random number to use for sleeping.
        Random rng = new Random();

        // Run everything in a loop to recover from connection errors
        while (true)
        {
            try
            {
                //connect to the server, get service proxy
                var channel = GrpcChannel.ForAddress("http://127.0.0.1:5000");
                var storageService = new Storage.StorageClient(channel);

                // Initialize cleaner data.
                cleanerData = new CleanerData();
                // Generate unique ID.
                cleanerData.Id = Guid.NewGuid().ToString();
                // Add cleaner to the list in server.
                storageService.AddToCleanersList(cleanerData);

                // Cleaner stuff
                while (true)
                {
                    Thread.Sleep(1000);

                    // If storage is not in cleaning mode, reset cleaner status.
                    if (!storageService.IsCleaningMode(new Empty()).Value)
                    {
                        // Reset cleaner's local status.
                        hasCleanedThisCycle = false;

                        _log.Info("Nothing to clean right now.");
                        continue;
                    }

                    // Cleaner is doing nothing and hasn't cleaned this cycle
                    if (storageService.GetCleanerState(new StringMsg { Value = cleanerData.Id }).Value && !hasCleanedThisCycle)
                    {
                        // Do the cleaning
                        Thread.Sleep(rng.Next(1500));
                        _log.Info("Retrieving a file from storage and deleting it...");

                        // Try to remove oldest file.
                        if (storageService.TryRemoveOldestFile(new StringMsg { Value = cleanerData.Id }).Value)
                        {
                            _log.Info("File successfully deleted!\n");
                        }
                        else
                        {
                            _log.Error("File has already been deleted!. Resuming work.\n");
                        }
                        hasCleanedThisCycle = true;
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
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        Cleaner self = new Cleaner();
        self.Run();
    }
}