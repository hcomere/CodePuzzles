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

    static List<Ticket> tickets = new List<Ticket>();
    static List<Road> roads = new List<Road>();
    static HashSet<UInt64> processedMasks = new HashSet<UInt64>();
    static int[] colorCount = new int[9];
    static int trainCarCount = 0;
    static int totalEvals = 0;
    static int maxScore = Int32.MinValue;

    class Road
    {
        public int Length { get; set; }
        public int RequiredEngines { get; set; }
        public int ColorIdx { get; set; }
        public String StartName { get; set; }
        public String EndName { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public Boolean IsBuilt { get; set; }
        public Boolean IsChecked { get; set; }
        public Boolean IsEvaluated { get; set; }
        public int Points { get; }
        public int Index { get; }

        public Road(int a_index, int a_length, int a_requiredEngines, String a_color, String a_startName, String a_endName)
        {
            Length = a_length;
            RequiredEngines = a_requiredEngines;
            StartName = a_startName;
            EndName = a_endName;
            Start = -1;
            End = -1;
            IsBuilt = false;
            IsChecked = false;
            IsEvaluated = false;
            Index = a_index;
            
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
           
            if (Length >= 6) Points = 15;
            else if (Length >= 4) Points = 7;
            else if (Length >= 3) Points = 4;
            else if (Length >= 2) Points = 2;
            else Points = 1;
        }

        public override string ToString()
        {
            return "Road start " + StartName + "(" + Start + ") end " + EndName + "(" + End + ") length " + Length + " color " + ColorIdx + " required engines " + RequiredEngines;
        }
    };

    class Ticket
    {
        public int Points { get; set; }
        public String StartName { get; set; }
        public String EndName { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public List <UInt64> RoadMasks { get; }

        public Ticket(int a_points, String a_startName, String a_endName)
        {
            Points = a_points;
            StartName = a_startName;
            EndName = a_endName;
            Start = -1;
            End = -1;

            RoadMasks = new List<UInt64>();
        }

        private void CheckRoad(int a_current, int a_end, ulong a_roadMask, HashSet <ulong> a_goodMasks)
        {
            for (int i = 0; i < roads.Count; ++i)
            {
                if (roads[i].IsEvaluated)
                    continue;

                int nstart = -1;
                int nend = -1;

                if (roads[i].Start == a_current)
                {
                    nstart = a_current;
                    nend = roads[i].End;
                }
                else if (roads[i].End == a_current)
                {
                    nstart = a_current;
                    nend = roads[i].Start;
                }

                if (nstart >= 0)
                {
                    if (nend == a_end)
                    {
                        ulong mask = a_roadMask | ((UInt64)1 << i);
                        if (!a_goodMasks.Contains(mask))
                        {
                            RoadMasks.Add(mask);
                            a_goodMasks.Add(mask);
                        }
                    }
                    else
                    {
                        roads[i].IsEvaluated = true;
                        CheckRoad(nend, a_end, a_roadMask | ((UInt64)1 << i), a_goodMasks);
                        roads[i].IsEvaluated = false;
                    }
                }
            }
        }

        public void FindRoads()
        {
            HashSet<ulong> goodMasks = new HashSet<ulong>();
            CheckRoad(Start, End, 0, goodMasks);
        }

        internal bool IsHonoredByRoadMask(ulong a_roadMask)
        {
            foreach(ulong roadMask in RoadMasks)
            {
                if ((roadMask & a_roadMask) == roadMask)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "Ticket start " + StartName + "(" + Start + ") end " + EndName + "(" + End + ") worth " + Points + " points";
        }
    };

    public static int Eval(UInt64 a_roadMask, int a_roadPoints)
    {
        /*for (int i = 0; i < 9; ++i)
        {
            if (colorCount[i] < 0)
                Console.Error.WriteLine("Error spotted ! color " + i + ", count " + colorCount[i]);
        }

        if (trainCarCount < 0)
            Console.Error.WriteLine("Error spotted ! train car count : " + trainCarCount);
        */

        ++totalEvals;

        int score = a_roadPoints;

        foreach(Ticket ticket in tickets)
        {
            if (ticket.IsHonoredByRoadMask(a_roadMask))
                score += ticket.Points;
            else
                score -= ticket.Points;
        }

        return score;
    }
    
    public static void ProcessGrayRoad(int a_currentRoadIndex, UInt64 a_roadMask, int a_roadsPoints)
    {
        int usedTrainCar = Math.Min(roads[a_currentRoadIndex].Length, trainCarCount);
        Boolean canBuild = usedTrainCar == roads[a_currentRoadIndex].Length;

        int usedEngine = 0;

        if (canBuild)
        {
            usedEngine = Math.Min(roads[a_currentRoadIndex].RequiredEngines, colorCount[ENGINE]);
            canBuild = usedEngine == roads[a_currentRoadIndex].RequiredEngines;
        }
        
        for (int i = 0; i < 8; ++i)
        {
            int usedColorCard = 0;
            int usedExtraEngine = 0;

            Boolean localCanBuild = canBuild;

            if (localCanBuild)
            {
                usedColorCard = Math.Min(roads[a_currentRoadIndex].Length - usedEngine, colorCount[i]);
                usedExtraEngine = Math.Min(roads[a_currentRoadIndex].Length - usedEngine - usedColorCard, colorCount[ENGINE] - usedEngine);

                localCanBuild = usedColorCard > 0 && (usedEngine + usedColorCard + usedExtraEngine) == roads[a_currentRoadIndex].Length;

                //if (usedEngine + usedColorCard + usedExtraEngine > roads[a_currentRoadIndex].Length)
                //    Console.Error.WriteLine("Error spotted ! Too much cards are used o.O");
            }

            if (localCanBuild)
            {
                trainCarCount -= usedTrainCar;
                roads[a_currentRoadIndex].IsBuilt = true;
                colorCount[i] -= usedColorCard;
                colorCount[ENGINE] -= (usedEngine + usedExtraEngine);

                UInt64 newRoadMask = a_roadMask;
                Compute(a_currentRoadIndex + 1, a_roadMask | ((UInt64)1 << a_currentRoadIndex), a_roadsPoints + roads[a_currentRoadIndex].Points);

                colorCount[ENGINE] += (usedEngine + usedExtraEngine);
                colorCount[i] += usedColorCard;
                roads[a_currentRoadIndex].IsBuilt = false;
                trainCarCount += usedTrainCar;
            }
        }

        Compute(a_currentRoadIndex + 1, a_roadMask, a_roadsPoints);
    }

    public static void ProcessColorRoad(int a_currentRoadIndex, UInt64 a_roadMask, int a_roadsPoints)
    {
        int usedTrainCar = Math.Min(roads[a_currentRoadIndex].Length, trainCarCount);
        Boolean canBuild = usedTrainCar == roads[a_currentRoadIndex].Length;

        int usedColorCard = 0;
        int usedEngine = 0;

        if (canBuild)
        {
            usedColorCard = Math.Min(colorCount[roads[a_currentRoadIndex].ColorIdx], roads[a_currentRoadIndex].Length);
            usedEngine = Math.Min(colorCount[ENGINE], roads[a_currentRoadIndex].Length - usedColorCard);
            canBuild = (usedColorCard + usedEngine) == roads[a_currentRoadIndex].Length;
        }

        if (canBuild)
        {
            trainCarCount -= usedTrainCar;
            roads[a_currentRoadIndex].IsBuilt = true;
            colorCount[roads[a_currentRoadIndex].ColorIdx] -= usedColorCard;
            colorCount[ENGINE] -= usedEngine;

            Compute(a_currentRoadIndex + 1, a_roadMask | ((UInt64)1 << a_currentRoadIndex), a_roadsPoints + roads[a_currentRoadIndex].Points);

            colorCount[ENGINE] += usedEngine;
            colorCount[roads[a_currentRoadIndex].ColorIdx] += usedColorCard;
            roads[a_currentRoadIndex].IsBuilt = false;
            trainCarCount += usedTrainCar;
        }

        Compute(a_currentRoadIndex + 1, a_roadMask, a_roadsPoints);
    }

    static void Compute(int a_currentRoadIndex, UInt64 a_roadMask, int a_roadsPoints)
    {
        if (a_currentRoadIndex == roads.Count)
        {
            if (! processedMasks.Contains(a_roadMask))
            {
                int score = Eval(a_roadMask, a_roadsPoints);
                if (score > maxScore) maxScore = score;
                
                processedMasks.Add(a_roadMask);
            }
        }
        else
        {
            if (roads[a_currentRoadIndex].ColorIdx == GRAY)
                ProcessGrayRoad(a_currentRoadIndex, a_roadMask, a_roadsPoints);
            else
                ProcessColorRoad(a_currentRoadIndex, a_roadMask, a_roadsPoints);     
        }
    }

    static void Solve()
    {
        tickets.Clear();
        roads.Clear();
        processedMasks.Clear();
        totalEvals = 0;
        maxScore = Int32.MinValue;

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
            roads.Add(new Road(i, int.Parse(inputs[0]), int.Parse(inputs[1]), inputs[2], inputs[3], inputs[4]));
        }

        int currentCityIndex = 0;
        Dictionary<String, int> cityMap = new Dictionary<String, int>();

        foreach(Road road in roads)
        {
            if (! cityMap.TryGetValue(road.StartName, out int start))
            {
                start = currentCityIndex++;
                cityMap.Add(road.StartName, start);
            }

            road.Start = start;

            if (!cityMap.TryGetValue(road.EndName, out int end))
            {
                end = currentCityIndex++;
                cityMap.Add(road.EndName, end);
            }

            road.End = end;

            Console.Error.WriteLine(road);
        }

        foreach(Ticket ticket in tickets)
        {
            if (cityMap.TryGetValue(ticket.StartName, out int start))
                ticket.Start = start;
            else
                Console.Error.WriteLine("A ticket reference unknown city " + ticket.StartName + " as start");

            if (cityMap.TryGetValue(ticket.EndName, out int end))
                ticket.End = end;
            else
                Console.Error.WriteLine("A ticket reference unknown city " + ticket.EndName + " as end");

            ticket.FindRoads();
            Console.Error.WriteLine(ticket);
        }

        Compute(0, 0, 0);
        Console.Error.WriteLine("Total evaluations : " + totalEvals);
        Console.WriteLine(maxScore);
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