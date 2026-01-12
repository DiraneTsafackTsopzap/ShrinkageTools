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

    }
}
