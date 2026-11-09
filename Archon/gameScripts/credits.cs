//---------------------------------------------------------------------------------------------
// Archonita - Credits Screen Logic
//---------------------------------------------------------------------------------------------

$creditsReturnSchedule = "";
$creditsShutdownSchedule = "";
$creditsFadeSchedule = "";
$creditsExplosionSchedule = "";

function creditsFadeOut() {
   $creditsFadeSchedule = "";
   if (isObject(credits1))
      credits1.startFadeOut(0.10);
   if (isObject(credits2))
      credits2.startFadeOut(0.10);
}

function stopExplosionEffect() {
   $creditsExplosionSchedule = "";
   if (isObject(explosion))
      explosion.stopEffect(true, true);
}

function lopetarajahdys() { stopExplosionEffect(); }

function returnFromCredits() {
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

   mainmenuScreen();
}

function creditsScreen() {
   if (isObject(mouseTrail))
      mouseTrail.stopEffect(true, false);
   $hiiri.piilota = false;
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   if (isObject(sceneWindow2D))
      sceneWindow2D.setUseWindowMouseEvents(true);

   setGameState($gameCreditsState);

   if (isObject(testActionMap))
      testActionMap.pop();

   if (isObject(creditsActionMap))
      creditsActionMap.delete();
   new ActionMap(creditsActionMap);
   creditsActionMap.bindCmd(keyboard, "escape", "", "returnFromCredits();");
   creditsActionMap.bindCmd(keyboard, "space", "", "returnFromCredits();");
   creditsActionMap.bindCmd(keyboard, "enter", "", "returnFromCredits();");
   creditsActionMap.push();

   startGame("Archon/data/levels/credits.t2d"); 

   if ($creditsReturnSchedule !$= "") cancel($creditsReturnSchedule);
   if ($creditsShutdownSchedule !$= "") cancel($creditsShutdownSchedule);
   if ($creditsFadeSchedule !$= "") cancel($creditsFadeSchedule);
   if ($creditsExplosionSchedule !$= "") cancel($creditsExplosionSchedule);

   $creditsReturnSchedule = schedule(5000, 0, returnFromCredits);
   $creditsFadeSchedule = schedule(4500, 0, creditsFadeOut);
   $creditsExplosionSchedule = schedule(1000, 0, stopExplosionEffect);
}
