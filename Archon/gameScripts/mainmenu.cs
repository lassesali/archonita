
function mainmenufadeout() {
   blowjob.startFadeOut(0.2);
   akka.startFadeOut(0.2);
   kuva.startFadeOut(0.2);
   archonita.startFadeOut(0.2);
   mouseTrail.stopEffect(true, false);


}

function mainmenuScreen() {
    sceneWindow2D.setUseWindowMouseEvents(true);

    new ActionMap(testActionMap);
    testActionMap.bindCmd(keyboard, "escape","", "creditsscreen();");
    testActionMap.bindCmd(keyboard, "d","", "debuggi();");
    testActionMap.push();


    setGameState($gameMainMenuState);

    $hiiri.piilota = false;
    Canvas.setCursor(DefaultCursor);
    Canvas.showCursor();
    startGame("Archon/data/levels/mainmenu.t2d");  
    schedule(1000,0, lopetasaippuakuplat); 
   
}

function lopetasaippuakuplat() {
   saippuakuplat.stopEffect(true, true);
   blowjob.setVisible(false);
}

function gotoSelectHeroScreen() {
         mainmenufadeout();
        schedule(500,0,SelectHeroScreen);
}
