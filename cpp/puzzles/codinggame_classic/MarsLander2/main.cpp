#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <memory>
#include <cmath>
#include <chrono>
#include <set>
#include <random>

#ifdef LOCAL
#include <SDL.h>
#endif

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

//#undef LOCAL
//#define DO_NOT_READ_FROM_CIN

using namespace std;

#ifdef LOCAL
static const char * WINDOW_TITLE = "Mars Lander 2";
static const int WINDOW_WIDTH = 1024;
static const int WINDOW_HEIGHT = 768;

SDL_Renderer * g_renderer;
SDL_Event g_event;

#endif

bool g_quit = false;
random_device g_randomDevice;
mt19937 g_random(g_randomDevice());
uniform_int_distribution<> g_randPower(-1, 1);
uniform_int_distribution<> g_randRotate(-15, 15);
uniform_int_distribution<> g_randMutate(0, 100);
uniform_int_distribution<> g_randTournament(0, 100);

static const double ZONE_WIDTH = 7000.;
static const double ZONE_HEIGHT = 3000.;
static const double GRAVITY = 3.711;
static const int LANDING_TARGET_ROTATE = 0;
static const double LANDING_MAX_VSPEED = 40.;
static const double LANDING_MAX_HSPEED = 20.;
static const int MAX_FUEL = 2000;
static const int MAX_SPEED = 500;

////////////////////////////////////////////////////////////
// AG PARAMS
static const int TURN_TO_SIMULATE_COUNT = 150;

static const int POPULATION_TOTAL = 500;
static const float ELITE_RATIO = 0.1f;
static const float OLD_GEN_RATIO = 0.3f;
static const float NEW_GEN_RATIO = 0.5f;
static const float RANDOM_RATIO = 0.1f;

static const int CROSSOVER_POINT_COUNT = 4;
static const float MUTATION_RATIO = 0.3f;
static const float TOURNAMENT_BEST_WIN_RATE = 0.85f;
////////////////////////////////////////////////////////////

static const int ELITE_COUNT = (int)round(POPULATION_TOTAL * ELITE_RATIO);
static const int NEW_GEN_COUNT = [&] {
	int childCount = (int)round(POPULATION_TOTAL * NEW_GEN_RATIO);
	return childCount % 2 == 0 ? childCount : childCount - 1;
}();
static const int GLADIATOR_COUNT = NEW_GEN_COUNT - ELITE_COUNT;
static const int SELECTED_COUNT = ELITE_COUNT + GLADIATOR_COUNT;

static const int TOTAL_TOURNAMENT_PARTICIPANTS = POPULATION_TOTAL - ELITE_COUNT;
static const int TOURNAMENT_COUNT = GLADIATOR_COUNT;
static const int PARTICIPANT_BY_TOURNAMENT_COUNT = [&] {

	if (TOURNAMENT_COUNT == 0)
		return 0;

	int participantByTournament = TOTAL_TOURNAMENT_PARTICIPANTS / TOURNAMENT_COUNT;
	int powOf2 = 2;
	while (participantByTournament >= pow(2, powOf2))
		++powOf2;

	return (int)pow(2, powOf2 - 1);
}();

static const int COUPLES_COUNT = SELECTED_COUNT / 2;
static const int OLD_GEN_COUNT = (int)round((float)POPULATION_TOTAL * OLD_GEN_RATIO);

static const int PARENT_REPLACED_COUNT = max(SELECTED_COUNT - ELITE_COUNT -	OLD_GEN_COUNT, 0);

static const int RANDOM_COUNT = (int)round((float)POPULATION_TOTAL * RANDOM_RATIO);
static const int RANDOM_EXTRA_COUNT = POPULATION_TOTAL - ELITE_COUNT - OLD_GEN_COUNT - NEW_GEN_COUNT - RANDOM_COUNT;

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
	double m_xmid, m_ymid;

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
		m_xmid = (m_x1 + m_x2) * 0.5;
		m_ymid = (m_y1 + m_y2) * 0.5;
	}

