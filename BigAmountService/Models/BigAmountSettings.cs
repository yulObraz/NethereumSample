namespace BigAmountService.Models;

public class BigAmountSettings
{
    /// <summary>
    /// URL RPC узла сети Ethereum
    /// </summary>
    public string EthereumRpcUrl { get; set; } = string.Empty;

    /// <summary>
    /// Адрес ERC-20 контракта для фильтра
    /// </summary>
    public string ContractAddress { get; set; } = string.Empty;
    /// <summary>
    /// Сумма для фильтра
    /// </summary>
    public long? BigSumTreshhold { get; set; }
}