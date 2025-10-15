using System;
using System.Collections.Generic;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<BankAccount> bankAccounts { get; set; }

        /* Herencia */
        public DbSet<LineOfCreditAccount> lineOfCreditAccounts { get; set; }
        public DbSet<GiftCardAccount> giftCardAccounts { get; set; }
        public DbSet<InterestEarningAccount> interestEarningAccounts { get; set; }

        public DbSet<Transaction> transactions { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }


        
    }   
     
}