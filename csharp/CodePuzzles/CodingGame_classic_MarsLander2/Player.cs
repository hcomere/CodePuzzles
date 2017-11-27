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

    const int TURN_TO_SIMULATE_COUNT = 10;
    const float GRADED_RETAIN_PERCENT = 0.3f;
    const float NONGRADED_RETAIN_PERCENT = 0.2f;

    const int POPULATION_SIZE = 500;
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
        
        public ShuttleState()
        {
            Position = new Vector2(0, 0);
            Speed = new Vector2(0, 0);
        }
    }

    class TurnResult
    {
        public ShuttleState State { get; set; }
        
        public TurnResult()
        {
            State = new ShuttleState();
        }
    }

    class FlatGround
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        public FlatGround()
        {
            Start = new Vector2(0, 0);
            End = new Vector2(0, 0);
        }

        public override string ToString()
        {
            return "FlatGround : (" + Start.X + "," + Start.Y + ") to (" + End.X + "," + End.Y + ")";
        }
    }

    static TurnResult PlayTurn(ShuttleState a_state, TurnAction a_action)
    {
        TurnResult result = new TurnResult();

        int newAngle = Math.Max(Math.Min(a_state.Angle + a_action.AngleDelta, 90), -90);
        int newThrust = Math.Max(Math.Min(a_state.Thrust + a_action.ThrustDelta, 4), 0);

        //Console.Error.WriteLine(">> " + newAngle);

        double newVx = a_state.Speed.X + Math.Cos(newAngle * Math.PI / 180) * newThrust;
        double newVy = a_state.Speed.Y - GRAVITY + Math.Sin(newAngle * Math.PI / 180) * newThrust;
        double newPosX = a_state.Position.X + newVx;
        double newPosY = a_state.Position.Y + newVy;

        result.State.Angle = newAngle;
        result.State.Speed.Set(newVx, newVy);
        result.State.Fuel = a_state.Fuel - newThrust;
        result.State.Position.Set(newPosX, newPosY);
        result.State.Thrust = newThrust;

        return result;
    }

    static double Evaluate(ShuttleState a_state, FlatGround a_flatGround)
    {
        double score = 0;

        if (a_state.Position.X > a_flatGround.Start.X && a_state.Position.X < a_flatGround.End.X)
            score += 10000;
        else
        {
            double dist = Math.Min(Math.Abs(a_state.Position.X - a_flatGround.Start.X), Math.Abs(a_state.Position.X - a_flatGround.End.X));
            //Console.Error.WriteLine("dist : " + dist);
            score += 10000 - dist;
        }

        return score;
    }

    static double GetScore(Chromosome a_chromosome, ShuttleState a_state, FlatGround a_flatGround)
    {
        ShuttleState state = a_state;

        //Console.Error.WriteLine("=====");

        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            //Console.Error.WriteLine(a_chromosome.TurnActions[i].AngleDelta);
            TurnResult turnResult = PlayTurn(state, a_chromosome.TurnActions[i]);
            state = turnResult.State;
        }

        return Evaluate(state, a_flatGround);
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

        return chroms;
    }

    static void Mutation(Chromosome a_chromosome)
    {
        int turn = g_random.Next(0, TURN_TO_SIMULATE_COUNT);
        int comp = g_random.Next(0, 2);
        int minValue = comp == 0 ? -MAX_ANGLE_DELTA : -MAX_THRUST_DELTA;
        int maxValue = comp == 0 ? MAX_ANGLE_DELTA : MAX_THRUST_DELTA;
        int val = g_random.Next(0, maxValue);

        if (comp == 0)
            a_chromosome.TurnActions[turn].AngleDelta = val;
        else
            a_chromosome.TurnActions[turn].ThrustDelta = val;
    }

    static List<Chromosome> Selection(List<Chromosome> a_chromosomes, ShuttleState a_state, FlatGround a_flatGround)
    {
        List<Tuple<Chromosome, double>> chromsWithScore = new List<Tuple<Chromosome, double>>();

        foreach (Chromosome chromosome in a_chromosomes)
        {
            double score = GetScore(chromosome, a_state, a_flatGround);
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

    static List<Chromosome> Generation(List<Chromosome> a_population, ShuttleState a_state, FlatGround a_flatGround)
    {
        List<Chromosome> select = Selection(a_population, a_state, a_flatGround);
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

        FlatGround flatGround = new FlatGround();
        bool found = false;

        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.

            Console.Error.WriteLine(landX + " " + landY);

            if (i == 0)
            {
                flatGround.Start.X = landX;
                flatGround.Start.Y = landY;
            }
            else if(! found)
            {
                if(flatGround.Start.Y == landY)
                {
                    flatGround.End.X = landX;
                    flatGround.End.Y = landY;
                    found = true;
                }
                else
                {
                    flatGround.Start.X = landX;
                    flatGround.Start.Y = landY;
                }
            }
        }

        Console.Error.WriteLine(flatGround);

        // game loop
        while (true)
        {
            Console.Error.WriteLine("===========");

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

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            List<Chromosome> population = CreatePopulation(POPULATION_SIZE);

            int generation = 0;

            while (stopWatch.ElapsedMilliseconds < 100)
            {
                //Console.Error.WriteLine("Elapsed time = " + elapsedTime + " | population size = " + population.Count);

                population = Generation(population, state, flatGround);
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
