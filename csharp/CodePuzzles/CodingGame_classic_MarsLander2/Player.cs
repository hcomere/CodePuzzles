using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    const int ZONE_WIDTH = 7000;
    const int ZONE_HEIGHT = 3000;
    const int FLAT_GROUP_MIN_WIDTH = 1000;
    const int MIN_ANGLE = -90;
    const int MAX_ANGLE = 90;
    const int MAX_ANGLE_DELTA = 15;
    const int MIN_THRUST = 0;
    const int MAX_THRUST = 4;
    const int MAX_THRUST_DELTA = 1;
    const double GRAVITY = 3.711;
    const int TARGET_LANDING_ANGLE = 0;
    const int TARGET_LANDING_MAX_VERTICAL_SPEED = 40;
    const int TARGET_LANDING_MAX_HORIZONTAL_SPEED = 20;

    const float GRADED_RETAIN_PERCENT = 0.3f;
    const float NONGRADED_RETAIN_PERCENT = 0.2f;

    const int TURN_TO_SIMULATE_COUNT = 2;
    const int POPULATION_SIZE = 10;
    const int MUTATION_PROBABILITY = 2;

    static Random g_random = new Random();
    
    class Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double a_x, double a_y)
        {
            X = a_x;
            Y = a_y;
        }

        public void Copy(Vector2 a_toCopy)
        {
            X = a_toCopy.X;
            Y = a_toCopy.Y;
        }

        public void Set(double a_x, double a_y)
        {
            X = a_x;
            Y = a_y;
        }

        public double Length2()
        {
            return X * X + Y * Y;
        }

        public double Length()
        {
            return Math.Sqrt(Length2());
        }

        public void Normalize()
        {
            double length = Length();
            X /= length;
            Y /= length;
        }

        public static double Distance2(Vector2 a_v1, Vector2 a_v2)
        {
            return (a_v2.X - a_v1.X) * (a_v2.X - a_v1.X) + (a_v2.Y - a_v1.Y) * (a_v2.Y - a_v1.Y);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }

    class TurnAction
    {
        public int AngleDelta { get; set; }
        public int ThrustDelta { get; set; }

        public void Copy(TurnAction a_toCopy)
        {
            AngleDelta = a_toCopy.AngleDelta;
            ThrustDelta = a_toCopy.ThrustDelta;
        }
    }

    class Chromosome
    {
        public TurnAction[] TurnActions { get; }

        public Chromosome()
        {
            TurnActions = Enumerable.Range(0, TURN_TO_SIMULATE_COUNT).Select(i => new TurnAction()).ToArray();
        }

        public override string ToString()
        {
            string str = "Chromosome :";
            foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
                str += " (" + TurnActions[i].AngleDelta + "," + TurnActions[i].ThrustDelta + ")";
            return str;
        }
    }

    class ShuttleState
    {
        public Vector2 Position { get; set; }
        public int Fuel { get; set; }
        public int Angle { get; set; }
        public Vector2 Speed { get; set; }
        public int Thrust { get; set; }
        public bool Alive { get; set; }
        public bool Landed { get; set; }
        
        public ShuttleState()
        {
            Position = new Vector2(0, 0);
            Speed = new Vector2(0, 0);
            Alive = true;
            Landed = false;
        }

        public override string ToString()
        {
            return "ShuttleState  : Pos(" + Position.X + "," + Position.Y + ") Speed(" + Speed.X + "," + Speed.Y + ") Fuel(" + Fuel + ") Angle(" + Angle + ") Thrust(" + Thrust + ")";
        }
    }

    class Line
    {
        public double StartX { get; private set; }
        public double EndX { get; private set; }
        public double StartY { get; private set; }
        public double EndY { get; private set; }
        public double MiddleX { get; private set; }
        public double MiddleY { get; private set; }

        private double A { get; set; }
        private double B { get; set; }
        
        public Line(double a_startX, double a_startY, double a_endX, double a_endY)
        {
            StartX = a_startX;
            StartY = a_startY;
            EndX = a_endX;
            EndY = a_endY;

            MiddleX = (a_startX + a_endX) * 0.5;
            MiddleY = (a_startY + a_endY) * 0.5;

            A = (EndY - StartY) / (EndX - StartX);
            B = StartY - StartX * A;
        }

        public bool IsFlat()
        {
            return StartY == EndY;
        }

        public override string ToString()
        {
            return "Line (" + StartX + "," + StartY + ") to (" + EndX + "," + EndY + ")";
        }

        public bool CollideWith(double a_startX, double a_startY, double a_endX, double a_endY)
        {
            if (Math.Max(StartX, EndX) < Math.Min(a_startX, a_endX))
                return false;

            double a = (a_endY - a_startY) / (a_endX - a_startX);
            double b = a_startY - a_startX * a;

            if (A == a)
                return false;

            double x = (b - B) / (A - a);
            double y = A * x + B;

            double xmin = Math.Max(Math.Min(StartX, EndX), Math.Min(a_startX, a_endX));
            double xmax = Math.Min(Math.Max(StartX, EndX), Math.Max(a_startX, a_endX));

            return x >= xmin && x <= xmax;
        }
    }

    static ShuttleState PlayTurn(ShuttleState a_state, TurnAction a_action, List <Line> a_groundLines)
    {
        ShuttleState result = new ShuttleState();

        int newAngle = Math.Max(Math.Min(a_state.Angle + a_action.AngleDelta, MAX_ANGLE), MIN_ANGLE);
        int newThrust = Math.Max(Math.Min(a_state.Thrust + a_action.ThrustDelta, MAX_THRUST), MIN_THRUST);

        //Console.Error.WriteLine(">> " + newAngle);

        double xdirection = newAngle >= 0 ? -1 : 1;

        double newVx = a_state.Speed.X + xdirection * Math.Sin(newAngle * Math.PI / 180) * newThrust;
        double newVy = a_state.Speed.Y - GRAVITY + Math.Cos(newAngle * Math.PI / 180) * newThrust;
        double newPosX = a_state.Position.X + newVx;
        double newPosY = a_state.Position.Y + newVy;

        result.Angle = newAngle;
        result.Speed.Set(newVx, newVy);
        result.Fuel = a_state.Fuel - newThrust;
        result.Position.Set(newPosX, newPosY);
        result.Thrust = newThrust;

        // Check intersection with ground

        int collidedLineIndex = a_groundLines.FindIndex(gl => gl.CollideWith(a_state.Position.X, a_state.Position.Y, newPosX, newPosY));

        if (collidedLineIndex != -1)
        {
            if(a_groundLines[collidedLineIndex].IsFlat())
            {
                if (result.Angle == TARGET_LANDING_ANGLE && result.Speed.X <= TARGET_LANDING_MAX_HORIZONTAL_SPEED && result.Speed.Y <= TARGET_LANDING_MAX_VERTICAL_SPEED)
                    result.Landed = true;
                else
                    result.Alive = false;
            }
            else
                result.Alive = false;
        }

        return result;
    }

    static double GetScore(Chromosome a_chromosome, ShuttleState a_state, List<Line> a_groundLines, int a_flatGroundLineIdx)
    {
        ShuttleState state = a_state;

        Console.Error.WriteLine("- Get score for " + a_chromosome + ", initial pos is " + a_state.Position);

        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            //Console.Error.WriteLine(a_chromosome.TurnActions[i].AngleDelta);
            state = PlayTurn(state, a_chromosome.TurnActions[i], a_groundLines);
            
            if (state.Fuel <= 0 || ! state.Alive || state.Landed)
                break;
        }

        // Score about landed state or not

        double landedScore = state.Landed ? 1 : 0;

        // Score about distance on X axis to middle of flat ground

        double xdist = Math.Abs(state.Position.X - a_groundLines[a_flatGroundLineIdx].MiddleX);
        double xdistScore = 1 - xdist / ZONE_WIDTH;

        // Score about distance on X axis to middle of flat ground

        double ydist = Math.Abs(state.Position.Y - a_groundLines[a_flatGroundLineIdx].MiddleY);
        double ydistScore = 1 - ydist / ZONE_HEIGHT;

        // Final weighted score
        // Solution that are landed wins
        // Favorise first solution close to the land area on x axis
        // Favorise the solutions close to the land area on y axis

        double score = 3 * landedScore + 2 * xdistScore + ydistScore;

        Console.Error.WriteLine("Final state : " + state);
        Console.Error.WriteLine("Score is " + score + " (" + landedScore + ", " + xdistScore + ", " + ydistScore);

        return score;
    }

    static Chromosome CreateChromosome()
    {
        Chromosome chromosome = new Chromosome();
        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            chromosome.TurnActions[i].AngleDelta = g_random.Next(-MAX_ANGLE_DELTA, MAX_ANGLE_DELTA + 1);
            chromosome.TurnActions[i].ThrustDelta = g_random.Next(-MAX_THRUST_DELTA, MAX_THRUST_DELTA + 1);

            //Console.Error.WriteLine("??? " + chromosome.TurnActions[i].AngleDelta);
        }

        /*foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            Console.Error.WriteLine("oooo " + chromosome.TurnActions[i].AngleDelta);
        }*/



        return chromosome;
    }

    static List<Chromosome> CreatePopulation(int a_popSize)
    {
        List<Chromosome> chroms = new List<Chromosome>();

        foreach (int i in Enumerable.Range(0, a_popSize))
            chroms.Add(CreateChromosome());

        Console.Error.WriteLine("=== NEW POP ===");
        chroms.ForEach(c => Console.Error.WriteLine(c));

        return chroms;
    }

    static void Mutation(Chromosome a_chromosome)
    {
        int turn = g_random.Next(0, TURN_TO_SIMULATE_COUNT);
        int comp = g_random.Next(0, 2);
        int minValue = comp == 0 ? -MAX_ANGLE_DELTA : -MAX_THRUST_DELTA;
        int maxValue = comp == 0 ? MAX_ANGLE_DELTA : MAX_THRUST_DELTA;
        int val = g_random.Next(minValue, maxValue+1);

        if (comp == 0)
            a_chromosome.TurnActions[turn].AngleDelta = val;
        else
            a_chromosome.TurnActions[turn].ThrustDelta = val;
    }

    static List<Chromosome> Selection(List<Chromosome> a_chromosomes, ShuttleState a_state, List<Line> a_groundLines, int a_flatGroundLineIdx)
    {
        List<Tuple<Chromosome, double>> chromsWithScore = new List<Tuple<Chromosome, double>>();

        foreach (Chromosome chromosome in a_chromosomes)
        {
            double score = GetScore(chromosome, a_state, a_groundLines, a_flatGroundLineIdx);
            chromsWithScore.Add(new Tuple<Chromosome, double>(chromosome, score));
        }

        chromsWithScore = chromsWithScore.OrderByDescending(t => t.Item2).ToList();

        /*
        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            Console.Error.WriteLine("--- Best ---");
            Console.Error.WriteLine(chromsWithScore[0].Item1);
        }*/

        List<Chromosome> selectedChroms = new List<Chromosome>();

        int bestChromsCountToSelect = (int)Math.Floor(a_chromosomes.Count * GRADED_RETAIN_PERCENT);
        int randomChromsCountToSelect = (int)Math.Floor(a_chromosomes.Count * NONGRADED_RETAIN_PERCENT);

        //Console.Error.WriteLine("" + bestChromsCountToSelect + " chroms to select from bests");
        //Console.Error.WriteLine("" + randomChromsCountToSelect + " chroms to select randomly");

        List<bool> selected = Enumerable.Repeat(false, a_chromosomes.Count).ToList();

        foreach (int i in Enumerable.Range(0, bestChromsCountToSelect))
        {
            selectedChroms.Add(chromsWithScore[i].Item1);
            selected[i] = true;
        }

        while (selectedChroms.Count < bestChromsCountToSelect + randomChromsCountToSelect)
        {
            int idx = g_random.Next(bestChromsCountToSelect, a_chromosomes.Count);

            if (!selected[idx])
            {
                selectedChroms.Add(chromsWithScore[idx].Item1);
                selected[idx] = true;
            }
        }

        return selectedChroms;
    }

    static Chromosome CrossOver(Chromosome a_parent1, Chromosome a_parent2)
    {
        Chromosome child = new Chromosome();

        if (TURN_TO_SIMULATE_COUNT % 2 == 0)
        {
            foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT / 2))
                child.TurnActions[i].Copy(a_parent1.TurnActions[i]);
            foreach (int i in Enumerable.Range(TURN_TO_SIMULATE_COUNT / 2, TURN_TO_SIMULATE_COUNT / 2))
                child.TurnActions[i].Copy(a_parent2.TurnActions[i]);
        }
        else
        {
            foreach (int i in Enumerable.Range(0, (TURN_TO_SIMULATE_COUNT - 1) / 2))
                child.TurnActions[i].Copy(a_parent1.TurnActions[i]);
            foreach (int i in Enumerable.Range((TURN_TO_SIMULATE_COUNT - 1) / 2 + 1, (TURN_TO_SIMULATE_COUNT - 1) / 2))
                child.TurnActions[i].Copy(a_parent2.TurnActions[i]);

            int middleIdx = (TURN_TO_SIMULATE_COUNT - 1) / 2;
            child.TurnActions[middleIdx].AngleDelta = a_parent1.TurnActions[middleIdx].AngleDelta;
            child.TurnActions[middleIdx].ThrustDelta = a_parent2.TurnActions[middleIdx].ThrustDelta;
        }

        return child;
    }

    static List<Chromosome> Generation(List<Chromosome> a_population, ShuttleState a_state, List <Line> a_groundLines, int a_flatGroundLineIdx)
    {
        List<Chromosome> select = Selection(a_population, a_state, a_groundLines, a_flatGroundLineIdx);

        //Console.Error.WriteLine("--- SELECTION ---");
        //select.ForEach(c => Console.Error.WriteLine(c));

        List<Chromosome> children = new List<Chromosome>();

        while (children.Count < POPULATION_SIZE - select.Count)
        {
            int idx1 = g_random.Next(0, select.Count);
            int idx2 = g_random.Next(0, select.Count);

            while (idx1 == idx2)
                idx2 = g_random.Next(0, select.Count);

            Chromosome parent1 = select[idx1];
            Chromosome parent2 = select[idx2];

            Chromosome child = CrossOver(parent1, parent2);

            int mutationTest = g_random.Next(0, 100);
            if (mutationTest <= MUTATION_PROBABILITY)
                Mutation(child);

            children.Add(child);
        }

        select.AddRange(children);
        return select;
    }


    static void Main(string[] args)
    {
        string[] inputs;
        int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

        List<Line> groundLines = new List<Line>();

        int prevLandX = -1;
        int prevLandY = -1;
        int flatGroundLineIdx = -1;

        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');

            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
        
            if (i > 0)
            {
                if (landY == prevLandY)
                    flatGroundLineIdx = groundLines.Count;

                groundLines.Add(new Line(prevLandX, prevLandY, landX, landY));
            }

            prevLandX = landX;
            prevLandY = landY;
        }

        Console.Error.WriteLine("Flat ground : " + groundLines[flatGroundLineIdx]);

        // game loop
        while (true)
        {
            //Console.Error.WriteLine("===========");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            ShuttleState state = new ShuttleState();

            inputs = Console.ReadLine().Split(' ');
            state.Position.X = int.Parse(inputs[0]);
            state.Position.Y = int.Parse(inputs[1]);
            state.Speed.X = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            state.Speed.Y = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            state.Fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            state.Angle = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            state.Thrust = int.Parse(inputs[6]); // the thrust power (0 to 4).

            Console.Error.WriteLine(state);

            List<Chromosome> population = CreatePopulation(POPULATION_SIZE);

            int generation = 0;

            //while (stopWatch.ElapsedMilliseconds < 100)
            while(generation < 2)
            {
                //Console.Error.WriteLine("Elapsed time = " + elapsedTime + " | population size = " + population.Count);

                population = Generation(population, state, groundLines, flatGroundLineIdx);
                ++generation;
            }

            Console.Error.WriteLine(generation + " generations");
            Console.Error.WriteLine("Best action is " + population[0].TurnActions[0].AngleDelta + " " + population[0].TurnActions[0].ThrustDelta);

            int angle = Math.Max(Math.Min(state.Angle + population[0].TurnActions[0].AngleDelta, 90), -90);
            int thrust = Math.Max(Math.Min(state.Thrust + population[0].TurnActions[0].ThrustDelta, 4), 0);
            Console.WriteLine(angle + " " + thrust);
        }
    }
}
