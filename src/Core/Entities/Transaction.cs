namespace Core.Entities;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string? Notes { get; private set; }

    /*foreing key BankAccount*/
    public int BankAccountId { get; private set; }
    public BankAccount? BankAccount { get; private set; } /* nullable reference type */

    private Transaction()
    {

    }

    public Transaction(decimal amount, DateTime date, string? notes)
    {
        Amount = amount;
        Date = date;
        Notes = notes;
        

    }
}