public:

	void get(int & a_outX1, int & a_outY1, int & a_outX2, int & a_outY2) const
	{
		a_outX1 = m_x1;
		a_outY1 = m_y1;
		a_outX2 = m_x2;
		a_outY2 = m_y2;
	}

	bool isFlat() const
	{
		return m_y1 == m_y2;
	}

	bool collidesWith(double a_x1, double a_y1, double a_x2, double a_y2) const
	{
		if (max(m_x1, m_x2) < min(a_x1, a_x2)
			|| min(m_x1, m_x2) > max(a_x1, a_x2)
			|| max(m_y1, m_y2) < min(a_y1, a_y2)
			|| min(m_y1, m_y2) > max(a_y1, a_y2))
			return false;

		bool isVertical = a_x1 == a_x2;

		double a = isVertical ? 0 : (a_y2 - a_y1) / (a_x2 - a_x1);

		if (isVertical && m_isVertical)
			return false;

		if (!isVertical && !m_isVertical && a == m_a)
			return false;

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

struct ShuttleAttributes
{
	enum class State
	{
		Flying,
		Crashed,
		LandingFailed,
		Landed,
		OutOfZone,
		UnderLandingArea
	};

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
};

class Shuttle
{
private:

	vector <ShuttleAttributes> m_attributes;

public:

	Shuttle(double a_x, double a_y, double a_vx, double a_vy, int a_rotate, int a_power, int a_fuel, ShuttleAttributes::State a_state)
	{
		m_attributes.push_back({ a_x, a_y, a_vx, a_vy, a_rotate, a_power, a_fuel, 0., 0., a_state });
	}

	Shuttle(const Shuttle & a_toCopy)
	{
		for(const auto & attr : a_toCopy.m_attributes)
			m_attributes.push_back(attr);
	}

public:

	const ShuttleAttributes & getLastAttributes() const { return m_attributes.back(); }
	const vector <ShuttleAttributes> & getAttributes() const { return m_attributes; }

public:

	unique_ptr<Shuttle> clone() const
	{
		return unique_ptr<Shuttle> (new Shuttle(*this));
	}

	void move(int a_targetRotate, int a_targetPower, const vector<LandLine> & a_landLines, int a_flatLandLineIndex)
	{
		int rotate = m_attributes.back().m_rotate;
		int power = m_attributes.back().m_power;

		int deltaRotate = a_targetRotate - rotate;
		if (deltaRotate < -15) rotate = max(rotate - 15, -90);
		else if (deltaRotate > 15) rotate = min(rotate + 15, 90);
		else rotate = min(max(rotate + deltaRotate, -90), 90);

		int deltaPower = a_targetPower - power;
		if (deltaPower < -1) power = max(power - 1, 0);
		else if (deltaPower > 1) power = min(power + 1, 4);
		else power = min(max(power + deltaPower, 0), 4);

		double ax = -sinDeg((double)rotate) * power;
		double ay = cosDeg((double)rotate) * power - GRAVITY;

		double prevX = m_attributes.back().m_x;
		double prevY = m_attributes.back().m_y;
		double vx = m_attributes.back().m_vx;
		double vy = m_attributes.back().m_vy;

		double x = prevX + vx + 0.5 * ax;
		double y = prevY + vy + 0.5 * ay;
		vx += ax;
		vy += ay;

		int fuel = m_attributes.back().m_fuel;
		fuel -= power;

		ShuttleAttributes::State state = m_attributes.back().m_state;

		//if (ydist < 0)
		//	m_state = State::UnderLandingArea;
		//else
		if (x < 0 || x >= ZONE_WIDTH || y < 0 || y >= ZONE_HEIGHT)
			state = ShuttleAttributes::State::OutOfZone;
		else
		{
			state = ShuttleAttributes::State::Flying;

			for (const auto & landLine : a_landLines)
			{
				if (landLine.collidesWith(prevX, prevY, x, y))
				{
					//cerr << "COLLIDE" << endl;
					if (landLine.isFlat())
					{
						if (rotate == LANDING_TARGET_ROTATE && abs(vx) <= LANDING_MAX_HSPEED && abs(vy) <= LANDING_MAX_VSPEED)
							state = ShuttleAttributes::State::Landed;
						else
							state = ShuttleAttributes::State::LandingFailed;
					}
					else
						state = ShuttleAttributes::State::Crashed;

					break;
				}
			}
		}
	
		m_attributes.push_back({ x, y, vx, vy, rotate, power, fuel, ax, ay, state });
	}
};

ostream& operator<<(ostream& a_ostream, const Shuttle& a_shuttle)
{
	const ShuttleAttributes & attr = a_shuttle.getLastAttributes();
	a_ostream << "Shuttle : X " << attr.m_x << " Y " << attr.m_y
		<< " Vx " << attr.m_vx << " Vy " << attr.m_vy
		<< " Rotate " << attr.m_rotate << " Power " << attr.m_power;
	return a_ostream;
}

struct EncodedActions
{
	static const int MASK_COUNT = 20;
	uint64_t masks[MASK_COUNT];

	EncodedActions()
	{
		for (int i = 0; i < MASK_COUNT; ++i)
			masks[i] = 0;
	}

	void encode(int a_actionIndex, int a_powerDelta, int a_rotateDelta)
	{
		uint64_t submask = (((a_powerDelta + 1) & 0b11) | ((a_rotateDelta + 15) << 2));
		masks[a_actionIndex / 8] &= ~((uint64_t) 0b11111111 << (8 * (a_actionIndex % 8)));
		masks[a_actionIndex / 8] |= submask << (8 * (a_actionIndex % 8)); 
	}

	void decode(int a_actionIndex, int & a_powerDelta, int & a_rotateDelta)
	{
		uint64_t actionMask = (uint64_t)(masks[a_actionIndex / 8] & ((uint64_t) 0b11111111 << (8 * (a_actionIndex % 8)))) >> (8 * (a_actionIndex % 8));
		a_powerDelta = (actionMask & 0b11) - 1;
		a_rotateDelta = ((actionMask & 0b1111100) >> 2) - 15;
	}

	void copy(const EncodedActions & a_source, int a_actionIndex)
	{
		uint64_t actionMask = (uint64_t) (a_source.masks[a_actionIndex / 8] & ((uint64_t) 0b11111111 << (8 * (a_actionIndex % 8)))) >> (8 * (a_actionIndex % 8));
		masks[a_actionIndex / 8] &= ~((uint64_t) 0b11111111 << (8 * (a_actionIndex % 8)));
		masks[a_actionIndex / 8] |= actionMask << (8 * (a_actionIndex % 8));
	}

	bool operator < (const EncodedActions & a_other) const
	{
		for (int i = 0; i < MASK_COUNT; ++i)
		{
			if (masks[i] < a_other.masks[i])
				return true;

			if (masks[i] > a_other.masks[i])
				return false;
		}

		return false;
	}
};

struct Genomia
{
	static const int LANDED_SCORE = 0;
	static const int FUEL_SCORE = 1;
	static const int LANDING_FAILED_SCORE = 2;
	static const int ROTATE_SCORE = 3;
	static const int VX_SCORE = 4;
	static const int VY_SCORE = 5;
	static const int XDIST_SCORE = 6;
	static const int YDIST_SCORE = 7;
	static const int OTHER_STATE_SCORE = 8;
	static const int TRAJECTORY_SCORE = 9;
	static const int SCORE_COUNT = 10;

	static const string SCORE_LABEL[SCORE_COUNT];

	int id;
	int mutationCount;
	EncodedActions actions;
	double score;
	bool evaluated;
	unique_ptr <Shuttle> finalShuttle;

	double scores[SCORE_COUNT];

	Genomia()
	{
		resetScores();
	}

	void resetScores()
	{
		for (int i = 0; i < SCORE_COUNT; ++i)
			scores[i] = -1.;
	}

	bool operator < (const Genomia & a_other) const
	{
		return score > a_other.score;
	}
};

const string Genomia::SCORE_LABEL[SCORE_COUNT] = { "LANDED", "FUEL", "LANDING_FAILED", "ROTATE", "VX", "VY", "XDIST", "YDIST", "OTHER_STATE", "TRAJECTORY" };


ostream& operator<<(ostream& a_ostream, const Genomia& a_genomia)
{
	a_ostream << "Genomia " << a_genomia.id << " [";

	if (a_genomia.finalShuttle->getLastAttributes().m_state == ShuttleAttributes::State::Landed)
		cerr << "Landed !";
	else if (a_genomia.finalShuttle->getLastAttributes().m_state == ShuttleAttributes::State::LandingFailed)
		cerr << "Landing Failed";
	else if (a_genomia.finalShuttle->getLastAttributes().m_state == ShuttleAttributes::State::Flying)
		cerr << "Flying";
	else if (a_genomia.finalShuttle->getLastAttributes().m_state == ShuttleAttributes::State::OutOfZone)
		cerr << "Out Of Zone";
	else if (a_genomia.finalShuttle->getLastAttributes().m_state == ShuttleAttributes::State::UnderLandingArea)
		cerr << "Under Landing Area";
	else
		cerr << "Crashed";

	cerr << " " << a_genomia.score << " - " << a_genomia.mutationCount << "]";
	//for (const auto & action : a_genomia.actions)
	//	a_ostream << " (" << action.rotateDelta << "," << action.powerDelta << ")";

	for (int i = 0; i < Genomia::SCORE_COUNT; ++i)
		cerr << " [" << Genomia::SCORE_LABEL[i] << " " << a_genomia.scores[i] << "]";

	return a_ostream;
}

#ifdef LOCAL

void handleEvents() 
{
	while (SDL_PollEvent(& g_event))
	{
		if (g_event.type == SDL_QUIT) 
			g_quit = true;
	}

	/*int x, y;
	SDL_GetMouseState(&x, &y);
	initialX = scale(x, 0, WINDOW_WIDTH - 1, 0, WIDTH - 1);
	initialY = scale(y, 0, WINDOW_HEIGHT - 1, HEIGHT - 1, 0);*/
}

int scale(int value, int fromMin, int fromMax, int toMin, int toMax) 
{
	return (int)round(((double)value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin);
}
void drawLine(int xa, int ya, int xb, int yb)
{
	int scaled_xa = scale(xa, 0, (int)ZONE_WIDTH - 1, 0, (int)WINDOW_WIDTH - 1);
	int scaled_ya = scale(ya, 0, (int)ZONE_HEIGHT - 1, (int)WINDOW_HEIGHT - 1, 0);
	int scaled_xb = scale(xb, 0, (int)ZONE_WIDTH - 1, 0, (int)WINDOW_WIDTH - 1);
	int scaled_yb = scale(yb, 0, (int)ZONE_HEIGHT - 1, (int)WINDOW_HEIGHT - 1, 0);
	SDL_RenderDrawLine(g_renderer, scaled_xa, scaled_ya, scaled_xb, scaled_yb);
}

void initWindow() 
{
	if (SDL_Init(SDL_INIT_VIDEO) != 0) 
	{
		fprintf(stdout, "Échec de l'initialisation de la SDL (%s)\n", SDL_GetError());
		exit(EXIT_FAILURE);
	}

	SDL_Window* pWindow = SDL_CreateWindow(
		WINDOW_TITLE,
		SDL_WINDOWPOS_UNDEFINED,
		SDL_WINDOWPOS_UNDEFINED,
		WINDOW_WIDTH,
		WINDOW_HEIGHT,
		SDL_WINDOW_SHOWN);

	if (!pWindow) {
		fprintf(stderr, "Erreur de création de la fenêtre: %s\n", SDL_GetError());
		exit(EXIT_FAILURE);
	}

	g_renderer = SDL_CreateRenderer(pWindow, -1, SDL_RENDERER_ACCELERATED);
	SDL_SetRenderDrawBlendMode(g_renderer, SDL_BLENDMODE_BLEND);
}

void render(const vector<LandLine> & a_landLines, const Shuttle & a_shuttle, const vector <Genomia> & a_population)
{
	SDL_SetRenderDrawColor(g_renderer, 20, 20, 20, 255);
	SDL_RenderClear(g_renderer);

	SDL_SetRenderDrawColor(g_renderer, 255, 255, 255, 255);

	for (const LandLine & line : a_landLines)
	{
		if (line.isFlat())
			SDL_SetRenderDrawColor(g_renderer, 0, 255, 0, 255);
		else
			SDL_SetRenderDrawColor(g_renderer, 255, 0, 0, 255);

		int x1, x2, y1, y2;
		line.get(x1, y1, x2, y2);
		drawLine(x1, y1, x2, y2);
	}

	for (int i = a_population.size() - 1; i >= 0; --i)
	{
		if(i == 0)
			SDL_SetRenderDrawColor(g_renderer, 255,	125, 0, 255);
		else
			SDL_SetRenderDrawColor(g_renderer, 255, 255, 0, 255);

		for (int j = 1; j < a_population[i].finalShuttle->getAttributes().size(); ++j)
		{
			const ShuttleAttributes & prevAttr = a_population[i].finalShuttle->getAttributes()[j - 1];
			const ShuttleAttributes & attr = a_population[i].finalShuttle->getAttributes()[j];
			drawLine(prevAttr.m_x, prevAttr.m_y, attr.m_x, attr.m_y);
		}
	}

	SDL_RenderPresent(g_renderer);
}

#endif

void eval(Genomia & a_ioGenomia, const Shuttle & a_shuttle, const vector<LandLine> & a_landLines, int a_flatLandLineIndex, int a_turnIndex)
{
	a_ioGenomia.score = 0;
	a_ioGenomia.resetScores();

	const ShuttleAttributes & attr = a_shuttle.getLastAttributes();

	a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE] = 0.;
	double turnDeltaScore = 1.f / ((double) a_shuttle.getAttributes().size() - (double)a_turnIndex);
	for (int i = a_turnIndex; i < a_shuttle.getAttributes().size(); ++i)
	{
		if (a_landLines[a_flatLandLineIndex].getDistY(a_shuttle.getAttributes()[i].m_y) >= 200.)
			a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE] += turnDeltaScore;
	}

	if (attr.m_state == ShuttleAttributes::State::Landed)
	{
		a_ioGenomia.scores[Genomia::LANDED_SCORE] = 100000.;
		a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE] *= 10.;
		a_ioGenomia.scores[Genomia::FUEL_SCORE] = attr.m_fuel;

		a_ioGenomia.score = a_ioGenomia.scores[Genomia::LANDED_SCORE] + a_ioGenomia.scores[Genomia::FUEL_SCORE] + a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE];
	}
	else if (attr.m_state == ShuttleAttributes::State::LandingFailed)
	{
		a_ioGenomia.scores[Genomia::LANDING_FAILED_SCORE] = 10000.;

		a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE] *= 200.;

		a_ioGenomia.scores[Genomia::ROTATE_SCORE] = 100. * (1. - (double)abs(attr.m_rotate) / 90);
		a_ioGenomia.scores[Genomia::VX_SCORE] = 200. * (1. - (max(abs(attr.m_vx), LANDING_MAX_HSPEED) - LANDING_MAX_HSPEED) / MAX_SPEED);
		a_ioGenomia.scores[Genomia::VY_SCORE] = 200. * (1. - (max(abs(attr.m_vy), LANDING_MAX_VSPEED) - LANDING_MAX_VSPEED) / MAX_SPEED);
	
		a_ioGenomia.score = a_ioGenomia.scores[Genomia::LANDING_FAILED_SCORE] + a_ioGenomia.scores[Genomia::ROTATE_SCORE] + a_ioGenomia.scores[Genomia::VX_SCORE] + a_ioGenomia.scores[Genomia::VY_SCORE] + a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE];
	}
	else // Crashed, flying, out of zone or under landing area
	{
		a_ioGenomia.scores[Genomia::OTHER_STATE_SCORE] = 1000.;

		a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE] *= 500.;

		a_ioGenomia.scores[Genomia::VX_SCORE] = 200. * (1. - (max(abs(attr.m_vx), LANDING_MAX_HSPEED) - LANDING_MAX_HSPEED) / MAX_SPEED);
		a_ioGenomia.scores[Genomia::VY_SCORE] = 500. * (1. - (max(abs(attr.m_vy), LANDING_MAX_VSPEED) - LANDING_MAX_VSPEED) / MAX_SPEED);

		double xdist = a_landLines[a_flatLandLineIndex].getDistX(attr.m_x);
		double xdistScore = 1. - abs(xdist) / ZONE_WIDTH;
		a_ioGenomia.scores[Genomia::XDIST_SCORE] = 150. * xdistScore;

		double ydist = a_landLines[a_flatLandLineIndex].getDistY(attr.m_y);
		double ydistScore = 1. - abs(ydist) / ZONE_HEIGHT;
		a_ioGenomia.scores[Genomia::YDIST_SCORE] = 50. * ydistScore;


		a_ioGenomia.score = a_ioGenomia.scores[Genomia::OTHER_STATE_SCORE] + a_ioGenomia.scores[Genomia::YDIST_SCORE] + a_ioGenomia.scores[Genomia::VX_SCORE] + a_ioGenomia.scores[Genomia::VY_SCORE] + a_ioGenomia.scores[Genomia::XDIST_SCORE] + a_ioGenomia.scores[Genomia::TRAJECTORY_SCORE];
	}
}

