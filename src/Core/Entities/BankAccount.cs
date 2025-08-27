using System;
using System.Text;
using System.Collections.Generic;

namespace Core.Entities
{
    public class BankAccount
    {
        // campo estatico; se comparte entre todas las instancias de BankAccount
        private static int s_accountNumberSeed = 1234567890; // s_ : Convención para campos estáticos
        // campo de intasncia cada cuenta tiene su propio saldo
        private readonly decimal _minimumBalance; 
        public string Number { get; private set; }
        public string Owner { get; set; }
        public int Id { get; set; }

        private List<Transaction> _allTransactions = new List<Transaction>(); // Privado por convención

        public decimal Balance 
        {
            get
            {
                decimal balance = 0;
                foreach (var item in _allTransactions)
                {
                    balance += item.Amount;
                }
                return balance;
            }
        }

        // Constructor modificado para aceptar mínimo opcional
        public BankAccount(string name, decimal initialBalance, decimal minimumBalance = 0m)
        {
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "El saldo inicial no puede ser negativo");

            Id = s_accountNumberSeed;
            Number = s_accountNumberSeed.ToString();
            s_accountNumberSeed++;

            Owner = name;
            _minimumBalance = minimumBalance; // ahora sí se asigna
            MakeDeposit(initialBalance, DateTime.Now, "saldo inicial");
        }

        public void MakeDeposit(decimal amount, DateTime date, string note)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "El monto del depósito debe ser positivo");

            var deposit = new Transaction(amount, date, note);
            _allTransactions.Add(deposit);
        }

        public void MakeWithdrawal(decimal amount, DateTime date, string note)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount of withdrawal must be positive");

            Transaction? overdraftTransaction = CheckWithdrawalLimit(Balance - amount < _minimumBalance);
            Transaction? withdrawal = new(-amount, date, note);
            _allTransactions.Add(withdrawal);
            if (overdraftTransaction != null)
                _allTransactions.Add(overdraftTransaction);
        }

        public virtual void PerformMonthEndTransactions()
        {
            // implementación vacía o lógica por defecto
        }

        protected virtual Transaction? CheckWithdrawalLimit(bool isOverdrawn)
        {
            if (isOverdrawn)
            {
                throw new InvalidOperationException("Not sufficient funds for this withdrawal");
            }
            else
            {
                return default;
            }
        }

        public string GetAccountHistory()
        {
            // representa un string de caracteres mutable
            var report = new StringBuilder();
            decimal balance = 0;
            report.AppendLine("Date\t\tAmount\tBalance\tNote");
            foreach (var item in _allTransactions)
            {
                balance += item.Amount;
                report.AppendLine($"{item.Date.ToShortDateString()}\t{item.Amount}\t{balance}\t{item.Notes}");
            }
            return report.ToString();
        } 
    }
}
