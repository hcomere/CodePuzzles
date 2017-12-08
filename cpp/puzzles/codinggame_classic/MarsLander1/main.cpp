#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <memory>
#include <cmath>
#include <chrono>
#include <set>

#ifndef M_PI
	#define M_PI 3.14159265358979323846
#endif

using namespace std;

static const double ZONE_WIDTH = 7000.;
static const double ZONE_HEIGHT = 3000.;
static const double GRAVITY = 3.711;
static const int MIN_ROTATE = -90;
static const int MAX_ROTATE = 90;
static const int MIN_POWER = 0;
static const int MAX_POWER = 4;
static const int MAX_ROTATE_DELTA = 15;
static const int MAX_POWER_DELTA = 1;
static const int LANDING_TARGET_ROTATE = 0;
static const int LANDING_MAX_VSPEED = 40;
static const int LANDING_MAX_HSPEED = 20;
static const int MAX_FUEL = 2000;
static const int MAX_SPEED = 500;

static const int TURN_TO_SIMULATE_COUNT = 100;
static const int POPULATION_SIZE = 200;
static const int SELECTED_COUNT = (int)ceil(POPULATION_SIZE * 0.5f);
static const int GRADED_SELECTED_COUNT = (int)ceil(SELECTED_COUNT * 0.6f);
static const int NON_GRADED_SELECTED_COUNT = SELECTED_COUNT - GRADED_SELECTED_COUNT;
static const int MUTATION_PROBABILITY = 80;

static int genomeNextUId = 0;
static int landedCount = 0;

int safeRound(double a_val)
{
	int sign = a_val < 0 ? -1 : 1;
	return sign * (int)round(sign * a_val);
}

double toRadians(double a_degrees)
{
	return a_degrees * (M_PI / 180.);
}

double sinDeg(double a_degrees)
{
	return sin(toRadians(a_degrees));
}

double cosDeg(double a_degrees)
{
	return cos(toRadians(a_degrees));
}

class LandLine
{
private:

	double m_x1, m_y1, m_x2, m_y2;
	double m_a, m_b;
	bool m_isVertical;

public:

	LandLine(double a_x1, double a_y1, double a_x2, double a_y2)
		: m_x1(a_x1)
		, m_y1(a_y1)
		, m_x2(a_x2)
		, m_y2(a_y2)
	{
		m_isVertical = a_x1 == a_x2;

		m_a = m_isVertical ? 0. : (a_y2 - a_y1) / (a_x2 - a_x1);
		m_b = m_isVertical ? 0. : a_y1 - m_a * a_x1;
	}

public:

	bool isFlat() const
	{
		return m_y1 == m_y2;
	}

	bool collidesWith(double a_x1, double a_y1, double a_x2, double a_y2) const
	{
		/*bool flat = isFlat();
		if(flat)
		{
		cerr << "test" << endl;
		cerr << "-- Line : " << m_x1 << " " << m_y1 << " " << m_x2 << " " << m_y2 << endl;
		cerr << "Test : " << a_x1 << " " << a_y1 << " " << a_x2 << " " << a_y2 << endl;

		}*/

		if (max(m_x1, m_x2) < min(a_x1, a_x2)
			|| min(m_x1, m_x2) > max(a_x1, a_x2)
			|| max(m_y1, m_y2) < min(a_y1, a_y2)
			|| min(m_y1, m_y2) > max(a_y1, a_y2))
			return false;

		bool isVertical = a_x1 == a_x2;

		double a = isVertical ? 0 : (a_y2 - a_y1) / (a_x2 - a_x1);

		//cerr << "2 : " << a << " vs " << m_a << endl;

		if (isVertical && m_isVertical)
			return false;

		if (!isVertical && !m_isVertical && a == m_a)
			return false;

		//if(flat) cerr << "3" << endl;

		double x = 0.;

		if (isVertical)
			x = a_x1;
		else if (m_isVertical)
			x = m_x1;
		else
		{
			double b = a_y1 - a_x1 * a;
			x = (b - m_b) / (m_a - a);
		}

		double xmin = max(min(m_x1, m_x2), min(a_x1, a_x2));
		double xmax = min(max(m_x1, m_x2), max(a_x1, a_x2));

		//if(flat)
		//    cerr << "==> " << (x >= xmin && x <= xmax) << endl;


		return x >= xmin && x <= xmax;
	}

