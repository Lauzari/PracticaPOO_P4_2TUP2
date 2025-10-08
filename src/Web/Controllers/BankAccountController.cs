using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;
using System.Linq.Expressions;
using Web.Models;
using Core.Exceptions;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountRepository _bankAccountRepository;

    public BankAccountController(IBankAccountRepository bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
    }
[HttpPost("create")]
    public ActionResult<BankAccountDto> CreateBankAccount([FromBody] CreateBankAccountDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Owner))
            throw new AppValidationException("Owner name is required.");

        BankAccount newAccount;

        switch (dto.AccountType)
        {
            case AccountType.Credit:
                if (dto.CreditLimit == null)
                    return BadRequest("Credit limit is required for a Line of Credit account.");
                newAccount = new LineOfCreditAccount(dto.Owner, dto.Balance, dto.CreditLimit.Value);
                break;

            case AccountType.Gift:
                newAccount = new GiftCardAccount(dto.Owner, dto.Balance, dto.MonthlyDeposit ?? 0);
                break;

            case AccountType.Interest:
                newAccount = new InterestEarningAccount(dto.Owner, dto.Balance);
                break;

            default:
                return BadRequest("Invalid account type.");
        }

        _bankAccountRepository.Add(newAccount);

        var dtoResult = BankAccountDto.Create(newAccount);
        return CreatedAtAction(nameof(GetAccountInfo), new { accountNumber = newAccount.Number }, dtoResult);
    }
    [HttpPost("monthEnd")]
    public ActionResult<string> PerformMonthEndForAccount([FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        account.PerformMonthEndTransactions();
        return Ok($"Month-end processing completed for account {account.Number}.");
    }

    [HttpPost("deposit")]
    public ActionResult<string> MakeDeposit([FromQuery] decimal amount, [FromQuery] string note, [FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        account.MakeDeposit(amount, DateTime.Now, note);
        _bankAccountRepository.Update(account);

        return Ok($"A deposit of ${amount} was made in account {account.Number}.");
    }

    [HttpPost("withdrawal")]
    public ActionResult<string> MakeWithdrawal([FromQuery] decimal amount, [FromQuery] string note, [FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        account.MakeWithdrawal(amount, DateTime.Now, note);
        _bankAccountRepository.Update(account);


        return Ok($"A withdrawal of ${amount} was made in account {account.Number}.");
    }

    [HttpGet("balance")]
    public ActionResult<string> GetBalance([FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        return Ok($"The balance in account {account.Number} is ${account.Balance}.");

    }

    [HttpGet("accountHistory")]
    public IActionResult GetAccountHistory([FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        var history = account.GetAccountHistory();

        return Ok(history);
    }

    [HttpGet("accountInfo")]
    public ActionResult<BankAccountDto> GetAccountInfo([FromQuery] string accountNumber)
    {
        var account = _bankAccountRepository.GetByAccountNumber(accountNumber)
            ?? throw new AppValidationException("Cuenta no encontrada.");

        return BankAccountDto.Create(account);
    }

    [HttpGet("allAccountsInfo")]
    public ActionResult<List<BankAccountDto>> GetAllAccountInfo()
    {
        var list = _bankAccountRepository.ListWithTransaction();
        return BankAccountDto.Create(list);
    }
}