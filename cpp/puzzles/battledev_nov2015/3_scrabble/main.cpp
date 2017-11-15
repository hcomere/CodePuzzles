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

	int values[26];
	
	for (int i = 0; i < 26; ++i)
		cin >> values[i];

	int max_val = 0;
	int min_length = 7;

	for (int i = 0; i < N; ++i)
	{
		string word;
		cin >> word;

		int val = 0;

		for (int j = 0; j < word.size(); ++j)
			val += values[word[j] - 'A'];

		if (val > max_val)
		{
			max_val = val;
			min_length = word.size();
		}
		else if (val == max_val && word.size() < min_length)
			min_length = word.size();
	}

	cout << max_val << " " << min_length << endl;

#ifdef _WIN32
	getchar();
#endif
}