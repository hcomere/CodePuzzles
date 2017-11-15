/*******
* Read input from cin
* Use cout to output your result.
* Use:
*  localPrint( string mystring)
* to display variable in a dedicated area.
* ***/

#include <iomanip>
#include <iostream>
#include <limits>
#include <sstream>
#include <fstream>
#include <vector>
#include <algorithm>
#include <set>
#include <map>

#ifndef _WIN32
#include "exercise.hpp"
#endif 

#ifndef _WIN32
#define PRINT(s) localPrint(s)
#else
#define PRINT(s) cout << s << endl;
#endif

#ifndef _WIN32
ContestExerciseImpl::ContestExerciseImpl() : Exercise() {}
#endif

using namespace std;

struct Cell
{
	bool target;
	int playerValue;
	int ghostValue;
};

int N;
vector <vector <Cell>> m;

Cell * get(int i, int j, int di, int dj)
{
	int newi = i + di;
	int newj = j + dj;

	if (newi < 0) newi += N;
	newi = newi % N;
	if (newj < 0) newj += N;
	newj = newj % N;

	return &m[newi][newj];
}

int tag_cell(Cell * source_cell, Cell * cell, int turn, int & playerMoves)
{
	if (source_cell->ghostValue >= 0 && source_cell->ghostValue < turn && cell->ghostValue == -1)
	{
		cell->ghostValue = source_cell->ghostValue + 1;
		cell->playerValue = -2;
		
		if (cell->target)
			return -1;
	}

	if (source_cell->playerValue >= 0 && source_cell->playerValue < turn && cell->playerValue == -1)
	{
		cell->playerValue = source_cell->playerValue + 1;
		++playerMoves;
	
		if (cell->target)
			return cell->playerValue;
	}

	return 0;
}

int tag(int i, int j, int turn, int & playerMoves)
{	
	Cell * current = &m[i][j];
	Cell * left = get(i, j, 0, -1);
	Cell * right = get(i, j, 0, 1);
	Cell * top = get(i, j, -1, 0);
	Cell * down = get(i, j, 1, 0);
		
	int res = tag_cell(current, left, turn, playerMoves);
	
	if(res == 0) res = tag_cell(current, right, turn, playerMoves);
	if(res == 0) res = tag_cell(current, top, turn, playerMoves);
	if(res == 0) res = tag_cell(current, down, turn, playerMoves);

	return res;
}

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input6.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 


	cin >> N;

	m.resize(N, vector <Cell>(N));

	for (int i = 0; i < N; ++i)
	{
		string row;
		cin >> row;

		for (int j = 0; j < row.size(); ++j)
		{
			Cell cell;
			cell.target = false;
			cell.playerValue = -2;
			cell.ghostValue = -2;

			if (row[j] == '.')
			{
				cell.playerValue = -1;
				cell.ghostValue = -1;
			}
			else if (row[j] == 'C')
				cell.playerValue = 0;
			else if (row[j] == 'M')
				cell.ghostValue = 0;
			else if (row[j] == 'O')
			{
				cell.ghostValue = -1;
				cell.playerValue = -1;
				cell.target = true;
			}

			m[i][j] = cell;
		}
	}
	
	int	required_time = -1;
	int turn = 1;

	while (required_time == -1)
	{
		int	playerMoves = 0;

		for (int i = 0;	required_time == -1 && i < N; ++i)
		{
			for (int j = 0; required_time == -1 && j < N; ++j)
			{
				int res = tag(i, j, turn, playerMoves);
				if (res > 0)
					required_time = res;
				else if (res < 0)
					required_time = 0;
			}
		}

		if (playerMoves == 0)
			required_time = 0;

		++turn;
	}

	cout << required_time << endl;

#ifdef _WIN32
	getchar();
#endif
}