using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
#if LOCAL
        if (System.Diagnostics.Debugger.IsAttached)
        Console.SetIn(new StreamReader("data/input1.txt"));
        StreamReader outStreamReader = new StreamReader("data/output1.txt");

        StringWriter resWriter = new StringWriter();
        Console.SetOut(resWriter);
#endif

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int trainCars = int.Parse(inputs[0]);
        int numTickets = int.Parse(inputs[1]);
        int numRoutes = int.Parse(inputs[2]);
        inputs = Console.ReadLine().Split(' ');
        int red = int.Parse(inputs[0]);
        int yellow = int.Parse(inputs[1]);
        int green = int.Parse(inputs[2]);
        int blue = int.Parse(inputs[3]);
        int white = int.Parse(inputs[4]);
        int black = int.Parse(inputs[5]);
        int orange = int.Parse(inputs[6]);
        int pink = int.Parse(inputs[7]);
        int engine = int.Parse(inputs[8]);
        for (int i = 0; i < numTickets; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int points = int.Parse(inputs[0]);
            string cityA = inputs[1];
            string cityB = inputs[2];
        }
        for (int i = 0; i < numRoutes; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int length = int.Parse(inputs[0]);
            int requiredEngines = int.Parse(inputs[1]);
            string color = inputs[2];
            string cityA = inputs[3];
            string cityB = inputs[4];
        }

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine("points");

#if LOCAL
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
        }
        Console.ReadKey();
#endif
    }
}