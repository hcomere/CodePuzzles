using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    const int MAP_WIDTH = 16000;
    const int MAP_HEIGHT = 9000;
    const int ASH_MAX_SPEED = 1000;
    const int ASH_RADIUS = 2000;
    const int ASH_RADIUS_2 = ASH_RADIUS * ASH_RADIUS;
    const int ZOMBIE_SPEED = 400;
    const int ZOMBIE_SPEED_2 = ZOMBIE_SPEED * ZOMBIE_SPEED;
    const int POPULATION_SIZE = 1000;
    const int MUTATION_PROBABILITY = 90;
    const float GRADED_RETAIN_PERCENT = 0.4f;
    const float NONGRADED_RETAIN_PERCENT = 0.1f;
    const int TURN_TO_SIMULATE_COUNT = 10;

    static Random g_random = new Random();
    static int[] g_fibonacci = new int[20] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946 };

    class Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double a_x, double a_y)
        {
            X = a_x;
            Y = a_y;
        }

        public static double Distance2(Vector2 a_v1, Vector2 a_v2)
        {
            return (a_v2.X - a_v1.X) * (a_v2.X - a_v1.X) + (a_v2.Y - a_v1.Y) * (a_v2.Y - a_v1.Y);
        }

        public void Copy(Vector2 a_toCopy)
        {
            X = a_toCopy.X;
            Y = a_toCopy.Y;
        }
    }

    class TurnResult
    {
        public int Score { get; set; }
        public List<Vector2> RemainingZombies { get; set; }
        public List<Vector2> RemainingHumans { get; set; }
        public Vector2 AshPosition { get; set; }

        public TurnResult()
        {
            Score = -1;
            RemainingZombies = new List<Vector2>();
            RemainingHumans = new List<Vector2>();
            AshPosition = new Vector2(0, 0);
        }
    }

    class Chromosome
    {
        public Vector2[] TurnTargets { get; }
           
        public Chromosome()
        {
            TurnTargets = Enumerable.Repeat(new Vector2(0, 0), TURN_TO_SIMULATE_COUNT).ToArray();
        }
    }

    static Chromosome CreateChromosome(Vector2 a_ashPosition)
    {
        Chromosome chromosome = new Chromosome();
        foreach(int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
            chromosome.TurnTargets[i] = new Vector2(g_random.Next(0, MAP_WIDTH), g_random.Next(0, MAP_HEIGHT));
        return chromosome;
    }

    static List <Chromosome> CreatePopulation(Vector2 a_ashPosition, List <Vector2> a_nextZombies, int a_popSize)
    {
        List<Chromosome> chroms = new List<Chromosome>();

        foreach (int i in Enumerable.Range(0, a_popSize))
            chroms.Add(CreateChromosome(a_ashPosition));

        return chroms;
    }

    static void Mutation(Chromosome a_chromosome)
    {
        int turn = g_random.Next(0, TURN_TO_SIMULATE_COUNT);
        int comp = g_random.Next(0, 2);
        int maxValue = comp == 0 ? MAP_WIDTH : MAP_HEIGHT;
        int val = g_random.Next(0, maxValue);

        if (comp == 0)
            a_chromosome.TurnTargets[turn].X = val;
        else
            a_chromosome.TurnTargets[turn].Y = val;
    }

    static List <Vector2> ComputeZombiesNextPositions(Vector2 a_ash, List <Vector2> a_zombies, List <Vector2> a_humans)
    {
        Vector2 target = null;

        List<Vector2> nextZombies = new List<Vector2>();

        foreach (Vector2 zombie in a_zombies)
        {
            double minDist2 = (zombie.X - a_ash.X) * (zombie.X - a_ash.X) + (zombie.Y - a_ash.Y) * (zombie.Y - a_ash.Y);
            target = a_ash;

            foreach (Vector2 human in a_humans)
            {
                double dist2 = (zombie.X - human.X) * (zombie.X - human.X) + (zombie.Y - human.Y) * (zombie.Y - human.Y);
                if (dist2 < minDist2)
                {
                    minDist2 = dist2;
                    target = human;
                }
            }

            Vector2 nextZombie = new Vector2(target.X - zombie.X, target.Y - zombie.Y);
            double length = Math.Sqrt(nextZombie.X * nextZombie.X + nextZombie.Y * nextZombie.Y);

            if (length <= ZOMBIE_SPEED)
            {
                nextZombie.X = target.X;
                nextZombie.Y = target.Y;
            }
            else
            {
                nextZombie.X = zombie.X + (int)Math.Floor((nextZombie.X / length) * ZOMBIE_SPEED);
                nextZombie.Y = zombie.Y + (int)Math.Floor((nextZombie.Y / length) * ZOMBIE_SPEED);
            }

            nextZombies.Add(nextZombie);
        }

        return nextZombies;
    }

    static Vector2 ComputeAshNextPosition(Vector2 a_ash, Vector2 a_target)
    {
        Vector2 nextAsh = new Vector2(a_target.X - a_ash.X, a_target.Y - a_ash.Y);
        double length = Math.Sqrt(nextAsh.X * nextAsh.X + nextAsh.Y * nextAsh.Y);
        if (length > ASH_MAX_SPEED)
        {
            nextAsh.X = a_ash.X + (int)Math.Floor((nextAsh.X / length) * ASH_MAX_SPEED);
            nextAsh.Y = a_ash.Y + (int)Math.Floor((nextAsh.Y / length) * ASH_MAX_SPEED);
        }
        else
        {
            nextAsh.X = a_target.X;
            nextAsh.Y = a_target.Y;
        }

        return nextAsh;
    }
    
    static TurnResult PlayTurn(Vector2 a_ash, Vector2 a_ashTarget, List<Vector2> a_zombies, List<Vector2> a_humans)
    {
        List<Vector2> nextZombies = ComputeZombiesNextPositions(a_ash, a_zombies, a_humans);

        TurnResult result = new TurnResult();

        result.AshPosition = ComputeAshNextPosition(a_ash, a_ashTarget);

        int killCount = 0;
        double minDist2 = double.MaxValue;

        foreach(Vector2 zombie in nextZombies)
        {
            double dist2 = Vector2.Distance2(result.AshPosition, zombie);
            if (dist2 <= ASH_RADIUS_2)
                ++killCount;
            else
            {
                result.RemainingZombies.Add(zombie);
                if (dist2 < minDist2)
                    minDist2 = dist2;
            }
        }

        int zombieScore = a_humans.Count * a_humans.Count * 10;
        result.Score = 0;
        foreach (int i in Enumerable.Range(0, killCount))
            result.Score += zombieScore * g_fibonacci[i];

        foreach(Vector2 human in a_humans)
        {
            bool killed = false;
            foreach(Vector2 zombie in result.RemainingZombies)
            {
                if(human.X == zombie.X && human.Y == zombie.Y)
                {
                    killed = true;
                    break;
                }
            }

            if (!killed)
                result.RemainingHumans.Add(human);
        }
       
        return result;
    }

    static double GetScore(Chromosome a_chromosome, Vector2 a_ashPosition, List<Vector2> a_zombies, List <Vector2> a_humans)
    {
        Vector2 currentAshPosition = a_ashPosition;
        List<Vector2> currentZombies = a_zombies;
        List<Vector2> currentHumans = a_humans;

        double score = 0;

        foreach(int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            TurnResult turnResult = PlayTurn(currentAshPosition, a_chromosome.TurnTargets[i], currentZombies, currentHumans);

            currentAshPosition = turnResult.AshPosition;
            currentZombies = turnResult.RemainingZombies;
            currentHumans = turnResult.RemainingHumans;

            if(currentHumans.Count == 0)
            {
                score = Int32.MinValue;
                break;
            }

            if(currentZombies.Count == 0)
            {
                score = Int32.MaxValue;
                break;
            }

            score += turnResult.Score;
        }

        return score;
    }

    static List<Chromosome> Selection(List<Chromosome> a_chromosomes, Vector2 a_ashPosition, List<Vector2> a_zombies, List <Vector2> a_humans)
    {
        List<Tuple<Chromosome, double>> chromsWithScore = new List<Tuple<Chromosome, double>>();

        foreach (Chromosome chromosome in a_chromosomes)
        {
            double score = GetScore(chromosome, a_ashPosition, a_zombies, a_humans);
            chromsWithScore.Add(new Tuple<Chromosome, double>(chromosome, score));
        }

        chromsWithScore = chromsWithScore.OrderByDescending(t => t.Item2).ToList();

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

        //Console.WriteLine("Best chromosome is " + selectedChroms[0]);

        return selectedChroms;
    }

    static Chromosome CrossOver(Chromosome a_parent1, Chromosome a_parent2)
    {
        Chromosome child = new Chromosome();
        
        if(TURN_TO_SIMULATE_COUNT % 2 == 0)
        {
            foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT / 2))
                child.TurnTargets[i].Copy(a_parent1.TurnTargets[i]);
            foreach (int i in Enumerable.Range(TURN_TO_SIMULATE_COUNT / 2, TURN_TO_SIMULATE_COUNT / 2))
                child.TurnTargets[i].Copy(a_parent2.TurnTargets[i]);
        }
        else
        {
            foreach(int i in Enumerable.Range(0, (TURN_TO_SIMULATE_COUNT - 1) / 2))
                child.TurnTargets[i].Copy(a_parent1.TurnTargets[i]);
            foreach (int i in Enumerable.Range((TURN_TO_SIMULATE_COUNT - 1) / 2 + 1, (TURN_TO_SIMULATE_COUNT - 1) / 2))
                child.TurnTargets[i].Copy(a_parent2.TurnTargets[i]);

            int middleIdx = (TURN_TO_SIMULATE_COUNT - 1) / 2;
            child.TurnTargets[middleIdx].X = a_parent1.TurnTargets[middleIdx].X;
            child.TurnTargets[middleIdx].Y = a_parent2.TurnTargets[middleIdx].Y;
        }

        return child;
    }

    static List<Chromosome> Generation(List<Chromosome> a_population, Vector2 a_ashPosition, List<Vector2> a_zombies, List<Vector2> a_humans)
    {
        List<Chromosome> select = Selection(a_population, a_ashPosition, a_zombies, a_humans);
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

        Vector2 ash = new Vector2(0, 0);
        List<Vector2> humans = new List<Vector2>();
        List<Vector2> zombies = new List<Vector2>();
        List<Vector2> nextZombies = new List<Vector2>();
        
        // game loop
        while (true)
        {
            Console.Error.WriteLine("===========");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            inputs = Console.ReadLine().Split(' ');

            humans.Clear();
            zombies.Clear();
            nextZombies.Clear(); 

            ash.X = int.Parse(inputs[0]);
            ash.Y = int.Parse(inputs[1]);

            int humanCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int humanId = int.Parse(inputs[0]);
                int humanX = int.Parse(inputs[1]);
                int humanY = int.Parse(inputs[2]);
                humans.Add(new Vector2(humanX, humanY));
            }
            int zombieCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zombieId = int.Parse(inputs[0]);
                int zombieX = int.Parse(inputs[1]);
                int zombieY = int.Parse(inputs[2]);
                int zombieXNext = int.Parse(inputs[3]);
                int zombieYNext = int.Parse(inputs[4]);
                zombies.Add(new Vector2(zombieX, zombieY));
                nextZombies.Add(new Vector2(zombieXNext, zombieYNext));
            }

            List <Chromosome> population = CreatePopulation(ash, nextZombies, POPULATION_SIZE);

            int generation = 0;
            
            while(stopWatch.ElapsedMilliseconds < 100)
            {
                //Console.Error.WriteLine("Elapsed time = " + elapsedTime + " | population size = " + population.Count);

                population = Generation(population, ash, zombies, humans);
                ++generation;
            }

            Console.Error.WriteLine(generation + " generations");
            
            Console.WriteLine(population[0].TurnTargets[0].X + " " + population[0].TurnTargets[0].Y);
        }
    }
}