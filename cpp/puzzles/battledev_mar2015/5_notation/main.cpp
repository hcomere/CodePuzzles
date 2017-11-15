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

bool is_operator(const string & a_token)
{
	return a_token == "+" || a_token == "-" || a_token == "*" || a_token == "/";
}

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input4.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	PRINT("--------");

	vector <string> tokens;
	string token;
	string exp;

	getline(cin, exp);
	PRINT(exp);

	stringstream s(exp);

	while (getline(s, token, ' '))
	{
		//PRINT(token);
		tokens.push_back(token);
	}

	while (tokens.size() > 1)
	{
		for (int i = 0; i < tokens.size() - 2; ++i)
		{
			if (!is_operator(tokens[i]))
				continue;

			if (!is_operator(tokens[i + 1]) && !is_operator(tokens[i + 2]))
			{
				int val1 = stoi(tokens[i + 1]);
				int val2 = stoi(tokens[i + 2]);
				int res = 0;

				if (tokens[i][0] == '+')
					res = val1 + val2;
				else if (tokens[i][0] == '-')
					res = val1 - val2;
				else if (tokens[i][0] == '*')
					res = val1 * val2;
				else if (tokens[i][0] == '/')
					res = val1 / val2;

				tokens.erase(tokens.begin() + i, tokens.begin() + i + 3);
				tokens.insert(tokens.begin() + i, to_string(res));
				break;
			}
		}
	}

	cout << stoi(tokens[0]) << endl;

#ifdef _WIN32
	getchar();
#endif
}