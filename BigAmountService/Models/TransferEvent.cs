using System.Numerics;

namespace BigAmountService.Models;

/// <summary>
/// Бизнес-объект события передачи средств (Transfer) из смарт-контракта ERC-20
/// </summary>
public class TransferEvent
{
    /// <summary>
    /// Адрес отправителя (от кого)
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Адрес получателя (кому)
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Сумма перевода (в наименьшей единице токена, например, wei для ETH или satoshi для BTC)
    /// </summary>
    public BigInteger Amount { get; set; }

    /// <summary>
    /// Номер блока, в котором было зафиксировано событие
    /// </summary>
    public long BlockNumber { get; set; }

    /// <summary>
    /// Хэш транзакции, в которой произошло событие
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;

    /// <summary>
    /// Индекс события в транзакции
    /// </summary>
    public int LogIndex { get; set; }

    /// <summary>
    /// Хэш блока, в котором было зафиксировано событие
    /// </summary>
    public string BlockHash { get; set; } = string.Empty;

    /// <summary>
    /// Адрес смарт-контракта токена, который сгенерировал это событие
    /// </summary>
    public string ContractAddress { get; set; } = string.Empty;
}