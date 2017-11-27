using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingGame_engine_MeanMax
{
    public class Referee
    {
        private static int GAME_VERSION = 3;

        public static bool SPAWN_WRECK = false;
        public static int LOOTER_COUNT = 3;
        public static bool REAPER_SKILL_ACTIVE = true;
        public static bool DESTROYER_SKILL_ACTIVE = true;
        public static bool DOOF_SKILL_ACTIVE = true;
        public static string EXPECTED = "<x> <y> <power> | SKILL <x> <y> | WAIT";

        public static double MAP_RADIUS = 6000.0;
        public static int TANKERS_BY_PLAYER;
        public static int TANKERS_BY_PLAYER_MIN = 1;
        public static int TANKERS_BY_PLAYER_MAX = 3;

        public static double WATERTOWN_RADIUS = 3000.0;

        public static int TANKER_THRUST = 500;
        public static double TANKER_EMPTY_MASS = 2.5;
        public static double TANKER_MASS_BY_WATER = 0.5;
        public static double TANKER_FRICTION = 0.40;
        public static double TANKER_RADIUS_BASE = 400.0;
        public static double TANKER_RADIUS_BY_SIZE = 50.0;
        public static int TANKER_EMPTY_WATER = 1;
        public static int TANKER_MIN_SIZE = 4;
        public static int TANKER_MAX_SIZE = 10;
        public static double TANKER_MIN_RADIUS = TANKER_RADIUS_BASE + TANKER_RADIUS_BY_SIZE * TANKER_MIN_SIZE;
        public static double TANKER_MAX_RADIUS = TANKER_RADIUS_BASE + TANKER_RADIUS_BY_SIZE * TANKER_MAX_SIZE;
        public static double TANKER_SPAWN_RADIUS = 8000.0;
        public static int TANKER_START_THRUST = 2000;

        public static int MAX_THRUST = 300;
        public static int MAX_RAGE = 300;
        public static int WIN_SCORE = 50;

        public static double REAPER_MASS = 0.5;
        public static double REAPER_FRICTION = 0.20;
        public static int REAPER_SKILL_DURATION = 3;
        public static int REAPER_SKILL_COST = 30;
        public static int REAPER_SKILL_ORDER = 0;
        public static double REAPER_SKILL_RANGE = 2000.0;
        public static double REAPER_SKILL_RADIUS = 1000.0;
        public static double REAPER_SKILL_MASS_BONUS = 10.0;

        public static double DESTROYER_MASS = 1.5;
        public static double DESTROYER_FRICTION = 0.30;
        public static int DESTROYER_SKILL_DURATION = 1;
        public static int DESTROYER_SKILL_COST = 60;
        public static int DESTROYER_SKILL_ORDER = 2;
        public static double DESTROYER_SKILL_RANGE = 2000.0;
        public static double DESTROYER_SKILL_RADIUS = 1000.0;
        public static int DESTROYER_NITRO_GRENADE_POWER = 1000;

        public static double DOOF_MASS = 1.0;
        public static double DOOF_FRICTION = 0.25;
        public static double DOOF_RAGE_COEF = 1.0 / 100.0;
        public static int DOOF_SKILL_DURATION = 3;
        public static int DOOF_SKILL_COST = 30;
        public static int DOOF_SKILL_ORDER = 1;
        public static double DOOF_SKILL_RANGE = 2000.0;
        public static double DOOF_SKILL_RADIUS = 1000.0;

        public static double LOOTER_RADIUS = 400.0;
        public const int LOOTER_REAPER = 0;
        public const int LOOTER_DESTROYER = 1;
        public const int LOOTER_DOOF = 2;

        public const int TYPE_TANKER = 3;
        public const int TYPE_WRECK = 4;
        public const int TYPE_REAPER_SKILL_EFFECT = 5;
        public const int TYPE_DOOF_SKILL_EFFECT = 6;
        public const int TYPE_DESTROYER_SKILL_EFFECT = 7;

        public static double EPSILON = 0.00001;
        public static double MIN_IMPULSE = 30.0;
        public static double IMPULSE_COEFF = 0.5;

        // Global first free id for all elements on the map 
        public static int GLOBAL_ID = 0;

        // Center of the map
        public static readonly Point WATERTOWN = new Point(0, 0);

        // The null collision 
        public static readonly Collision NULL_COLLISION = new Collision(1.0 + EPSILON);

        public class Point
        {
            public double x { get; protected set; }
            public double y { get; protected set; }

            public Point(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public double distance(Point p)
            {
                return Math.Sqrt((this.x - p.x) * (this.x - p.x) + (this.y - p.y) * (this.y - p.y));
            }

            // Move the point to x and y
            public void move(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            // Move the point to an other point for a given distance
            public void moveTo(Point p, double distance)
            {
                double d = this.distance(p);

                if (d < EPSILON)
                {
                    return;
                }

                double dx = p.x - x;
                double dy = p.y - y;
                double coef = distance / d;

                this.x += dx * coef;
                this.y += dy * coef;
            }

            public bool isInRange(Point p, double range)
            {
                return p != this && distance(p) <= range;
            }

            public override int GetHashCode()
            {
                const int prime = 31;
                int result = 1;
                long temp;
                temp = BitConverter.DoubleToInt64Bits(x);
                result = prime * result + (int)(temp ^ (temp >> 32)); // TODO : check equivalent to JAVA >>>
                temp = BitConverter.DoubleToInt64Bits(y);
                result = prime * result + (int)(temp ^ (temp >> 32)); // TODO : check equivalent to JAVA >>>
                return result;
            }

            public override bool Equals(Object obj)
            {
                Point point = obj as Point;
                return BitConverter.DoubleToInt64Bits(x) == BitConverter.DoubleToInt64Bits(point.x) && BitConverter.DoubleToInt64Bits(y) == BitConverter.DoubleToInt64Bits(point.y);
            }
        }

        public class Wreck : Point
        {
            public int id;
            public double radius;
            public int water;
            public bool known;
            public Player player;

            public Wreck(double x, double y, int water, double radius)
                        : base(x, y)
            {
                id = GLOBAL_ID++;

                this.radius = radius;
                this.water = water;
            }

            public string getFrameId()
            {
                return id + "@" + water;
            }

            public string toFrameData()
            {
                if (known)
                {
                    return getFrameId();
                }

                known = true;

                return Utils.join(getFrameId(), Utils.Round(x), Utils.Round(y), 0, 0, TYPE_WRECK, radius);
            }

            // Reaper harvesting
            public bool harvest(List<Player> players, SortedSet<SkillEffect> skillEffects)
            {
                foreach (Player p in players)
                {
                    if (isInRange(p.getReaper(), radius) && !p.getReaper().isInDoofSkill(skillEffects))
                    {
                        p.score += 1;
                        water -= 1;
                    }
                }

                return water > 0;
            }
        }

        public abstract class Unit : Point
        {
            public int type;
            public int id;
            public double vx;
            public double vy;
            public double radius;
            public double mass;
            public double friction;
            public bool known;

            protected Unit(int type, double x, double y)
                    : base(x, y)
            {
                id = GLOBAL_ID++;
                this.type = type;

                vx = 0.0;
                vy = 0.0;

                known = false;
            }

            public void move(double t)
            {
                x += vx * t;
                y += vy * t;
            }

            public double speed()
            {
                return Math.Sqrt(vx * vx + vy * vy);
            }

            public override int GetHashCode()
            {
                const int prime = 31;
                int result = 1;
                result = prime * result + id;
                return result;
            }

            public override bool Equals(Object obj)
            {
                Unit unit = obj as Unit;
                return id == unit.id;
            }

            public virtual string getFrameId()
            {
                return "" + id;
            }

            public virtual string toFrameData()
            {
                if (known)
                {
                    return Utils.join(getFrameId(), Utils.Round(x), Utils.Round(y), Utils.Round(vx), Utils.Round(vy));
                }

                known = true;

                return Utils.join(getFrameId(), Utils.Round(x), Utils.Round(y), Utils.Round(vx), Utils.Round(vy), type, Utils.Round(radius));
            }

            public void thrust(Point p, int power)
            {
                double distance = this.distance(p);

                // Avoid a division by zero
                if (Math.Abs(distance) <= EPSILON)
                {
                    return;
                }

                double coef = (((double)power) / mass) / distance;
                vx += (p.x - this.x) * coef;
                vy += (p.y - this.y) * coef;
            }

            public bool isInDoofSkill(SortedSet<SkillEffect> skillEffects)
            {
                int tot = skillEffects.Where(s => s.GetType() == typeof(DoofSkillEffect) && isInRange(s, s.radius + radius)).Count();
                return tot > 0;
            }

            public void adjust(SortedSet<SkillEffect> skillEffects)
            {
                x = Utils.Round(x);
                y = Utils.Round(y);

                if (isInDoofSkill(skillEffects))
                {
                    // No friction if we are in a doof skill effect
                    vx = Utils.Round(vx);
                    vy = Utils.Round(vy);
                }
                else
                {
                    vx = Utils.Round(vx * (1.0 - friction));
                    vy = Utils.Round(vy * (1.0 - friction));
                }
            }

            // Search the next collision with the map border
            public virtual Collision getCollision()
            {
                // Check instant collision
                if (this.distance(WATERTOWN) + radius >= MAP_RADIUS)
                {
                    return new Collision(0.0, this);
                }

                // We are not moving, we can't reach the map border
                if (vx == 0.0 && vy == 0.0)
                {
                    return NULL_COLLISION;
                }

                // Search collision with map border
                // Resolving: sqrt((x + t*vx)^2 + (y + t*vy)^2) = MAP_RADIUS - radius <=> t^2*(vx^2 + vy^2) + t*2*(x*vx + y*vy) + x^2 + y^2 - (MAP_RADIUS - radius)^2 = 0
                // at^2 + bt + c = 0;
                // a = vx^2 + vy^2
                // b = 2*(x*vx + y*vy)
                // c = x^2 + y^2 - (MAP_RADIUS - radius)^2

                double a = vx * vx + vy * vy;

                if (a <= 0.0)
                {
                    return NULL_COLLISION;
                }

                double b = 2.0 * (x * vx + y * vy);
                double c = x * x + y * y - (MAP_RADIUS - radius) * (MAP_RADIUS - radius);
                double delta = b * b - 4.0 * a * c;

                if (delta <= 0.0)
                {
                    return NULL_COLLISION;
                }

                double t = (-b + Math.Sqrt(delta)) / (2.0 * a);

                if (t <= 0.0)
                {
                    return NULL_COLLISION;
                }

                return new Collision(t, this);
            }

            // Search the next collision with an other unit
            public Collision getCollision(Unit u)
            {
                // Check instant collision
                if (distance(u) <= radius + u.radius)
                {
                    return new Collision(0.0, this, u);
                }

                // Both units are motionless
                if (vx == 0.0 && vy == 0.0 && u.vx == 0.0 && u.vy == 0.0)
                {
                    return NULL_COLLISION;
                }

                // Change referencial
                // Unit u is not at point (0, 0) with a speed vector of (0, 0)
                double x2 = x - u.x;
                double y2 = y - u.y;
                double r2 = radius + u.radius;
                double vx2 = vx - u.vx;
                double vy2 = vy - u.vy;

                // Resolving: sqrt((x + t*vx)^2 + (y + t*vy)^2) = radius <=> t^2*(vx^2 + vy^2) + t*2*(x*vx + y*vy) + x^2 + y^2 - radius^2 = 0
                // at^2 + bt + c = 0;
                // a = vx^2 + vy^2
                // b = 2*(x*vx + y*vy)
                // c = x^2 + y^2 - radius^2 

                double a = vx2 * vx2 + vy2 * vy2;

                if (a <= 0.0)
                {
                    return NULL_COLLISION;
                }

                double b = 2.0 * (x2 * vx2 + y2 * vy2);
                double c = x2 * x2 + y2 * y2 - r2 * r2;
                double delta = b * b - 4.0 * a * c;

                if (delta < 0.0)
                {
                    return NULL_COLLISION;
                }

                double t = (-b - Math.Sqrt(delta)) / (2.0 * a);

                if (t <= 0.0)
                {
                    return NULL_COLLISION;
                }

                return new Collision(t, this, u);
            }

            // Bounce between 2 units
            public void bounce(Unit u)
            {
                double mcoeff = (mass + u.mass) / (mass * u.mass);
                double nx = x - u.x;
                double ny = y - u.y;
                double nxnysquare = nx * nx + ny * ny;
                double dvx = vx - u.vx;
                double dvy = vy - u.vy;
                double product = (nx * dvx + ny * dvy) / (nxnysquare * mcoeff);
                double fx = nx * product;
                double fy = ny * product;
                double m1c = 1.0 / mass;
                double m2c = 1.0 / u.mass;

                vx -= fx * m1c;
                vy -= fy * m1c;
                u.vx += fx * m2c;
                u.vy += fy * m2c;

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

                vx -= fx * m1c;
                vy -= fy * m1c;
                u.vx += fx * m2c;
                u.vy += fy * m2c;

                double diff = (distance(u) - radius - u.radius) / 2.0;
                if (diff <= 0.0)
                {
                    // Unit overlapping. Fix positions.
                    moveTo(u, diff - EPSILON);
                    u.moveTo(this, diff - EPSILON);
                }
            }

            // Bounce with the map border
            public void bounce()
            {
                double mcoeff = 1.0 / mass;
                double nxnysquare = x * x + y * y;
                double product = (x * vx + y * vy) / (nxnysquare * mcoeff);
                double fx = x * product;
                double fy = y * product;

                vx -= fx * mcoeff;
                vy -= fy * mcoeff;

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
                vx -= fx * mcoeff;
                vy -= fy * mcoeff;

                double diff = distance(WATERTOWN) + radius - MAP_RADIUS;
                if (diff >= 0.0)
                {
                    // Unit still outside of the map, reposition it
                    moveTo(WATERTOWN, diff + EPSILON);
                }
            }

            public virtual int getExtraInput()
            {
                return -1;
            }

            public virtual int getExtraInput2()
            {
                return -1;
            }

            public virtual int getPlayerIndex()
            {
                return -1;
            }
        }

        public class Tanker : Unit
        {
            public int water;
            public int size;
            public Player player;
            
            public Tanker(int size, Player player)
                    : base(TYPE_TANKER, 0.0, 0.0)
            {
                this.player = player;
                this.size = size;

                water = TANKER_EMPTY_WATER;
                mass = TANKER_EMPTY_MASS + TANKER_MASS_BY_WATER * water;
                friction = TANKER_FRICTION;
                radius = TANKER_RADIUS_BASE + TANKER_RADIUS_BY_SIZE * size;
            }

            public override string getFrameId()
            {
                return id + "@" + water;
            }

            public Wreck die()
            {
                // Don't spawn a wreck if our center is outside of the map
                if (distance(WATERTOWN) >= MAP_RADIUS)
                {
                    return null;
                }

                return new Wreck(Utils.Round(x), Utils.Round(y), water, radius);
            }

            public bool isFull()
            {
                return water >= size;
            }

            public void play()
            {
                if (isFull())
                {
                    // Try to leave the map
                    thrust(WATERTOWN, -TANKER_THRUST);
                }
                else if (distance(WATERTOWN) > WATERTOWN_RADIUS)
                {
                    // Try to reach watertown
                    thrust(WATERTOWN, TANKER_THRUST);
                }
            }

            public override Collision getCollision()
            {
                // Tankers can go outside of the map
                return NULL_COLLISION;
            }

            public override int getExtraInput()
            {
                return water;
            }

            public override int getExtraInput2()
            {
                return size;
            }
        }

        public abstract class Looter : Unit
        {
            public int skillCost;
            public double skillRange;
            public bool skillActive;

            public Player player;

            public Point wantedThrustTarget;
            public int wantedThrustPower;

            public string message;
            public Action attempt;
            public SkillResult skillResult;

            public Looter(int type, Player player, double x, double y)
                    : base(type, x, y)
            {
                this.player = player;

                radius = LOOTER_RADIUS;
            }

            public SkillEffect skill(Point p)
            {
                if (player.rage < skillCost)
                    throw new SystemException("RAGE");
                if (distance(p) > skillRange)
                    throw new SystemException("FAR");

                player.rage -= skillCost;
                return skillImpl(p);
            }

            public override string toFrameData()
            {
                if (known)
                {
                    return base.toFrameData();
                }

                return Utils.join(base.toFrameData(), player.index);
            }

            public override int getPlayerIndex()
            {
                return player.index;
            }

            public abstract SkillEffect skillImpl(Point p);

            public void setWantedThrust(Point target, int power)
            {
                if (power < 0)
                {
                    power = 0;
                }

                wantedThrustTarget = target;
                wantedThrustPower = Math.Min(power, MAX_THRUST);
            }

            public void reset()
            {
                message = null;
                attempt = Action.UNDEFINED;
                skillResult = null;
                wantedThrustTarget = null;
            }
        }

        public class Reaper : Looter
        {
            public Reaper(Player player, double x, double y)
                    : base(LOOTER_REAPER, player, x, y)
            {
                mass = REAPER_MASS;
                friction = REAPER_FRICTION;
                skillCost = REAPER_SKILL_COST;
                skillRange = REAPER_SKILL_RANGE;
                skillActive = REAPER_SKILL_ACTIVE;
            }

            public override SkillEffect skillImpl(Point p)
            {
                return new ReaperSkillEffect(TYPE_REAPER_SKILL_EFFECT, p.x, p.y, REAPER_SKILL_RADIUS, REAPER_SKILL_DURATION, REAPER_SKILL_ORDER, this);
            }
        }

        public class Destroyer : Looter
        {
            public Destroyer(Player player, double x, double y)
                : base(LOOTER_DESTROYER, player, x, y)
            {

                mass = DESTROYER_MASS;
                friction = DESTROYER_FRICTION;
                skillCost = DESTROYER_SKILL_COST;
                skillRange = DESTROYER_SKILL_RANGE;
                skillActive = DESTROYER_SKILL_ACTIVE;
            }

            public override SkillEffect skillImpl(Point p)
            {
                return new DestroyerSkillEffect(TYPE_DESTROYER_SKILL_EFFECT, p.x, p.y, DESTROYER_SKILL_RADIUS, DESTROYER_SKILL_DURATION,
                        DESTROYER_SKILL_ORDER, this);
            }
        }

        public class Doof : Looter
        {
            public Doof(Player player, double x, double y)
                : base(CodingGame_engine_MeanMax.Referee.LOOTER_DOOF, player, x, y)
            {
                mass = CodingGame_engine_MeanMax.Referee.DOOF_MASS;
                friction = CodingGame_engine_MeanMax.Referee.DOOF_FRICTION;
                skillCost = CodingGame_engine_MeanMax.Referee.DOOF_SKILL_COST;
                skillRange = CodingGame_engine_MeanMax.Referee.DOOF_SKILL_RANGE;
                skillActive = CodingGame_engine_MeanMax.Referee.DOOF_SKILL_ACTIVE;
            }

            public override SkillEffect skillImpl(Point p)
            {
                return new DoofSkillEffect(TYPE_DOOF_SKILL_EFFECT, p.x, p.y, DOOF_SKILL_RADIUS, DOOF_SKILL_DURATION, DOOF_SKILL_ORDER, this);
            }

            // With flame effects! Yeah!
            public int sing()
            {
                return (int)Math.Floor(speed() * DOOF_RAGE_COEF);
            }
        }

        public class Player
        {
            public int score;
            public int index;
            public int rage;
            public Looter[] looters;
            public bool dead;
            public LinkedList<TankerSpawn> tankers;

            public Player(int index)
            {
                this.index = index;

                looters = new CodingGame_engine_MeanMax.Referee.Looter[CodingGame_engine_MeanMax.Referee.LOOTER_COUNT];
            }

            public void kill()
            {
                dead = true;
            }

            public Reaper getReaper()
            {
                return (Reaper)looters[LOOTER_REAPER];
            }

            public Destroyer getDestroyer()
            {
                return (Destroyer)looters[LOOTER_DESTROYER];
            }

            public Doof getDoof()
            {
                return (Doof)looters[LOOTER_DOOF];
            }
        }

        public class TankerSpawn
        {
            public int size;
            public double angle;

            public TankerSpawn(int size, double angle)
            {
                this.size = size;
                this.angle = angle;
            }
        }

        public class Collision
        {
            public double t;
            public Unit a;
            public Unit b;

            public Collision(double t) : this(t, null, null) { }
            public Collision(double t, CodingGame_engine_MeanMax.Referee.Unit a) : this(t, a, null) { }

            public Collision(double t, CodingGame_engine_MeanMax.Referee.Unit a, CodingGame_engine_MeanMax.Referee.Unit b)
            {
                this.t = t;
                this.a = a;
                this.b = b;
            }

            public Tanker dead()
            {
                if (a.GetType() == typeof(Destroyer) && b.GetType() == typeof(Tanker) && b.mass < REAPER_SKILL_MASS_BONUS)
                {
                    return (Tanker)b;
                }

                if (b.GetType() == typeof(Destroyer) && a.GetType() == typeof(Tanker) && a.mass < REAPER_SKILL_MASS_BONUS)
                {
                    return (Tanker)a;
                }

                return null;
            }
        }

        public abstract class SkillEffect : CodingGame_engine_MeanMax.Referee.Point
        {
            public int id;
            public int type;
            public double radius;
            public int duration;
            public int order;
            public bool known;
            public CodingGame_engine_MeanMax.Referee.Looter looter;

            public SkillEffect(int type, double x, double y, double radius, int duration, int order, CodingGame_engine_MeanMax.Referee.Looter looter)
                : base(x, y)
            {
                id = CodingGame_engine_MeanMax.Referee.GLOBAL_ID++;

                this.type = type;
                this.radius = radius;
                this.duration = duration;
                this.looter = looter;
                this.order = order;
            }

            public void apply(List<CodingGame_engine_MeanMax.Referee.Unit> units)
            {
                duration -= 1;
                applyImpl(units.FindAll(u => u.isInRange(u, radius + u.radius)));
            }

            public string toFrameData()
            {
                if (known)
                {
                    return "" + id;
                }

                known = true;

                return Utils.join(id, Utils.Round(x), Utils.Round(y), looter.id, 0, type, Utils.Round(radius));
            }

            public abstract void applyImpl(List<CodingGame_engine_MeanMax.Referee.Unit> units);

            override public int GetHashCode()
            {
                const int prime = 31;
                int result = 1;
                result = prime * result + id;
                return result;
            }

            override public bool Equals(Object obj)
            {
                SkillEffect se = obj as SkillEffect;
                return se.id == id;
            }
        }

        public class ReaperSkillEffect : SkillEffect
        {
            public ReaperSkillEffect(int type, double x, double y, double radius, int duration, int order, CodingGame_engine_MeanMax.Referee.Reaper reaper)
                : base(type, x, y, radius, duration, order, reaper)
            {
            }

            public override void applyImpl(List<CodingGame_engine_MeanMax.Referee.Unit> units)
            {
                // Increase mass
                units.ForEach(u => u.mass += REAPER_SKILL_MASS_BONUS);
            }
        }

        public class DestroyerSkillEffect : SkillEffect
        {

            public DestroyerSkillEffect(int type, double x, double y, double radius, int duration, int order, Destroyer destroyer)
                        : base(type, x, y, radius, duration, order, destroyer)
            {
            }

            public override void applyImpl(List<Unit> units)
            {
                // Push units
                units.ForEach(u => u.thrust(this, -DESTROYER_NITRO_GRENADE_POWER));
            }
        }

        public class DoofSkillEffect : SkillEffect
        {

            public DoofSkillEffect(int type, double x, double y, double radius, int duration, int order, Doof doof)
                : base(type, x, y, radius, duration, order, doof)
            {
            }

            public override void applyImpl(List<Unit> units)
            {
                // Nothing to do now
            }
        }

        static class Utils
        {
            static public int Round(double x)
            {
                int s = x < 0 ? -1 : 1;
                return s * (int)Math.Round(s * x);
            }

            // Join multiple object into a space separated string
            static public string join(params object[] args)
            {
                string str = "";
                bool first = true;
                foreach (object arg in args)
                {
                    if (!first)
                        str += " ";
                    else
                        first = false;

                    str += arg;
                }
                return str;
            }
        }

        public int seed;
        public int playerCount;
        public List<Unit> units;
        public List<Unit> looters;
        public List<Unit> tankers;
        public List<Tanker> deadTankers;
        public List<Wreck> wrecks;
        public List<List<Unit>> unitsByType;
        public List<Player> players;
        public List<string> frameData;
        public SortedSet<SkillEffect> skillEffects;

        void spawnTanker(Player player)
        {
            TankerSpawn spawn = player.tankers.First();
            player.tankers.RemoveFirst();

            double angle = (player.index + spawn.angle) * Math.PI * 2.0 / ((double)playerCount);

            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            if (SPAWN_WRECK)
            {
                // Spawn a wreck directly
                Wreck wreck = new Wreck(cos * WATERTOWN_RADIUS, sin * WATERTOWN_RADIUS, spawn.size,
                        TANKER_RADIUS_BASE + spawn.size * TANKER_RADIUS_BY_SIZE);
                wreck.player = player;

                wrecks.Add(wreck);

                return;
            }

            Tanker tanker = new Tanker(spawn.size, player);

            double distance = TANKER_SPAWN_RADIUS + tanker.radius;

            bool safe = false;
            while (!safe)
            {
                tanker.move(cos * distance, sin * distance);

                safe = units.All(u => tanker.distance(u) > tanker.radius + u.radius);
                distance += TANKER_MIN_RADIUS;
            }

            tanker.thrust(WATERTOWN, TANKER_START_THRUST);

            units.Add(tanker);
            tankers.Add(tanker);
        }

        Looter createLooter(int type, Player player, double x, double y)
        {
            if (type == LOOTER_REAPER)
            {
                return new Reaper(player, x, y);
            }
            else if (type == LOOTER_DESTROYER)
            {
                return new Destroyer(player, x, y);
            }
            else if (type == LOOTER_DOOF)
            {
                return new Doof(player, x, y);
            }

            // Not supposed to happen
            return null;
        }

        void newFrame(double t)
        {
            frameData.Add("#" + t);
        }

        void addToFrame(Wreck w)
        {
            frameData.Add(w.toFrameData());
        }

        void addToFrame(Unit u)
        {
            frameData.Add(u.toFrameData());
        }

        void addToFrame(SkillEffect s)
        {
            frameData.Add(s.toFrameData());
        }

        void addDeadToFrame(SkillEffect s)
        {
            frameData.Add(Utils.join(s.toFrameData(), "d"));
        }

        void addDeadToFrame(Unit u)
        {
            frameData.Add(Utils.join(u.toFrameData(), "d"));
        }

        void addDeadToFrame(Wreck w)
        {
            frameData.Add(Utils.join(w.toFrameData(), "d"));
        }

        void snapshot()
        {
            unitsByType.ForEach(list => frameData.AddRange(list.Select(u => u.toFrameData()).ToList()));
            frameData.AddRange(wrecks.Select(w => w.toFrameData()));
            frameData.AddRange(skillEffects.Select(s => s.toFrameData()));
        }

        public void initReferee(int playerCount, int gameVersion)
        {
            GAME_VERSION = gameVersion;
            switch (GAME_VERSION)
            {
                case 0:
                    SPAWN_WRECK = true;
                    LOOTER_COUNT = 1;
                    REAPER_SKILL_ACTIVE = false;
                    DESTROYER_SKILL_ACTIVE = false;
                    DOOF_SKILL_ACTIVE = false;
                    EXPECTED = "<x> <y> <power> | WAIT";
                    break;
                case 1:
                    LOOTER_COUNT = 2;
                    REAPER_SKILL_ACTIVE = false;
                    DESTROYER_SKILL_ACTIVE = false;
                    DOOF_SKILL_ACTIVE = false;
                    EXPECTED = "<x> <y> <power> | WAIT";
                    break;
                case 2:
                    LOOTER_COUNT = 3;
                    REAPER_SKILL_ACTIVE = false;
                    DOOF_SKILL_ACTIVE = false;
                    break;
            }

            this.playerCount = playerCount;

            seed = new Random().Next();

            Random random = new Random(seed);

            TANKERS_BY_PLAYER = TANKERS_BY_PLAYER_MIN + random.Next(TANKERS_BY_PLAYER_MAX - TANKERS_BY_PLAYER_MIN + 1);

            units = new List<Unit>();
            looters = new List<Unit>();
            tankers = new List<Unit>();
            deadTankers = new List<Tanker>();
            wrecks = new List<Wreck>();
            players = new List<Player>();

            unitsByType = new List<List<Unit>>();
            unitsByType.Add(looters);
            unitsByType.Add(tankers);

            frameData = new List<string>();

            skillEffects = new SortedSet<SkillEffect>(Comparer<SkillEffect>.Create((a, b) =>
            {
                int order = a.order - b.order;

                if (order != 0)
                {
                    return order;
                }

                return a.id - b.id;
            }));

            // Create players
            for (int i = 0; i < playerCount; ++i)
            {
                Player player = new Player(i);
                players.Add(player);
            }

            // Generate the map
            LinkedList<TankerSpawn> queue = new LinkedList<TankerSpawn>();
            for (int i = 0; i < 500; ++i)
            {
                queue.AddLast(new TankerSpawn(TANKER_MIN_SIZE + random.Next(TANKER_MAX_SIZE - TANKER_MIN_SIZE), random.NextDouble()));
            }

            players.ForEach(p => p.tankers = new LinkedList<TankerSpawn>(queue));

            // Create looters
            foreach (Player player in players)
            {
                for (int i = 0; i < LOOTER_COUNT; ++i)
                {
                    Looter looter = createLooter(i, player, 0, 0);
                    player.looters[i] = looter;
                    units.Add(looter);
                    looters.Add(looter);
                }
            }

            // Random spawns for looters
            bool finished = false;
            while (!finished)
            {
                finished = true;

                for (int i = 0; i < LOOTER_COUNT && finished; ++i)
                {
                    double distance = random.NextDouble() * (MAP_RADIUS - LOOTER_RADIUS);
                    double angle = random.NextDouble();

                    foreach (Player player in players)
                    {
                        double looterAngle = (player.index + angle) * (Math.PI * 2.0 / ((double)playerCount));
                        double cos = Math.Cos(looterAngle);
                        double sin = Math.Sin(looterAngle);

                        Looter looter = player.looters[i];
                        looter.move(cos * distance, sin * distance);

                        // If the looter touch a looter, reset everyone and try again
                        if (units.Any(u => u != looter && looter.distance(u) <= looter.radius + u.radius))
                        {
                            finished = false;
                            looters.ForEach(l => l.move(0.0, 0.0));
                            break;
                        }
                    }
                }
            }

            // Spawn start tankers
            for (int j = 0; j < TANKERS_BY_PLAYER; ++j)
            {
                foreach (Player player in players)
                {
                    spawnTanker(player);
                }
            }

            adjust();
            newFrame(1.0);
            snapshot();
        }

        public void prepare(int round)
        {
            frameData.Clear();
            looters.ForEach(l => (l as Looter).reset());
        }

        int getPlayerId(int id, int forId)
        {
            // This method can be called with id=-1 because of the default player for units
            if (id < 0)
            {
                return id;
            }

            if (id == forId)
            {
                return 0;
            }

            if (id < forId)
            {
                return id + 1;
            }

            return id;
        }

        public string[] getInputForPlayer(int round, int playerIdx)
        {
            List<Object> lines = new List<Object>();

            // Scores
            // My score is always first
            lines.Add(players[playerIdx].score);
            for (int i = 0; i < playerCount; ++i)
            {
                if (i != playerIdx)
                {
                    lines.Add(players[i].score);
                }
            }

            // Rages
            // My rage is always first
            lines.Add(players[playerIdx].rage);
            for (int i = 0; i < playerCount; ++i)
            {
                if (i != playerIdx)
                {
                    lines.Add(players[i].rage);
                }
            }

            // Units
            List<string> unitsLines = new List<string>();
            // Looters and tankers
            unitsLines.AddRange(units.Select(u => Utils.join(u.id, u.type, getPlayerId(u.getPlayerIndex(), playerIdx), u.mass, Utils.Round(u.radius), Utils.Round(u.x), Utils.Round(u.y), Utils.Round(u.vx), Utils.Round(u.vy), u.getExtraInput(), u.getExtraInput2())));
            // Wrecks
            unitsLines.AddRange(wrecks.Select(w => Utils.join(w.id, TYPE_WRECK, -1, -1, Utils.Round(w.radius), Utils.Round(w.x), Utils.Round(w.y), 0, 0, w.water, -1)));
            // Skill effects
            unitsLines.AddRange(skillEffects.Select(s => Utils.join(s.id, s.type, -1, -1, Utils.Round(s.radius), Utils.Round(s.x), Utils.Round(s.y), 0, 0, s.duration, -1)));

            lines.Add(unitsLines.Count);
            lines.AddRange(unitsLines);

            return lines.Select(l => l.ToString()).ToArray();
        }

        public enum Action
        {
            SKILL, MOVE, WAIT, UNDEFINED
        }

        public class SkillResult
        {
            public const int OK = 0;
            public const int NO_RAGE = 1;
            public const int TOO_FAR = 2;

            public Point target;
            public int code;

            public SkillResult(int x, int y)
            {
                target = new Point(x, y);
                code = OK;
            }

            public int getX()
            {
                return (int)target.x;
            }

            public int getY()
            {
                return (int)target.y;
            }
        }

        public void handlePlayerOutput(int round, int playerIdx, string[] outputs)
        {
            Player player = players[playerIdx];
            string expected = EXPECTED;

            for (int i = 0; i < LOOTER_COUNT; ++i)
            {
                try
                {
                    string line = outputs[i];

                    Looter looter = players[playerIdx].looters[i];

                    if (line == "WAIT")
                    {
                        looter.attempt = Action.WAIT;
                        continue;
                    }

                    string[] tokens = line.Split(' ');

                    if (tokens[0] == "SKILL")
                    {
                        if (!looter.skillActive)
                        {
                            // Don't kill the player for that. Just do a WAIT instead
                            looter.attempt = Action.WAIT;
                            continue;
                        }

                        looter.attempt = Action.SKILL;
                        int x = Int32.Parse(tokens[1]);
                        int y = Int32.Parse(tokens[2]);

                        SkillResult result = new SkillResult(x, y);
                        looter.skillResult = result;

                        try
                        {
                            SkillEffect effect = looter.skill(new Point(x, y));
                            skillEffects.Add(effect);
                        }
                        catch (SystemException e)
                        {
                            if (e.Message.Equals("RAGE"))
                                result.code = SkillResult.NO_RAGE;
                            else if (e.Message.Equals("FAR"))
                                result.code = SkillResult.TOO_FAR;
                            else
                                Console.Error.WriteLine("Unkown exception message");
                        }
                    }
                    else
                    {
                        looter.attempt = Action.MOVE;
                        int x = Int32.Parse(tokens[0]);
                        int y = Int32.Parse(tokens[1]);
                        int power = Int32.Parse(tokens[2]);

                        looter.setWantedThrust(new Point(x, y), power);
                    }
                }
                catch(Exception e)
                {
                    player.kill();
                    throw e;
                }
                
            }
        }


        // Get the next collision for the current Utils.Round
        // All units are tested
        Collision getNextCollision()
        {
            Collision result = NULL_COLLISION;

            for (int i = 0; i < units.Count; ++i)
            {
                Unit unit = units[i];

                // Test collision with map border first
                Collision collision = unit.getCollision();

                if (collision.t < result.t)
                {
                    result = collision;
                }

                for (int j = i + 1; j < units.Count; ++j)
                {
                    collision = unit.getCollision(units[j]);

                    if (collision.t < result.t)
                    {
                        result = collision;
                    }
                }
            }

            return result;
        }

        // Play a collision
        void playCollision(Collision collision)
        {
            if (collision.b == null)
            {
                // Bounce with border
                addToFrame(collision.a);
                collision.a.bounce();
            }
            else
            {
                Tanker dead = collision.dead();

                if (dead != null)
                {
                    // A destroyer kill a tanker
                    addDeadToFrame(dead);
                    deadTankers.Add(dead);
                    tankers.Remove(dead);
                    units.Remove(dead);

                    Wreck wreck = dead.die();

                    // If a tanker is too far away, there's no wreck
                    if (wreck != null)
                    {
                        wrecks.Add(wreck);
                        addToFrame(wreck);
                    }
                }
                else
                {
                    // Bounce between two units
                    addToFrame(collision.a);
                    addToFrame(collision.b);
                    collision.a.bounce(collision.b);
                }
            }
        }

        public void updateGame(int round)
        {
            Console.WriteLine("===== ROUND " + round + "=====");
            // Apply skill effects
            foreach (SkillEffect effect in skillEffects)
            {
                effect.apply(units);
            }

            // Apply thrust for tankers
            foreach (Tanker tanker in tankers)
            {
                tanker.play();
            }

            // Apply wanted thrust for looters
            foreach (Player player in players)
            {
                foreach (Looter looter in player.looters)
                {
                    if (looter.wantedThrustTarget != null)
                    {
                        looter.thrust(looter.wantedThrustTarget, looter.wantedThrustPower);
                    }
                }
            }

            double t = 0.0;

            // Play the Utils.Round. Stop at each collisions and play it. Reapeat until t > 1.0

            Collision collision = getNextCollision();

            while (collision.t + t <= 1.0)
            {
                double del = collision.t;
                units.ForEach(u => u.move(del));
                t += collision.t;

                newFrame(t);

                playCollision(collision);

                collision = getNextCollision();
            }

            // No more collision. Move units until the end of the Utils.Round
            double delta = 1.0 - t;
            units.ForEach(u => u.move(delta));

            List<Tanker> tankersToRemove = new List<Tanker>();

            tankers.ForEach(tanker => {
                double distance = tanker.distance(WATERTOWN);
                bool full = (tanker as Tanker).isFull();

                if (distance <= WATERTOWN_RADIUS && !full)
                {
                    // A non full tanker in watertown collect some water
                    (tanker as Tanker).water += 1;
                    (tanker as Tanker).mass += TANKER_MASS_BY_WATER;
                }
                else if (distance >= TANKER_SPAWN_RADIUS + tanker.radius && full)
                {
                    // Remove too far away and not full tankers from the game
                    tankersToRemove.Add((tanker as Tanker));
                }
            });

            newFrame(1.0);
            snapshot();

            if (tankersToRemove.Count > 0)
            {
                tankersToRemove.ForEach(tanker => addDeadToFrame(tanker));
            }

            tankersToRemove.ForEach(tanker =>
            {
                units.Remove(tanker);
                tankers.Remove(tanker);
            });

            deadTankers.AddRange(tankersToRemove);

            // Spawn new tankers for each dead tanker during the Utils.Round
            deadTankers.ForEach(tanker => spawnTanker(tanker.player));
            deadTankers.Clear();

            List<Wreck> deadWrecks = new List<Wreck>();

            // Water collection for reapers
            wrecks = wrecks.Where(w => {
                bool alive = w.harvest(players, skillEffects);

                if (!alive)
                {
                    addDeadToFrame(w);
                    deadWrecks.Add(w);
                }

                return alive;
            }).ToList();

            if (SPAWN_WRECK)
            {
                deadWrecks.ForEach(w => spawnTanker(w.player));
            }

            // Utils.Round values and apply friction
            adjust();

            // Generate rage
            if (LOOTER_COUNT >= 3)
            {
                players.ForEach(p => p.rage = Math.Min(MAX_RAGE, p.rage + p.getDoof().sing()));
            }

            // Restore masses
            units.ForEach(u => {
                while (u.mass >= REAPER_SKILL_MASS_BONUS)
                {
                    u.mass -= REAPER_SKILL_MASS_BONUS;
                }
            });

            // Remove dead skill effects
            List <SkillEffect> effectsToRemove = new List<SkillEffect>();
            foreach (SkillEffect effect in skillEffects)
            {
                if (effect.duration <= 0)
                {
                    addDeadToFrame(effect);
                    effectsToRemove.Add(effect);
                }
            }
            
            effectsToRemove.ForEach(e => skillEffects.Remove(e));
        }

        public void adjust()
        {
            units.ForEach(u => u.adjust(skillEffects));
        }
        
        public bool isGameOver()
        {
            if (players.Any(p => p.score >= WIN_SCORE))
            {
                // We got a winner !
                return true;
            }

            List<Player> alive = players.Where(p => !p.dead).ToList();

            if (alive.Count == 1)
            {
                Player survivor = alive[0];

                // If only one player is alive and he got the highest score, end the game now.
                return players.Where(p => p != survivor).All(p => p.score < survivor.score);
            }

            // Everyone is dead. End of the game.
            return alive.Count == 0;
        }

        public string[] getInitDataForView()
        {
            List<string> lines = new List<string>();

            lines.Add("" + playerCount);
            lines.Add("" + Utils.Round(MAP_RADIUS));
            lines.Add("" + Utils.Round(WATERTOWN_RADIUS));
            lines.Add("" + Utils.Round(LOOTER_COUNT));

            return lines.ToArray();
        }

        public string[] getFrameDataForView(int round, int frame, bool keyFrame)
        {
            List<string> lines = new List<string>();

            lines.AddRange(players.Select(p => "" + p.score));
            lines.AddRange(players.Select(p => "" + p.rage));
            lines.AddRange(looters.Select(l => (l as Looter).message == null ? "" : (l as Looter).message));
            lines.AddRange(frameData);

            return lines.ToArray();
        }    
    }
}
