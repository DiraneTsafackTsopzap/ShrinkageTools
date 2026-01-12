using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingConsoleC_.Modeles;

namespace TrainingConsoleC_
{
    public static class Deleguation
    {
        /// <summary>
        /// Demonstrates delegation between Teacher and Student
        /// </summary>
        public static void Run()
        {
            var student = new Student("Dirane", "Informatique");
            var teacher = new Teacher(student);
            teacher.AssignHomework();

        }
    }
}
