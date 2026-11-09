//---------------------------------------------------------------------------------------------
// Archonita - Hero Selection Screen Logic
//---------------------------------------------------------------------------------------------

$cardSelectTransitioning = false;
$boardScreenSchedule = "";

function cardScreenFadeOut() {
   if (isObject($FactionSelectGui))
      $FactionSelectGui.show(false);
   if (isObject(card_0))
      card_0.startFadeOut(0.2);
   if (isObject(card_1))
      card_1.startFadeOut(0.2);
   if (isObject(teksti))
      teksti.startFadeOut(0.2);
   if (isObject(scrolleri))
      scrolleri.startFadeOut(0.2);
   if (isObject(mouseTrail))
      mouseTrail.stopEffect(true, false);
}

function selectHeroScreen() {
   $heroSelectSchedule = "";
   $cardSelectTransitioning = false;
   if (isObject($FactionSelectGui))
      $FactionSelectGui.show(false);

   setGameState($gameCardSelectState);
   startBackgroundMusic();
   $hiiri.piilota = false;
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();
   startGame("Archon/data/levels/createhero.t2d");
}

function checkSelectedCard() {
   if ($cardSelectTransitioning || getGameState() != $gameCardSelectState)
      return;

   if (isObject(card_0) && (card_0.selected || card_0.valittu)) {
      $cardSelectTransitioning = true;
      $hiiri.piilota = false;
      Canvas.setCursor(DefaultCursor);
      Canvas.showCursor();
      $selectedHero = 1;
      $sankarivalittu = 1;

      if (isObject($FactionSelectGui)) {
         $FactionSelectGui.show(true);
      } else {
         cardScreenFadeOut();
         if ($boardScreenSchedule !$= "")
            cancel($boardScreenSchedule);
         $boardScreenSchedule = schedule(1000, 0, showBoardScreen);
      }
   } else if (isObject(card_1) && (card_1.selected || card_1.valittu)) {
      $cardSelectTransitioning = true;
      $hiiri.piilota = false;
      Canvas.setCursor(DefaultCursor);
      Canvas.showCursor();
      $selectedHero = 2;
      $sankarivalittu = 2;

      if (isObject($FactionSelectGui)) {
         $FactionSelectGui.show(true);
      } else {
         cardScreenFadeOut();
         if ($boardScreenSchedule !$= "")
            cancel($boardScreenSchedule);
         $boardScreenSchedule = schedule(1000, 0, showBoardScreen);
      }
   } 
}

function tarkastavalittukortti() { checkSelectedCard(); }
