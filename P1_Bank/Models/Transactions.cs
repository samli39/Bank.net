using P1_Bank.Models.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models
{
    public class Transactions
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Trans_Type { get; set; }
        [DataType(DataType.Date)]
        public DateTime Trans_date { get; set; }
        [ForeignKey("Trans_from")]
        public int Trans_from_id { get; set; }
        [ForeignKey("Trans_to")]
        public int? Trans_to_id { get; set; }

        public virtual BankAccount Trans_from { get; set; }
        public virtual BankAccount Trans_to { get; set; }
    }
}
