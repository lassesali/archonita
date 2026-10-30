function creditsfadeout() {
   credits1.startFadeOut(0.10);
   credits2.startFadeOut(0.10);

}


function creditsScreen() {
      // play ambient audio
   mouseTrail.stopEffect(true, false);
   $hiiri.piilota = true;
   testActionMap.pop();
   startGame("Archon/data/levels/credits.t2d"); 
   schedule(5000,0,shutdown);
   schedule(4500,0,creditsfadeout);
   schedule(1000,0,lopetarajahdys);
}


function lopetarajahdys() {
   explosion.stopEffect(true, true);
   
}

