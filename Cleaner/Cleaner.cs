namespace Cleaner;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;

using NLog;
using Services;
using NLog.Targets;
using SimpleRpc.Transports.Http.Client;
using System.Data.SqlTypes;

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
                //connect to the server, get service client proxy
                var sc = new ServiceCollection();

                // Connection to the server
                sc.AddSimpleRpcClient("storageService", new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/filestoragerpc",
                    Serializer = "HyperionMessageSerializer"
                }).AddSimpleRpcHyperionSerializer();

                sc.AddSimpleRpcProxy<IStorageService>("storageService");

                var sp = sc.BuildServiceProvider();

                var storageService = sp.GetService<IStorageService>();

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
                    if (!storageService.IsCleaningMode())
                    {
                        // Reset cleaner's status.
                        hasCleanedThisCycle = false;
                        // Cleaner was not reset on the server. Do it now.
                        if (!storageService.GetCleanerState(cleanerData.Id))
                        {
                            // Reset cleaner state if storage stopped cleaning.
                            storageService.ChangeCleanerState(cleanerData.Id, true);
                            _log.Info($"Cleaner {cleanerData.Id} reset.\n");
                        }

                        _log.Info("Nothing to clean right now.");
                        continue;
                    }

                    // Cleaner is doing nothing and hasn't cleaned this cycle
                    if (storageService.GetCleanerState(cleanerData.Id) && !hasCleanedThisCycle)
                    {
                        // Do the cleaning
                        storageService.ChangeCleanerState(cleanerData.Id, false);
                        Thread.Sleep(rng.Next(1500));
                        _log.Info("Retrieving a file from storage and deleting it...");

                        // Try to remove oldest file.
                        if (storageService.TryRemoveOldestFile())
                        {
                            _log.Info("File successfully deleted!\n");
                        }
                        else
                        {
                            _log.Error("File has already been deleted!. Resuming work.\n");
                        }
                        storageService.ChangeCleanerState(cleanerData.Id, true);
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