using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingConsoleC_.Modeles.Animals;

namespace TrainingConsoleC_.Modeles
{
    public static class Inheritance
    {
        public static void Run()
        {
            // Create an instance of Dog
            var myDog = new Dog("Buddy", 3);

            myDog.DisplayInfo();
            myDog.Bark();

            // Create an instance of HerdingDog : il Herite de la class Dog et de ses methodes
            var myHerdingDog = new HerdingDog("Max", 4);
            myHerdingDog.DisplayInfo();
            myHerdingDog.Bark();


            // Call the Herd method specific to HerdingDog
            myHerdingDog.Herd();


        }
    }
}
