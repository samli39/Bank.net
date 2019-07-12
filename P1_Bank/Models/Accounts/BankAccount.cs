using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models.Accounts
{
    public class BankAccount
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        [DataType(DataType.Date)]
        public DateTime Start_date { get; set; }
        [DataType(DataType.Date)]
        public DateTime? End_date { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(DataFormatString = "{0:p2}")]
        public decimal Interest { get; set; }
        public bool Exist { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Transactions> TransFrom { get; set; }
        public virtual ICollection<Transactions> TransTo { get; set; }

        public void Deposit(decimal amount)
        {
            Balance += amount;

        }

        public virtual bool Withdraw(decimal amount)
        {
            if (amount > Balance)
                return false;
            Balance -= amount;
            return true;
        }
    }
}
