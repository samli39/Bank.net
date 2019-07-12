using P1_Bank.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1_Bank.Models.multi
{
    public class TdChList
    {
        public TermDeposit Td { get; set; }
        public List<Checking> ChList { get; set; }
    }
}
