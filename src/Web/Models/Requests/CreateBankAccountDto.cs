using Core.Entities;

namespace Web.Models;

// DTO de entrada 
public record CreateBankAccountDto(
    string Owner,
    decimal Balance,
    AccountType AccountType,
    decimal? CreditLimit = null,
    decimal? MonthlyDeposit = null
);
