using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if LOCAL
using Validation;
#endif

class Solution
{
    static void Solve()
    {
        // Code Here
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