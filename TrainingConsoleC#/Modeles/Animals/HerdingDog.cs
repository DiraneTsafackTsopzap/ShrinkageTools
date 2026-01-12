using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles.Animals
{
    public class HerdingDog : Dog
    {
        public HerdingDog(string name, int age) : base(name, age)
        {

        }

        /// <summary>
        /// Method to Represent a Herding Dog Herding
        ///</summary>
        
        public void Herd()
        {
            Console.WriteLine("Whoo Whoo of HerdingDog");
        }
    }
}
