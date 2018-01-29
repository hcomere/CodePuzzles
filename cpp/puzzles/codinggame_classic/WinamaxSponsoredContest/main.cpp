#if _WIN32
#include <Validator.h>
#endif 

#include <iostream>
#include <string>
#include <vector>
#include <tuple>
#include <algorithm>

using namespace std;

enum class CellType
{
	BALL_IN_HOLE = 0,
	BALL_1,
	BALL_2,
	BALL_3,
	BALL_4,
	BALL_5,
	BALL_6,
	BALL_7,
	BALL_8,
	BALL_9,
	EMPTY,
	WATER,
	HOLE, // Hole and below are considered as not passable but Hole can be the end point.
	UP_EMPTY,
	UP_WATER,
	RIGHT_EMPTY,
	RIGHT_WATER,
	DOWN_EMPTY,
	DOWN_WATER,
	LEFT_EMPTY,
	LEFT_WATER
};

struct Ball
{
	CellType type;
	int row;
	int col;
};

static vector <pair <int, int>> DIR_DELTAS =
{
	pair<int,int>(-1, 0),
	pair<int,int>(0, 1),
	pair<int,int>(1, 0),
	pair<int,int>(0, -1)
};

static const int DIR_LEFT = 0;
static const int DIR_UP = 1;
static const int DIR_RIGHT = 2;
static const int DIR_DOWN = 3;

bool isPathFree(const vector <vector <CellType>> & a_map, int a_row1, int a_col1, int a_row2, int a_col2)
{
	bool pathIsFree = true;

	if (a_row1 == a_row2)
	{
		if (a_col1 > a_col2)
		{
			for (int col = a_col1; pathIsFree && col > a_col2; --col)
				pathIsFree = (int)a_map[a_row1][col] < (int)CellType::HOLE;
		}
		else
		{
			for (int col = a_col1; pathIsFree && col < a_col2; ++col)
				pathIsFree = (int)a_map[a_row1][col] < (int)CellType::HOLE;
		}
	}
	else if (a_col1 == a_col2)
	{
		if (a_row1 > a_row2)
		{
			for (int row = a_row1; pathIsFree && row > a_row2; --row)
				pathIsFree = (int)a_map[row][a_col1] < (int)CellType::HOLE;
		}
		else
		{
			for (int row = a_row1; pathIsFree && row < a_row2; ++row)
				pathIsFree = (int)a_map[row][a_col1] < (int)CellType::HOLE;
		}
	}
	else
		throw runtime_error("Bad path definition");

	return pathIsFree;
}

void tracePath(vector <vector <CellType>> & a_map, int a_row1, int a_col1, int a_row2, int a_col2, bool a_revert)
{
	if (a_row1 == a_row2)
	{
		if (a_col1 > a_col2)
		{
			CellType dirEmptyType = CellType::LEFT_EMPTY;
			CellType dirWaterType = CellType::LEFT_WATER;

			for (int col = a_col1; col > a_col2; --col)
			{
				if (a_revert)
					a_map[a_row1][col] = a_map[a_row1][col] == dirEmptyType ? CellType::EMPTY : CellType::WATER;
				else
					a_map[a_row1][col] = a_map[a_row1][col] == CellType::EMPTY ? dirEmptyType : dirWaterType;
			}
		}
		else
		{
			CellType dirEmptyType = CellType::RIGHT_EMPTY;
			CellType dirWaterType =	CellType::RIGHT_WATER;

			for (int col = a_col1; col < a_col2; ++col)
			{
				if (a_revert)
					a_map[a_row1][col] = a_map[a_row1][col] == dirEmptyType ? CellType::EMPTY : CellType::WATER;
				else
					a_map[a_row1][col] = a_map[a_row1][col] == CellType::EMPTY ? dirEmptyType : dirWaterType;
			}
		}
	}
	else if (a_col1 == a_col2)
	{
		if(a_row1 > a_row2)
		{
			CellType dirEmptyType =	CellType::UP_EMPTY;
			CellType dirWaterType = CellType::UP_WATER;

			for (int row = a_row1; row > a_row2; --row)
			{
				if(a_revert)
					a_map[row][a_col1] = a_map[row][a_col1] == dirEmptyType ? CellType::EMPTY : CellType::WATER;
				else
					a_map[row][a_col1] = a_map[row][a_col1] == CellType::EMPTY ? dirEmptyType : dirWaterType;
			}
		}
		else
		{
			CellType dirEmptyType = CellType::DOWN_EMPTY;
			CellType dirWaterType =	CellType::DOWN_WATER;

			for (int row = a_row1; row < a_row2; ++row)
			{
				if (a_revert)
					a_map[row][a_col1] = a_map[row][a_col1] == dirEmptyType ? CellType::EMPTY : CellType::WATER;
				else
					a_map[row][a_col1] = a_map[row][a_col1] == CellType::EMPTY ? dirEmptyType : dirWaterType;
			}
		}
	}
	else
		throw runtime_error("Bad path definition");
}

