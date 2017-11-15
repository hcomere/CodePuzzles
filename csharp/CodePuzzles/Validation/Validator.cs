using System;
using System.Collections.Generic;
using System.IO;

namespace Validation
{
    public delegate void SolveAction();

    public static class Validator
    {
        public static void ValidateSolution(SolveAction a_action)
        {
            String path = "data";

            List<int> foundTests = new List<int>();

            if (Directory.Exists(path))
            {
                string[] fileEntries = Directory.GetFiles(path);

                for (int i = 0; i < 20; ++i)
                {
                    Boolean inputFound = Array.Find(fileEntries, s => s.Equals("data\\input" + i + ".txt")) != null;
                    Boolean outputFound = Array.Find(fileEntries, s => s.Equals("data\\output" + i + ".txt")) != null;

                    if (inputFound && outputFound)
                        foundTests.Add(i);
                }                
            }

            if (foundTests.Count == 0)
                Console.WriteLine("Not Test Case found");
            else
            {
                foreach (int test in foundTests)
                {
                    Console.WriteLine("------ TEST CASE " + test + " ------");

                    Console.SetIn(new StreamReader("data\\input" + test + ".txt"));
                    StreamReader outStreamReader = new StreamReader("data\\output" + test + ".txt");

                    StringWriter resWriter = new StringWriter();
                    Console.SetOut(resWriter);

                    a_action();

                    String outContent = outStreamReader.ReadToEnd();
                    String resContent = resWriter.ToString();

                    StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
                    standardOutput.AutoFlush = true;
                    Console.SetOut(standardOutput);

                    StreamReader standardInput = new StreamReader(Console.OpenStandardInput());
                    Console.SetIn(standardInput);

                    if (outContent.Equals(resContent))
                        Console.WriteLine("GOOD !");
                    else
                    {
                        Console.WriteLine("===== Not good :( =====");
                        Console.WriteLine("--- Found ---");
                        Console.WriteLine(resContent);
                        Console.WriteLine("--- Expected ---");
                        Console.WriteLine(outContent);
                        break;
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
