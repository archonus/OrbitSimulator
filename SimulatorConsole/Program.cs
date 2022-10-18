using System;
using System.Collections.Generic;

namespace OrbitSimulator.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

        }

        public static void Print_Array<T>(IEnumerable<T> array, string separator = ",")
        {
            string print_s = string.Join(separator, array);
            Console.WriteLine($"[{print_s}]");

        }

        /// <summary>
        /// Compares two double values to a specified number of decimal places
        /// </summary>
        /// <param name="left">The first value</param>
        /// <param name="right">The second value</param>
        /// <param name="digits">The precision of the comparison; default to 3 d.p.</param>
        /// <returns>True if left and right are same to 3 d.p.</returns>
        public static bool CompareDoubles(double left, double right, int digits = 3)
        {
            var roundedLeft = Math.Round(left, digits, MidpointRounding.AwayFromZero); //Normal rounding
            var roundedRight = Math.Round(right, digits, MidpointRounding.AwayFromZero);
            return roundedLeft == roundedRight;
        }

        public static bool AssertThrows<T>(Action action) where T : Exception
        {
            bool pass = false;
            try
            {
                action();
            }
            catch (T)
            {
                Console.WriteLine($"Test threw expected exception of {typeof(T)}");
                pass = true;
            }
            catch (AggregateException e)
            {
                if (e.InnerException is T)
                {
                    Console.WriteLine($"Test threw expected exception of {typeof(T)}");
                    pass = true;
                }
                else
                {
                    Console.WriteLine($"Test threw exception of {e.InnerException.GetType()} with message {e.Message}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Test threw exception of {e.GetType()} and with message {e.Message}");
                pass = false;
            }
            return pass;
        }
    }
}
