using BigAmountService.Interfaces;
using BigAmountService.Models;
using BigAmountService.Test.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.Blocks;
using System.Collections.Concurrent;
using System.Numerics;

namespace BigAmountService.Test;

/// <summary>
/// Тест Ethereum публичной тестовой сети
/// </summary>
public class TestEthereumTestnet : IClassFixture<TestWebApplicationFactory>
{
    /// <summary>
    /// Актуальная на данный момент версия тестового окружения - hoodi.
    /// </summary>

    private const string TEST_URL = "https://rpc.hoodi.ethpandaops.io";
    /*
     Network Configuration DetailsChain ID: 560048Block Explorer: EthPandaOps Hoodi ExplorerPublic RPC Endpoints: Available via providers like https://rpc.hoodi.ethpandaops.io or https://hoodi.rpc.sentio.xyzFaucets for Test ETHGoogle Cloud Faucet: Google Cloud Web3 Faucet (requires a Google account)EthPandaOps Faucet: EthPandaOps Hoodi FaucetProof-of-Work Faucet: pk910 POW FaucetStakely Faucet: Stakely ETH Faucet
     */
    private readonly WebApplicationFactory<Program> _factory;
    /// 
    public TestEthereumTestnet(TestWebApplicationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            _ = builder.UseSetting($"{nameof(BigAmountSettings)}:{nameof(BigAmountSettings.EthereumRpcUrl)}", TEST_URL);
            _ = builder.UseSetting($"{nameof(BigAmountSettings)}:{nameof(BigAmountSettings.BigSumTreshhold)}", "1000");
        });
    }

    [Fact]
    public async Task CheckSettingsUrl()
    {
        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BigAmountSettings>>();

        var settings = options.Value;
        
        Assert.NotNull(settings.EthereumRpcUrl);
        Assert.Equal(TEST_URL, settings.EthereumRpcUrl);
    }

    [Fact]
    [Trait("Category", "CallEthereum")]
    public async Task IsConnectedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var ethereumService = scope.ServiceProvider.GetRequiredService<IEthereumService>();

        var isConnected = await ethereumService.IsConnectedAsync();
        Assert.True(isConnected);
    }

    [Fact]
    [Trait("Category", "CallEthereum")]
    public async Task GetBlockNumberAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var ethereumService = scope.ServiceProvider.GetRequiredService<IEthereumService>();

        var blockNumber = await ethereumService.GetBlockNumberAsync();
        Assert.True(blockNumber > 0);
    }
    [Fact]
    [Trait("Category", "CallEthereum")]
    [Trait("Random", "Запрашивает данные, которые заполняются другими пользователями случайным образом")]
    public async Task ProcessAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var ethereumService = scope.ServiceProvider.GetRequiredService<IEthereumService>();

        var blockNumber = await ethereumService.GetBlockNumberAsync();
        Assert.NotNull(blockNumber);

        var collection = new ConcurrentQueue<TransferEvent>();
        var processor = await ethereumService.GetProcessorTransferBigSumAsync(collection);
        var cancellationToken = new CancellationTokenSource(1000).Token;
        try
        {
            await processor.ExecuteAsync(cancellationToken, new BigInteger(blockNumber.Value - LastConfirmedBlockNumberService.DEFAULT_BLOCK_CONFIRMATIONS - 1));
        }
        catch (OperationCanceledException) { }
        Assert.NotEmpty(collection);
        //Assert.Equal(1, collection.Count);
        Assert.InRange(collection.Count, 1, 1000);
    }
}