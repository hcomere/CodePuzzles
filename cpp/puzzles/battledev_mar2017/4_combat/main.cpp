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
#include <tuple>

#ifndef _WIN32
#include "exercise.hpp"
#endif 


using namespace std;

#ifndef _WIN32
#define PRINT(s) localPrint(s)
#else
#define PRINT(s) cout << s << endl;
#endif

#ifndef _WIN32
ContestExerciseImpl::ContestExerciseImpl() : Exercise() {}
#endif


int N;
int M;
vector <int> availableCats;
vector <int> ennemyOrder;
vector <bool> dispo;

void browse(vector <int> & order, int & bestRes, vector <int> & bestOrder)
{
	if (order.size() == N)
	{
		vector <int> ennemies = ennemyOrder;
		vector <int> cats = order;
		
		while (!ennemies.empty() && !cats.empty())
		{
			int e = ennemies.front();
			int c = cats.front();

			if (e == c)
			{
				cats.erase(cats.begin());
				ennemies.erase(ennemies.begin());
			}
			else if(e == (c+1)%3)
				ennemies.erase(ennemies.begin());
			else
				cats.erase(cats.begin());
		}
		
		int res = -2;
		
		if (ennemies.empty() && cats.empty()) res = 0;
		else if (ennemies.empty()) res = 1;
		else res = -1;

		if (res > bestRes)
		{
			bestRes = res;
			bestOrder = order;
		}
	}
	else
	{
		for (int i = 0; bestRes != 1 && i < N; ++i)
		{
			if (dispo[i])
			{
				order.push_back(availableCats[i]);
				dispo[i] = false;

				browse(order, bestRes, bestOrder);

				dispo[i] = true;
				order.pop_back();
			}
		}
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

	PRINT("------");

	string T;
	string E;

	cin >> N;
	availableCats.resize(N);
	dispo.resize(N, true);

	cin >> T;

	cin >> M;
	ennemyOrder.resize(M);
	
	cin >> E;

	PRINT(T);
	PRINT(E);


	for (int i = 0; i < T.size(); ++ i)
	{
		if (T[i] == 'P') availableCats[i] = 0;
		else if (T[i] == 'E') availableCats[i] = 1;
		else if (T[i] == 'F') availableCats[i] = 2;
	}


	for (int i = 0; i < M; ++i)
	{
		if (E[i] == 'P') ennemyOrder[i] = 0;
		else if (E[i] == 'E') ennemyOrder[i] = 1;
		else if (E[i] == 'F') ennemyOrder[i] = 2;
	}

	vector <int> order;
	vector <int> bestOrder;
	int bestRes = -2;
	browse(order, bestRes, bestOrder);

	string sorder;
	for (int i = 0; i < N; ++i)
	{
		if (bestOrder[i] == 0) sorder += "P";
		else if (bestOrder[i] == 1) sorder += "E";
		else sorder += "F";
	}

	if (bestRes == -1)
		cout << "-" << sorder << endl;
	else if (bestRes == 0)
		cout << "=" << sorder << endl;
	else
		cout << "+" << sorder << endl;

#ifdef _WIN32
	getchar();
#endif
}