namespace BigAmountService.Models;

public class TokenMetadata
{
    public string Address { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public byte Decimals { get; set; }
}