using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingConsoleC_.Modeles;

namespace TrainingConsoleC_
{
    public static class ProgrammationObjetOriente
    {
        public static void Run()
        {
            var owner = new Owner("Dirane", "Tsafack");
            var account = new BankAccount(owner, 1000);

            account.Deposit(500);
            Console.WriteLine($"Final Balance: {account.GetBalance()}");
            account.Withdraw(300);
            Console.WriteLine($"Final Balance: {account.GetBalance()}");
            account.Withdraw(2000);
        }
    }
}