void generateAction(EncodedActions & a_ioActions, int a_actionIndex)
{
	int powerDelta = g_randPower(g_random);
	int rotateDelta = g_randRotate(g_random);
	a_ioActions.encode(a_actionIndex, powerDelta, rotateDelta);
}

void generateGenomia(Genomia & a_outGenomia, set<EncodedActions> & a_existingActions)
{
	a_outGenomia.id = genomeNextUId++;
	a_outGenomia.mutationCount = 0;
	a_outGenomia.score = -1;
	a_outGenomia.evaluated = false;

	bool actionsAreUnique = false;

	while (!actionsAreUnique)
	{
		for (int i = 0; i < TURN_TO_SIMULATE_COUNT; ++i)
			generateAction(a_outGenomia.actions, i);

		actionsAreUnique = a_existingActions.find(a_outGenomia.actions) == a_existingActions.end();
	}

	a_existingActions.insert(a_outGenomia.actions);
}

void createPopulation(vector <Genomia> & a_outPopulation, set<EncodedActions> & a_existingActions)
{
	for (int i = 0; i < POPULATION_TOTAL; ++i)
	{
		generateGenomia(a_outPopulation[i], a_existingActions);
	}
}


void performMutation(Genomia & a_ioGenomia, int a_turnIndex, set<EncodedActions> & a_existingActions, bool a_isBorn)
{
	if (a_isBorn)
		a_existingActions.erase(a_ioGenomia.actions);

	bool actionsAreUnique = false;

	while (!actionsAreUnique)
	{
		for (int i = 0; i < TURN_TO_SIMULATE_COUNT - a_turnIndex; ++i)
		{
			bool mutate = (float) g_randMutate(g_random) / 100.f <= MUTATION_RATIO;

			if(mutate)
				generateAction(a_ioGenomia.actions, i);
		}

		actionsAreUnique = a_existingActions.find(a_ioGenomia.actions) == a_existingActions.end();
	}

	if (a_isBorn)
	{
		a_existingActions.insert(a_ioGenomia.actions);

		++a_ioGenomia.mutationCount;
		a_ioGenomia.id = genomeNextUId++;

		a_ioGenomia.evaluated = false;
	}
}

