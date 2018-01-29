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
        int n = int.Parse(Console.ReadLine());
        string[] inputs = Console.ReadLine().Split(' ');

        IEnumerable<int> vals = Enumerable.Range(0, n).Select(i => int.Parse(inputs[i]));

        int swapCount = 0;

        while (vals.Count() > 0)
        {
            vals = vals.SkipWhile(v => v == 1).Reverse().SkipWhile(v => v == 0).Reverse();

            if (vals.Count() > 0)
            {
                ++swapCount;
                vals = vals.Skip(1).Take(vals.Count() - 2);
            }
        }

        Console.WriteLine(swapCount);

    }

    static void Main(string[] args)
    {
#if LOCAL
        SolveAction handler = Solve;
        Validator.ValidateSolution(handler);
#else
        Solve();
#endif
    }
}