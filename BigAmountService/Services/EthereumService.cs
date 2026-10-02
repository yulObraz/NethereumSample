using BigAmountService.Interfaces;
using BigAmountService.Models;
using Microsoft.Extensions.Options;
using Nethereum.BlockchainProcessing;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Web3;
using System.Collections.Concurrent;
using System.Numerics;

namespace BigAmountService.Services;

/// <summary>
/// Реализация сервиса для взаимодействия с Ethereum-сетью (Sepolia)
/// </summary>
public class EthereumService : IEthereumService
{
    private readonly string _contractAddress;
    private readonly IBlockProgressRepository _blockProgressRepository;
    private readonly IWeb3 _web3;
    private readonly ILogger<EthereumService> _log;
    private readonly BigInteger _bigSumTreshhold;

    public EthereumService(IOptions<BigAmountSettings> settings, IWeb3 web3, IBlockProgressRepository blockProgressRepository, ILogger<EthereumService> log)
    {
        _web3 = web3;
        var treshhold = settings.Value.BigSumTreshhold ?? throw new ArgumentNullException(nameof(settings.Value.BigSumTreshhold));
        _bigSumTreshhold = new BigInteger(treshhold);
        _contractAddress = settings.Value.ContractAddress;
        _blockProgressRepository = blockProgressRepository;
        _log = log;
    }

    public async Task<long?> GetBlockNumberAsync()
    {
        var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        return (long?)blockNumber?.Value;
    }
    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            _ = await GetBlockNumberAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<Dictionary<string, TokenMetadata>> GetMetadataForArrayAsync(string[] contractAddresses)
    {
        //string cleanAddress = AddressUtil.Current.ConvertToChecksumAddress(rawAddress);

        var tokenRequests = contractAddresses
            .Select(addr => new
            {
                Address = addr,
                NameCall = new MulticallInputOutput<NameFunction, NameOutputDTO>(new NameFunction(), addr),
                SymbolCall = new MulticallInputOutput<SymbolFunction, SymbolOutputDTO>(new SymbolFunction(), addr),
                DecimalsCall = new MulticallInputOutput<DecimalsFunction, DecimalsOutputDTO>(new DecimalsFunction(), addr)
            })
            .ToList();

        var allCalls = tokenRequests
            .SelectMany(t => new IMulticallInputOutput[] { t.NameCall, t.SymbolCall, t.DecimalsCall })
            .ToArray();

        try
        {
            var multiQueryHandler = _web3.Eth.GetMultiQueryHandler();
            await multiQueryHandler.MultiCallAsync(allCalls);

            return tokenRequests.ToDictionary(
                t => t.Address,
                t => new TokenMetadata
                {
                    Address = t.Address,
                    Name = t.NameCall.Output?.Name ?? "Unknown Token",
                    Symbol = t.SymbolCall.Output?.Symbol ?? "UNK",
                    Decimals = t.DecimalsCall.Output?.Decimals ?? 18
                }
            );
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Ошибка при выполнении MultiCall: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BlockchainProcessor> GetProcessorTransferBigSumAsync(ConcurrentQueue<TransferEvent> transferEvents)
    {
        var addresses = string.IsNullOrEmpty(_contractAddress) ? null : new[] { _contractAddress };

        var logProcessor = _web3.Processing.Logs.CreateProcessorForContracts<TransferEventDTO>(
            addresses,
            async (logEvent) =>
            {
                _log.LogInformation("Получено событие");

                var ev = new TransferEvent
                {
                    From = logEvent.Event.From,
                    To = logEvent.Event.To,
                    Amount = logEvent.Event.Value,
                    BlockNumber = (long)logEvent.Log.BlockNumber.Value,
                    TransactionHash = logEvent.Log.TransactionHash,
                    LogIndex = (int)logEvent.Log.LogIndex.Value,
                    BlockHash = logEvent.Log.BlockHash,
                    ContractAddress = logEvent.Log.Address
                };
                transferEvents.Enqueue(ev);
                _log.LogInformation("Обработан {@TransferEvent}", ev);
            },
            blockProgressRepository: _blockProgressRepository,
            criteria: logEvent => logEvent.Event.Value > _bigSumTreshhold,
            log: _log
            );
        return logProcessor;
    }
}