	double getDistX(double a_x) const
	{
		return min(abs(a_x - m_x1), abs(a_x - m_x2));
	}

	double getDistY(double a_y) const
	{
		if (isFlat())
			return a_y - m_y1;
		else
			return ZONE_HEIGHT;
	}
};

class Shuttle
{
public:

	enum class State
	{
		Flying,
		Crashed,
		LandingFailed,
		Landed
	};

private:

	double m_x;
	double m_y;
	double m_vx;
	double m_vy;
	int m_rotate;
	int m_power;
	int m_fuel;

	double m_ax;
	double m_ay;
	State m_state;

public:

	Shuttle(double a_x, double a_y, double a_vx, double a_vy, int a_rotate, int a_power, int a_fuel, State a_state)
		: m_x(a_x)
		, m_y(a_y)
		, m_vx(a_vx)
		, m_vy(a_vy)
		, m_rotate(a_rotate)
		, m_power(a_power)
		, m_fuel(a_fuel)
		, m_ax(0.)
		, m_ay(0.)
		, m_state(a_state)
	{
	}

public:

	double getX() const { return m_x; }
	double getY() const { return m_y; }
	double getVx() const { return m_vx; }
	double getVy() const { return m_vy; }
	int getRotate() const { return m_rotate; }
	int getPower() const { return m_power; }
	int getFuel() const { return m_fuel; }

public:

	unique_ptr<Shuttle> clone() const
	{
//		return nullptr;
		unique_ptr<Shuttle> clone(new Shuttle(m_x, m_y, m_vx, m_vy, m_rotate, m_power, m_fuel, m_state));
		clone->m_ax = m_ax;
		clone->m_ay = m_ay;
		//clone->m_state = m_state;
		return clone;
	}

	State getState() const
	{
		return m_state;
	}

	void move(int a_targetRotate, int a_targetPower, const vector<LandLine> & a_landLines)
	{
		int deltaRotate = a_targetRotate - m_rotate;
		if (deltaRotate < -MAX_ROTATE_DELTA) m_rotate = max(m_rotate - MAX_ROTATE_DELTA, MIN_ROTATE);
		else if (deltaRotate > MAX_ROTATE_DELTA) m_rotate = min(m_rotate + MAX_ROTATE_DELTA, MAX_ROTATE);
		else m_rotate = min(max(m_rotate + deltaRotate, MIN_ROTATE), MAX_ROTATE);

		int deltaPower = a_targetPower - m_power;
		if (deltaPower < -MAX_POWER_DELTA) m_power = max(m_power - MAX_POWER_DELTA, MIN_POWER);
		else if (deltaPower > MAX_POWER_DELTA) m_power = min(m_power + MAX_POWER_DELTA, MAX_POWER);
		else m_power = min(max(m_power + deltaPower, MIN_POWER), MAX_POWER);

		m_ax = -sinDeg((double)m_rotate) * m_power;
		m_ay = cosDeg((double)m_rotate) * m_power - GRAVITY;

		double prevX = m_x;
		double prevY = m_y;

		m_x += m_vx + 0.5 * m_ax;
		m_y += m_vy + 0.5 * m_ay;
		m_vx += m_ax;
		m_vy += m_ay;
		m_fuel -= m_power;

		if (m_x < 0 || m_x >= ZONE_WIDTH || m_y < 0 || m_y >= ZONE_HEIGHT)
			m_state = State::Crashed;
		else
		{
			m_state = State::Flying;

			for (const auto & landLine : a_landLines)
			{
				if (landLine.collidesWith(prevX, prevY, m_x, m_y))
				{
					//cerr << "COLLIDE" << endl;
					if (landLine.isFlat())
					{
						if (m_rotate == LANDING_TARGET_ROTATE && abs(m_vx) <= LANDING_MAX_HSPEED && abs(m_vy) <= LANDING_MAX_VSPEED)
							m_state = State::Landed;
						else
							m_state = State::LandingFailed;
					}
					else
						m_state = State::Crashed;

					break;
				}
			}
		}
	}
};

