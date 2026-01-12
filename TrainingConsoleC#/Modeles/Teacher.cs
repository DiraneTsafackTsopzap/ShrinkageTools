using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles
{
    public class Teacher 
    {
        // The teacher has a student (composition)
        private Student student { get; set; }

        /// <summary>
        /// Creates a teacher with a student
        /// </summary>
        public Teacher(Student student)
        {
            this.student = student;
        }

        /// <summary>
        /// The teacher assigns homework
        /// The real work is delegated to the student
        /// </summary>
        
        public void AssignHomework()
        {
            // 🔁 Delegation: the teacher asks the student to study
            // Teacher ne fait pas le travail
            // Le Teacher délègue le travail au Student
            student.Study();
        }
    }
}