void performCrossOver(Genomia & a_outChild1, Genomia & a_outChild2, const Genomia & a_parent1, const Genomia & a_parent2, int a_turnIndex, set<EncodedActions> & a_existingActions)
{
	a_outChild1.id = genomeNextUId++;
	a_outChild1.evaluated = false;
	a_outChild1.mutationCount = 0;

	a_outChild2.id = genomeNextUId++;
	a_outChild2.evaluated = false;
	a_outChild2.mutationCount = 0;

	a_existingActions.erase(a_outChild1.actions);
	a_existingActions.erase(a_outChild2.actions);

	int points[CROSSOVER_POINT_COUNT + 1];
	for (int i = 0; i < CROSSOVER_POINT_COUNT; ++i)
	{
		if (i == 0)
			points[i] = rand() % (TURN_TO_SIMULATE_COUNT - 1) + 1;
		else
			points[i] = rand() % (TURN_TO_SIMULATE_COUNT - (points[i - 1] + 1)) + (points[i - 1] + 1);

		if (points[i] == TURN_TO_SIMULATE_COUNT - 1)
		{
			for (int j = i + 1; j < CROSSOVER_POINT_COUNT; ++j)
				points[j] = -1;
			break;
		}
	}

	points[CROSSOVER_POINT_COUNT] = -1;

	for (int i = 0; i <= CROSSOVER_POINT_COUNT; ++i)
	{
		int start = (i == 0) ? 0 : points[i - 1];
		int end = points[i] != -1 ? points[i] : TURN_TO_SIMULATE_COUNT;

		int firstIs1 = rand() % 2;
		const Genomia & parentForChild1 = firstIs1 ? a_parent1 : a_parent2;
		const Genomia & parentForChild2 = firstIs1 ? a_parent2 : a_parent1;

		for (int j = start; j < end; ++j)
		{
			a_outChild1.actions.copy(parentForChild1.actions, j);
			a_outChild2.actions.copy(parentForChild2.actions, j);
		}

		if (points[i] == -1)
			break;
	}

	// Perform mutations on child that are same as a parent ....
	// We want to ensure that we have always unique solutions.

	if (a_existingActions.find(a_outChild1.actions) != a_existingActions.end())
		performMutation(a_outChild1, a_turnIndex, a_existingActions, false);

	a_existingActions.insert(a_outChild1.actions);

	if (a_existingActions.find(a_outChild2.actions) != a_existingActions.end())
		performMutation(a_outChild2, a_turnIndex, a_existingActions, false);

	a_existingActions.insert(a_outChild2.actions);
}

