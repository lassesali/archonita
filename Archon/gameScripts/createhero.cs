function cardscreenfadeout() {
   card_0.startFadeOut(0.2);
   card_1.startFadeOut(0.2);
   teksti.startFadeOut(0.2);
   scrolleri.startFadeOut(0.2);
   mouseTrail.stopEffect(true, false);

}

function SelectHeroScreen() {
        setGameState($gameCardSelectState);
        $hiiri.piilota = false;
        startGame("Archon/data/levels/createhero.t2d");
}

function tarkastavalittukortti() {

   if (card_0.valittu) {
      $hiiri.piilota = true;
      $sankarivalittu=1;
      cardscreenfadeout();
      schedule(1000,0,  BoardScreen);
   } else if (card_1.valittu) {
      $hiiri.piilota = true;
      $sankarivalittu=2;
      cardscreenfadeout();
      schedule(1000,0, BoardScreen);
   } 
        
}

