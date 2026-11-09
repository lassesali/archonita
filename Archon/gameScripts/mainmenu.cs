//---------------------------------------------------------------------------------------------
// Archonita - Main Menu Screen Logic
//---------------------------------------------------------------------------------------------

$mainMenuTransitioning = false;
$bubbleSchedule = "";
$heroSelectSchedule = "";

function onEscapeKeyPressed() {
   if (isObject($FactionSelectGui) && $FactionSelectGui.isVisible())
   {
      $FactionSelectGui.onBackClicked();
      return;
   }
   if (isObject($OptionsMenuGui) && $OptionsMenuGui.isVisible())
   {
      $OptionsMenuGui.show(false);
      return;
   }
   if (isObject($PauseMenuManager) &&
       ($PauseMenuManager.isBoardState(getGameState()) || $PauseMenuManager.isPaused))
   {
      $PauseMenuManager.togglePauseMenu();
      return;
   }
   if (isObject($TitleMenuManager))
   {
      $TitleMenuManager.onQuitClicked();
      return;
   }
   shutdown();
}

function mainMenuFadeOut() {
   if (isObject($TitleMenuManager))
      $TitleMenuManager.show(false);
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.show(false);
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
   showVersionWatermark(false);
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
   if (isObject(creditsActionMap))
   {
      creditsActionMap.pop();
      creditsActionMap.delete();
   }
   if ($creditsReturnSchedule !$= "")
   {
      cancel($creditsReturnSchedule);
      $creditsReturnSchedule = "";
   }
   if ($creditsShutdownSchedule !$= "")
   {
      cancel($creditsShutdownSchedule);
      $creditsShutdownSchedule = "";
   }
   if ($creditsFadeSchedule !$= "")
   {
      cancel($creditsFadeSchedule);
      $creditsFadeSchedule = "";
   }
   if ($creditsExplosionSchedule !$= "")
   {
      cancel($creditsExplosionSchedule);
      $creditsExplosionSchedule = "";
   }
   // Ensure background music continues seamlessly across scene transitions
   startBackgroundMusic();

   $mainMenuTransitioning = false;

   sceneWindow2D.setUseWindowMouseEvents(true);

   if (isObject(testActionMap))
      testActionMap.delete();
   new ActionMap(testActionMap);
   testActionMap.bindCmd(keyboard, "escape", "", "onEscapeKeyPressed();");
   testActionMap.bindCmd(keyboard, "m", "toggleMusicMute();", "");
   testActionMap.bindCmd(keyboard, "n", "toggleSFXMute();", "");
   testActionMap.bindCmd(keyboard, "d", "", "dumpBoardDebug();");
   testActionMap.push();

   setGameState($gameMainMenuState);

   $hiiri.piilota = false;
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   startGame("Archon/data/levels/mainmenu.t2d");  
   if (isObject($ResolutionManager))
      $ResolutionManager.adaptSceneBackground();
   if (isObject($BoardAudioHud))
      $BoardAudioHud.show(false);
   if (isObject($TitleMenuManager))
      $TitleMenuManager.show(true);
   showVersionWatermark(true);  

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
