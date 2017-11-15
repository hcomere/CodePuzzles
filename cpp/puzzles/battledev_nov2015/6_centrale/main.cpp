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

	int N;
	cin >> N;

	struct Cable
	{
		int s, e;
	};

	vector <Cable> c(N);

	for (int i = 0; i < N; ++i)
		cin >> c[i].s >> c[i].e;

	sort(c.begin(), c.end(), [](const Cable & a, const Cable & b) -> bool
	{
		if (a.s < b.s) 
			return true;
		else if(a.s == b.s) 
			return a.e < b.e;
		else return false;
	});
	
	vector <int> chain(N, 1);

	for (int i = 1; i < N; ++i)
	{
		for (int j = 0; j < i; ++j)
		{
			if (c[i].e >= c[j].e)
				chain[i] = max(chain[i], chain[j] + 1);
		}
	}

	sort(chain.begin(), chain.end());

	cout << chain.back() << endl;

#ifdef _WIN32
	getchar();
#endif
}