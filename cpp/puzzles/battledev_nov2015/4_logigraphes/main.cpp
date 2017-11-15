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

	int N, M;
	cin >> N >> M;

	vector <vector <bool>> drawing(N, vector <bool>(M, false));

	for (int i = 0; i < N; ++i)
	{
		string row;
		cin >> row;

		stringstream s(row);
		
		for (int j = 0; j < M; ++j)
		{
			char c;
			s >> c;
			drawing[i][j] = (c == 'x');
		}
	}

	// rows

	for (int i = 0; i < N; ++i)
	{
		bool first = true;
		int count = 0;

		for (int j = 0; j < M; ++j)
		{
			if (drawing[i][j])
				++count;
			else
			{
				if (count) // end of block
				{
					if (first)
					{
						cout << count;
						first = false;
					}
					else
						cout << "-" << count;

					count = 0;
				}
			}
		}

		if (first)
			if (count) cout << count << " "; else cout << ". ";
		else
			if (count) cout << "-" << count << " "; else cout << " ";
	}

	// cols

	for (int i = 0; i < M; ++i)
	{
		bool first = true;
		int count = 0;

		for (int j = 0; j < N; ++j)
		{
			if (drawing[j][i])
				++count;
			else
			{
				if (count) // end of block
				{
					if (first)
					{
						cout << count;
						first = false;
					}
					else
						cout << "-" << count;

					count = 0;
				}
			}
		}

		if (first)
			if (count) cout << count << " "; else cout << ". ";
		else
			if (count) cout << "-" << count << " "; else cout << " ";
	}

#ifdef _WIN32
	getchar();
#endif
}