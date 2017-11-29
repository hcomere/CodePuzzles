using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Solve()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int L = int.Parse(inputs[0]);
        int C = int.Parse(inputs[1]);
        int N = int.Parse(inputs[2]);

        int[] G = Enumerable.Range(0, N).Select(v => int.Parse(Console.ReadLine())).ToArray();
        int[] next = Enumerable.Range(0, N).ToArray();
        int[] gain = Enumerable.Range(0, N).ToArray();

        foreach (int i in Enumerable.Range(0, N))
        {
            next[i] = (i+1)%N;
            gain[i] = G[i];
            int count = G[i];

            while(next[i] != i && count + G[next[i]] <= L)
            {
                count += G[next[i]];
                next[i] = (next[i]+1) % N;
            }

            gain[i] = count;
        }

        int turnCount = 0;
        long totalGain = 0;
        int groupIndex = 0;

        while(turnCount < C)
        {
            totalGain += gain[groupIndex];
            groupIndex = next[groupIndex];
            ++turnCount;
        }

        Console.WriteLine(totalGain);
    }

    static void Main(string[] args)
    {
#if LOCAL
        Validation.SolveAction handler = Solve;
        Validation.Validator.ValidateSolution(handler);
#else
        Solve();
#endif
    }
}