ostream& operator<<(ostream& a_ostream, const Shuttle& a_shuttle)
{
	a_ostream << "Shuttle : X " << a_shuttle.getX() << " Y " << a_shuttle.getY()
		<< " Vx " << a_shuttle.getVx() << " Vy " << a_shuttle.getVy()
		<< " Rotate " << a_shuttle.getRotate() << " Power " << a_shuttle.getPower();
	return a_ostream;
}

struct Action
{
	int rotateDelta;
	int powerDelta;
};

struct Genomia
{
	int id;
	int mutationCount;
	Action actions[TURN_TO_SIMULATE_COUNT];
	double score;
	Shuttle::State state;

	bool operator < (const Genomia & a_other) const
	{
		return score > a_other.score;
	}
};

ostream& operator<<(ostream& a_ostream, const Genomia& a_genomia)
{
	a_ostream << "Genomia " << a_genomia.id << " [";

	if (a_genomia.state == Shuttle::State::Landed)
		cerr << "Landed !";
	else if (a_genomia.state == Shuttle::State::LandingFailed)
		cerr << "Landing Failed";
	else if (a_genomia.state == Shuttle::State::Flying)
		cerr << "Flying";
	else
		cerr << "Crashed";

	cerr << " " << a_genomia.score << " - " << a_genomia.mutationCount << "]";
	for (const auto & action : a_genomia.actions)
		a_ostream << " (" << action.rotateDelta << "," << action.powerDelta << ")";

	return a_ostream;
}

double eval(const Shuttle & a_shuttle, const vector<LandLine> & a_landLines, int a_flatLandLineIndex)
{
	double score = 0;

	if (a_shuttle.getState() == Shuttle::State::Landed)
	{
		score = 100000.;
		score += a_shuttle.getFuel();
	}
	else if (a_shuttle.getState() == Shuttle::State::LandingFailed)
	{
		score = 10000.;

		// Rotate closer to 0 is better
		score += 90. - (double)abs(a_shuttle.getRotate());

		score += (double)MAX_SPEED - (abs(a_shuttle.getVx()));

		score += (double)MAX_SPEED - (abs(a_shuttle.getVy()));
	}
	else // Crashed or flying
	{
		score += 1000.;

		// Nearest to land area is better

		double xdist = a_landLines[a_flatLandLineIndex].getDistX(a_shuttle.getX());
		double xdistScore = 1. - xdist / ZONE_WIDTH;
		score += 100. * xdistScore;

		double ydist = a_landLines[a_flatLandLineIndex].getDistY(a_shuttle.getY());
		double ydistScore = 1. - ydist / ZONE_HEIGHT;
		score += 100. * ydistScore;
	}

	return score;
}

void generateAction(Action & a_outAction)
{
	a_outAction.powerDelta = rand() % (2 * MAX_POWER_DELTA + 1) - MAX_POWER_DELTA;
	//a_outAction.rotateDelta = rand() % (2 * MAX_ROTATE_DELTA + 1) - MAX_ROTATE_DELTA;
	a_outAction.rotateDelta = 0;
}

void generateGenomia(Genomia & a_outGenomia)
{
	a_outGenomia.id = genomeNextUId++;
	a_outGenomia.mutationCount = 0;
	a_outGenomia.score = -1;

	for (int i = 0; i < TURN_TO_SIMULATE_COUNT; ++i)
		generateAction(a_outGenomia.actions[i]);
}

void createPopulation(vector <Genomia> & a_outPopulation)
{
	for (int i = 0; i < POPULATION_SIZE; ++i)
		generateGenomia(a_outPopulation[i]);
}

