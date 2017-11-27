using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Player
{
    /***************************************************************
     * CONSTANTS
     ***************************************************************/
    private const double MIN_IMPULSE = 30.0;
    private const double IMPULSE_COEFF = 0.5;
    private const double EPSILON = 0.00001;

    private const int TURN_TO_SIMULATE_COUNT = 2;
    private const int POPULATION_SIZE = 25;
    private const int MUTATION_PROBABILITY = 15;
    private const float GRADED_RETAIN_PERCENT = 0.3f;
    private const float NONGRADED_RETAIN_PERCENT = 0.2f;

    private const int MAP_RADIUS = 6000;

    private const int PLAYER_COUNT = 3;
    private const int MY_ID = 0;
    private const int ENNEMY1_ID = 1;
    private const int ENNEMY2_ID = 2;

    private const int REAPER_TYPE = 0;
    private const int DESTROYER_TYPE = 1;
    private const int DOOF_TYPE = 2;
    private const int TANKER_TYPE = 3;
    private const int WRECK_TYPE = 4;

    public const int THROTTLE_MIN = 0;
    public const int THROTTLE_MAX = 300;
    public const int TANKER_THROTTLE = 500;
    public const double REAPER_FRICTION = 0.2f;
    public const double DESTROYER_FRICTION = 0.3f;
    public const double DOOF_FRICTION = 0.25f;

    public const int ANGLE_SELECTION_STEP = 10;
    public const int THROTTLE_SELECTION_STEP = 10;

    public const int GRENADE_RADIUS = 1000;
    public const int GRENADE_COST = 60;

    public const int MAX_WATER_IN_ONE_TURN = 5;
    public const int MAX_WATER_IN_ALL_TURNS = MAX_WATER_IN_ONE_TURN * TURN_TO_SIMULATE_COUNT;
    public static readonly int[] WATER_POINTS_BY_TURN = new int[3] {3, 2, 1};
    public static readonly int MAX_WATER_POINTS = 0;

    public const int MAX_TANKER_COLLISION_IN_ONE_TURN = 5;
    public const int MAX_TANKER_COLLISION_IN_ALL_TURNS = MAX_TANKER_COLLISION_IN_ONE_TURN * TURN_TO_SIMULATE_COUNT;
    public static readonly int[] TANKER_COLLISION_POINTS_BY_TURN = new int[3] { (int)Math.Pow(MAX_TANKER_COLLISION_IN_ONE_TURN + 1, 2), MAX_TANKER_COLLISION_IN_ONE_TURN + 1, 1 };
    public static readonly int MAX_TANKER_COLLISION_POINTS = 0;

    static Player()
    {
        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            MAX_WATER_POINTS += MAX_WATER_IN_ONE_TURN * WATER_POINTS_BY_TURN[i];
            MAX_TANKER_COLLISION_POINTS += MAX_TANKER_COLLISION_IN_ONE_TURN * TANKER_COLLISION_POINTS_BY_TURN[i];
        }
    }

    /***************************************************************
     * GLOBALS 
     ****************************************************************/

    static Random g_random = new Random();

    /***************************************************************
     * CLASSES
     ***************************************************************/

    class Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Distance(Vector2 p)
        {
            return Math.Sqrt(Distance2(p));
        }

        public double Distance2(Vector2 p)
        {
            return (X - p.X) * (X - p.X) + (Y - p.Y) * (Y - p.Y);
        }

        public void Set(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Normalize()
        {
            double length = Length();
            X /= length;
            Y /= length;
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double Length2()
        {
            return X * X + Y * Y;
        }

        public static double DotProduct(Vector2 a_v1, Vector2 a_v2)
        {
            return a_v1.X * a_v2.X + a_v1.Y * a_v2.Y;
        }

        public static Vector2 operator- (Vector2 a_v1, Vector2 a_v2)
        {
            return new Vector2(a_v1.X - a_v2.X, a_v1.Y - a_v2.Y);
        }

        public static Vector2 operator *(Vector2 a_v1, double a_scalar)
        {
            return new Vector2(a_v1.X * a_scalar, a_v1.Y * a_scalar);
        }

        public Vector2 Clone()
        {
            return new Vector2(X, Y);
        }
    }

    class Unit
    {
        public int Id { get; }
        public int Type { get; }
        public float Mass { get; }
        public int Radius { get; }
        public Vector2 Position { get; set; }
        public Vector2 Speed { get; set; }
        public int WaterQuantity { get; set; }
        public int WaterCapacity { get; set; }

        public Unit(int a_id, int a_type, float a_mass, int a_radius, Vector2 a_position, Vector2 a_speed, int a_waterQuantity, int a_waterCapacity)
        {
            Id = a_id;
            Type = a_type;
            Mass = a_mass;
            Radius = a_radius;
            Position = a_position;
            Speed = a_speed;
            WaterQuantity = a_waterQuantity;
            WaterCapacity = a_waterCapacity;
        }

        public void ApplyAction(TurnAction a_action)
        {
            double angleRadians = a_action.Angle * Math.PI / 180;
            Speed.X += Math.Cos(angleRadians) * a_action.Throttle;
            Speed.Y += Math.Sin(angleRadians) * a_action.Throttle;
        }

        public void Move(double a_t)
        {
            Position.X += Speed.X * a_t;
            Position.Y += Speed.Y * a_t;
        }

        public static double GetAngle(Unit a_unit, Vector2 a_target)
        {
            Vector2 AB = new Vector2(1, 0);
            Vector2 AC = a_unit.Position - a_target;
            AC.Normalize();

            double angle = Math.Acos(Vector2.DotProduct(AB, AC)) * 180 / Math.PI;

            if (a_unit.Position.Y > a_target.Y)
                angle = 360 - angle;

            return angle;
        }

        public Unit Clone()
        {
            return new Unit(Id, Type, Mass, Radius, Position.Clone(), Speed.Clone(), WaterQuantity, WaterCapacity);
        }

        public static bool IsPointInUnitRange(Vector2 a_point, Unit a_unit)
        {
            double dist2 = a_point.Distance2(a_unit.Position);
            return dist2 < a_unit.Radius * a_unit.Radius;
        }

        // Search the next collision with an other unit
        public Collision GetCollision(Unit u)
        {
            // Check instant collision
            if (Position.Distance2(u.Position) <= (Radius + u.Radius) * (Radius + u.Radius))
            {
                return new Collision
                {
                    T = 0.0,
                    Unit1 = this,
                    Unit2 = u
                };
            }

            // Both units are motionless
            if (Speed.X == 0.0 && Speed.Y == 0.0 && u.Speed.X == 0.0 && u.Speed.Y == 0.0)
            {
                return null;
            }

            // Change referencial
            // Unit u is not at point (0, 0) with a speed vector of (0, 0)
            double x2 = Position.X - u.Position.X;
            double y2 = Position.Y - u.Position.Y;
            double r2 = Radius + u.Radius;
            double vx2 = Speed.X - u.Speed.X;
            double vy2 = Speed.Y - u.Speed.Y;

            // Resolving: sqrt((x + t*vx)^2 + (y + t*vy)^2) = radius <=> t^2*(vx^2 + vy^2) + t*2*(x*vx + y*vy) + x^2 + y^2 - radius^2 = 0
            // at^2 + bt + c = 0;
            // a = vx^2 + vy^2
            // b = 2*(x*vx + y*vy)
            // c = x^2 + y^2 - radius^2 

            double a = vx2 * vx2 + vy2 * vy2;

            if (a <= 0.0)
            {
                return null;
            }

            double b = 2.0 * (x2 * vx2 + y2 * vy2);
            double c = x2 * x2 + y2 * y2 - r2 * r2;
            double delta = b * b - 4.0 * a * c;

            if (delta < 0.0)
            {
                return null;
            }

            double t = (-b - Math.Sqrt(delta)) / (2.0 * a);

            if (t <= 0.0)
            {
                return null;
            }

            return new Collision
            {
                T = t,
                Unit1 = this,
                Unit2 = u
            };
        }

        public void bounce(Unit u)
        {
            double mcoeff = (Mass + u.Mass) / (Mass * u.Mass);
            double nx = Position.X - u.Position.X;
            double ny = Position.Y - u.Position.Y;
            double nxnysquare = nx * nx + ny * ny;
            double dvx = Speed.X - u.Speed.X;
            double dvy = Speed.Y - u.Speed.Y;
            double product = (nx * dvx + ny * dvy) / (nxnysquare * mcoeff);
            double fx = nx * product;
            double fy = ny * product;
            double m1c = 1.0 / Mass;
            double m2c = 1.0 / u.Mass;

            Speed.X -= fx * m1c;
            Speed.Y -= fy * m1c;
            u.Speed.X += fx * m2c;
            u.Speed.Y += fy * m2c;

            fx = fx * IMPULSE_COEFF;
            fy = fy * IMPULSE_COEFF;

            // Normalize vector at min or max impulse
            double impulse = Math.Sqrt(fx * fx + fy * fy);
            double coeff = 1.0;
            if (impulse > EPSILON && impulse < MIN_IMPULSE)
            {
                coeff = MIN_IMPULSE / impulse;
            }

            fx = fx * coeff;
            fy = fy * coeff;

            Speed.X -= fx * m1c;
            Speed.Y -= fy * m1c;
            u.Speed.X += fx * m2c;
            u.Speed.Y += fy * m2c;

            double diff = (Position.Distance(u.Position) - Radius - u.Radius) / 2.0;
            if (diff <= 0.0)
            {
                // Unit overlapping. Fix positions.
                MoveTo(u.Position, diff - EPSILON);
                u.MoveTo(Position, diff - EPSILON);
            }
        }

        // Move the point to an other point for a given distance
        public void MoveTo(Vector2 p, double distance)
        {
            double d = Position.Distance(p);

            if (d < EPSILON)
            {
                return;
            }

            double dx = p.X - Position.X;
            double dy = p.Y - Position.Y;
            double coef = distance / d;

            Position.X += dx * coef;
            Position.Y += dy * coef;
        }
    }

    class PlayerAttributes
    {
        public int Score { get; set; }
        public int Rage { get; set; }
        public Unit ReaperUnit { get; set; }
        public Unit DestroyerUnit { get; set; }
        public Unit DoofUnit { get; set; }

        public PlayerAttributes()
        {
            Score = 0;
            Rage = 0;
            ReaperUnit = null;
            DestroyerUnit = null;
            DoofUnit = null;
        }

        public PlayerAttributes Clone()
        {
            return new PlayerAttributes
            {
                Score = Score,
                Rage = Rage,
                ReaperUnit = ReaperUnit.Clone(),
                DestroyerUnit = DestroyerUnit.Clone(),
                DoofUnit = DoofUnit?.Clone()
            };
        }
    }

    class GayaAttributes
    {
        public List<Unit> Wrecks { get; }
        public List<Unit> Tankers { get; }

        public GayaAttributes()
        {
            Wrecks = new List<Unit>();
            Tankers = new List<Unit>();
        }

        public void InvalidateWrecksAndTankers()
        {
            foreach (Unit wreck in Wrecks)
                wreck.WaterQuantity = 0;

            foreach (Unit tanker in Tankers)
                tanker.WaterQuantity = 0;
        }

        public void ClearInvalidWrecksAndTankers()
        {
            Wrecks.RemoveAll(wreck => wreck.WaterQuantity == 0);
            Tankers.RemoveAll(tanker => tanker.WaterQuantity == 0);
        }
    }

    class TurnAction
    {
        public int Angle { get; set; }
        public int Throttle { get; set; }

        public TurnAction()
        {
            Angle = 0;
            Throttle = 0;
        }

        public void Copy(TurnAction a_other)
        {
            Angle = a_other.Angle;
            Throttle = a_other.Throttle;
        }
    }

    class Chromosome
    {
        public TurnAction[] ReaperTurnActions { get; }
        //public TurnAction[] DestroyerTurnActions { get; }

        public Chromosome()
        {
            ReaperTurnActions = Enumerable.Range(0, TURN_TO_SIMULATE_COUNT).Select(i => new TurnAction()).ToArray();
            //DestroyerTurnActions = Enumerable.Range(0, TURN_TO_SIMULATE_COUNT).Select(i => new TurnAction()).ToArray();
        }
    }

    class SimuResult
    {
        public double WaterPoints { get; set; }
        public double TankerCollisionPoints { get; set; }

        public SimuResult()
        {
            WaterPoints = 0;
            TankerCollisionPoints = 0;
        }
    }

    class Collision
    {
        public double T { get; set; }
        public Unit Unit1 { get; set; }
        public Unit Unit2 { get; set; }
    }

    /***************************************************************
     * UTIL METHODS
     ***************************************************************/

    static public int Round(double x)
    {
        int s = x < 0 ? -1 : 1;
        return s * (int)Math.Round(s * x);
    }

    /***************************************************************
     * EVALUATION METHODS
     ***************************************************************/

    static Collision GetNextCollision(List <Unit> a_tankers, PlayerAttributes [] a_players)
    {
        Collision result = null;

        a_tankers.ForEach(t =>
        {
            Collision collision = a_players[0].ReaperUnit.GetCollision(t);
            if (result == null || (collision != null && result.T > collision.T))
                result = collision;
        });

        foreach(int i in Enumerable.Range(0, 3))
        {
            Collision collision = null;

            if(i > 0)
            {
                collision = a_players[0].ReaperUnit.GetCollision(a_players[i].ReaperUnit);
                if (result == null || (collision != null && result.T > collision.T))
                    result = collision;
            }

            collision = a_players[0].ReaperUnit.GetCollision(a_players[i].DestroyerUnit);
            if (result == null || (collision != null && result.T > collision.T))
                result = collision;

            if (a_players[i].DoofUnit != null)
            {
                collision = a_players[0].ReaperUnit.GetCollision(a_players[i].DoofUnit);
                if (result == null || (collision != null && result.T > collision.T))
                    result = collision;
            }
        }

        return result;
    }

    //static void PlayTurn(int a_turnIndex, TurnAction a_reaperAction, TurnAction a_destroyerAction, PlayerAttributes [] a_players, List <Unit> a_wrecks, List <Unit> a_tankers, SimuResult a_simuResult)
    static void PlayTurn(int a_turnIndex, TurnAction a_reaperAction, PlayerAttributes[] a_players, List<Unit> a_wrecks, List<Unit> a_tankers, SimuResult a_simuResult)
    {
        // Apply throttle

        int reaperDx = Round(Math.Cos(a_reaperAction.Angle * Math.PI / 180));
        int reaperDy = Round(Math.Sin(a_reaperAction.Angle * Math.PI / 180));
        //int destroyerDx = Round(Math.Cos(a_destroyerAction.Angle * Math.PI / 180));
        //int destroyerDy = Round(Math.Sin(a_destroyerAction.Angle * Math.PI / 180));

        a_players[0].ReaperUnit.Speed.X = (int)a_players[0].ReaperUnit.Speed.X + reaperDx * (a_reaperAction.Throttle / a_players[0].ReaperUnit.Mass);
        a_players[0].ReaperUnit.Speed.Y = (int)a_players[0].ReaperUnit.Speed.Y + reaperDy * (a_reaperAction.Throttle / a_players[0].ReaperUnit.Mass);
        //a_players[0].DestroyerUnit.Speed.X = (int)a_players[0].DestroyerUnit.Speed.X + destroyerDx * (a_destroyerAction.Throttle / a_players[0].DestroyerUnit.Mass);
        //a_players[0].DestroyerUnit.Speed.Y = (int)a_players[0].DestroyerUnit.Speed.Y + destroyerDy * (a_destroyerAction.Throttle / a_players[0].DestroyerUnit.Mass);

        // Play all collisions

        double time = 0.0;
        Collision collision = GetNextCollision(a_tankers, a_players);

        while (collision != null && collision.T + time <= 1.0)
        {
            double del = collision.T;

            Array.ForEach(a_players, player =>
            {
                player.ReaperUnit.Position.X = (int)player.ReaperUnit.Position.X + player.ReaperUnit.Speed.X * del;
                player.ReaperUnit.Position.Y = (int)player.ReaperUnit.Position.Y + player.ReaperUnit.Speed.Y * del;

                player.DestroyerUnit.Position.X = (int)player.DestroyerUnit.Position.X + player.DestroyerUnit.Speed.X * del;
                player.DestroyerUnit.Position.Y = (int)player.DestroyerUnit.Position.Y + player.DestroyerUnit.Speed.Y * del;

                if (player.DoofUnit != null)
                {
                    player.DoofUnit.Position.X = (int)player.DoofUnit.Position.X + player.DoofUnit.Speed.X * del;
                    player.DoofUnit.Position.Y = (int)player.DoofUnit.Position.Y + player.DoofUnit.Speed.Y * del;
                }
            });

            a_tankers.ForEach(t =>
            {
                t.Position.X = (int)t.Position.X + Round(t.Speed.X * del);
                t.Position.Y = (int)t.Position.Y + Round(t.Speed.Y * del);
            });

            time += collision.T;

            if (collision.Unit1.Type == TANKER_TYPE && collision.Unit2.Type == DESTROYER_TYPE)
            {
                a_wrecks.Add(new Unit(-1, WRECK_TYPE, -1, collision.Unit1.Radius, collision.Unit1.Position, new Vector2(0, 0), collision.Unit1.WaterQuantity, -1));
                a_tankers.Remove(collision.Unit1);
            }
            else if (collision.Unit2.Type == TANKER_TYPE && collision.Unit1.Type == DESTROYER_TYPE)
            {
                a_wrecks.Add(new Unit(-1, WRECK_TYPE, -1, collision.Unit2.Radius, collision.Unit2.Position, new Vector2(0, 0), collision.Unit2.WaterQuantity, -1));
                a_tankers.Remove(collision.Unit2);
            }
            else
                collision.Unit1.bounce(collision.Unit2);

            collision = GetNextCollision(a_tankers, a_players);
        }

        if(time < 1)
        {
            double del = 1 - time;

            Array.ForEach(a_players, player =>
            {
                player.ReaperUnit.Position.X = (int)player.ReaperUnit.Position.X + player.ReaperUnit.Speed.X * del;
                player.ReaperUnit.Position.Y = (int)player.ReaperUnit.Position.Y + player.ReaperUnit.Speed.Y * del;

                player.DestroyerUnit.Position.X = (int)player.DestroyerUnit.Position.X + player.DestroyerUnit.Speed.X * del;
                player.DestroyerUnit.Position.Y = (int)player.DestroyerUnit.Position.Y + player.DestroyerUnit.Speed.Y * del;

                if (player.DoofUnit != null)
                {
                    player.DoofUnit.Position.X = (int)player.DoofUnit.Position.X + player.DoofUnit.Speed.X * del;
                    player.DoofUnit.Position.Y = (int)player.DoofUnit.Position.Y + player.DoofUnit.Speed.Y * del;
                }
            });

            a_tankers.ForEach(t =>
            {
                t.Position.X = (int)t.Position.X + Round(t.Speed.X * del);
                t.Position.Y = (int)t.Position.Y + Round(t.Speed.Y * del);
            });
        }

        int waterCollected = 0;

        a_wrecks.ForEach(w =>
        {
            if(Unit.IsPointInUnitRange(a_players[0].ReaperUnit.Position, w))
            {
                ++waterCollected;
                --w.WaterQuantity;
            }

            foreach (int i in Enumerable.Range(1, 2))
            {
                if(Unit.IsPointInUnitRange(a_players[i].ReaperUnit.Position, w))
                    --w.WaterQuantity;
            }
        });

        a_wrecks.RemoveAll(w => w.WaterQuantity <= 0);

        // Apply friction

        Array.ForEach(a_players, player =>
        {
            player.ReaperUnit.Speed = player.ReaperUnit.Speed * (1 - REAPER_FRICTION);
            player.DestroyerUnit.Speed = player.DestroyerUnit.Speed * (1 - DESTROYER_FRICTION);
            if(player.DoofUnit != null)
                player.DoofUnit.Speed = player.DoofUnit.Speed * (1 - DOOF_FRICTION);
        });

        // Compute intermediary score.

        a_simuResult.WaterPoints += waterCollected * WATER_POINTS_BY_TURN[a_turnIndex];
    }

    static double GetScore(Chromosome a_chromosome, PlayerAttributes[] a_players, GayaAttributes a_gaya, out int a_outWater)
    {
        List<Unit> wrecks = new List<Unit>();
        a_gaya.Wrecks.ForEach(w => wrecks.Add(w.Clone()));

        List<Unit> tankers = new List<Unit>();
        a_gaya.Tankers.ForEach(t => tankers.Add(t.Clone()));

        PlayerAttributes[] players = new PlayerAttributes[3];
        foreach(int i in Enumerable.Range(0, 3))
        {
            players[i] = a_players[i].Clone();
        }

        SimuResult result = new SimuResult();

        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
            PlayTurn(i, a_chromosome.ReaperTurnActions[i], players, wrecks, tankers, result);
            //PlayTurn(i, a_chromosome.ReaperTurnActions[i], a_chromosome.DestroyerTurnActions[i], players, wrecks, tankers, result);

        //// Eval dist to nearest wreck

        double reaperMaxDist2 = MAP_RADIUS * MAP_RADIUS * 4;
        double reaperMinDist2 = reaperMaxDist2;
        wrecks.ForEach(w =>
        {
            double dist2 = players[0].ReaperUnit.Position.Distance2(w.Position);
            if (reaperMinDist2 > dist2)
                reaperMinDist2 = dist2;
        });

        double reaperDistScore = 1 - (reaperMinDist2 / reaperMaxDist2);

        //// Eval dist to nearest tanker

        double destroyerMaxDist2 = MAP_RADIUS * MAP_RADIUS * 4;
        double destroyerMinDist2 = destroyerMaxDist2;

        tankers.ForEach(t =>
        {
            double dist2 = players[0].DestroyerUnit.Position.Distance2(t.Position);
            if (destroyerMinDist2 > dist2)
                destroyerMinDist2 = dist2;
        });

        double destroyerDistScore = 1 - (destroyerMinDist2 / destroyerMaxDist2);
        
        //// Water score, more water collected is better. Consider that 20 water collected is the max

        double waterScore = result.WaterPoints / MAX_WATER_POINTS;
        a_outWater = (int) result.WaterPoints;

        //// More weight for water score

        return 1000 * waterScore + 100 * reaperDistScore + 50 * destroyerDistScore;
        //return 1000 * waterScore + 100 * reaperDistScore;
    }

    /***************************************************************
     * GENETIC METHODS
     ***************************************************************/

    static Chromosome CreateChromosome()
    {
        Chromosome chromosome = new Chromosome();
        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            chromosome.ReaperTurnActions[i].Angle = g_random.Next(0, (360 / ANGLE_SELECTION_STEP) + 1) * ANGLE_SELECTION_STEP;
            chromosome.ReaperTurnActions[i].Throttle = g_random.Next(0, THROTTLE_MAX / THROTTLE_SELECTION_STEP + 1) * THROTTLE_SELECTION_STEP;
            //chromosome.DestroyerTurnActions[i].Angle = g_random.Next(0, (360 / ANGLE_SELECTION_STEP) + 1) * ANGLE_SELECTION_STEP;
            //chromosome.DestroyerTurnActions[i].Throttle = g_random.Next(0, THROTTLE_MAX / THROTTLE_SELECTION_STEP + 1) * THROTTLE_SELECTION_STEP;
        }

        return chromosome;
    }

    static List<Chromosome> CreatePopulation(int a_popSize)
    {
        List<Chromosome> chroms = new List<Chromosome>();

        foreach (int i in Enumerable.Range(0, a_popSize))
            chroms.Add(CreateChromosome());

        return chroms;
    }

    static List<Chromosome> Selection(List<Chromosome> a_chromosomes, PlayerAttributes[] a_players, GayaAttributes a_gaya, out int a_outMaxWater)
    {
        a_outMaxWater = 0;
        List<Tuple<Chromosome, double>> chromsWithScore = new List<Tuple<Chromosome, double>>();

        foreach (Chromosome chromosome in a_chromosomes)
        {
            int water;
            double score = GetScore(chromosome, a_players, a_gaya, out water);
            chromsWithScore.Add(new Tuple<Chromosome, double>(chromosome, score));
            if (water > a_outMaxWater)
                a_outMaxWater = water;
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

    static void Mutation(Chromosome a_chromosome)
    {
        int turn = g_random.Next(0, TURN_TO_SIMULATE_COUNT);
        //int unit = g_random.Next(0, 2);
        int comp = g_random.Next(0, 2);

        //TurnAction action = unit == 0 ? a_chromosome.ReaperTurnActions[turn] : a_chromosome.DestroyerTurnActions[turn];
        TurnAction action = a_chromosome.ReaperTurnActions[turn];

        if (comp == 0)
            action.Angle = g_random.Next(0, (360 / ANGLE_SELECTION_STEP) + 1) * ANGLE_SELECTION_STEP;
        else
            action.Throttle = g_random.Next(0, THROTTLE_MAX / THROTTLE_SELECTION_STEP + 1) * THROTTLE_SELECTION_STEP;
    }

    static Chromosome CrossOver(Chromosome a_parent1, Chromosome a_parent2)
    {
        Chromosome child = new Chromosome();

        bool isParent1 = true;

        foreach (int i in Enumerable.Range(0, TURN_TO_SIMULATE_COUNT))
        {
            Chromosome parent = isParent1 ? a_parent1 : a_parent2;
            child.ReaperTurnActions[i].Copy(parent.ReaperTurnActions[i]);
            //child.DestroyerTurnActions[i].Copy(parent.DestroyerTurnActions[i]);
            isParent1 = !isParent1;
        }

        return child;
    }

    static List<Chromosome> Generation(List<Chromosome> a_population, PlayerAttributes[] a_players, GayaAttributes a_gaya, out int a_outMaxWater)
    {
        List<Chromosome> select = Selection(a_population, a_players, a_gaya, out a_outMaxWater);
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

    public static void Main(string[] args)
    {
        bool isLocal = false;
        Array.ForEach(args, arg =>
        {
            if (arg.Equals("--local"))
                isLocal = true;
        });

        if(isLocal && ! Debugger.IsAttached)
            Debugger.Launch();

        PlayerAttributes[] playersAttributes = Enumerable.Range(0, PLAYER_COUNT).Select(i => new PlayerAttributes()).ToArray();

        GayaAttributes gayaAttributes = new GayaAttributes();

        // game loop    
        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            gayaAttributes.InvalidateWrecksAndTankers();

            playersAttributes[MY_ID].Score = int.Parse(Console.ReadLine());

            playersAttributes[ENNEMY1_ID].Score = int.Parse(Console.ReadLine());
            playersAttributes[ENNEMY2_ID].Score = int.Parse(Console.ReadLine());

            playersAttributes[MY_ID].Rage = int.Parse(Console.ReadLine());
            playersAttributes[ENNEMY1_ID].Rage = int.Parse(Console.ReadLine());
            playersAttributes[ENNEMY2_ID].Rage = int.Parse(Console.ReadLine());

            int unitCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < unitCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');

                int unitId = int.Parse(inputs[0]);
                int unitType = int.Parse(inputs[1]);
                int playerId = int.Parse(inputs[2]);
                float mass = float.Parse(inputs[3]);
                int radius = int.Parse(inputs[4]);
                int x = int.Parse(inputs[5]);
                int y = int.Parse(inputs[6]);
                int vx = int.Parse(inputs[7]);
                int vy = int.Parse(inputs[8]);
                int waterQuantity = int.Parse(inputs[9]);
                int waterCapacity = int.Parse(inputs[10]);

                if (unitType == REAPER_TYPE || unitType == DESTROYER_TYPE || unitType == DOOF_TYPE)
                {
                    Unit unit = null;

                    if (unitType == REAPER_TYPE)
                    {
                        if (playersAttributes[playerId].ReaperUnit == null)
                            playersAttributes[playerId].ReaperUnit = new Unit(unitId, unitType, mass, radius, new Vector2(x, y), new Vector2(vx, vy), waterQuantity, waterCapacity);

                        unit = playersAttributes[playerId].ReaperUnit;
                    }
                    else if (unitType == DESTROYER_TYPE)
                    {
                        if (playersAttributes[playerId].DestroyerUnit == null)
                            playersAttributes[playerId].DestroyerUnit = new Unit(unitId, unitType, mass, radius, new Vector2(x, y), new Vector2(vx, vy), waterQuantity, waterCapacity);

                        unit = playersAttributes[playerId].DestroyerUnit;
                    }
                    else if (unitType == DOOF_TYPE)
                    {
                        if (playersAttributes[playerId].DoofUnit == null)
                            playersAttributes[playerId].DoofUnit = new Unit(unitId, unitType, mass, radius, new Vector2(x, y), new Vector2(vx, vy), waterQuantity, waterCapacity);

                        unit = playersAttributes[playerId].DoofUnit;
                    }

                    unit.Position.Set(x, y);
                    unit.Speed.Set(vx, vy);
                }
                else if (unitType == WRECK_TYPE)
                {
                    Unit wreckUnit = gayaAttributes.Wrecks.Find(wreck => wreck.Id == unitId);
                    if (wreckUnit == null)
                        gayaAttributes.Wrecks.Add(new Unit(unitId, unitType, -1.0f, radius, new Vector2(x, y), new Vector2(vx, vy), waterQuantity, waterCapacity));
                    else
                        wreckUnit.WaterQuantity = waterQuantity;
                }
                else if (unitType == TANKER_TYPE)
                {
                    Unit tankerUnit = gayaAttributes.Tankers.Find(t => t.Id == unitId);

                    if (tankerUnit == null)
                    {
                        gayaAttributes.Tankers.Add(new Unit(unitId, unitType, mass, radius, new Vector2(x, y), new Vector2(vx, vy), waterQuantity, waterCapacity));
                        tankerUnit = gayaAttributes.Tankers.Last();
                    }
                    else
                    {
                        tankerUnit.Position.Set(x, y);
                        tankerUnit.Speed.Set(vx, vy);
                        tankerUnit.WaterQuantity = waterQuantity;
                    }
                }
                else
                    Console.Error.WriteLine("Unknown unit type : " + unitType);
            }

            List<Chromosome> population = CreatePopulation(POPULATION_SIZE);

            int generation = 0;
            int maxWater = 0;

            while (stopWatch.ElapsedMilliseconds < 45)
            //while(generation <= 50)
            {
                //Console.Error.WriteLine("Elapsed time = " + elapsedTime + " | population size = " + population.Count);

                population = Generation(population, playersAttributes, gayaAttributes, out maxWater);
                ++generation;
            }

            Console.Error.WriteLine(generation + " generations");

            gayaAttributes.ClearInvalidWrecksAndTankers();

            // Last minute hack. If AG does not find solution gathering at least 1 water, boost to nearest wreck

            if (maxWater == 0 && gayaAttributes.Wrecks.Count > 0)
            {

                double minDist2 = Double.MaxValue;
                Unit wreck = null;
                gayaAttributes.Wrecks.ForEach(w =>
                {
                    double dist2 = w.Position.Distance2(playersAttributes[0].ReaperUnit.Position);
                    if (minDist2 > dist2)
                    {
                        minDist2 = dist2;
                        wreck = w;
                    }
                });

                Console.WriteLine(wreck.Position.X + " " + wreck.Position.Y + " " + THROTTLE_MAX);
            }
            else
            {
                TurnAction bestReaperAction = population[0].ReaperTurnActions[0];
                int reaperTargetX = (int)(playersAttributes[0].ReaperUnit.Position.X + Math.Cos(bestReaperAction.Angle * Math.PI / 180) * bestReaperAction.Throttle);
                int reaperTargetY = (int)(playersAttributes[0].ReaperUnit.Position.Y + Math.Sin(bestReaperAction.Angle * Math.PI / 180) * bestReaperAction.Throttle);

                Console.WriteLine(reaperTargetX + " " + reaperTargetY + " " + bestReaperAction.Throttle);

                //TurnAction bestDestroyerAction = population[0].DestroyerTurnActions[0];
                //int destroyerTargetX = (int)(playersAttributes[0].DestroyerUnit.Position.X + Math.Cos(bestDestroyerAction.Angle * Math.PI / 180) * bestDestroyerAction.Throttle);
                //int destroyerTargetY = (int)(playersAttributes[0].DestroyerUnit.Position.Y + Math.Sin(bestDestroyerAction.Angle * Math.PI / 180) * bestDestroyerAction.Throttle);

                //Console.WriteLine(destroyerTargetX + " " + destroyerTargetY + " " + bestDestroyerAction.Throttle);
            }

            // Destroyer always target tanker nearest reaper

            double tminDist2 = Double.MaxValue;
            Unit bestTanker = null;

            gayaAttributes.Tankers.ForEach(t =>
            {
                double tdist2 = t.Position.Distance2(playersAttributes[0].ReaperUnit.Position);
                if (tdist2 < tminDist2)
                {
                    tminDist2 = tdist2;
                    bestTanker = t;
                }
            });

            if (bestTanker != null)
                Console.WriteLine(bestTanker.Position.X + " " + bestTanker.Position.Y + " " + THROTTLE_MAX);
            else
                Console.WriteLine(playersAttributes[0].ReaperUnit.Position.X + " " + playersAttributes[0].ReaperUnit.Position.Y + " " + THROTTLE_MAX);

            Unit target = null;
            if (playersAttributes[1].Score >= playersAttributes[2].Score)
                target = playersAttributes[1].ReaperUnit;
            else
                target = playersAttributes[2].ReaperUnit;

            // Check if doof should use skill

            bool useDoofSkill = false;

            if (playersAttributes[0].DoofUnit != null)
            {
                if (playersAttributes[0].Rage >= 30 && playersAttributes[0].DoofUnit.Position.Distance2(target.Position) < 2000 * 2000)
                {
                    gayaAttributes.Wrecks.ForEach(w =>
                    {
                        if (target.Position.Distance2(w.Position) <= 1.5 * w.Radius * w.Radius)
                            useDoofSkill = true;
                    });
                }
            }

            if (useDoofSkill)
                Console.WriteLine("SKILL " + target.Position.X + " " + target.Position.Y);
            else
               Console.WriteLine(target.Position.X + " " + target.Position.Y + " " + THROTTLE_MAX);
        }
    }
}