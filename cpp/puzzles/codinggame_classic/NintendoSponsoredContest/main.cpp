#if _WIN32
#include <Validator.h>
#endif 

#include <iostream>
#include <string>
#include <vector>
#include <tuple>
#include <algorithm>
#include <iterator>
#include <sstream>
#include <iomanip>
#include <bitset>
#include <set>

using namespace std;

enum class ByteType
{
	UNKNOWN,
	ONE,
	ZERO
};

vector<string> encode(int a_size, const vector <string> & a_in)
{
	vector<unsigned int> a(a_size / 16); // <- input tab to encrypt
	vector<unsigned int> b(a_size / 16, 0); // <- output tab

	for (int i = 0; i < a_size / 16; i++) {   // Read size / 16 integers to a
		std::stringstream ss;
		ss << std::hex << a_in[i];
		ss >> a[i];
	}

	//for (int i = 0; i < a_size/16; ++i)
	//	cerr << bitset<32>(a[i]) << " " << a[i] << " " << bitset<32>(b[i]) << " " << b[i] << endl;

	vector<vector<int>> test(a_size / 16, vector<int>(a_size, 0));

	for (int i = 0; i < a_size; i++)
	{
		for (int j = 0; j < a_size; j++)
		{ 
			//cerr << "i " << i << " j " << j << endl;
			uint32_t ijdiv = (i + j) / 32;
			
			uint32_t ijmod = (i + j) % 32;
			uint32_t idiv = i / 32;
			uint32_t imod = i % 32;
			uint32_t jdiv = j / 32;
			uint32_t jmod = j % 32;
			uint32_t sdiv = a_size / 32;

			++test[ijdiv][ijmod];

			b[ijdiv] ^=
				(
				(a[idiv] >> imod) &
					(a[jdiv + sdiv] >> jmod) & 1
					) << ijmod;   // Magic centaurian operation
		
			//for (int i = 0; i < a_size / 16; ++i)
			//	cerr << bitset<32>(a[i]) << " " << a[i] << " " << bitset<32>(b[i]) << " " << b[i] << endl;
		}
	}
	vector <string> res(a_size / 16);

	for (int i = 0; i < a_size / 16; i++)
	{
		stringstream ss;
		ss << setfill('0') << setw(8) << hex << b[i];       // print result
		res[i] = ss.str();
	}

	return res;
}

struct Solution
{
	vector<vector<ByteType>> uncryptedBytes;
	vector<uint32_t> crypted;
};

vector <Solution> foundSolutions;

