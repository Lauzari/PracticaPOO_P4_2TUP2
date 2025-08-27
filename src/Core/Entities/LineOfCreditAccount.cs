using System;

namespace Core.Entities
{
    public class LineOfCreditAccount : BankAccount
    {
        private readonly decimal _creditLimit;

        public LineOfCreditAccount(string name, decimal initialBalance, decimal creditLimit)
            : base(name, initialBalance) //  coincide con el constructor de BankAccount
        {
            _creditLimit = creditLimit;
        }

        protected override Transaction? CheckWithdrawalLimit(bool isOverdrawn) =>
            isOverdrawn
                ? new Transaction(-20, DateTime.Now, "Apply overdraft fee")
                : default;

        public override void PerformMonthEndTransactions()
        {
            if (Balance < 0)
            {
                decimal interest = -Balance * 0.07m;
                MakeWithdrawal(interest, DateTime.Now, "Charge monthly interest");
            }
        }
    }
}
