//---------------------------------------------------------------------------------------------
// Archonita - Main Menu Screen Logic
//---------------------------------------------------------------------------------------------

$mainMenuTransitioning = false;
$bubbleSchedule = "";
$heroSelectSchedule = "";

function mainMenuFadeOut() {
   if (isObject(blowjob))
      blowjob.startFadeOut(0.2);
   if (isObject(akka))
      akka.startFadeOut(0.2);
   if (isObject(kuva))
      kuva.startFadeOut(0.2);
   if (isObject(archonita))
      archonita.startFadeOut(0.2);
   if (isObject(mouseTrail))
      mouseTrail.stopEffect(true, false);
}

function mainmenuScreen() {
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

   $mainMenuTransitioning = false;

   sceneWindow2D.setUseWindowMouseEvents(true);

   if (isObject(testActionMap))
      testActionMap.delete();
   new ActionMap(testActionMap);
   testActionMap.bindCmd(keyboard, "escape", "", "creditsScreen();");
   testActionMap.bindCmd(keyboard, "d", "", "dumpBoardDebug();");
   testActionMap.push();

   setGameState($gameMainMenuState);

   $hiiri.piilota = false;
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   startGame("Archon/data/levels/mainmenu.t2d");  

   if ($bubbleSchedule !$= "")
      cancel($bubbleSchedule);
   $bubbleSchedule = schedule(1000, 0, stopBubbleEffect); 
}

function stopBubbleEffect() {
   $bubbleSchedule = "";
   if (isObject(saippuakuplat))
      saippuakuplat.stopEffect(true, true);
   if (isObject(blowjob))
      blowjob.setVisible(false);
}

function lopetasaippuakuplat() { stopBubbleEffect(); }

function gotoSelectHeroScreen() {
   if ($mainMenuTransitioning || getGameState() != $gameMainMenuState)
      return;

   $mainMenuTransitioning = true;

   if ($bubbleSchedule !$= "")
   {
      cancel($bubbleSchedule);
      $bubbleSchedule = "";
   }

   mainMenuFadeOut();

   if ($heroSelectSchedule !$= "")
      cancel($heroSelectSchedule);
   $heroSelectSchedule = schedule(500, 0, selectHeroScreen);
}
