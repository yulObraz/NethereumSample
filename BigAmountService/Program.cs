using BigAmountService.Interfaces;
using BigAmountService.Models;
using BigAmountService.Services;
using Microsoft.Extensions.Options;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Nethereum.Web3;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting application");
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Регистрация настроек
            builder.Services.Configure<BigAmountSettings>(builder.Configuration.GetSection("BigAmountSettings"));
            builder.Services.AddScoped<IWeb3>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BigAmountSettings>>().Value;
                var rpcUrl = options.EthereumRpcUrl ?? throw new ArgumentNullException(nameof(options.EthereumRpcUrl));
                return new Web3(rpcUrl);
            });

            builder.Services.AddScoped<IEthereumService, EthereumService>();
            builder.Services.AddSingleton<IBlockProgressRepository, InMemoryBlockchainProgressRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}