#ifndef __VALIDATION_VALIDATOR_H_
#define __VALIDATION_VALIDATOR_H_

#include <functional>

namespace Validation
{
	typedef std::function <void ()> SolveAction;

	class Validator
	{
	private :
		Validator() = delete;

	public :
		static void validateSolution(SolveAction a_action);
	};
}
#endif