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

struct Station
{
	int x, y, z;

	bool operator <(const Station & s) const
	{
		return y < s.y;
	};
};


int N;
vector <Station> stations;
vector <vector <double>> dists;
vector <bool> used;
double minDist;

void browse(int totUsed, int prevStation, double dist, bool reverse, string path)
{
	//PRINT("======");
	//PRINT(string("totUsed: ") + to_string(totUsed) + ", prevStation: " + to_string(prevStation) + ", dist: " + to_string(dist) + ", reverse: " + to_string(reverse));

	bool newReverse = reverse;

	if (!reverse && prevStation == stations.size() - 1)
		newReverse = true;

	if (newReverse && prevStation == 0)
	{
		if (totUsed == N)
		{
			if (minDist == -1. || dist < minDist)
			{
				PRINT(string("Better path found : ") + to_string(dist) + " | " + path);
				minDist = dist;
			}
			else
				PRINT(string("Path found : ") + to_string(dist) + " | " + path);
		}

		//if (totUsed > N)
			//PRINT(string("Hum ..... ") + to_string(totUsed - N));
	}
	else
	{
		if (!newReverse)
		{
			for (int i = prevStation + 1; i < N; ++i)
			{
				if (!used[i])
				{
					string newPath = path;
					newPath += string(" ") + to_string(i);

					//PRINT(string("Use station ") + to_string(i));
					used[i] = true;
					browse(totUsed + 1, i, dist + dists[prevStation][i], newReverse, newPath);
					used[i] = false;
				}
			}
		}
		else
		{
			for (int i = prevStation - 1; i >= 0; --i)
			{
				if (!used[i])
				{
					string newPath = path;
					newPath += string(" ") + to_string(i);

					//PRINT(string("Use station ") + to_string(i));
					used[i] = true;
					browse(totUsed + 1, i, dist + dists[i][prevStation], newReverse, newPath);
					used[i] = false;
				}
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
	ifstream in("data/input5.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	cin >> N;

	stations.resize(N);
	used.resize(N);

	for (int i = 0; i < N; ++i)
		cin >> stations[i].x >> stations[i].y >> stations[i].z;

	sort(stations.begin(), stations.end());
	
	dists.resize(N, vector <double>(N, -1.));

	for (int i = 0; i < N; ++i)
		for (int j = 0; j < N; ++j)
			dists[i][j] = sqrt(pow(stations[i].x - stations[j].x, 2.) + pow(stations[i].y - stations[j].y, 2.) + pow(stations[i].z - stations[j].z, 2.));

	minDist = -1.;
	string path = "0";
	browse(0, 0, 0., false, path);

	cout << (int) floor(minDist) << endl;

#ifdef _WIN32
	getchar();
#endif
}