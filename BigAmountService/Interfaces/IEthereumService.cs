using BigAmountService.Models;
using Nethereum.BlockchainProcessing;
using System.Collections.Concurrent;

namespace BigAmountService.Interfaces;

/// <summary>
/// Взаимодействие с Ethereum-сетью
/// </summary>
public interface IEthereumService
{
    /// <summary>
    /// Получает текущий номер блока в сети Ethereum
    /// </summary>
    Task<long?> GetBlockNumberAsync();
    /// <summary>
    /// Получение данных о контракте
    /// </summary>
    /// <param name="contractAddresses">Проведите очистку адреса перед вызовом</param>
    /// <returns></returns>
    Task<Dictionary<string, TokenMetadata>> GetMetadataForArrayAsync(string[] contractAddresses);
    /// <summary>
    /// Получение процессора для обработки транзакций
    /// </summary>
    /// <param name="transferEvents"></param>
    /// <returns></returns>
    Task<BlockchainProcessor> GetProcessorTransferBigSumAsync(ConcurrentQueue<TransferEvent> transferEvents);
    /// <summary>
    /// Проверяет соединение с сетью Ethereum
    /// </summary>
    Task<bool> IsConnectedAsync();
}


