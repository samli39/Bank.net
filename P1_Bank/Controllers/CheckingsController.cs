using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using P1_Bank.Data;
using P1_Bank.Models.Accounts;
using P1_Bank.Models;
using Bank_p1.DAL;
using P1_Bank.Models.multi;

namespace Bank_p1.Controllers
{
    [Authorize]
    public class CheckingsController : Controller
    {


        private AccountDAL DAL;


        public CheckingsController(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            string uid = contextAccessor.HttpContext.User.Identity.Name;
         
            DAL = new AccountDAL(context,uid);
        }

        // GET: Checkings
        public async Task<IActionResult> Index()
        {
            var checkingList = await DAL.FetchCheckingList();
            return View(checkingList);
        }

        // GET: Checkings/Create
        public IActionResult Create()
        {
            Checking checking = new Checking
            {
                Start_date = DateTime.Today,
                Exist = true,
                Interest = 0.01m,
            };
            //create new checking account
            DAL.CreateChecking(checking);
            return RedirectToAction(nameof(Index));
        }

        // GET: Checkings/deposit/5
        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checking = await DAL.FetchAccount(id) as Checking;
            if (checking == null)
            {
                return NotFound();
            }

            return View(checking);
        }

        // POST: Checkings/Deposit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            var checking = await DAL.FetchAccount(id) as Checking;

            if (checking != null)
            {
                try
                {

                    checking.Deposit(amount);
                    //create transaction object
                    Transactions trans = new Transactions()
                    {
                        Trans_date = DateTime.Today,
                        Trans_from_id = checking.Id,
                        Amount = amount,
                        Trans_Type = "Deposit"
                    };
                    //save to database
                    DAL.Update(checking, trans);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Checkings/Withdraw/5
        public async Task<IActionResult> Withdraw(int? id, string error)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checking = await DAL.FetchAccount(id);
            if (checking == null)
            {
                return NotFound();
            }
            ViewData["Error"] = error;
            return View(checking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Checkings/Withdraw/5
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            var checking = await DAL.FetchAccount(id);
            
            

            if (checking == null)
            {
                return NotFound();
            }
            else
            {
                //find the checking account.
                if (checking.Withdraw(amount))
                {
                    //success
                    //update to database
                    try
                    {
                        //create transaction object
                        Transactions trans = new Transactions()
                        {
                            Trans_date = DateTime.Today,
                            Trans_from_id = checking.Id,
                            Amount = amount,
                            Trans_Type = "Withdraw"
                        };
                        DAL.Update(checking, trans);
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                        throw;

                    }
                    //back to chekcing list page
                    return RedirectToAction(nameof(Index));
                }
            }
            //fail to withdraw
            return RedirectToAction("Withdraw", new
            {
                id = checking.Id,
                error = "Fail to Withdraw,Insufficient funds"
            });
        }

        //Get:Checkings/Transfer/5
        public async Task<IActionResult> Transfer(int? id, string error)
        {
            if (id == null)
            {
                return NotFound();
            }
            var checking = await DAL.FetchAccount(id) as Checking;
            if (checking == null)
            {
                return NotFound();
            }

            var chList = await DAL.FetchCheckingList();
            var bsList = await DAL.FetchBusinessList();
            ChBsList list = new ChBsList()
            {
                Ch = checking,
                ChList = chList,
                BsList = bsList
            };
            ViewData["Error"] = error;
            return View(list);
        }

        //POST:Checking/Transfer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, int Account, decimal amount)
        {

            //get the account
            var tFrom = await DAL.FetchAccount(id);

            BankAccount tTo = await DAL.FetchAccount(Account);
            if (tFrom.Withdraw(amount))
            {
                //success withdraw
                tTo.Deposit(amount);
                //create transaction object
                Transactions trans = new Transactions()
                {
                    Trans_date = DateTime.Today,
                    Trans_from_id = tFrom.Id,
                    Trans_to_id = tTo.Id,
                    Amount = amount,
                    Trans_Type = "Transfer"
                };
                DAL.Transfer(tFrom, tTo, trans);
                return RedirectToAction(nameof(Index));
            }


            //fail to withdraw
            return RedirectToAction("Transfer", new
            {
                id = tFrom.Id,
                error = "Fail to Withdraw,Insufficient funds"
            });
        }
        //Get:Checking/Transaction/5
        public async Task<IActionResult> Transaction(int? id, string error, DateTime? start, DateTime? end)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (start == null)
                start = DateTime.Today;
                
            if (end == null)
                end = DateTime.Today;

            var checking = await DAL.FetchAccount(id);
            List<Transactions> list = await DAL.FetchTransList(id,start,end);

            AccountTransactions trans = new AccountTransactions()
            {
                Ac = checking,
                List = list,
                Start = start,
                End = end
            };
            ViewData["error"] = error;

            return View(trans);
        }
        //Post:Checking/Transaction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Transaction(int id, AccountTransactions trans)
        {
            if (trans.Start > trans.End)
                //start date > end date
                return RedirectToAction("Transaction", new
                {
                    id = id,
                    error = "The Start Date cannot later than End Date"
                });
            else
                return RedirectToAction("Transaction", new
                {
                    id = id,
                    start = trans.Start,
                    end = trans.End
                });



        }
        // GET: Checkings/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checking = await DAL.FetchAccount(id);
            if (checking == null)
            {
                return NotFound();
            }

            return View(checking);
        }

        // POST: Checkings/Close/5
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checking = await DAL.FetchAccount(id);

            DAL.CloseAccount(checking);
            return RedirectToAction(nameof(Index));
        }
    }
}
