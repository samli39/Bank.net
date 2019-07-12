using Microsoft.AspNetCore.Identity;
using P1_Bank.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<BankAccount> Accounts { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
