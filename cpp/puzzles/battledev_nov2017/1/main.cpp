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
#else
#include <Validator.h>
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

void solve()
{
	int N;
	cin >> N;

	int p[2] = { 0, 0 };
	for (int i = 0; i < N; ++i)
	{
		int val1, val2;
		cin >> val1 >> val2;

		if (val1 > val2)
			++p[0];
		else if (val2 > val1)
			++p[1];
	}

	cout << (p[0] > p[1] ? "A" : "B") << endl;
}

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#if _WIN32
	Validation::SolveAction action = bind(solve);
	Validation::Validator::validateSolution(action);
#else
	solve();
#endif
}