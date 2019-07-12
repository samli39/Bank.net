using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models.Accounts
{
    public class Loan : BankAccount
    {
        public void Pay(decimal amount)
        {
            //if user balance of loan account is 0 or < 0, close the account
            if ((Balance -= amount) <= 0)
            {
                Balance = 0;
                Exist = false;
            }

        }
    }
}
