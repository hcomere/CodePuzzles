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

	PRINT("---------");

	for (int i = 0; i < N; ++i)
	{
		string str1, str2;
		cin >> str1 >> str2;

		PRINT(str1 + " " + str2);

		int max_length = 0;
		vector <string> factors;

		for (int len = 1; len <= str1.size(); ++len)
		{
			for (int pos = 0; pos <= str1.size() - len; ++ pos)
			{
				string factor = str1.substr(pos, len);
				if (str2.find(factor) != string::npos)
				{
					if (factor.size() > max_length)
					{
						max_length = factor.size();
						factors.clear();
					}

					factors.push_back(factor);
				}
			}
		}

		if (factors.empty())
			cout << "AUCUN FACTEUR" << endl;
		else
		{
			sort(factors.begin(), factors.end());
			cout << factors[0] << endl;
		}
	}

#ifdef _WIN32
	getchar();
#endif
}