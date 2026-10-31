//---------------------------------------------------------------------------------------------
// Archonita - Credits Screen Logic
//---------------------------------------------------------------------------------------------

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

function creditsScreen() {
   if (isObject(mouseTrail))
      mouseTrail.stopEffect(true, false);
   $hiiri.piilota = false;
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   if (isObject(testActionMap))
      testActionMap.pop();
   startGame("Archon/data/levels/credits.t2d"); 

   if ($creditsShutdownSchedule !$= "") cancel($creditsShutdownSchedule);
   if ($creditsFadeSchedule !$= "") cancel($creditsFadeSchedule);
   if ($creditsExplosionSchedule !$= "") cancel($creditsExplosionSchedule);

   $creditsShutdownSchedule = schedule(5000, 0, shutdown);
   $creditsFadeSchedule = schedule(4500, 0, creditsFadeOut);
   $creditsExplosionSchedule = schedule(1000, 0, stopExplosionEffect);
}

function stopExplosionEffect() {
   $creditsExplosionSchedule = "";
   if (isObject(explosion))
      explosion.stopEffect(true, true);
}

function lopetarajahdys() { stopExplosionEffect(); }
