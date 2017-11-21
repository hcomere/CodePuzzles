using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

////////////////////////////////////////////////////////////////
// https://tech.io/playgrounds/334/genetic-algorithms/history //
////////////////////////////////////////////////////////////////

namespace TechIO_GeneticAlgorithms
{
    class Program
    {
        const float GRADED_RETAIN_PERCENT = 0.3f; // percentage of retained best fitting individuals
        const float NONGRADED_RETAIN_PERCENT = 0.2f; // percentage of retained remaining individuals (randomly selected)
        const int POPULATION_SIZE = 1000;
        const int MAX_GENERATION = 2000;
        const float CHANCE_TO_MUTATE = 50;
        const string g_alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ !'.";
        const string g_anwser = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static Random g_random = new Random();

        static string CreateChromosome(int a_size)
        {
            string chromosome = "";

            foreach (int i in Enumerable.Range(0, a_size))
                chromosome += g_alphabet[g_random.Next(0, g_alphabet.Length)];

            return chromosome;
        }

        static bool IsAnswer(string a_chromosome)
        {
            return String.Compare(a_chromosome, g_anwser) == 0;
        }

        static float GetScore(string a_chromosome)
        {
            int valid = 0;
            foreach (int i in Enumerable.Range(0, a_chromosome.Length))
                if (a_chromosome[i] == g_anwser[i])
                    ++valid;

            return (float) valid / a_chromosome.Length;
        }
       
        static float GetMeanScore(List <string> a_population)
        {
            float mean = (float) 0;
            foreach (string chrom in a_population)
                mean += GetScore(chrom);

            return mean / a_population.Count;
        }

        static List <string> Selection(List <string> a_chromosomes)
        {
            List<Tuple<string, float>> chromsWithScore = new List<Tuple<string, float>>();       
        
            foreach(string chromosome in a_chromosomes)
            {
                float score = GetScore(chromosome);
                chromsWithScore.Add(new Tuple<string, float>(chromosome, score));
            }

            chromsWithScore = chromsWithScore.OrderByDescending(t => t.Item2).ToList();

            List<string> selectedChroms = new List<string>();

            int bestChromsCountToSelect = (int) Math.Floor(a_chromosomes.Count * GRADED_RETAIN_PERCENT);
            int randomChromsCountToSelect = (int)Math.Floor(a_chromosomes.Count * NONGRADED_RETAIN_PERCENT);

            List<bool> selected = Enumerable.Repeat(false, a_chromosomes.Count).ToList();

            foreach (int i in Enumerable.Range(0, bestChromsCountToSelect))
            {
                selectedChroms.Add(chromsWithScore[i].Item1);
                selected[i] = true;
            }

            while(selectedChroms.Count < bestChromsCountToSelect + randomChromsCountToSelect)
            {
                int idx = g_random.Next(bestChromsCountToSelect, a_chromosomes.Count);

                if (!selected[idx])
                {
                    selectedChroms.Add(chromsWithScore[idx].Item1);
                    selected[idx] = true;
                }
            }

            Console.WriteLine("Best chromosome is " + selectedChroms[0]);

            return selectedChroms;
        }

        static string CrossOver(string a_parent1, string a_parent2)
        {
            string part1 = a_parent1.Substring(0, (int)Math.Floor(a_parent1.Length * 0.5f));
            string part2 = a_parent2.Substring((int)Math.Floor(a_parent2.Length * 0.5f));
            return part1 + part2;
        }

        static char GetRandomAlphabetItem()
        {
            int idx = g_random.Next(0, g_alphabet.Length);
            return g_alphabet[idx];
        }

        static string Mutation(string a_chromosome)
        {
            char c = GetRandomAlphabetItem();
            int id = g_random.Next(0, a_chromosome.Length);

            StringBuilder sb = new StringBuilder(a_chromosome);
            sb[id] = c;
            return sb.ToString();
        }

        static List <string> CreatePopulation(int a_popSize, int a_chromSize)
        {
            List<string> chroms = new List<string>();

            foreach (int i in Enumerable.Range(0, a_popSize))
                chroms.Add(CreateChromosome(a_chromSize));

            return chroms;
        }

        static List<string> Generation(List<string> a_population)
        {
            List<string> select = Selection(a_population);
            List<string> children = new List<string>();

            while (children.Count < POPULATION_SIZE - select.Count)
            {
                int idx1 = g_random.Next(0, select.Count);
                int idx2 = g_random.Next(0, select.Count);

                while (idx1 == idx2)
                    idx2 = g_random.Next(0, select.Count);

                string parent1 = select[idx1];
                string parent2 = select[idx2];

                string child = CrossOver(parent1, parent2);

                int mutationTest = g_random.Next(0, 1000);
                if (mutationTest / 1000 <= CHANCE_TO_MUTATE)
                    child = Mutation(child);

                children.Add(child);
            }

            select.AddRange(children);
            return select;
        }

        static void Main(string[] args)
        {
            List<string> population = CreatePopulation(POPULATION_SIZE, g_anwser.Length);
            List<string> answers = new List<string>();

            Console.WriteLine("=== Initial Population ===");
            //foreach (string chrom in population)
            //    Console.WriteLine(chrom);

            int generation = 0;

            while(generation < MAX_GENERATION && answers.Count == 0)
            {
                population = Generation(population);

                Console.WriteLine("==== New Generation " + generation + " ==== " + population.Count + " chromosomes");
                Console.WriteLine("Mean score is : " + GetMeanScore(population));

                foreach(string chrom in population)
                {
                    //Console.WriteLine(chrom);
                    if (IsAnswer(chrom))
                        answers.Add(chrom);
                }

                ++generation;
            }

            if (generation == MAX_GENERATION)
                Console.WriteLine("Fail...");
            else
                Console.WriteLine(answers[0]);

            Console.Read();
        }
    }
}
