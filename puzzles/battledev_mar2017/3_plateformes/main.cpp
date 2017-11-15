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
	ifstream in("data/input2.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	PRINT("----");

	int N;
	cin >> N;

	string level;
	cin >> level;

	PRINT(level);

	int max_hole = 0;
	int cur_hole = -1;

	for (int i = 0; i < N; ++i)
	{
		if (level[i] == '_')
		{
			if (cur_hole == -1)
				cur_hole = 1;
			else
				++cur_hole;
		}
		else
		{
			if (cur_hole > 0)
			{
				if (cur_hole > max_hole)
					max_hole = cur_hole;

				cur_hole = -1;
			}
		}
	}

	if (cur_hole > 0 && cur_hole > max_hole)
		max_hole = cur_hole;

	if (max_hole)
		cout << max_hole + 1 << endl;
	else
		cout << 1 << endl;

#ifdef _WIN32
	getchar();
#endif
}