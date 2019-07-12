using P1_Bank.Models.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace P1_Bank.Models.multi
{
    public class AccountTransactions
    {
        public BankAccount Ac { get; set; }
        public List<Transactions> List { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? Start { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? End { get; set; }
    }
}