void generateNewGeneration(vector <Genomia> & a_ioPopulation, const Shuttle & a_shuttle, const vector<LandLine> & a_landLines, int a_flatLandLineIndex, int a_turnIndex, set<EncodedActions> & a_existingActions)
{
	////////
	//////// Evaluate the population
	////////

	for (int i = 0; i < POPULATION_TOTAL; ++i)
	{
		unique_ptr <Shuttle> shuttle = a_shuttle.clone();

		for (int j = a_turnIndex; j < TURN_TO_SIMULATE_COUNT; ++j)
		{
			int powerDelta, rotateDelta;
			a_ioPopulation[i].actions.decode(j, powerDelta, rotateDelta);
			shuttle->move(shuttle->getLastAttributes().m_rotate + rotateDelta, shuttle->getLastAttributes().m_power + powerDelta, a_landLines, a_flatLandLineIndex);


			if (shuttle->getLastAttributes().m_state != ShuttleAttributes::State::Flying)
				break;

			if (j == TURN_TO_SIMULATE_COUNT)
				cerr << "NOT ENOUGH TURNS !!!!" << endl;
		}

		eval(a_ioPopulation[i], *shuttle, a_landLines, a_flatLandLineIndex, a_turnIndex);
		a_ioPopulation[i].finalShuttle = move(shuttle);
	}

	////////
	//////// Sort the population from best genomia to worst one
	////////

	sort(a_ioPopulation.begin(), a_ioPopulation.end());

	////////
	//////// We will keep SELECTED_ELITE_COUNT best genomias
	//////// We have to select now SELECTED_TOURNAMENT_COUNT chroms using tournament method
	////////

	random_shuffle(a_ioPopulation.begin() + ELITE_COUNT, a_ioPopulation.end());

	int availableSlot = ELITE_COUNT;

	for (int i = 0; i < TOURNAMENT_COUNT; ++i)
	{
		int participantsLeft = PARTICIPANT_BY_TOURNAMENT_COUNT;

		while (participantsLeft > 1)
		{
			int start = ELITE_COUNT + i * participantsLeft;
			int end = ELITE_COUNT + (i + 1) * participantsLeft;
			int battleIndex = 0;

			for (int j = start; j < end; j += 2)
			{
				bool strongerWin = (float) g_randTournament(g_random) / 100.f <= TOURNAMENT_BEST_WIN_RATE;
				int stronger = j;
				int weaker = j + 1;

				if (a_ioPopulation[j + 1].score > a_ioPopulation[j].score)
					swap(stronger, weaker);

				if (strongerWin)
					iter_swap(a_ioPopulation.begin() + availableSlot + battleIndex, a_ioPopulation.begin() + stronger);
				else
					iter_swap(a_ioPopulation.begin() + availableSlot + battleIndex, a_ioPopulation.begin() + weaker);
			
				++battleIndex;
			}

			participantsLeft /= 2;
		}

		++availableSlot;
	}

	////////
	//////// Worst PARENT_REPLACED_COUNT parents will be replaced by children.
	////////

	sort(a_ioPopulation.begin(), a_ioPopulation.begin() + SELECTED_COUNT);
	availableSlot -= PARENT_REPLACED_COUNT;

	////////
	//////// Make COUPLES_COUNT couples of genomias in selected ones and do cross over.
	//////// Each couple produce 1 child.
	//////// First CHILD_MUTATION_COUNT produced children will mutate. 
	////////

	for (int i = 0; i < COUPLES_COUNT * 2; i += 2)
	{
		performCrossOver(a_ioPopulation[availableSlot], a_ioPopulation[availableSlot+1], a_ioPopulation[i], a_ioPopulation[i + 1], a_turnIndex, a_existingActions);
		performMutation(a_ioPopulation[availableSlot], a_turnIndex, a_existingActions, true);
		availableSlot += 2;
	}

	////////
	//////// Finally, fill the next generation with new random genomias
	////////

	for (int i = 0; i < RANDOM_COUNT + RANDOM_EXTRA_COUNT; ++i)
	{
		a_existingActions.erase(a_ioPopulation[availableSlot].actions);
		generateGenomia(a_ioPopulation[availableSlot], a_existingActions);
		++availableSlot;
	}

}