bool findSolution(vector <vector <CellType>> & a_ioMap, vector <Ball> a_ioBalls, int a_width, int a_height)
{
	Ball * currBall = nullptr;

	for (auto & ball : a_ioBalls)
	{
		if (ball.type != CellType::BALL_IN_HOLE)
		{
			currBall = &ball;
			break;
		}
	}

	bool solutionIsFound = false;

	if (currBall != nullptr)
	{
		int len = 1 + ((int)currBall->type - (int)CellType::BALL_1);

		for (int i = 0; !solutionIsFound && i < 4; ++i)
		{
			int targetRow = currBall->row + DIR_DELTAS[i].first * len;
			int targetCol = currBall->col + DIR_DELTAS[i].second * len;

			if (targetRow < 0 || targetRow >= a_height || targetCol < 0 || targetCol >= a_width)
				continue;

			if (a_ioMap[targetRow][targetCol] != CellType::EMPTY && a_ioMap[targetRow][targetCol] != CellType::HOLE)
				continue;

			if (currBall->type == CellType::BALL_1 && a_ioMap[targetRow][targetCol] != CellType::HOLE)
				continue;

			if (! isPathFree(a_ioMap, currBall->row, currBall->col, targetRow, targetCol))
				continue;

			Ball prevBall = *currBall;
			CellType prevCell = a_ioMap[targetRow][targetCol];

			if (a_ioMap[targetRow][targetCol] == CellType::HOLE)
			{
				currBall->type = CellType::BALL_IN_HOLE;
				a_ioMap[targetRow][targetCol] = CellType::BALL_IN_HOLE;
			}
			else
			{
				currBall->type = (CellType)((int)currBall->type - 1);
				a_ioMap[targetRow][targetCol] = currBall->type;
			}

			int prevRow = currBall->row;
			int prevCol = currBall->col;

			a_ioMap[prevRow][prevCol] = CellType::EMPTY;

			tracePath(a_ioMap, prevRow, prevCol, targetRow, targetCol, false);

			currBall->row = targetRow;
			currBall->col = targetCol;

			solutionIsFound = findSolution(a_ioMap, a_ioBalls, a_width, a_height);

			if (!solutionIsFound)
			{
				*currBall = prevBall;
				tracePath(a_ioMap, prevRow, prevCol, targetRow, targetCol, true); 
				a_ioMap[targetRow][targetCol] = prevCell;
				a_ioMap[prevRow][prevCol] = prevBall.type;
			}
		}
	}
	else
		solutionIsFound = true;

	return solutionIsFound;
}

void solve()
{
	int width;
	int height;
	cin >> width >> height; cin.ignore();
	
	vector <vector <CellType>> map(height, vector <CellType>(width, CellType::EMPTY));
	vector <Ball> balls;

	for (int i = 0; i < height; i++) 
	{
		string row;
		cin >> row; cin.ignore();

		for (int j = 0; j < width; ++j)
		{	
			if (row[j] == '.')
				continue;

			if (row[j] == 'X')
				map[i][j] = CellType::WATER;
			else if (row[j] == 'H')
				map[i][j] = CellType::HOLE;
			else
			{
				CellType type = (CellType)((int)CellType::BALL_1 + (row[j] - '1'));
				map[i][j] = type;
				balls.push_back({ type, i, j });
			}
		}
	}

	bool solutionFound = findSolution(map, balls, width, height);

	if (solutionFound)
	{
		for (const auto & line : map)
		{
			for (auto type : line)
			{
				switch (type)
				{
				case CellType::UP_EMPTY:
				case CellType::UP_WATER:
					cout << "^";
					break;

				case CellType::RIGHT_EMPTY:
				case CellType::RIGHT_WATER:
					cout << ">";
					break;

				case CellType::DOWN_EMPTY:
				case CellType::DOWN_WATER:
					cout << "v";
					break;

				case CellType::LEFT_EMPTY:
				case CellType::LEFT_WATER:
					cout << "<";
					break;

				default :
					cout << ".";
				};
			}
			cout << endl;
		}
	}
	else
		cout << "Not found o.O" << endl;
}

int main(int argc, char ** argv)
{
#if _WIN32
	Validation::SolveAction action = bind(solve);
	Validation::Validator::validateSolution(action);
#else
	solve();
#endif
}