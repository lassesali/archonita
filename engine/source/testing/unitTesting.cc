//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

// We don't want tests in a shipping version.
#ifndef TORQUE_SHIPPING

#ifndef _CONSOLEINTERNAL_H_
#include "console/consoleInternal.h"
#endif

#ifndef _UNIT_TESTING_H_
#include "testing/unitTesting.h"
#endif

// Include GoogleTest source files - updated for latest GoogleTest version
#include "src/gtest-assertion-result.cc"
#include "src/gtest-death-test.cc"
#include "src/gtest-filepath.cc"
#include "src/gtest-matchers.cc"
#include "src/gtest-port.cc"
#include "src/gtest-printers.cc"
#include "src/gtest-test-part.cc"
#include "src/gtest-typed-test.cc"
#include "src/gtest.cc"
//-----------------------------------------------------------------------------

class TorqueUnitTestListener : public ::testing::EmptyTestEventListener
{
	// Called before a test starts.
	void OnTestStart( const ::testing::TestInfo& testInfo ) override
	{
		Con::printf("> Starting Test '%s.%s'",
				testInfo.test_suite_name(), testInfo.name()); 
	}

	// Called after a failed assertion or a SUCCEED() invocation.
	void OnTestPartResult( const ::testing::TestPartResult& testPartResult ) override
	{
		if ( testPartResult.failed() )
		{
			Con::warnf(">> Failed with '%s' in '%s' at (line:%d)",
					testPartResult.summary(),
					testPartResult.file_name(),
					testPartResult.line_number()
					);
		}
		else if (testPartResult.passed())
		{
			Con::printf(">> Test successful!");
		}
		else
		{
			Con::printf(">> Test part result of unknown type in '%s' at (line:%d)",
					testPartResult.file_name(),
					testPartResult.line_number()
			);
		}
	}

	// Called after a test ends.
	void OnTestEnd( const ::testing::TestInfo& testInfo ) override
	{
		Con::printBlankLine();
	}
};

#include "unitTesting_ScriptBinding.h"

#endif // TORQUE_SHIPPING
