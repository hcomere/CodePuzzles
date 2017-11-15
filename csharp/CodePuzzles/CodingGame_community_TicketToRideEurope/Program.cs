using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    public const int RED = 0;
    public const int YELLOW = 1;
    public const int GREEN = 2;
    public const int BLUE = 3;
    public const int WHITE = 4;
    public const int BLACK = 5;
    public const int ORANGE = 6;
    public const int PINK = 7;
    public const int GRAY = 8;
    public const int ENGINE = 8;

    class Ticket
    {
        public int Points { get; set; }
        public String Start { get; set; }
        public String End { get; set; }

        public Ticket(int a_points, String a_start, String a_end)
        {
            Points = a_points;
            Start = a_start;
            End = a_end;
        }
    };

    class Road
    {
        public int Length { get; set; }
        public int RequiredEngines { get; set; }
        public int ColorIdx { get; set; }
        public String Start { get; set; }
        public String End { get; set; }
        public Boolean IsBuilt { get; set; }
        public Boolean IsChecked { get; set; }
        public Boolean IsEvaluated { get; set; }

        public Road(int a_length, int a_requiredEngines, String a_color, String a_start, String a_end)
        {
            Length = a_length;
            RequiredEngines = a_requiredEngines;
            Start = a_start;
            End = a_end;
            IsBuilt = false;
            IsChecked = false;
            IsEvaluated = false;

            // 0 red 1 yellow 2 green 3 blue 4 white 5 black 6 orange 7 pink 8 engine 
            if (a_color.Equals("Red")) ColorIdx = RED;
            else if (a_color.Equals("Yellow")) ColorIdx = YELLOW;
            else if (a_color.Equals("Green")) ColorIdx = GREEN;
            else if (a_color.Equals("Blue")) ColorIdx = BLUE;
            else if (a_color.Equals("White")) ColorIdx = WHITE;
            else if (a_color.Equals("Black")) ColorIdx = BLACK;
            else if (a_color.Equals("Orange")) ColorIdx = ORANGE;
            else if (a_color.Equals("Pink")) ColorIdx = PINK;
            else if (a_color.Equals("Gray")) ColorIdx = GRAY;
            else
                Console.Error.WriteLine("Unknown road color : " + a_color);
        }
    };

    static List<Ticket> tickets = new List<Ticket>();
    static List<Road> roads = new List<Road>();
    static int[] colorCount = new int[9];
    static int trainCarCount = 0;

    public static int ConvertLengthToPoints(int a_length)
    {
        int pts = 0;

        if (a_length >= 6) pts = 15;
        else if (a_length >= 4) pts = 7;
        else if (a_length >= 3) pts = 4;
        else if (a_length >= 2) pts = 2;
        else pts = 1;

        return pts;
    }

    public static Boolean CheckRoad(String a_current, String a_end)
    {
        Boolean ok = false;

        for (int i = 0; i < roads.Count; ++i)
        {
            if (roads[i].IsEvaluated)
                continue;

            String nstart = null;
            String nend = null;

            if (roads[i].Start.Equals(a_current))
            {
                nstart = a_current;
                nend = roads[i].End;
            }
            else if (roads[i].End.Equals(a_current))
            {
                nstart = a_current;
                nend = roads[i].Start;
            }

            if (nstart != null)
            {
                Console.Error.WriteLine("Use road " + nstart + " to " + nend);

                if (nend.Equals(a_end))
                {
                    Console.Error.WriteLine("End reached !!");
                    ok = true;
                    break;
                }
                else
                {
                    roads[i].IsEvaluated = true;
                    ok = CheckRoad(nend, a_end);
                    roads[i].IsEvaluated = false;
                }
            }
        }

        return ok;
    }

    public static int Eval()
    {
        Console.Error.WriteLine("=== Eval ==== " + tickets.Count + " tickets");

        int score = 0;

        for (int i = 0; i < tickets.Count; ++i)
        {
            Console.Error.WriteLine("Ticket " + i + " : " + tickets[i].Start + " to " + tickets[i].End + " for " + tickets[i].Points + " pts");

            Boolean ok = CheckRoad(tickets[i].Start, tickets[i].End);

            if (ok)
            {
                Console.Error.WriteLine("Ticket success");
                score += tickets[i].Points;
            }
            else
            {
                Console.Error.WriteLine("Ticket failure");
                score -= tickets[i].Points;
            }
        }

        for (int i = 0; i < roads.Count; ++i)
        {
            if (roads[i].IsBuilt)
            {
                int pts = ConvertLengthToPoints(roads[i].Length);
                Console.Error.WriteLine("Built road " + roads[i].Start + " to " + roads[i].End + " gives " + pts + " points");
                score += pts;
            }
        }

        Console.Error.WriteLine("Score = " + score);

        return score;
    }

    public static int ProcessGrayRoad(int a_roadIndex, int a_checkedRoadCount)
    {
        int maxScore = 0;
        Boolean needInit = true;

        int usedTrainCar = Math.Min(roads[a_roadIndex].Length, trainCarCount);
        Boolean canBuild = usedTrainCar == roads[a_roadIndex].Length;

        int usedEngine = 0;

        if (canBuild)
        {
            usedEngine = Math.Min(roads[a_roadIndex].RequiredEngines, colorCount[ENGINE]);
            canBuild = usedEngine == roads[a_roadIndex].RequiredEngines;
        }

        Boolean hasBeenBuiltOnce = false;

        for (int i = 0; i < 8; ++i)
        {
            int usedColorCard = 0;
            int usedExtraEngine = 0;

            Boolean localCanBuild = canBuild;

            if (localCanBuild)
            {
                usedColorCard = Math.Min(roads[a_roadIndex].Length - usedEngine, colorCount[i]);
                usedExtraEngine = Math.Min(roads[a_roadIndex].Length - usedEngine - usedColorCard, colorCount[ENGINE]);
                localCanBuild = (usedEngine + usedColorCard + usedExtraEngine) == roads[a_roadIndex].Length;
            }

            if (localCanBuild)
            {
                Console.Error.WriteLine("Can build gray road " + roads[a_roadIndex].Start + " to " + roads[a_roadIndex].End + " using color " + i);
                hasBeenBuiltOnce = true;

                trainCarCount -= usedTrainCar;
                roads[a_roadIndex].IsBuilt = true;
                colorCount[i] -= usedColorCard;
                colorCount[ENGINE] = colorCount[ENGINE] - usedEngine - usedExtraEngine;

                int score = Compute(a_checkedRoadCount + 1);

                if (needInit || score > maxScore)
                {
                    maxScore = score;
                    needInit = false;
                }

                trainCarCount += usedTrainCar;
                roads[a_roadIndex].IsBuilt = false;
                colorCount[i] += usedColorCard;
                colorCount[ENGINE] += usedEngine + usedExtraEngine;
            }
            //else
            //    Console.Error.WriteLine("CANNOT build gray road " + roads[a_roadIndex).start + " to " + roads[a_roadIndex).end + " using color " + i); 

        }

        if (!hasBeenBuiltOnce)
            maxScore = Compute(a_checkedRoadCount + 1);

        return maxScore;
    }

    public static int ProcessColorRoad(int a_roadIndex, int a_checkedRoadCount)
    {
        int usedTrainCar = Math.Min(roads[a_roadIndex].Length, trainCarCount);
        Boolean canBuild = usedTrainCar == roads[a_roadIndex].Length;

        int usedColorCard = 0;
        int usedEngine = 0;

        if (canBuild)
        {
            usedColorCard = Math.Min(colorCount[roads[a_roadIndex].ColorIdx], roads[a_roadIndex].Length);
            usedEngine = Math.Min(colorCount[ENGINE], roads[a_roadIndex].Length - usedColorCard);
            canBuild = (usedColorCard + usedEngine) == roads[a_roadIndex].Length;
        }

        if (canBuild)
        {
            trainCarCount -= usedTrainCar;
            roads[a_roadIndex].IsBuilt = true;
            colorCount[roads[a_roadIndex].ColorIdx] -= usedColorCard;
            colorCount[ENGINE] -= usedEngine;
            Console.Error.WriteLine("Can build color road " + roads[a_roadIndex].ColorIdx + " " + roads[a_roadIndex].Start + " to " + roads[a_roadIndex].End);
        }
        else
        {
            //Console.Error.WriteLine("CANNOT build color " + roads[a_roadIndex].ColorIdx + " " + roads[a_roadIndex).start + " to " + roads[a_roadIndex).end);
            //if(trainCarCount < roads[a_roadIndex].Length) Console.Error.WriteLine("Not enough train cars");
            //else if(colorCount[roads[a_roadIndex].ColorIdx] < roads[a_roadIndex].Length) Console.Error.WriteLine("Not enough color cards");
        }


        int score = Compute(a_checkedRoadCount + 1);

        if (canBuild)
        {
            trainCarCount += usedTrainCar;
            roads[a_roadIndex].IsBuilt = false;
            colorCount[roads[a_roadIndex].ColorIdx] += usedColorCard;
            colorCount[ENGINE] += usedEngine;
        }

        return score;
    }

    public static int Compute(int a_checkedRoadCount)
    {
        int maxScore = 0;
        Boolean needInit = true;

        if (a_checkedRoadCount == roads.Count)
        {
            Console.Error.WriteLine("Permutation found");
            maxScore = Eval();
        }
        else
        {
            int score = 0;

            for (int i = 0; i < roads.Count; ++i)
            {
                if (roads[i].IsChecked)
                    continue;

                roads[i].IsChecked = true;

                if (roads[i].ColorIdx == GRAY)
                    score = ProcessGrayRoad(i, a_checkedRoadCount);
                else
                    score = ProcessColorRoad(i, a_checkedRoadCount);

                if (needInit || score > maxScore)
                {
                    needInit = false;
                    maxScore = score;
                }

                roads[i].IsChecked = false;
            }
        }

        return maxScore;
    }

    static void Solve()
    {
        tickets.Clear();
        roads.Clear();

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        trainCarCount = int.Parse(inputs[0]);
        Console.Error.WriteLine("Train car count : " + trainCarCount);
        
        int numTickets = int.Parse(inputs[1]);
        int numRoutes = int.Parse(inputs[2]);

        inputs = Console.ReadLine().Split(' ');
        Console.Error.WriteLine("-- Colors --");
        for (int i = 0; i < 9; ++i)
        {
            colorCount[i] = int.Parse(inputs[i]);
            Console.Error.Write(colorCount[i] + " ");
        }
        Console.Error.WriteLine("");


        for (int i = 0; i < numTickets; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            tickets.Add(new Ticket(int.Parse(inputs[0]), inputs[1], inputs[2]));
        }

        for (int i = 0; i < numRoutes; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            roads.Add(new Road(int.Parse(inputs[0]), int.Parse(inputs[1]), inputs[2], inputs[3], inputs[4]));
            Console.Error.WriteLine(roads[roads.Count - 1]);
        }
        
        Console.WriteLine(Compute(0));
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