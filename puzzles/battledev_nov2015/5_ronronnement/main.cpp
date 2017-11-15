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

	string serie;
	cin >> serie;

	int period = 1;
	bool found = false;

	for (int i = 0; i < serie.size() / 2; ++i)
	{
		string pattern = serie.substr(0, period);
		bool ok = true;

		for (int j = period; j < serie.size(); j += period)
		{
			string totest = serie.substr(j, period);
			if (totest != pattern)
			{
				ok = false;
				break;
			}
		}

		if (ok)
		{
			found = true;
			break;
		}
		else
			++period;
	}
		
	if (!found)
		cout << serie.size() << endl;
	else
		cout << period << endl;

#ifdef _WIN32
	getchar();
#endif
}