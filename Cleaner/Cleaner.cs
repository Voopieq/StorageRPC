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

    private void Run()
    {
        ConfigureLogging();

        _log.Info("It's a new day to start cleaning bits!\n");

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

                    // If storage is not in cleaning mode, reset cleaner status
                    if (!storageService.IsCleaningMode())
                    {
                        hasCleanedThisCycle = false;
                        if (!storageService.GetCleanerState(cleanerData.Id))
                        {
                            // Reset cleaner state if storage stopped cleaning
                            storageService.ChangeCleanerState(cleanerData.Id, true);
                            _log.Info($"Cleaner {cleanerData.Id} reset.\n");
                        }
                        continue;
                    }

                    if (storageService.GetCleanerState(cleanerData.Id) && !hasCleanedThisCycle)
                    {
                        // Do the cleaning
                        storageService.ChangeCleanerState(cleanerData.Id, false);
                        Thread.Sleep(rng.Next(1500));
                        _log.Info("Retrieving a file from storage and deleting it...");

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

    public static void Main(string[] args)
    {
        Cleaner self = new Cleaner();
        self.Run();
    }
}