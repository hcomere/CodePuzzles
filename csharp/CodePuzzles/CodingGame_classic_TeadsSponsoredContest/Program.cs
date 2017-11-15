using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    class Node
    {
        public int Id { get; }
        public List<Node> Links { get; }
        public int MaxDepthFromLeaf { get; set; }
        public bool IsUsed { get; set; }

        public Node(int id)
        {
            Id = id;
            Links = new List<Node>();


            MaxDepthFromLeaf = -1;
            IsUsed = false;
        }
    };

    static List<Node> nodes;
    static List<Node> leaves;

    static void Solve()
    {
        nodes = Enumerable.Repeat<Node>(null, 200000).ToList();
        leaves = new List<Node>();

        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            //Console.Error.WriteLine(xi + " " + yi);

            Node n1 = nodes[xi];
            Node n2 = nodes[yi];

            if (n1 == null)
            {
                nodes[xi] = new Node(xi);
                n1 = nodes[xi];
            }

            if (n2 == null)
            {
                nodes[yi] = new Node(yi);
                n2 = nodes[yi];
            }

            n1.Links.Add(n2);
            n2.Links.Add(n1);

            if (n1.Links.Count == 1)
            {
                leaves.Add(n1);
                n1.IsUsed = true;
            }
            else if (n1.Links.Count == 2)
            {
                leaves.Remove(n1);
                n1.IsUsed = false;
            }

            if (n2.Links.Count == 1)
            {
                leaves.Add(n2);
                n2.IsUsed = true;
            }
            else if (n2.Links.Count == 2)
            {
                leaves.Remove(n2);
                n2.IsUsed = false;
            }
        }

        Console.Error.WriteLine("=====");

        HashSet<Node> junctions = new HashSet<Node>();
        List<int> scores = Enumerable.Repeat<int>(-1, 200000).ToList();
        bool isStraight = false;
        List<int> straightScores = new List<int>();

        bool terminated = false;

        while (!terminated)
        {
            junctions.Clear();
            straightScores.Clear();
            straightScores.Add(0);
            //Console.Error.WriteLine("Leaf Count : " + leaves.Count);

            foreach (Node leaf in leaves)
            {
                Node current = leaf;
                Node prev = null;
                int score = scores[leaf.Id] == -1 ? 0 : scores[leaf.Id];

                //Console.Error.WriteLine("Check leaf " + leaf.Id);

                while (!isStraight && (current == leaf || current.Links.Count <= 2))
                {
                    //Console.Error.WriteLine("Current id = " + current.Id + " | links : " + current.Links.Count);

                    current.IsUsed = true;

                    if (scores[current.Id] > -1)
                    {
                        straightScores.Add(straightScores[0] + scores[current.Id] + 1);

                        if (current == leaf || current.Links.Count == 1)
                        {
                            straightScores[0] += scores[current.Id];
                        }
                    }

                    ++straightScores[0];


                    if (current.Links.Count == 2)
                    {
                        if (current.Links[0] == prev)
                        {
                            prev = current;
                            current = current.Links[1];
                        }
                        else if (current.Links[1] == prev)
                        {
                            prev = current;
                            current = current.Links[0];
                        }
                        else
                        {
                            if (prev == null)
                                Console.Error.WriteLine("Error : prev is null");

                            Console.Error.WriteLine("Hm ? prev = " + prev.Id + ", link 0 = " + current.Links[0].Id + ", link 1 = " + current.Links[1].Id);
                        }
                    }
                    else if (current.Links.Count == 1)
                    {
                        if (prev == null)
                        {
                            prev = current;
                            current = current.Links[0];
                        }
                        else
                            isStraight = true;
                    }

                    ++score;
                }

                if (isStraight)
                {
                    terminated = true;
                    break;
                }

                junctions.Add(current);

                scores[current.Id] = Math.Max(score, scores[current.Id]);

                //Console.Error.WriteLine("Found junction at node " + current.Id + ", new score is " + scores[current.Id]);
            }

            if (!isStraight)
            {
                if (junctions.Count == 1)
                    terminated = true;
                else
                {
                    leaves.Clear();
                    //Console.Error.WriteLine("Junction Count : " + junctions.Count);

                    bool doit = junctions.Count == 2;

                    foreach (Node junction in junctions)
                    {
                        for (int linkIdx = junction.Links.Count - 1; linkIdx >= 0; --linkIdx)
                        {
                            if (junction.Links[linkIdx].IsUsed)
                                junction.Links.Remove(junction.Links[linkIdx]);
                        }

                        if (junction.Links.Count == 1)
                            leaves.Add(junction);
                    }
                }
            }
        }

        if (!isStraight)
        {
            if (junctions.Count == 1)
            {
                Console.Error.WriteLine("Only one junction left");
                Console.WriteLine(scores[junctions.First().Id]);
            }
            else
            {
                Console.Error.WriteLine("== Many junctions (" + junctions.Count + ") ==");

                foreach (Node junction in junctions)
                {
                    int notUsedLinks = 0;

                    foreach (Node link in junction.Links)
                        if (!link.IsUsed) ++notUsedLinks;

                    Console.Error.WriteLine(notUsedLinks);
                }
            }
        }
        else
        {
            int maxScore = 0;

            foreach (int score in straightScores)
            {
                maxScore = Math.Max(maxScore, score);
                Console.Error.WriteLine("Straight score = " + score);
            }


            if (maxScore % 2 == 1)
                Console.WriteLine((maxScore - 1) / 2);
            else
                Console.WriteLine(maxScore / 2);
        }
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
