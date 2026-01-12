using System;
using System.Data;

namespace TrainingConsoleC_
{
    public static class BasicVariable
    {
        /// <summary>
        /// Point d’entrée de cette classe de démonstration
        /// </summary>
        public static void Run()
        {
            Example1_BasicVariables();
            Example2_Constants();
            Example3_Enums();
            Example4_StringManipulation();
            Example5_NullableTypes();
        }

        /// <summary>
        /// Example 1 : Basic variable declarations and initializations
        /// </summary>
        private static void Example1_BasicVariables()
        {
            int age = 25;
            string name = "Dirane Tsafack Tsopzap";
            bool isStudent = true;
            decimal salary = 2500.50m;

            Console.WriteLine($"Name: {name}, Age: {age}, Is Student: {isStudent}, Salary: {salary}");
        }

        /// <summary>
        /// Example 2 : Understanding constants
        /// </summary>
        private static void Example2_Constants()
        {
            const double Pi = 3.14159;
            Console.WriteLine($"Value of Pi: {Pi}");
        }

        /// <summary>
        /// Example 3 : Working with Enumerations
        /// </summary>
        private static void Example3_Enums()
        {
            Console.WriteLine($"Days of the Week: {Days.Monday}, {Days.Wednesday}, {Days.Friday}");
            Console.WriteLine($"Different User Roles: {Role.Admin}, {Role.User}, {Role.Guest}");
        }

        /// <summary>
        /// Example 4 : String manipulation
        /// </summary>
        private static void Example4_StringManipulation()
        {
            string firstName = "Dirane";
            string lastName = "Tsafack";
            string fullName = $"{firstName} {lastName}";

            Console.WriteLine($"Full Name: {fullName}");
        }

        /// <summary>
        /// Example 5 : Using nullable types
        /// </summary>
        private static void Example5_NullableTypes()
        {
            int? nullableInt = null;
            double? nullableDouble = 45.67;

            if (nullableInt.HasValue)
                Console.WriteLine($"Nullable Integer Value: {nullableInt.Value}");
            else
                Console.WriteLine("Nullable Integer is null.");

            if (nullableDouble.HasValue)
                Console.WriteLine($"Nullable Double Value: {nullableDouble.Value}");
            else
                Console.WriteLine("Nullable Double is null.");
        }
    }

    public enum Role
    {
        Admin = 1,
        User = 2,
        Guest = 3
    }
    public enum Days
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}
