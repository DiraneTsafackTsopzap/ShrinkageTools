using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingConsoleC_.Modeles.Abstractions
{
    public static class AbstractionDemo
    {
        public static void Run()
        {
            Notification notification = new EmailNotification();

            notification.SendNotification("Hello via Email");

        
        }


    }
}
