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

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input3.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	int H, L;
	cin >> H >> L;

	vector <vector <bool>> map(H, vector <bool>(L, false));

	for (int i = 0; i < H; ++i)
	{
		string row;
		cin >> row;
		
		for (int j = 0; j < L; ++j)
		{
			if (row[j] == '#')
				map[i][j] = true;
		}
	}

	int res = -1;

	for(int size = min(H, L); res == -1 && size > 0; -- size)
	{
		int x = 0;
		int y = 0;

		bool found = false;

		for (int cx = x; ! found && cx + size - 1 < H; ++cx)
		{
			for (int cy = y; ! found && cy + size - 1 < L; ++cy)
			{
				bool ok = true;

				for (int dx = 0; ok && dx < size; ++dx)
				{
					for (int dy = 0; ok && dy < size; ++dy)
					{
						ok = map[cx + dx][cy + dy];
					}
				}

				found = ok;
			}
		}

		if (found)
			res = size;
	}

	if (res == -1)
		res = 0;

	cout << res << endl;

#ifdef _WIN32
	getchar();
#endif
}