using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles.Abstractions
{
    public class EmailNotification : Notification
    {

        public override void SendNotification(string message)
        {
            Console.WriteLine($"Sending Email :  { message }");
        }
    }
}
