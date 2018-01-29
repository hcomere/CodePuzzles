using System;
using System.Collections.Generic;
using System.Linq;

#if LOCAL
using Validation;
#endif

class Solution
{
    static void Solve()
    {
        int n = int.Parse(Console.ReadLine()); // the number of temperatures to analyse
        string[] inputs = Console.ReadLine().Split(' ');

        Console.WriteLine(Enumerable
            .Range(0, n)
            .Select(i => int.Parse(inputs[i]))
            .Select(t => new Tuple<int, int>(t, Math.Abs(t)))
            .OrderBy(tp => tp.Item2)
            .ThenByDescending(tp => tp.Item1) 
            .Select(tp => tp.Item1)
            .FirstOrDefault()
        );
    }

    static void Main(string[] args)
    {
#if LOCAL
        SolveAction handler = Solve;
        Validation.Validator.ValidateSolution(handler);
#else
        Solve();
#endif
    }
}