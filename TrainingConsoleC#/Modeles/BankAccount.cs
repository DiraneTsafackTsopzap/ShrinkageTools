using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles
{
    public class BankAccount
    {
        // Champs privés
        private string accountNumber;
        private double balance;

        private Owner owner; // 🔹 COMPOSITION

        /// <summary>
        /// Composition :
        ///— Un compte bancaire a un propriétaire.
        /// Le compte utilise le propriétaire,
        /// mais le propriétaire n’est pas un compte.
        /// La composition, c’est quand une classe utilise une autre classe. la Classe BankAccount utilise la Classe Owner
        /// </summary>
        /// <param name="owner">La personne qui possède le compte</param>
        /// <param name="initialBalance">L’argent de départ sur le compte</param>

        public BankAccount(Owner owner, double initialBalance)
        {
            this.owner = owner;
            this.balance = initialBalance;
        }

        /// <summary>
        /// Deposit money into the account
        /// </summary>
        public void Deposit(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            balance += amount;

            Console.WriteLine($"Deposited: {amount} -  New Balance: {balance} -  ({owner.FullName()})");
        }

        public double GetBalance()
        {
            return balance;
        }

        /// <summary>
        /// Withdraw money from the account
        /// </summary>
        public void Withdraw(double amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Withdrawal amount must be positive");
                return;
            }

            /// <summary>
            ///  Le Montant à retirer ne doit pas dépasser le solde disponible , si jai 1200 $ et que je veux retirer 1500$ , 
            ///  le systeme doit m'afficher un message d'erreur
            ///</summary>

            if (amount > balance)
            {
                Console.WriteLine("Insufficient funds");
                return;
            }

            balance -= amount;
            Console.WriteLine($"Withdrawn: {amount}, New Balance -  {balance} -  ({owner.FullName()})");
        }

    }
}
