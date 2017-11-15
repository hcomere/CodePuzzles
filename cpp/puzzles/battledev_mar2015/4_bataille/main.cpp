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
#include <tuple>

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

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input1.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	
	int M, N, P;
	cin >> M >> N >> P;

	vector <vector <int>> map(M, vector <int>(N, -2));

	for (int i = 0; i < M; ++i)
	{
		string row;
		cin >> row;
	
		stringstream s(row);

		for (int j = 0; j < N; ++j)
		{
			char c;
			s >> c;

			if (c == '#') map[i][j] = -1;
		}
	}

	vector <pair <int, int>> shoots(P);
	for (int i = 0; i < P; ++i)
	{
		cin >> shoots[i].first >> shoots[i].second;
	}
	
	// Determine boats

	struct Boat
	{
		vector <tuple <int, int, bool>> cases;
	};

	vector <Boat> boats;

	for (int i = 0; i < M; ++i)
	{
		for (int j = 0; j < N; ++j)
		{
			if (map[i][j] == -1)
			{
				Boat boat;

				boat.cases.push_back(tuple <int, int, bool> ( i, j, true ));
				
				bool h = false;

				int i2 = i;
				while (i2-1 >= 0 && map[i2-1][j] == -1)
				{
					boat.cases.push_back(tuple <int, int, bool>(i2-1, j, true));
					--i2;
					h = true;
				}

				i2 = i;
				while (i2+1 < M && map[i2+1][j] == -1)
				{
					boat.cases.push_back(tuple <int, int, bool>(i2 + 1, j, true ));
					++i2;
					h = true;
				}

				if (!h)
				{
					int j2 = j;
					while (j2-1 >= 0 && map[i][j2-1] == -1)
					{
						boat.cases.push_back(tuple <int, int, bool>(i, j2-1, true ));
						--j2;
					}

					j2 = j;
					while (j2+1 < N && map[i][j2+1] == -1)
					{
						boat.cases.push_back(tuple <int, int, bool>(i, j2+1, true ));
						++j2;
					}
				}

				boats.push_back(boat);
			}
		}
	}

	for (int i = 0; i < P; ++i)
	{
		bool done = false;

		for (int j = 0; !done && j < boats.size(); ++j)
		{
			for (int k = 0; !done && k < boats[j].cases.size(); ++k)
			{
				if (get <0>(boats[j].cases[k]) == shoots[i].first && get <1>(boats[j].cases[k]) == shoots[i].second)
				{
					get <2>(boats[j].cases[k]) = false;
					done = true;
				}
			}
		}
	}

	int down_boats = 0;
	int injuried_boats = 0;

	for (int j = 0; j < boats.size(); ++j)
	{
		int cases_down = 0;

		for (int k = 0; k < boats[j].cases.size(); ++k)
		{
			if (!get <2>(boats[j].cases[k]))
				++ cases_down;
		}

		if (cases_down)
		{
			if (cases_down == boats[j].cases.size())
				++down_boats;
			else
				++injuried_boats;
		}
	}

	cout << down_boats << " " << injuried_boats << endl;

#ifdef _WIN32
	getchar();
#endif
}