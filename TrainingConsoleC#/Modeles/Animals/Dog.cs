using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles.Animals
{
    public class Dog
    {
        /// <summary>
        /// Base Class represent a Dog
        /// </summary>

        private string name { get; set; }
        private int age { get; set; }

        public Dog(string name, int age)
        {
            this.name = name;
            this.age = age;
        }


        /// <summary>
        /// Method to display Dog information
        /// </summary>
        public void DisplayInfo()
        {
            Console.WriteLine($"Dog Name: {name}, Age: {age}");
        }

        /// <summary>
        /// Method to Represent a Dog Barking
        ///</summary>
        
        public void Bark()
        {
            Console.WriteLine("Woof! Woof!");
        }
    }
}
