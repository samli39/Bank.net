using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using P1_Bank.Models;
using P1_Bank.Models.Accounts;

namespace P1_Bank.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<BankAccount> Accounts { get; set; }
        public DbSet<Transactions> Transaction { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // one-to-zero or one relationship between ApplicationUser and Customer
            // UserId column in Customers table will be foreign key
            modelBuilder.Entity<BankAccount>()
              .HasOne(c => c.User)
              .WithMany(x => x.Accounts)
              .HasForeignKey(f => f.UserId)
              .HasConstraintName("UserId")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            //transaction  from relationship
            modelBuilder.Entity<Transactions>()
                .HasOne(c => c.Trans_from)
                .WithMany(x => x.TransFrom)
                .HasForeignKey(f => f.Trans_from_id)
                .HasConstraintName("Trans_from_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


        }
        public DbSet<P1_Bank.Models.Accounts.Checking> Checking { get; set; }
        public DbSet<P1_Bank.Models.Accounts.Business> Business { get; set; }
        public DbSet<P1_Bank.Models.Accounts.Loan> Loan { get; set; }
        public DbSet<P1_Bank.Models.Accounts.TermDeposit> TermDeposit { get; set; }
        
    }
}
