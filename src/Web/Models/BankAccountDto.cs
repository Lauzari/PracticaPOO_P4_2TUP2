using Core.Dtos;
using Core.Entities;

namespace Web.Models;

public record BankAccountDto(int Id, string Number, string Owner,decimal Balance,List<TransactionDto> Transactions)
{
  /*  public static BankAccountDto Create(BankAccount entity)
    {
        var dto = new BankAccountDto(entity.Id, entity.Number, entity.Owner, entity.Balance);

        return dto;
    } */
public static BankAccountDto Create(BankAccount entity)
{
    var transactions = entity.Transactions != null
        ? TransactionDto.Create(entity.Transactions.ToList())
        : new List<TransactionDto>();

    var dto = new BankAccountDto(
        entity.Id,
        entity.Number,
        entity.Owner,
        entity.Balance,
        transactions // agregando la lista de transacciones
    );

    return dto;
}

    public static List<BankAccountDto> Create(IEnumerable<BankAccount> entities)
    {
        var listDto = new List<BankAccountDto>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }

        return listDto;
    }
}
