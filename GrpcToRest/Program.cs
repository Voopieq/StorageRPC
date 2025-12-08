namespace Servers;

using System.Net;

using NLog;

using GrpcToRestAdapter.Services;

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
    private void Run(string[] args)
    {
        ConfigureLogging();
        _log.Info("GRPC to REST Adapter is starting...");

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

        // gRPC and HTTP client
        builder.Services.AddGrpc();
        builder.Services.AddHttpClient<StorageRestClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000/"); // REST server port
        });

        //configure integrated server
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5003, o =>
            {
                o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });

        //build the server
        var app = builder.Build();

        //publish the background logic as (an internal) service through dependency injection
        app.MapGrpcService<StorageGrpcService>();

        //run the server
        app.Run();

    }
}
