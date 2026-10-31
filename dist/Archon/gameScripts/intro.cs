//---------------------------------------------------------------------------------------------
// Archonita - Intro Screen Logic
//---------------------------------------------------------------------------------------------

$introFadeSchedule = "";
$introMenuSchedule = "";
$introMusic = 0;
$introSkipping = false;
$introSkipTime = 0;

function gamelonIntro() {
   $introSkipping = false;
   // ALUSTETAAN TILAKONE
   initStateMachine();
   setGameState($gameIntroState);

   // Cancel any existing intro schedules
   if ($introFadeSchedule !$= "")
   {
      cancel($introFadeSchedule);
      $introFadeSchedule = "";
   }
   if ($introMenuSchedule !$= "")
   {
      cancel($introMenuSchedule);
      $introMenuSchedule = "";
   }

   // Play ambient music
   if ($introMusic != 0)
   {
      alxStop($introMusic);
      $introMusic = 0;
   }
   $introMusic = playArchonSound(ambient1Audio);

   startGame("Archon/data/levels/intro.t2d");

   $introFadeSchedule = schedule(2000, 0, logofadeout);
   $introMenuSchedule = schedule(3000, 0, mainmenuScreen);
}

function logofadeout() {
   $introFadeSchedule = "";
   if (isObject(logo1))
      logo1.startFadeOut(0.10);
   if (isObject(logo1tex))
      logo1tex.startFadeOut(0.10);
}

function skipIntro() {
   if ($introSkipping || getGameState() != $gameIntroState)
      return;

   $introSkipping = true;
   $introSkipTime = getSimTime();

   if ($introFadeSchedule !$= "")
   {
      cancel($introFadeSchedule);
      $introFadeSchedule = "";
   }
   if ($introMenuSchedule !$= "")
   {
      cancel($introMenuSchedule);
      $introMenuSchedule = "";
   }
   if ($introMusic != 0)
   {
      alxStop($introMusic);
      $introMusic = 0;
   }

   mainmenuScreen();
}
