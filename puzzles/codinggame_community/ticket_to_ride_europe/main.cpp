#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

#ifdef _WIN32
	#include <fstream>
#endif 

using namespace std;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
int main()
{
#ifdef _WIN32
	ifstream in("data/input1.txt");
	if (!in) throw runtime_error("Invalid input file");
	streambuf *cinbuf = std::cin.rdbuf();
	cin.rdbuf(in.rdbuf());
#endif 

    int trainCars;
    int numTickets;
    int numRoutes;
    cin >> trainCars >> numTickets >> numRoutes; cin.ignore();
    int red;
    int yellow;
    int green;
    int blue;
    int white;
    int black;
    int orange;
    int pink;
    int engine;
    cin >> red >> yellow >> green >> blue >> white >> black >> orange >> pink >> engine; cin.ignore();
    for (int i = 0; i < numTickets; i++) {
        int points;
        string cityA;
        string cityB;
        cin >> points >> cityA >> cityB; cin.ignore();
    }
    for (int i = 0; i < numRoutes; i++) {
        int length;
        int requiredEngines;
        string color;
        string cityA;
        string cityB;
        cin >> length >> requiredEngines >> color >> cityA >> cityB; cin.ignore();
    }

    // Write an action using cout. DON'T FORGET THE "<< endl"
    // To debug: cerr << "Debug messages..." << endl;

    cout << "points" << endl;
}