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
    Logger _log = LogManager.GetCurrentClassLogger();

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
        _log.Info("SimpleRPC to REST Adapter is starting...");

        StartServer(args);
    }

    private void StartServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel if needed
        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.Listen(IPAddress.Loopback, 5002);
        });

        // Add SimpleRPC services
        builder.Services
            .AddSimpleRpcServer(new SimpleRpc.Transports.Http.Server.HttpServerTransportOptions
            {
                Path = "/filestoragerpc"
            })
            .AddSimpleRpcHyperionSerializer();

        builder.Services.AddSingleton<IStorageService>(provider =>
        {
            // HttpClient that talks to REST server
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
            return new StorageAdapter(httpClient);
        });

        var app = builder.Build();

        // Add SimpleRPC middleware
        app.UseSimpleRpcServer();

        // Run the adapter
        app.Run();
    }
}
