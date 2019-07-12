using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models.Accounts
{
    public class Business : BankAccount
    {
        public override bool Withdraw(decimal amount)
        {
            //100 is overdraft 
            if (Balance < 0 || amount > Balance + 100)
                return false;

            //charge for overdraft
            if ((Balance -= amount) < 0)
                Balance *= (1 + Interest);
            return true;
        }
    }
}
