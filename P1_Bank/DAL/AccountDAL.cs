
using Microsoft.EntityFrameworkCore;
using P1_Bank.Data;
using P1_Bank.Models;
using P1_Bank.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Bank_p1.DAL
{
    public class AccountDAL
    {
        private readonly ApplicationDbContext _context;
        private string uid;



        #region fetch list of account/transaction/loan/ter deposit information
        /*
        * single account information
        */
        public AccountDAL(ApplicationDbContext context, string id)
        {
            _context = context;
            uid = _context.Users.Where(x => x.Email == id).FirstOrDefault().Id;
        }
        /*
         * fetch account information
         */
        public async Task<BankAccount> FetchAccount(int? id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            return acc;
        }
        /*
         * fetch the list of checking account
         */
        public async Task<List<Checking>> FetchCheckingList()
        {
            var list = await _context.Checking.Include(c => c.User).Where(c => c.UserId == uid && c.Exist == true).ToListAsync();
            return list;
        }

        /*
         * fetch the list of checking account
         */
        public async Task<List<Business>> FetchBusinessList()
        {
            var list = await _context.Business.Include(c => c.User).Where(c => c.UserId == uid && c.Exist == true).ToListAsync();
            return list;
        }
        /*
         * list of transactio of given account
         */

        public async Task<List<Transactions>> FetchTransList(int? id,DateTime? start,DateTime? end)
        {
            var list = await _context.Transaction
                .Include("Trans_from")
                .Include("Trans_to")
                .Where(c => (c.Trans_from_id == id || c.Trans_to_id == id)
                        && (c.Trans_date >= start && c.Trans_date <= end))
                .OrderByDescending(d => d.Trans_date)
                .ThenByDescending(i => i.Id)
                .Take(10)
                .ToListAsync();
            return list;


        }
        public async Task<List<Loan>> FetchLoanList()
        {
            var list = await _context.Loan.Where(c => c.UserId == uid && c.Exist == true).ToListAsync();
            return list;
        }

        public async Task<List<TermDeposit>> FetchTDList()
        {
            var list = await _context.TermDeposit.Where(c => c.UserId == uid && c.Exist == true).ToListAsync();
            return list;
        }

        #endregion

        #region create account
        public void CreateChecking(Checking checking)
        {
            checking.UserId = uid;

            //add to the database
            _context.Add(checking);
            _context.SaveChanges();

        }
        public void CreateBusiness(Business business)
        {
            business.UserId = uid;

            //add to the database
            _context.Add(business);
            _context.SaveChanges();
        }

        public void CreateLoan(Loan loan)
        {
            loan.UserId = uid;
            _context.Add(loan);
            _context.SaveChanges();
        }

        public void CreateTD(TermDeposit termDeposit)
        {
            termDeposit.UserId = uid;
            _context.Add(termDeposit);
            _context.SaveChanges();
        }
        #endregion
        #region deposit ,withdraw,transfer,close
        /*
         * deposit withdraw
         */
        public void Update(BankAccount acc, Transactions trans)
        {
            _context.Add(trans);
            _context.Update(acc);
            _context.SaveChanges();
        }

        //transfer
        public void Transfer(BankAccount from, BankAccount to, Transactions trans)
        {
            _context.Add(trans);
            _context.Update(from);
            _context.Update(to);
            _context.SaveChanges();
        }

        public void CloseAccount(BankAccount acc)
        {
            //change the exist to false
            acc.Exist = false;
            //update to database
            _context.Update(acc);
            _context.SaveChanges();
        }
        #endregion
    }
}
