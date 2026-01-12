using System;

namespace TrainingConsoleC_
{
    public static class FunctionAndMethod
    {
        public static void Run()
        {
            var Names = new List<string> { "Dirane", "Astrid", "Chanelle" };

            foreach ( var name in Names)
            {
                Greet(name);
            }

            int number = 5;
            int result = Factorial(number);
            Console.WriteLine($"Factorial of {number} is {result}");

            string originalString = "Image";
            string reversedString = ReverseString(originalString);
            Console.WriteLine($"Original String: {originalString}, Reversed String: {reversedString}");


            int FirstNumber = 80;
            int SecondNumber = 5;
            Console.WriteLine("Result : " + Summe(FirstNumber, SecondNumber));


            var fruits = GenerateFruits();
            Console.WriteLine("Fruits : " + string.Join(", ", fruits));

            // Exercice About the Function and Method
            Console.WriteLine("Convert 2 Hours to Seconds : " + ConvertHoursToSeconds(2) + " Seconds");

            double voltage = 220.0;
            double current = 5.0;
            double power = CalculatePower(voltage, current);
            Console.WriteLine($"Power: {power} Watts");


            // Exercice about the Password Validation
            Console.WriteLine(ValidatePassword("Password1"));    // true
            Console.WriteLine(ValidatePassword("password1"));    // false (no uppercase)
            Console.WriteLine(ValidatePassword("PASSWORD1"));    // false (no lowercase)
            Console.WriteLine(ValidatePassword("Password"));     // false (no digit)
            Console.WriteLine(ValidatePassword("Pass1"));        // false (too short)
            Console.WriteLine(ValidatePassword("P@ssw0rd!"));    // true



        }

        /// <summary>
        /// Void Method With Parameters
        /// </summary>
        /// <param name="name"></param>
        private static void Greet(string name)
        {
            Console.WriteLine($"Welcome :  {name} to Lowell ");
        }

        /// <summary>
        ///  Recursive Method to calculate Factorial
        ///</summary>
        private static int Factorial(int number)
        {
            if(number == 0 || number == 1)
            {
                return 1;
            }
            else
            {
                return number * Factorial(number - 1);
            }
        }

        /// <summary>
        /// Method to Reverse a String
        /// </summary>
        public static string ReverseString (string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }


        /// <summary>
        ///  Simple Function With Return Type
        /// </summary>
        public static int Summe(int a, int b)
        {
            return a + b;
        }

        
        public static List<string> GenerateFruits()
        {
            List<string> ListesFruits = new List<string>
            {
                "Avocat" , "Banane" , "Tomato"
            };


            return ListesFruits;
        }


        // Exercice About the Function and Method

        /// <summary>
        ///  Function pour Convertir les Heures en Secondes
        /// </summary>
        
        public static int ConvertHoursToSeconds(int hours)
        {
            return hours * 3600;
        }

        /// <summary>
        /// Create A Function that takes Voltage and Current as parameters and returns the Power.
        /// </summary>
        
        public static double CalculatePower(double voltage, double current)
        {
            return voltage * current;
        }

        ///<summary>
        /// Notes :
        ///  - The Password must be at least 8 Characters long
        ///  - It must contain at least one Uppercase Letter , one LowerCase Letter , and one Digit
        ///  - It may also contain special characters such as ! , @ , etc
        /// </summary>
        public static bool ValidatePassword (string password)
        {
            /// <summary>
            /// Rule 1 : Check if Password nest pas null , " " , "  " , string.empty : Retourne False si cest le Cas
            /// </summary>
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            ///<summary>
            /// Rule 2: at least 8 characters // SI le Password est Inferieur a 8 , Retourner False
            /// </summary>
            if (password.Length < 8)
            {
                return false;
            }

            ///<summary>
            /// Retourne Vrai si le Password contient un Uppercase
            /// </summary>
            bool hasUppercase = password.Any(char.IsUpper);

            ///<summary>
            /// Rule 4: at least one lowercase letter retourne Vrai si le Password contient aussi un Lowercase
            /// </summary>
            
            bool hasLowercase = password.Any(char.IsLower);
            ///<summary>
            /// Rule 5: at least one lowercase letter retourne Vrai si le Password contient aussi un IsDigit
            /// </summary>
         
            bool hasDigit = password.Any(char.IsDigit);

            return hasUppercase && hasLowercase && hasDigit;
        }
    }
}
