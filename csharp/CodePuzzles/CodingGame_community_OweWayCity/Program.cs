using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

#if LOCAL
using Validation;
#endif

class Solution
{
    static void Solve()
    {
        int M = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());

        Console.Error.WriteLine(M + " WE, " + N + " NS");

        if (M > N)
        {
            Console.Error.WriteLine("SWAP");
            int tmp = M;
            M = N;
            N = tmp;
        }

        string res = "";

        if (M == 1 || N == 1)
            res = "1";
        else
        {
            List<BigInteger> prevRow = Enumerable.Range(0, N).Select(v => new BigInteger(0)).ToList();

            for (int row = 0; row < M; ++row)
            {
                for (int col = row; col < N; ++col)
                {
                    //Console.Error.WriteLine(row + " " + col);

                    if (row == col)
                    {
                        if (row != 0)
                            prevRow[col] *= 2;
                    }
                    else if (row == 0)
                        prevRow[col] = 1;
                    else
                        prevRow[col] += prevRow[col - 1];
                }
            }

            res = prevRow[N - 1].ToString("");
        }

        Console.WriteLine(res);
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