void performSelection(vector <Genomia> & a_ioPopulation, const Shuttle & a_shuttle, const vector<LandLine> & a_landLines, int a_flatLandLineIndex, int a_turnIndex)
{
	for (int i = 0; i < POPULATION_SIZE; ++i)
	{
		//cerr << "------------" << endl;
		unique_ptr <Shuttle> shuttle = a_shuttle.clone();

		for (int j = a_turnIndex; j < TURN_TO_SIMULATE_COUNT; ++j)
		{
			shuttle->move(shuttle->getRotate() + a_ioPopulation[i].actions[j].rotateDelta, shuttle->getPower() + a_ioPopulation[i].actions[j].powerDelta, a_landLines);

			if (shuttle->getState() == Shuttle::State::Landed)
			{
				//cerr << "Found !! " << a_ioPopulation[i] << endl;
				//cerr << *shuttle << endl;
			}

			if (shuttle->getState() != Shuttle::State::Flying)
				break;

			if (j == TURN_TO_SIMULATE_COUNT)
				cerr << "NOT ENOUGH TURNS !!!!" << endl;
		}

		Shuttle::State oldState = a_ioPopulation[i].state;
		a_ioPopulation[i].score = eval(*shuttle, a_landLines, a_flatLandLineIndex);
		a_ioPopulation[i].state = shuttle->getState();

		if (oldState == Shuttle::State::Landed && a_ioPopulation[i].state != Shuttle::State::Landed)
		{
			//cerr << "Oh noooooooooooo : " << a_ioPopulation[i] << endl;
			//cerr << *shuttle << endl;
		}
	}

	sort(a_ioPopulation.begin(), a_ioPopulation.end());
	
	//cerr << "=== Evaluation ===" << endl;
	//for(int i = 0; i < POPULATION_SIZE; ++ i)
	//   cerr << a_ioPopulation[i] << endl;

	set <int> selected;

	while ((int) selected.size() < NON_GRADED_SELECTED_COUNT)
	{
		int idx = rand() % (POPULATION_SIZE - GRADED_SELECTED_COUNT) + GRADED_SELECTED_COUNT;

		if (selected.find(idx) != selected.end())
			continue;

		swap(a_ioPopulation[GRADED_SELECTED_COUNT + selected.size()], a_ioPopulation[idx]);
		selected.insert(idx);
	}
}

void performCrossOver(Genomia & a_outChild, const Genomia & a_parent1, const Genomia & a_parent2, int a_turnIndex)
{
	a_outChild.id = genomeNextUId++;

	for (int i = a_turnIndex; i < (TURN_TO_SIMULATE_COUNT - a_turnIndex) / 2; ++i)
		a_outChild.actions[i] = a_parent1.actions[i];

	for (int i = (TURN_TO_SIMULATE_COUNT - a_turnIndex) / 2; i < TURN_TO_SIMULATE_COUNT; ++i)
		a_outChild.actions[i] = a_parent2.actions[i];
}

void performMutation(Genomia & a_ioGenomia, int a_turnIndex)
{
	int turn = a_turnIndex + rand() % (TURN_TO_SIMULATE_COUNT - a_turnIndex);
	generateAction(a_ioGenomia.actions[turn]);
	++a_ioGenomia.mutationCount;
}

void generateNewGeneration(vector <Genomia> & a_ioPopulation, const Shuttle & a_shuttle, const vector<LandLine> & a_landLines, int a_flatLandLineIndex, int a_turnIndex)
{
	performSelection(a_ioPopulation, a_shuttle, a_landLines, a_flatLandLineIndex, a_turnIndex);

	for (int i = SELECTED_COUNT; i < POPULATION_SIZE; ++i)
	{
		int idx1 = rand() % SELECTED_COUNT;
		int idx2 = rand() % SELECTED_COUNT;

		while (idx1 == idx2)
			idx2 = rand() % SELECTED_COUNT;

		performCrossOver(a_ioPopulation[i], a_ioPopulation[idx1], a_ioPopulation[idx2], a_turnIndex);

		int mutationTest = rand() % 100;
		if (mutationTest <= MUTATION_PROBABILITY)
			performMutation(a_ioPopulation[i], a_turnIndex);
	}
}

