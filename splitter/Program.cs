using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ganss.IO;

namespace splitter
{
    /// <summary>The main entry point to the splitter application.</summary>
    public class Program
    {
        /// <summary>The main entry point to the splitter application.</summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            var splitter = new Splitter();
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-v":
                            splitter.Verbose = true;
                            break;
                        default:
                            Console.Error.WriteLine($"Unknown option {arg} ignored.");
                            break;
                    }
                }
                else
                {
                    try
                    {
                        foreach (var filename in Glob.ExpandNames(arg))
                        {
                            try
                            {
                                splitter.Split(filename);
                            }
                            catch (Exception exception)
                            {
                                Console.Error.WriteLine($"{filename}: {exception.Message}");
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine($"{exception.Message}");
                    }
                }
            }
        }
    }
}
