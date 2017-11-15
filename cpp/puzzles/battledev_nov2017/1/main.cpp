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
	ifstream in("data/input4.txt");
	if (! in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

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
#ifdef _WIN32
	getchar();
#endif
}