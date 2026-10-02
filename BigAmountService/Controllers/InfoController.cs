using Microsoft.AspNetCore.Mvc;
using Nethereum.BlockchainProcessing.ProgressRepositories;

namespace BigAmountService.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    private readonly IBlockProgressRepository _blockProgressRepository;
    public InfoController(IBlockProgressRepository blockProgressRepository)
    {
        _blockProgressRepository = blockProgressRepository;
    }

    [HttpGet("[action]")]
    public async Task<long?> CurrentBlockAsync()
    {
        return (long?)await _blockProgressRepository.GetLastBlockNumberProcessedAsync();
    }
}
