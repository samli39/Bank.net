using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank_p1.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using P1_Bank.Data;
using P1_Bank.Models;
using P1_Bank.Models.Accounts;
using P1_Bank.Models.multi;

namespace P1_Bank.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        
        private AccountDAL DAL;
        public LoansController(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            string uid = contextAccessor.HttpContext.User.Identity.Name;

            DAL = new AccountDAL(context,uid);
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var list = await DAL.FetchLoanList();
            return View(list);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Balance,Start_date,End_date,Interest,Exist,UserId")] Loan loan)
        {

            if (loan.Balance <= 0)
            {
                ViewData["error"] = "Invaild input";
                return View(loan);
            }
            //default setting for new checking account
            loan.Start_date = DateTime.Today;
            loan.End_date = DateTime.Today.AddDays(1);
            loan.Exist = true;
            //random interest for overdraft

            loan.Interest = (decimal)Math.Round((new Random().NextDouble() * (0.9 - 0.1) + 0.1) / 10, 3);
            loan.Balance *= (1 + loan.Interest);
            DAL.CreateLoan(loan);
            return RedirectToAction(nameof(Index));
            
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Pay(int? id, string error)
        {
            if (id == null)
            {
                return NotFound();
            }
            var chList = await DAL.FetchCheckingList();
            var bsList = await DAL.FetchBusinessList();
            if (id == null)
            {
                return NotFound();
            }

            var loan = await DAL.FetchAccount(id);
            if (loan == null)
            {
                return NotFound();
            }
            ChBsList list = new ChBsList()
            {
                Ch = loan,
                ChList = chList,
                BsList = bsList
            };
            ViewData["Error"] = error;
            return View(list);
        }

        // POST: Loans/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, int Account, decimal amount)
        {
            var loan = await DAL.FetchAccount(id) as Loan;
            BankAccount acc = await DAL.FetchAccount(Account);

            if (acc.GetType().Name == "Checking")
            {
                acc = acc as Checking;
            }

            else
            {
                acc = acc as Business;
            }

            if (acc.Withdraw(amount))
            {
                loan.Pay(amount);
                //create transaction object
                Transactions trans = new Transactions()
                {
                    Trans_date = DateTime.Today,
                    Trans_from_id = acc.Id,
                    Trans_to_id = loan.Id,
                    Amount = amount,
                    Trans_Type = "Withdraw"
                };
                
                DAL.Transfer(acc,loan,trans);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Pay",new
            {
                id = id,
                error ="Fail! Maybe Insufficient Funds."
            });
        }
    }
}
