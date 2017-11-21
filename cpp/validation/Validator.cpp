#include <Validator.h>

#include <fstream>
#include <iostream>
#include <string>
#include <sstream>
#include <vector>

using namespace Validation;
using namespace std;

void Validator::validateSolution(SolveAction a_action)
{
	string path = "data";

	vector <int> foundTests;
	
	for (int i = 0; i < 30; ++i)
	{
		ifstream in(string("data\\input") + to_string(i) + ".txt");
		ifstream out(string("data\\output") + to_string(i) + ".txt");

		bool inputFound = false;
		
		if (in)
		{
			inputFound = true;
			in.close();
		}

		bool outputFound = false;
		
		if (out)
		{
			outputFound = true;
			out.close();
		}

		if (inputFound && outputFound)
			foundTests.push_back(i);
	}

	
	if (foundTests.empty())
		cout << "Not Test Case found" << endl;
	else
	{
		for(int test : foundTests)
		{
			cout << endl;
			cout << "......................................................" << endl;
			cout << ".. TEST CASE " << test << endl;
			cout << "......................................................" << endl;
			cout << endl;

			ifstream in(string("data\\input") + to_string(test) + ".txt");
			ifstream out(string("data\\output") + to_string(test) + ".txt");
			streambuf * cinbuf = cin.rdbuf();
			cin.rdbuf(in.rdbuf());
			
			stringstream resStream;
			streambuf * coutbuf = cout.rdbuf(resStream.rdbuf());
			
			a_action();
			
			string outContent;
			outContent.assign(istreambuf_iterator<char>(out), istreambuf_iterator<char>());
			string resContent;
			resContent.assign(istreambuf_iterator<char>(resStream), istreambuf_iterator<char>());
			
			cin.rdbuf(cinbuf);
			cout.rdbuf(coutbuf);
			
			if (outContent.compare(resContent) == 0)
			{
				cout << "GOOD !" << endl;
			}
			else
			{
				cout << "===== Not good :( =====" << endl;
				cout << "--- Found ---" << endl;
				cout << resContent << endl;
				cout << "--- Expected ---" << endl;
				cout << outContent << endl;
				break;
			}
		}
	}
	
	char c;
	cin >> c;
}