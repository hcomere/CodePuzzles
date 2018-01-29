using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;

#if LOCAL
using Validation;
#endif

class Solution
{
    static BigInteger Factoriel(BigInteger a_nb)
    {
        if (a_nb == 0)
            return 0;

        BigInteger res = 1;
        BigInteger curr = a_nb;

        while (curr > 1)
        {
            res *= curr;
            --curr;
        }

        return res;
    }

    static void Solve()
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        int M = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());

        string res;

        if (M == 1 || N == 1)
            res = "1";
        else
        {
            BigInteger pathLen = (M - 1) + (N - 1);
            BigInteger result = Factoriel(pathLen) / (Factoriel(M - 1) * Factoriel(pathLen - (M - 1)));
            res = result.ToString();
        }

        if (res.Length <= 1000)
            Console.WriteLine(res);
        else
            Console.WriteLine(res.Substring(0, 1000));

        Console.Error.WriteLine("Done in " + stopWatch.ElapsedMilliseconds + " ms");
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