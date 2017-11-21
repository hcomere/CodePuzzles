#if _WIN32
#include <Validator.h>
#endif 

#include <iostream>
#include <string>

using namespace std;

void solve()
{
	int width;
	int height;
	cin >> width >> height; cin.ignore();
	
	for (int i = 0; i < height; i++) 
	{
		string row;
		cin >> row; cin.ignore();
	}
}

int main(int argc, char ** argv)
{
#if _WIN32
	Validation::SolveAction action = bind(solve);
	Validation::Validator::validateSolution(action);
#else
	solve();
#endif
}