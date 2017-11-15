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

struct Node
{
	int	index;
	bool visited;

	vector <Node *> children;
	Node * selected;
};

vector <Node> nodes;

typedef vector <int> Loops;
vector <vector <Loops>> loopMap;

using namespace std;

void browse(Node * node, Node * source, Loops & loops, vector <Node *> & path)
{
	path.push_back(node);
	node->visited = true;

	for (auto & child : node->children)
	{
		if (!child->visited)
			browse(child, source, loops, path);
		else
		{
			if (child == source)
			{

			}
		}
	}

	node->visited = false;
	path.pop_back();
}

#ifndef _WIN32
void ContestExerciseImpl::main()
#else
int main(int argc, char ** argv)
#endif
{
#ifdef _WIN32
	ifstream in("data/input3.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

	int N;
	cin >> N;

	loopMap.resize(N, vector <Loops>(N));

	int M;
	cin >> M;

	typedef pair <int, int> Interet;
	vector <Interet> interets(M);

	for (int i = 0; i < M; ++i)
	{
		cin >> interets[i].first >> interets[i].second;
	}

	nodes.resize(N);

	for (int i = 0; i < N; ++i)
	{
		nodes[i].index = i;
		nodes[i].visited = false;
		nodes[i].selected = nullptr;
	}

	for (auto & interet : interets)
		nodes[interet.first].children.push_back(&nodes[interet.second]);

	for (int i = 0; i < N; ++i)
	{
		for (int j = 0; j < N; ++j)
		{
			if (i == j)
				continue;
			
			if (find(nodes[j].children.begin(), nodes[j].children.end(), &nodes[i]) == nodes[j].children.end())
				continue;

			vector <Node *> path;

			browse(&nodes[i], &nodes[i], loopMap[i][j], path);
		}
	}


#ifdef _WIN32
	getchar();
#endif
}