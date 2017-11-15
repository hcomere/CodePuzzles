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
	ifstream in("data/input1.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	int N;
	cin >> N;

	vector <string> map(N);

	for (int i = 0; i < N; ++i)
		cin >> map[i];

	int tot = 0;

	for (int i = 0; i < N; ++i)
	{
		for (int j = 0; j < N; ++j)
		{
			if (map[i][j] == 'X')
			{
				for (int dx = -1; dx <= 1; ++dx)
				{
					for (int dy = -1; dy <= 1; ++dy)
					{
						int x = j + dx;
						int y = i + dy;

						if (x >= 0 && x < N && y >= 0 && y < N)
						{
							if (map[y][x] == '.')
							{
								map[y][x] = 'C';
								++tot;
							}
						}
					}
				}
			}
		}
	}

	cout << tot << endl;

#ifdef _WIN32
	getchar();
#endif
}