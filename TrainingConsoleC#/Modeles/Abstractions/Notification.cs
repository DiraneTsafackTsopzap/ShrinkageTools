using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles.Abstractions
{
    public abstract class Notification
    {
        ///<summary>
        /// Une classe (ou interface) où on définit une méthode sans l’écrire.
        /// Cette méthode est un contrat.
        /// Les autres classes doivent respecter ce contrat.
        /// </summary>


        public abstract void SendNotification(string message);




    }
}
