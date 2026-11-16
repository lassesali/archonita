//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
// Portions Copyright (c) 2014 James S Urquhart
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
#include "platformEmscripten/platformEmscripten.h"
#include "platform/threads/thread.h"
#include "game/gameInterface.h"
#include "io/fileObject.h"
#include "game/version.h"


#ifndef DUMMY_PLATFORM
#include <emscripten/emscripten.h>
#endif

//------------------------------------------------------------------------------

S32 gLastStart = 0;
EmscriptenPlatState gPlatState;

bool appIsRunning = true;
static bool gHasShutdown = false;

int _EmscriptenRunTorqueMain()
{
  gPlatState.firstThreadId = ThreadManager::getCurrentThreadId();
   
   printf("performing mainInit()\n");
   
   gPlatState.lastTimeTick = Platform::getRealMilliseconds();
   
   if(!Game->mainInitialize(gPlatState.argc, gPlatState.argv))
   {
     return 0;
   }

   return 1;
}


static void _EmscriptenShutdownOnce()
{
   if (gHasShutdown)
      return;

   gHasShutdown = true;
   appIsRunning = false;
#ifndef DUMMY_PLATFORM
   // Cancel the browser rAF main loop before tearing down the engine so
   // MainLoop_runner cannot re-enter mainShutdown when -sEXIT_RUNTIME=0.
   emscripten_cancel_main_loop();
#endif
   Game->mainShutdown();
}

void _EmscriptenGameInnerLoop()
{
   if (gHasShutdown || !appIsRunning)
   {
      return;
   }
   else if (Game->isRunning())
   {
      S32 start = Platform::getRealMilliseconds();
     
      Game->mainLoop();
         
      gLastStart = start;
   }
   else
   {
      _EmscriptenShutdownOnce();
   }
}

void _EmscriptenGameResignActive()
{
   if (gHasShutdown)
      return;

   if ( Con::isFunction("onEmscriptenResignActive") )
      Con::executef( 1, "onEmscriptenResignActive" );
   
   appIsRunning = false;
}

void _EmscriptenGameBecomeActive()
{
   if (gHasShutdown)
      return;

   if ( Con::isFunction("onEmscriptenBecomeActive") )
      Con::executef( 1, "onEmscriptenBecomeActive" );
   
   appIsRunning = true;
}

void _EmscriptenGameWillTerminate()
{
   if (gHasShutdown)
      return;

   if ( Con::isFunction("onEmscriptenWillTerminate") )
      Con::executef( 1, "onEmscriptenWillTerminate" );

   _EmscriptenShutdownOnce();
}

static void _EmscriptenGetTxtFileArgs(int &argc, char** argv, int maxargc)
{
   argc = 0;

   // Avoid noisy pre-init IO error logs when the optional command-line file is absent.
   if (!Platform::isFile("EmscriptenCmdLine.txt") && !Platform::isFile("/.data/EmscriptenCmdLine.txt"))
      return;

   const U32 kMaxTextLen = 2048;

   U32 textLen = 0;

   // Keep storage static because argv entries point into this buffer after return.
   static char text[kMaxTextLen];

   File cmdfile;

   File::Status err = cmdfile.open("EmscriptenCmdLine.txt", cmdfile.Read);

   if (err == File::Ok)
   {
      // read in the first kMaxTextLen bytes, kick out if we get errors or no data
      err = cmdfile.read(kMaxTextLen - 1, text, &textLen);

      if ((err == File::Ok || err == File::EOS) && textLen > 0)
      {
         // Null terminate
         text[textLen++] = '\0';

         // Truncate to the 1st line of the file
         for (int i = 0; i < (int)textLen; i++)
         {
            if (text[i] == '\n' || text[i] == '\r')
            {
               text[i] = '\0';
               textLen = i + 1;
               break;
            }
         }

         // Tokenize the args with nulls, save them in argv, count them in argc
         char* tok;

         for (tok = dStrtok(text, " "); tok && argc < maxargc; tok = dStrtok(NULL, " "))
            argv[argc++] = tok;
      }
   }

   cmdfile.close();
}

int main(int argc, char **argv)
{      
   // get the actual command line args
   S32   newArgc = argc;
   
   const char* newArgv[10];
   
   for(int i=0; i < argc && i < 10; i++)
      newArgv[i] = argv[i];
   
   // get the text file args
   S32 textArgc;
   char* textArgv[10];
   
   _EmscriptenGetTxtFileArgs(textArgc, textArgv, 10);
   
   // merge them
   int i=0;
   
   while(i < textArgc && newArgc < 10)
      newArgv[newArgc++] = textArgv[i++];
   
   // store them in platState
   gPlatState.argc = newArgc;
   gPlatState.argv = newArgv;

   if (_EmscriptenRunTorqueMain() > 0)
   {
#ifndef DUMMY_PLATFORM
      // Pass 0 for fps so Emscripten drives the loop via requestAnimationFrame.
      emscripten_set_main_loop(_EmscriptenGameInnerLoop, 0, false);
#endif
      return 0;
   }
   else
   {
      return 1;
   }
}