void browse(int a_i, int a_j, int a_size, vector<uint32_t> & a_ioCrypted, vector<vector<ByteType>> a_ioBytes)
{
	uint32_t ijdiv = (a_i + a_j) / 32;
	uint32_t ijmod = (a_i + a_j) % 32;
	uint32_t idiv = a_i / 32;
	uint32_t imod = a_i % 32;
	uint32_t jdiv = a_j / 32;
	uint32_t jmod = a_j % 32;
	uint32_t sdiv = a_size / 32;

	vector<uint32_t> maskPossibilities;

	ByteType old1 = a_ioBytes[idiv][imod];
	ByteType old2 = a_ioBytes[jdiv + sdiv][jmod];

	if (old1 == ByteType::UNKNOWN || old2 == ByteType::UNKNOWN)
	{
		maskPossibilities.push_back(0 << ijmod);
		maskPossibilities.push_back(1 << ijmod);
	}
	else if(old1 == ByteType::ONE && old2 == ByteType::ONE)
		maskPossibilities.push_back(1 << ijmod);
	else
		maskPossibilities.push_back(0 << ijmod);

	for (uint32_t mask : maskPossibilities)
	{
		uint32_t oldCryptedIJDiv = a_ioCrypted[ijdiv];
		uint32_t cryptedIJDiv = a_ioCrypted[ijdiv] ^ mask;

		if (mask > 0)
		{
			// This means first byte of	a[idiv] << imod is 1
			// and first byte of a[jdiv + sdiv] << jmod is 1

			if (old1 == ByteType::ZERO || old2 == ByteType::ZERO) // bad solution
				continue;

			if (old1 == ByteType::UNKNOWN) a_ioBytes[idiv][imod] = ByteType::ONE;
			if (old2 == ByteType::UNKNOWN) a_ioBytes[jdiv + sdiv][jmod] = ByteType::ONE;

			a_ioCrypted[ijdiv] = cryptedIJDiv;

			if (a_j > 0)
				browse(a_i, a_j - 1, a_size, a_ioCrypted, a_ioBytes);
			else
			{
				if (a_i > 0)
					browse(a_i - 1, a_size - 1, a_size, a_ioCrypted, a_ioBytes);
				else
				{
					bool valid = true;

					for (auto val : a_ioCrypted)
						if (val != 0)
							valid = false;

					if(valid)
						foundSolutions.push_back({ a_ioBytes, a_ioCrypted });
				}
			}

			a_ioCrypted[ijdiv] = oldCryptedIJDiv;
			a_ioBytes[idiv][imod] = old1;
			a_ioBytes[jdiv + sdiv][jmod] = old2;
		}
		else
		{
			// Here the mask is 0, this result could be reached by 0&0 0&1 1&0
			vector<pair<ByteType, ByteType>> bytePossibilities = {
				pair<ByteType, ByteType>(ByteType::ZERO, ByteType::ZERO),
				pair<ByteType, ByteType>(ByteType::ONE, ByteType::ZERO),
				pair<ByteType, ByteType>(ByteType::ZERO, ByteType::ONE)
			};

			for (const auto & possibility : bytePossibilities)
			{
				if (old1 != possibility.first && old1 != ByteType::UNKNOWN) // bad solution
					continue;

				if (old2 != possibility.second && old2 != ByteType::UNKNOWN) // bad solution
					continue;
				
				if (old1 == ByteType::UNKNOWN) a_ioBytes[idiv][imod] = possibility.first;
				if (old2 == ByteType::UNKNOWN) a_ioBytes[jdiv + sdiv][jmod] = possibility.second;

				a_ioCrypted[ijdiv] = cryptedIJDiv;

				if (a_j > 0)
					browse(a_i, a_j - 1, a_size, a_ioCrypted, a_ioBytes);
				else
				{
					if (a_i > 0)
						browse(a_i - 1, a_size - 1, a_size, a_ioCrypted, a_ioBytes);
					else
						foundSolutions.push_back({ a_ioBytes });
				}

				a_ioCrypted[ijdiv] = oldCryptedIJDiv;
				a_ioBytes[idiv][imod] = old1;
				a_ioBytes[jdiv + sdiv][jmod] = old2;
			}
		}
	}

	/*b[ijdiv] ^=
	(
	(a[idiv] >> imod) &
	(a[jdiv + sdiv] >> jmod) & 1
	) << ijmod;   // Magic centaurian operation*/
}

void decode(int a_size, const vector <string> & a_in)
{
	vector<unsigned int> in(a_size / 16);
	vector<vector<ByteType>> bytes(a_size / 16, vector<ByteType>(32, ByteType::UNKNOWN));
	
	for (int i = 0; i < a_size / 16; i++)
	{
		std::stringstream ss;
		ss << std::hex << a_in[i];
		ss >> in[i];
	}

	browse(a_size - 1, a_size - 1, a_size, in, bytes);

	cerr << "Solutions : " << endl;

	/*for (int i = a_size-1; i >= 0; --i)
	{
		for (int j = a_size-1; j >= 0; --j)
		{
			uint32_t ijdiv = (i + j) / 32;

			uint32_t ijmod = (i + j) % 32;
			uint32_t idiv = i / 32;
			uint32_t imod = i % 32;
			uint32_t jdiv = j / 32;
			uint32_t jmod = j % 32;
			uint32_t sdiv = a_size / 32;


		}
	}*/
}

void solve()
{
	foundSolutions.clear();

	vector<string> estr = encode(32, vector<string>{"b0c152f9", "ebf2831f"});

	int inputSize;
	cin >> inputSize;

	vector<string> hexVals;
	copy(istream_iterator<string>(cin), istream_iterator<string>(), back_inserter(hexVals));

	decode(inputSize, hexVals);
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