int main(int argc, char ** argv)
{
#ifdef LOCAL
	initWindow();
#endif

	//EncodedActions test1;
	//test1.encode(0, -1, 10);
	//test1.encode(1, 1, -5);
	//test1.encode(85, 0, 3);
	//int r, p;
	//test1.decode(0, p, r);
	//test1.decode(1, p, r);
	//test1.decode(85, p, r);

	//EncodedActions test2;
	//test2.copy(test1, 0);
	//test2.copy(test1, 1);
	//test2.copy(test1, 85);

	srand((uint32_t) time(NULL));

	chrono::steady_clock::time_point startTime = chrono::steady_clock::now();

	int surfaceN; // the number of points used to draw the surface of Mars.

#if ! defined(LOCAL) && ! defined(DO_NOT_READ_FROM_CIN) 
	cin >> surfaceN; cin.ignore();
#else
	surfaceN = 20;
#endif

	vector <LandLine> landLines;
	int prevLandX;
	int prevLandY;
	int flatLandLineIndex = -1;

	for (int i = 0; i < surfaceN; i++)
	{
		int landX;
		int landY;
		
#if ! defined(LOCAL) && ! defined(DO_NOT_READ_FROM_CIN) 
		cin >> landX >> landY; cin.ignore();
#else
		switch (i)
		{
			case 0:  landX = 0; landY = 1000; break;		  
			case 1:  landX = 300; landY = 1500; break;	   
			case 2:  landX = 350; landY = 1400; break;	   
			case 3:  landX = 500; landY = 2100; break;	   
			case 4:  landX = 1500; landY = 2100;break;	
			case 5:  landX = 2000; landY = 200; break;	
			case 6:  landX = 2500; landY = 500; break;	
			case 7:  landX = 2900; landY = 300; break;	
			case 8:  landX = 3000; landY = 200; break;	
			case 9:  landX = 3200; landY = 1000;break;	
			case 10: landX = 3500; landY = 500; break;	
			case 11: landX = 3800; landY = 800; break;	
			case 12: landX = 4000; landY = 200; break;	
			case 13: landX = 4200; landY = 800; break;	
			case 14: landX = 4800; landY = 600; break;	
			case 15: landX = 5000; landY = 1200;break;	
			case 16: landX = 5500; landY = 900;	break;	
			case 17: landX = 6000; landY = 500;	break;	
			case 18: landX = 6500; landY = 300;	break;	
			case 19: landX = 6999; landY = 500;	break;	
		};
#endif 

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

	vector <Genomia> population(POPULATION_TOTAL);
	set<EncodedActions> existingActions;

	createPopulation(population, existingActions);

	int turnIndex = 0;
	double prevBestScore = -1.;

#ifndef DO_NOT_READ_FROM_CIN 
	while (! g_quit)
#else
	while(shuttle == nullptr || shuttle->getState() == Shuttle::State::Flying)
#endif
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
		
#if ! defined(LOCAL) && ! defined(DO_NOT_READ_FROM_CIN) 
		cin >> X >> Y >> hSpeed >> vSpeed >> fuel >> rotate >> power; cin.ignore();

		if (shuttle != nullptr)
		{
			if (safeRound(shuttle->getLastAttributes().m_x) != X) cerr << "Bad X : " << safeRound(shuttle->getLastAttributes().m_x) << " vs " << X << endl;
			if (safeRound(shuttle->getLastAttributes().m_y) != Y) cerr << "Bad Y : " << safeRound(shuttle->getLastAttributes().m_y) << " vs " << Y << endl;
			if (safeRound(shuttle->getLastAttributes().m_vx) != hSpeed) cerr << "Bad Vx : " << safeRound(shuttle->getLastAttributes().m_vx) << " vs " << hSpeed << endl;
			if (safeRound(shuttle->getLastAttributes().m_vy) != vSpeed) cerr << "Bad Vy : " << safeRound(shuttle->getLastAttributes().m_vy) << " vs " << vSpeed << endl;
			if (shuttle->getLastAttributes().m_rotate != rotate) cerr << "Bad Rotate : " << shuttle->getLastAttributes().m_rotate << " vs " << rotate << endl;
			if (shuttle->getLastAttributes().m_power != power) cerr << "Bad Power : " << shuttle->getLastAttributes().m_power << " vs " << power << endl;
			if (shuttle->getLastAttributes().m_fuel != fuel) cerr << "Bad Fuel : " << shuttle->getLastAttributes().m_fuel << " vs " << fuel << endl;
		}
#else
		X = 6500;
		Y = 2700;
		hSpeed = -50;
		vSpeed = 0;
		fuel = 1000;
		rotate = 90;
		power = 0;
#endif 

		if (shuttle == nullptr)
		{
			cerr << "Create shuttle" << endl;
			shuttle.reset(new Shuttle((double)X, (double)Y, (double)hSpeed, (double)vSpeed, rotate, power, fuel, ShuttleAttributes::State::Flying));
		}

		int generationId = 0;

		int64_t elapsedTime = chrono::duration_cast <chrono::milliseconds> (chrono::steady_clock::now() - startTime).count();

#ifndef LOCAL
		while (elapsedTime < 95)
#else
		while(! g_quit && generationId <= 100)
#endif
		{
			//cerr << "========" << endl;
			generateNewGeneration(population, *shuttle, landLines, flatLandLineIndex, turnIndex, existingActions);

			elapsedTime = chrono::duration_cast <chrono::milliseconds> (chrono::steady_clock::now() - startTime).count();
			++generationId;
		
#ifdef LOCAL
			//cerr << "==================" << endl;
			//for(const auto & genomia : population)
			//	cerr << genomia << endl;
			//cerr << existingActions.size() << endl;
			//cerr << population[0] << endl;
			if (prevBestScore < population[0].score)
			{
				prevBestScore = population[0].score;
				cerr << population[0] << endl;
			}

			handleEvents();
			sort(population.begin(), population.end());
			render(landLines, *shuttle, population);
#endif 
		}

		cerr << "Turn " << turnIndex << " : " << generationId << " generations" << endl;
		cerr << population[0] << endl;

		int rotateDelta, powerDelta;
		population[0].actions.decode(turnIndex, powerDelta, rotateDelta);

		shuttle->move(shuttle->getLastAttributes().m_rotate + rotateDelta, shuttle->getLastAttributes().m_power + powerDelta, landLines, flatLandLineIndex);

#ifndef LOCAL
		cout << shuttle->getLastAttributes().m_rotate << " " << shuttle->getLastAttributes().m_power << endl;
#endif
		++turnIndex;
	}

#ifdef LOCAL
	SDL_Quit();
#endif

#ifdef DO_NOT_READ_FROM_CIN 
	cout << "END" << endl;
	int a;
	cin >> a;
#endif

	return 0;
}