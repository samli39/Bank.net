using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models.Accounts
{
  
        public class TermDeposit : BankAccount
        {
            public void Close()
            {
                Balance = 0;
                Exist = false;
            }
        }
    
}
