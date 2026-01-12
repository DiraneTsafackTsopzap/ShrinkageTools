using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles
{
    public class Owner
    {
        private string FirstName { get; }
        private string LastName { get; }

        public Owner(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
