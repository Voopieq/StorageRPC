namespace Servers;

using System.Net;

using NLog;

using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;

using Services;
using NLog.Targets;

public class Server
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

    public static void Main(string[] args)
    {
        var self = new Server();
        self.Run(args);
    }

    private void Run(string[] args)
    {
        ConfigureLogging();

        _log.Info("Server is starting...");

        StartServer(args);
    }

    private void StartServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.Listen(IPAddress.Loopback, 5000);
        });

        builder.Services.AddSimpleRpcServer(new HttpServerTransportOptions { Path = "/filestoragerpc" }).AddSimpleRpcHyperionSerializer();

        builder.Services.AddSingleton<IStorageService>(new StorageService());

        var app = builder.Build();

        app.UseSimpleRpcServer();

        app.Run();
    }
}