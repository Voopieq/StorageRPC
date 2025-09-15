namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;

using NLog;
using Services;
using NLog.Targets;
using SimpleRpc.Transports.Http.Client;

class Client
{
    /// <summary>
    /// Logger for this class
    /// </summary>
    Logger _log = LogManager.GetCurrentClassLogger();

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
        var rng = new Random();

        int decision = rng.Next(0, 2);
        _log.Info("Number: " + decision);

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

                //int decision = rng.Next(0, 1);
                // User cycle
                // while (true)
                // {
                //     // TODO: determine whether to send or receive a file
                // }
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
