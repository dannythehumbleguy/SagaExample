using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Common;
using PaymentService.Api.Models;
using PaymentService.Api.Repositories;

namespace PaymentService.Api.Controllers;

[Route("api/[controller]")]
public class AccountController(AccountRepository accountRepository) : AbstractController
{
    [HttpGet("user/{userId:guid}")]
    public async Task<Results<Ok<AccountDto>, BadRequest<Error>>> GetAccount([FromRoute] Guid userId) => 
        await Wrap(accountRepository.GetAccount(userId));
    
    [HttpPost("balance")]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> ChangeBalance([FromBody] ChangeBalanceRequest request) => 
        await Wrap(accountRepository.ChangeBalance(request));
}