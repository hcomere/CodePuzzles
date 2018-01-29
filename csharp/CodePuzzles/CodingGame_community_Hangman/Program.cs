using System;
using System.Linq;
using System.Collections.Generic;

#if LOCAL
using Validation;
#endif

class Solution
{
    static void Solve()
    {
        string word = Console.ReadLine();
        string lowerWord = word.ToLower();
        string [] chars = Console.ReadLine().Split(' ');

        int errorCount = 0;
        char[] current = Enumerable.Range(0, word.Length).Select(i => word[i] == ' ' ? ' ' : '_').ToArray();

        HashSet<char> used = new HashSet<char>();

        foreach (string c in chars)
        {
            bool found = false;
            bool error = used.Contains(c[0]);

            if (!error)
            {
                for(int i = 0; i < word.Length; ++i)
                {
                    if(current[i] == '_' && lowerWord[i] == c[0])
                    {
                        current[i] = word[i];
                        if (!found) used.Add(c[0]);
                        found = true;
                    }
                }
            }

            if (!found || error)
                ++errorCount;

            if (errorCount >= 6)
                break;
        }

        string[] hangman = new string[4];
        hangman[0] = "+--+";
        hangman[1] = "|";
        hangman[2] = "|";
        hangman[3] = "|\\";

        if (errorCount > 0) hangman[1] += "  o";

        if (errorCount >= 4)
            hangman[2] += " /|\\";
        else if (errorCount >= 3)
            hangman[2] += " /|";
        else if(errorCount >= 2)
            hangman[2] += "  |";
        
        if (errorCount == 6)
            hangman[3] += "/ \\";
        else if (errorCount >= 5)
            hangman[3] += "/";

        Array.ForEach(hangman, line =>
        {
            Console.WriteLine(line);
        });

        Array.ForEach(current, c => Console.Write(c));
        Console.WriteLine("");
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