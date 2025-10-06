namespace Servers;

using System.Net;

using NLog;

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

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var self = new Server();
        self.Run(args);
    }

    /// <summary>
    /// Program body.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private void Run(string[] args)
    {
        ConfigureLogging();

        _log.Info("Server is starting...");

        StartServer(args);
    }

    /// <summary>
    /// Starts integrated server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private void StartServer(string[] args)
    {
        //create web app builder
        var builder = WebApplication.CreateBuilder(args);

        //configure integrated server
        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.Listen(IPAddress.Loopback, 5000);
        });

        //add support for GRPC services
        builder.Services.AddGrpc();

        //add the actual services
        builder.Services.AddSingleton(new StorageService());

        //build the server
        var app = builder.Build();

        //turn on request routing
        app.UseRouting();

        //configure routes
        app.MapGrpcService<StorageService>();

        //run the server
        app.Run();
    }
}