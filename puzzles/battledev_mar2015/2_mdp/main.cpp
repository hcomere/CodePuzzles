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

bool isletter(char c)
{
	return c >= 'a' && c <= 'z';
}

bool isnumber(char c)
{
	return c >= '0' && c <= '9';
}

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

	int good = 0;

	for (int i = 0; i < N; ++i)
	{
		string pwd;
		cin >> pwd;

		if (pwd.size() != 6)
			continue;

		transform(pwd.begin(), pwd.end(), pwd.begin(), ::tolower);

		bool ok = true;

		for (int c = 0; ok && c < 6; ++c)
		{
			if (c == 1)
				ok = isnumber(pwd[c]);
			else
				ok = isletter(pwd[c]);
		}

		if (ok)
			++good;
	}

	cout << good << endl;

#ifdef _WIN32
	getchar();
#endif
}