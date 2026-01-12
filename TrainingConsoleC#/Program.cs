

//Console.WriteLine("Hello, World!");


using System.Runtime.InteropServices.ComTypes;

/// <summary>
///     Ceci est la Version 12.0 de C#
/// </summary>
/// <summary>
///     Ceci est l'ancienne version de C#
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        //Console.WriteLine("This is a sample C# 12.0 program.");

        ///<summary>
        /// Example 1 : Basic variable declarations and Initializations
        /// </summary>
        int age = 25;
        string Name = "Dirane Tsafack Tsopzap";
        bool isStudent = true;
        decimal Salary = 2500.50m;
        Console.WriteLine($"Name: {Name}, Age: {age}, Is Student: {isStudent} , Salary : {Salary}");


        /// </summary>
        /// Example 2 :        Understanding Constantes
        /// <summary

        const double Pi = 3.14159;
        Console.WriteLine($"Value of Pi: {Pi}");


        ///<summary>
        /// Working With the Enumerations
        /// </summary>
        
        Console.WriteLine($"Days of the Week: {Days.Monday}, {Days.Wednesday}, {Days.Friday}");

        Console.WriteLine($"Differents Users Roles : {Role.Admin}, {Role.User}, {Role.Guest}");


        /// <summary>
        ///  String Manipulations
        /// </summary>
        
        string firstName = "Dirane";
        string lastName = "Tsafack";
        string fullName = $"{firstName} {lastName}";
        Console.WriteLine($"Full Name: {fullName}");

        ///<summary>
        /// Using Nullables Types
        /// </summary>
        
        int? nullableInt = null;

        double? nullableDouble = 45.67;

        if (nullableInt.HasValue)
        {
            Console.WriteLine($"Nullable Integer Value: {nullableInt.Value}");
        }
        else
        {
            Console.WriteLine("Nullable Integer is null.");
           
        }

        if (nullableDouble.HasValue)
        {
            Console.WriteLine($"Nullable Double Value: {nullableDouble.Value}");
        }
        else
        {
            Console.WriteLine("Nullable Double is null.");
        }

    }



    enum Days { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };

    enum Role { Admin = 1, User = 2, Guest = 3 };



}