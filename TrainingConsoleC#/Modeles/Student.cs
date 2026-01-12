using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles
{
    public class Student 
    {
        // Private fields (data of the student)
        private string name { get; set; }
        private string matiere { get; set; }

        /// <summary>
        /// Creates a student with a name and a subject
        /// </summary>
        public Student(string name , string matiere)
        {
            this.name = name;
            this.matiere = matiere;
        }

        /// <summary>
        /// The student studies the subject
        /// </summary>
        public void Study()
        {
            Console.WriteLine($"{name} is studying {matiere}.");
        }   
    }
}
