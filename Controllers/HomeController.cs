using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {

        private BankAccountContext dbContext;

        //Dependency Injection
        public HomeController(BankAccountContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("account/{userId}")]
        public IActionResult Account(int userId)
        {
            // User currentUser = HttpContext.Session.GetObjectFromJson<User>("User");

            int? sessionUserId = HttpContext.Session.GetInt32("UserId");

            User currentUser = dbContext.Users.FirstOrDefault(user => user.UserId == userId);

            AccountViewModel accountModel = new AccountViewModel();

            accountModel.Account = currentUser;

            // accountModel.Transaction = new Transaction(currentUser);

            if(sessionUserId != userId || sessionUserId == null)
            {
                return RedirectToAction("Index");
            }

            //Current Balance 
            List<Transaction> currUserTransactions = dbContext.Transactions.Where(t => t.User == currentUser).ToList();

            double currentBalance = 0;
            foreach(Transaction t in currUserTransactions)
            {
                currentBalance += t.Amount;
            }
            
            // ViewBag.SetObjectAsJson("User", currentUser);

            ViewBag.UserId = sessionUserId;
            ViewBag.Name = currentUser.FirstName;
            ViewBag.Balance = currentBalance;
            ViewBag.Transactions = currUserTransactions;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // // // // //
        // CREATE  //
        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                dbContext.Add(user);
                dbContext.SaveChanges();

                User currentUser = dbContext.Users.FirstOrDefault(u => u.UserId == user.UserId);
                
                HttpContext.Session.SetObjectAsJson("User", currentUser);
                HttpContext.Session.SetInt32("UserId", user.UserId);

                return RedirectToAction("Account", new {userId = user.UserId});
            }
            else
                return View("Index");
        }

        [HttpPost("loginConfirmation")]
        public IActionResult LoginConfirmation(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);

                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                else
                {
                    if(userInDb.Password != userSubmission.Password)
                    {
                        ModelState.AddModelError("Email", "Invalid Email/Password");
                        return View("Login");
                    }
                    else
                    {
                        HttpContext.Session.SetObjectAsJson("User", userInDb);
                        HttpContext.Session.SetInt32("UserId", userInDb.UserId);

                        return RedirectToAction("Account", new {userId = userInDb.UserId});
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Index");
            }
        }
    
        // // // //
        // Actions //
        [HttpPost("account/{userId}/transaction")]
        public IActionResult Transaction(
            Transaction submission, 
            int userId,
            double userBalance
            )
        {
            if(ModelState.IsValid)
            {

                int? sessionUserId = HttpContext.Session.GetInt32("UserId");

                User currentUser = dbContext.Users.FirstOrDefault(user => user.UserId == sessionUserId);

                // withdrawal check
                if(userBalance + submission.Amount < 0)
                {
                    return View("Account", new {userId = userId});
                }

                submission.User = currentUser;

                dbContext.Add(submission);
                dbContext.SaveChanges();

                return RedirectToAction("Account", new {userId = sessionUserId});

            }
            else
                return View("Account", new {userId = userId});
        }

    }
}
