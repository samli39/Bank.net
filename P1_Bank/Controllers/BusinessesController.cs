using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank_p1.DAL;
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
    public class BusinessesController : Controller
    {
        private AccountDAL DAL;


        public BusinessesController(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            string uid = contextAccessor.HttpContext.User.Identity.Name;

            DAL = new AccountDAL(context,uid);
        }

        // GET: Business
        public async Task<IActionResult> Index()
        {
            var businessList = await DAL.FetchBusinessList();
            return View(businessList);
        }

        // GET: Business/Create
        public IActionResult Create()
        {
            Business business = new Business
            {
                Start_date = DateTime.Today,
                Exist = true,
                Interest = (decimal)Math.Round((new Random().NextDouble() * (0.9 - 0.1) + 0.1) / 10, 3),
            };
            //create new business account
            DAL.CreateBusiness(business);
            return RedirectToAction(nameof(Index));
        }

        // GET: Business/deposit/5
        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await DAL.FetchAccount(id) as Business;
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Business/Deposit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            var business = await DAL.FetchAccount(id) as Business;

            if (business != null)
            {
                try
                {

                    business.Deposit(amount);
                    //create transaction object
                    Transactions trans = new Transactions()
                    {
                        Trans_date = DateTime.Today,
                        Trans_from_id = business.Id,
                        Amount = amount,
                        Trans_Type = "Deposit"
                    };
                    //save to database
                    DAL.Update(business, trans);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Business/Withdraw/5
        public async Task<IActionResult> Withdraw(int? id, string error)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await DAL.FetchAccount(id);
            if (business == null)
            {
                return NotFound();
            }
            ViewData["Error"] = error;
            return View(business);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Business/Withdraw/5
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            var business = await DAL.FetchAccount(id);

            if (business == null)
            {
                return NotFound();
            }
            else
            {
                //find the business account.
                if (business.Withdraw(amount))
                {
                    //success
                    //update to database
                    try
                    {
                        //create transaction object
                        Transactions trans = new Transactions()
                        {
                            Trans_date = DateTime.Today,
                            Trans_from_id = business.Id,
                            Amount = amount,
                            Trans_Type = "Withdraw"
                        };
                        DAL.Update(business, trans);
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
                id = business.Id,
                error = "Fail to Withdraw,Insufficient funds"
            });
        }

        //Get:Business/Transfer/5
        public async Task<IActionResult> Transfer(int? id, string error)
        {
            if (id == null)
            {
                return NotFound();
            }
            var business = await DAL.FetchAccount(id) as Business;
            if (business == null)
            {
                return NotFound();
            }

            var chList = await DAL.FetchCheckingList();
            var bsList = await DAL.FetchBusinessList();
            ChBsList list = new ChBsList()
            {
                Ch = business,
                ChList = chList,
                BsList = bsList
            };
            ViewData["Error"] = error;
            return View(list);
        }

        //POST:Business/Transfer/5
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

        //Get:Business/Transaction/5
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

            var business = await DAL.FetchAccount(id);
            List<Transactions> list = await DAL.FetchTransList(id, start, end);

            AccountTransactions trans = new AccountTransactions()
            {
                Ac = business,
                List = list,
                Start = start,
                End = end
            };
            ViewData["error"] = error;

            return View(trans);
        }
        //Post:business/Transaction/5
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

        //Get:Business/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await DAL.FetchAccount(id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Business/Close/5
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await DAL.FetchAccount(id);

            DAL.CloseAccount(business);
            return RedirectToAction(nameof(Index));
        }
    }
}
