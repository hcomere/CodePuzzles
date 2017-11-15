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

vector <string>	frag;
vector <bool> used;
int chainMax;
int N;

vector <string> rchain1;
vector <string> rchain2;
string rplain1, rplain2;

void browse(vector <string> chain1, string plain1, vector <string> chain2, string plain2)
{
	if (plain1.size() < chainMax || plain2.size() < chainMax)
	{
		for (int i = 0; i < N; ++i)
		{
			if (!used[i])
			{
				if (plain1.size() != chainMax)
				{
					if (plain1.size() + frag[i].size() <= chainMax)
					{
						used[i] = true;
						vector <string> newChain1 = chain1;
						newChain1.push_back(frag[i]);
						string newPlain1 = plain1;
						newPlain1 += frag[i];
						browse(newChain1, newPlain1, chain2, plain2);
						used[i] = false;
					}
				}
				else
				{
					int start = plain2.size();
					bool good = true;

					for (int j = 0; j < frag[i].size(); ++j)
					{
						char c = plain1[start + j];
						char comp;
						if (c == 'A') comp = 'T';
						else if (c == 'T') comp = 'A';
						else if (c == 'C') comp = 'G';
						else if (c == 'G') comp = 'C';

						if (frag[i][j] != comp)
							good = false;
					}

					if (good)
					{
						used[i] = true;
						vector <string> newChain2 = chain2;
						newChain2.push_back(frag[i]);
						string newPlain2 = plain2;
						newPlain2 += frag[i];
						browse(chain1, plain1, newChain2, newPlain2);
						used[i] = false;
					}
				}
			}
		}
	}
	else
	{
		rchain1 = chain1;
		rplain1 = plain1;
		rchain2 = chain2;
		rplain2 = plain2;
	}
}

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input7.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	PRINT("==");

	cin >> N;

	frag.resize(N);
	used.resize(N);
	chainMax = 0;

	for (int i = 0; i < N; ++i)
	{
		cin >> frag[i];
		chainMax += frag[i].size();
		PRINT(frag[i]);
	}

	chainMax /= 2;

	vector <string> chain1;
	vector <string> chain2;
	string plain1, plain2;

	browse(chain1, plain1, chain2, plain2);

	for (int i = 0; i < rchain1.size(); ++i)
	{
		if (i > 0) cout << " ";
		cout << rchain1[i];
	}

	cout << "#";

	for (int i = 0; i < rchain2.size(); ++i)
	{
		if (i > 0) cout << " ";
		cout << rchain2[i];
	}

	cout << endl;

#ifdef _WIN32
	getchar();
#endif
}