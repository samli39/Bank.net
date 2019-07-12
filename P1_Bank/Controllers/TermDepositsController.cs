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
    public class TermDepositsController : Controller
    {
        private AccountDAL DAL;

        public TermDepositsController(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            string uid = contextAccessor.HttpContext.User.Identity.Name;
            DAL = new AccountDAL(context,uid);
        }

        // GET: TermDeposits
        public async Task<IActionResult> Index()
        {
            var tdList = await DAL.FetchTDList();
            ViewData["Time"] = DateTime.Today;
            return View(tdList);
        }

        // GET: TermDeposit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TermDeposit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Balance,Start_date,End_date,Interest,Exist,UserId")] TermDeposit td)
        {
            if (td.Balance <= 0)
            {
                ViewData["error"] = "Invaild input";
                return View(td);
            }
            //default setting for new checking account
            td.Start_date = DateTime.Today;
            td.End_date = DateTime.Today.AddDays(1);
            td.Exist = true;
            //random interest for overdraft

            td.Interest = (decimal)Math.Round((new Random().NextDouble() * (0.9 - 0.1) + 0.1) / 10, 3);
            td.Balance *= (1 + td.Interest);
            DAL.CreateTD(td);
            return RedirectToAction(nameof(Index));

        }

        // GET: TermDeposits/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var termDeposit = await DAL.FetchAccount(id) as TermDeposit;
            if (termDeposit == null)
            {
                return NotFound();
            }
            //find the current user id
            var chList = await DAL.FetchCheckingList();
            TdChList list = new TdChList()
            {
                Td = termDeposit,
                ChList = chList

            };
            return View(list);
        }

        // POST: TermDeposits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id, int CheckingAccount)
        {
            //find the conresponing account
            var termDeposit = await DAL.FetchAccount(id) as TermDeposit;
            var checking = await DAL.FetchAccount(CheckingAccount) as Checking;


            try
            {
                //deposit to checking account
                checking.Deposit(termDeposit.Balance);
                Transactions trans = new Transactions()
                {
                    Trans_date = DateTime.Today,
                    Trans_from_id = termDeposit.Id,
                    Trans_to_id = checking.Id,
                    Amount = termDeposit.Balance,
                    Trans_Type = "Transfer"
                };
                //close term deposit account
                termDeposit.Close();
                DAL.Transfer(termDeposit, checking,trans);
            }
            catch (DbUpdateConcurrencyException)
            {
                
                    throw;

            }
            return RedirectToAction(nameof(Index));

        }


    }
}
