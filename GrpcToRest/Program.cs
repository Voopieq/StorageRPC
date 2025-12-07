namespace Servers;

using System.Net;

using NLog;

using GrpcToRestAdapter.Services;

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
        _log.Info("GRPC to REST Adapter is starting...");

        StartServer(args);
    }

    private void StartServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // gRPC and HTTP client
        builder.Services.AddGrpc();
        builder.Services.AddHttpClient<StorageRestClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000/"); // REST server
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5003, o =>
            {
                o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // <-- important
            });
        });

        var app = builder.Build();
        app.MapGrpcService<StorageGrpcService>();

        app.Run();

    }
}
