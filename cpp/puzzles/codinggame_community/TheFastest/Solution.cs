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
    class Time
    {
        public String Def { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
    };

    static void Solve()
    {
        List<Time> times = new List<Time>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            String timeDef = Console.ReadLine();
            String[] components = timeDef.Split(':');
            times.Add(new Time { Def = timeDef, Hours = Int32.Parse(components[0]), Minutes = Int32.Parse(components[1]), Seconds = Int32.Parse(components[2]) });
        }

        times.Sort(delegate (Time t1, Time t2)
        {
            int[] comp1 = new int[3] { t1.Hours, t1.Minutes, t1.Seconds };
            int[] comp2 = new int[3] { t2.Hours, t2.Minutes, t2.Seconds };

            for(int i = 0; i < 3; ++ i)
            {
                if (comp1[i] < comp2[i])
                    return -1;
                else if (comp1[i] > comp2[i])
                    return 1;
            }

            return 0;
        });

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine(times[0].Def);
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