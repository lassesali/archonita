function gamelonIntro() {




//ALUSTETAAN TILAKONE
initStateMachine();
setGameState($gameIntroState);

   // play ambient music
    $introMusic = playArchonSound(ambient1Audio);
   startGame("Archon/data/levels/intro.t2d");

   schedule(2000,0, logofadeout);
   schedule(3000,0, mainmenuScreen);
}

function logofadeout() {
   logo1.startFadeOut(0.10);
   logo1tex.startFadeOut(0.10);

}

