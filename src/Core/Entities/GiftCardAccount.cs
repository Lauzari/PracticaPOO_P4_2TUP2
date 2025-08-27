using System;

namespace Core.Entities
{
    public class GiftCardAccount : BankAccount
    {
        private readonly decimal _monthlyDeposit;

        public GiftCardAccount(string accountNumber, string name, decimal initialBalance, decimal monthlyDeposit = 0)
            : base(name, initialBalance)
        {
            _monthlyDeposit = monthlyDeposit;
        }

        public override void PerformMonthEndTransactions()
        {
            if (_monthlyDeposit != 0)
            {
                MakeDeposit(_monthlyDeposit, DateTime.Now, "Add monthly deposit");
            }
        }
    }
}