int main()
{
	srand(time(NULL));

	chrono::steady_clock::time_point startTime = chrono::steady_clock::now();

	//int surfaceN; // the number of points used to draw the surface of Mars.
	//cin >> surfaceN; cin.ignore();
	int surfaceN = 6;

	vector <LandLine> landLines;
	int prevLandX;
	int prevLandY;
	int flatLandLineIndex = -1;

	for (int i = 0; i < surfaceN; i++)
	{
		int landX;
		int landY;
		//		cin >> landX >> landY; cin.ignore();

		switch (i)
		{
		case 0: landX = 0; landY = 100; break;
		case 1: landX = 1000; landY = 500; break;
		case 2: landX = 1500; landY = 100; break;
		case 3: landX = 3000; landY = 100; break;
		case 4: landX = 5000; landY = 1500; break;
		case 5: landX = 6999; landY = 1000; break;
		};

		if (i > 0)
		{
			landLines.push_back(LandLine((double)prevLandX, (double)prevLandY, (double)landX, (double)landY));
			if (landLines.back().isFlat())
				flatLandLineIndex = landLines.size() - 1;
		}

		prevLandX = landX;
		prevLandY = landY;
	}

	unique_ptr <Shuttle> shuttle = nullptr;

	vector <Genomia> population(POPULATION_SIZE);
	createPopulation(population);

	int turnIndex = 0;

	// game loop
	while (turnIndex < 80)
	{
		if (turnIndex > 0)
			startTime = chrono::steady_clock::now();

		int X;
		int Y;
		int hSpeed; // the horizontal speed (in m/s), can be negative.
		int vSpeed; // the vertical speed (in m/s), can be negative.
		int fuel; // the quantity of remaining fuel in liters.
		int rotate; // the rotation angle in degrees (-90 to 90).
		int power; // the thrust power (0 to 4).
		//cin >> X >> Y >> hSpeed >> vSpeed >> fuel >> rotate >> power; cin.ignore();

		X = 2500;
		Y = 2500;
		hSpeed = 0;
		vSpeed = 0;
		fuel = 500;
		rotate = 0;
		power = 0;

/*		if (shuttle != nullptr)
		{
			if (safeRound(shuttle->getX()) != X) cerr << "Bad X : " << safeRound(shuttle->getX()) << " vs " << X << endl;
			if (safeRound(shuttle->getY()) != Y) cerr << "Bad Y : " << safeRound(shuttle->getY()) << " vs " << Y << endl;
			if (safeRound(shuttle->getVx()) != hSpeed) cerr << "Bad Vx : " << safeRound(shuttle->getVx()) << " vs " << hSpeed << endl;
			if (safeRound(shuttle->getVy()) != vSpeed) cerr << "Bad Vy : " << safeRound(shuttle->getVy()) << " vs " << vSpeed << endl;
			if (shuttle->getRotate() != rotate) cerr << "Bad Rotate : " << shuttle->getRotate() << " vs " << rotate << endl;
			if (shuttle->getPower() != power) cerr << "Bad Power : " << shuttle->getPower() << " vs " << power << endl;
			if (shuttle->getFuel() != fuel) cerr << "Bad Fuel : " << shuttle->getFuel() << " vs " << fuel << endl;
		}*/

		if (shuttle == nullptr)
			shuttle.reset(new Shuttle((double)X, (double)Y, (double)hSpeed, (double)vSpeed, rotate, power, fuel, Shuttle::State::Flying));

		int generationId = 0;

		int64_t elapsedTime = chrono::duration_cast <chrono::milliseconds> (chrono::steady_clock::now() - startTime).count();

		//while (elapsedTime < 95)
		while(generationId < 10000)
		{
			//cerr << "========" << endl;
			generateNewGeneration(population, *shuttle, landLines, flatLandLineIndex, turnIndex);

			elapsedTime = chrono::duration_cast <chrono::milliseconds> (chrono::steady_clock::now() - startTime).count();
			++generationId;

			cerr << population[0] << endl;
		}

		//cerr << "Turn " << turnIndex << " : " << generationId << " generations" << endl;

		cerr << population[0] << endl;
		
		//shuttle->move(shuttle->getRotate() + population[0].actions[turnIndex].rotateDelta, shuttle->getPower() + population[0].actions[turnIndex].powerDelta, landLines);

		cout << shuttle->getRotate() << " " << shuttle->getPower() << endl;

		//int bestRotate = max(min(rotate + population[0].actions[turnIndex].rotateDelta, MAX_ROTATE), MIN_ROTATE);
		//int bestPower = max(min(power + population[0].actions[turnIndex].powerDelta, MAX_POWER), MIN_POWER);

		//cout << bestRotate << " " << bestPower << endl;

		//shuttle->move(bestRotate, bestPower, landLines);

		++turnIndex;
	}

	int aaa;
	cin >> aaa;
}