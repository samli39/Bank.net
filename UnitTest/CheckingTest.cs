using Bank_p1.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using P1_Bank.Data;
using P1_Bank.Models.Accounts;
using System;

namespace UnitTest
{
    [TestClass]
    public class CheckingTest
    {
        Checking checking = new Checking()
        {
            Id = 1,
            Balance=500,
            Start_date=DateTime.Today,
            Interest = 0.01m,
            UserId="abc"

        };
        [TestMethod]
        public void DepositMethod1()
        {
            //test deposit 
            checking.Deposit(100);
            Assert.AreEqual(600, checking.Balance);
        }

        [TestMethod]
        public void WithDraw()
        {
            var result = checking.Withdraw(1000);
            Assert.AreEqual(false, result);
        } 
    }
}
