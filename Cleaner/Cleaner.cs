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

    CleanerData cleanerData = new CleanerData();

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

        _log.Info("It's a new day to start cleaning bits!");

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

                cleanerData = new CleanerData();
                cleanerData.Id = Guid.NewGuid().ToString();

                storageService.AddToCleanersList(cleanerData);

                // Cleaner stuff
                while (true)
                {
                    Thread.Sleep(1000);

                    // TODO: If storage is empty, reset status.
                    if (!storageService.IsCleaningMode())
                    {
                        if (storageService.GetCleanerState(cleanerData.Id))
                        {
                            // Reset cleaner state if storage stopped cleaning
                            storageService.ChangeCleanerState(cleanerData.Id, false);
                            _log.Info($"Cleaner {cleanerData.Id} reset (no cleaning mode).");
                        }
                        continue;
                    }

                    // Check if cleaning mode has been activated
                    _log.Info("Cleaner is cleaning: " + storageService.GetCleanerState(cleanerData.Id));
                    if (!storageService.IsCleaningMode()) { _log.Info("Nothing to clean."); continue; }

                    // Do the cleaning
                    Thread.Sleep(rng.Next(1500));
                    _log.Info("Retrieving a file from storage and deleting it...");
                    storageService.ChangeCleanerState(cleanerData.Id, false);

                    if (storageService.TryGetOldestFile())
                    {
                        _log.Info("File successfully deleted!");
                        storageService.ChangeCleanerState(cleanerData.Id, true);
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
        Cleaner self = new Cleaner();
        self.Run();
    }
}