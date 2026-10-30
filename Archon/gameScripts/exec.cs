







function shutdown() {
   shutdownProject();
}







function BoardScreen() {
    setGameState($gamePreBattleState);
    dismountobject();
    startGame("Archon/data/levels/board.t2d");  
    
    
    
    InitNewGame();
    
    
    
}




function InitNewGame() {

  game.setVuoro(0);

  $Sprite = new Sprite() { scene = t2dScene; };
  $Kortti = new Sprite() { scene = t2dScene; };

//LUODAAN IHMISPELAAJAN SANKARI


// pelaaja on valinnut soturikortin
if ($sankarivalittu == 1) {
   
   HeroGroup.getObject(0).addOlentopisteet(1);
}

// pelaaja on valinnut velhokortin
if ($sankarivalittu == 2) {

   HeroGroup.getObject(0).addMana(1);
   HeroGroup.getObject(0).setAmmatti(1);
}

//LUODAAN ANIMAATIOTILAKONE
initAnimStates();


//LUODAAN AI-SANKARI
// AI valitsee aina soturin
HeroGroup.getObject(1).addOlentopisteet(1);

//VAIHDETAAN SANKARI-IKONI
heroicon.setFrame(HeroGroup.getObject(0).getAmmatti());

//PÄIVITETÄÄN OLENTOPISTEET
updateOlentopisteet();

//PÄIVITETÄÄN OLENTOIKONIT
updateOlentoikonit();


//PÄIVITETÄÄN TOIMINTAIKONIT
updateActionIcons();

//ALUSTETAAN LAUTA
initBoard();

//PÄIVITETÄÄN VIHREÄT RUUDUT
updateGreenGrids();

//LUODAAN ASETTELUNAPPULAT
createSetupNappulat();



}

function InitGameForBattle() {
   
   //LUODAAN AI-NAPPULAT
   createAINappulat();
   
   //JAA NAPPULOIDEN VUOROT
   jaaNappuloidenVuorot();
   
   setGameState($gameBattleState);
   deleteSetupNappulat();
   seuraavaVuoro();
   katsoMihinRuutuunVoiLiikkua();
}

function jaaKortit() {

        $kortitnum++;
        
        if ($kortitnum<6) {

            schedule(450,0,  jaaKortit);
        
        
        }
        
        %i = $kortitnum;
        
        $specialcard[%i] = new Sprite() { scene = t2dScene; class = Kortti; };
        $specialcard[%i].setImageMap("cardsImageMap");
        %posx = -20-getRandom(50);
        %posy = 80;

        
        $specialcard[%i].setPosition( %posx SPC %posy );
        $specialcard[%i].setSize("24 24");

        //$specialcard[%i].setFrame(%i+2);
        $specialcard[%i].setFrame(0);
        $specialcard[%i].setVisible(true);
        $specialcard[%i].setLayer(10-%i);
        $specialcard[%i].setUseMouseEvents(0);
        $specialcard[%i].isSelectable = false;
        $specialcard[%i].moveto("-40" SPC (0+(%i*0.5)), 50, true, true, true);
         
            


}

function selectedNappula(%nappulaid) {

   $nappulaselectedgrid = new Sprite() { scene = t2dScene; };
   $nappulaselectedgrid.setImageMap("boardgridgreenImageMap");
   $nappulaselectedgrid.setPosition(boardToWorld($nappulaSprite[%nappulaid].BoardX, $nappulaSprite[%nappulaid].BoardY));
   $nappulaselectedgrid.setSize("8 8");
   $nappulaselectedgrid.setFrame("1");
   $nappulaselectedgrid.setVisible("True");
   $nappulaselectedgrid.setLayer("4");
   $nappulaselectedgrid.setBlendAlpha("0.9");
   $nappulaselectedgrid.setUseMouseEvents(0);
   $nappulaselectedgrid.isSelectable = false;  
   $nappulaselectedgrid.isValintakehys = true;  
   $nappulaselectedgrid.SrcBlendFactor = "ONE_MINUS_DST_COLOR";
   $nappulaselectedgrid.positionOnBoard = $nappulaSprite[%nappulaid].positionOnBoard;

}


function seuraavaVuoro() {

   game.addVuoro(1);

   deleteGrids(1);
   
   HeroGroup.getObject(0).setValittuRuutu("-1 -1");
   HeroGroup.getObject(1).setValittuRuutu("-1 -1");

   if ( getWordCount(game.getVuorotVector()) == 0 ) 
   {
      jaaNappuloidenVuorot();
      
   }

   $nappulaSprite[game.getNappulaSelected()].hasMoved = 0;
   $nappulaSprite[game.getNappulaSelected()].hasDoneAction = 0;
   
   if ( onkoNappulatElossa() == 1 ) {
         
      
      if (game.getState() != $gameEndState) {
            

         %nappulaid = getword(game.getVuorotVector(),0);
         
         echo("!!! nappulaid:" SPC %nappulaid SPC "vuorotvector:" SPC game.getVuorotVector());
         
         game.setVuorotVector(   removeWord(game.getVuorotVector(),0)   );
         
         if ( $nappulaSprite[%nappulaid].alive == 0 ) {
            endgame();
         }
         
         game.setNappulaSelected(%nappulaid);
         game.setVuorossaOlevaPelaaja( $nappulaSprite[%nappulaid].groupid);
         game.setVuorossaOlevanPelaajanVastustaja(1 - game.getVuorossaOlevaPelaaja()) ;
          
         %nappulapos = $nappulaSprite[%nappulaid].getPosition();
         %nappulaposX = $nappulaSprite[%nappulaid].getPositionX();
         %nappulaposY = $nappulaSprite[%nappulaid].getPositionY();
               
         selectedNappula(%nappulaid);

         if (game.getVuorossaOlevaPelaaja() == 0) {
                sceneWindow2D.setUseWindowMouseEvents(true);
         } else {
         
                sceneWindow2D.setUseWindowMouseEvents(false);
         }
         
         
      }
   }
   else
   {
      game.setState($gameEndState);
      endbattle();
   }
}

function deleteGrids(%i) {
   if (%i == 1) { 
      if ( isObject($nappulaselectedgrid) ) {
         $nappulaselectedgrid.safeDelete();
      }
   }
   for (%i=0; %i<16; %i++)
   {
            if ( isObject($movementgrid[%i]) ) {
               $movementgrid[%i].safeDelete();
            }
   }
}



function katsoMihinRuutuunVoiLiikkua()
{
   
   deleteGrids(0);
 
   
   %nappula = game.getNappulaSelected();
   
   %sankari = HeroGroup.getObject(game.getVuorossaOlevaPelaaja());

   
   if (%nappula > 0) {
 
      %foundai= ""; 
      %pos = $nappulaSprite[%nappula].positionOnBoard;
      %posx = getword(%pos,0);
      %posy = getword(%pos,1);
      %north = 0;
      %south = 0;
      %west = 0;
      %east = 0;
      if ($nappulaSprite[%nappula].hasMoved == 0) 
      {
      
         //Voiko liikkua pohjoiseen?
         if (%north == 0 && %posy > 0) {
            if ($board.obstacles[%posx,%posy-1] == 0  ) 
            {
               luoLiikuGrid(%posx,%posy-1,0);
                  %north = 1;
            }
         }

         //Voiko liikkua etelään?
         if (%south == 0 && %posy < 3) {
            if ($board.obstacles[%posx,%posy+1] == 0  )  
            {
               luoLiikuGrid(%posx,%posy+1,1);
                  %south = 1;
            }
         }

         //Voiko liikkua länteen?
         if (%west == 0 && %posx > 0) {
            if ($board.obstacles[%posx-1,%posy] == 0  )  
            {
               luoLiikuGrid(%posx-1,%posy,2);
                  %west = 1;
            }
         }
      
         //Voiko liikkua itään?
         if (%east == 0 && %posx < 3) {
            if ($board.obstacles[%posx+1,%posy] == 0  )  
            {
               luoLiikuGrid(%posx+1,%posy,3);
                  %east = 1;
            }
         }

      }
      


      if ($nappulaSprite[%nappula].hasDoneAction == 0) 
      {
      
         if ($nappulaSprite[%nappula].hasMoved == 1) {
            selectedNappula(%nappula);
         }
         
      
         //voiko ampua pääilmansuuntiin?
         if ($nappulaSprite[%nappula].missile == 1) {
            //debuggi();
            //echo("tutkitaan voiko ampua...");
            for (%x=0;%x<4;%x++) {
              //echo (%x SPC %posy);
              if ( $board.nappulat[%x,%posy] > 0 ) {
                 if ($nappulaSprite[$board.nappulat[%x,%posy]].groupid == game.getVuorossaOlevanPelaajanVastustaja()) {
                 
                     luoAttackGrid(%x,%posy,4+%x);
                     %foundai = %x SPC %posy;
                     //echo("joo: " @ %foundai);
                 
                 }
              }
           
            }
            for (%y=0;%y<4;%y++) {
              //echo (%posx SPC %y);
              if ( $board.nappulat[%posx,%y] > 0 ) {
                 if ($nappulaSprite[$board.nappulat[%posx,%y]].groupid == game.getVuorossaOlevanPelaajanVastustaja()) {
              
                    luoAttackGrid(%posx,%y,8+%y);
                    %foundai = %posx SPC %y;
                    //echo("joo: " @ %foundai);
              
                 }
              }
           
            }
         }
         
         //echo("found ai:" @ %foundai);
         
         for (%x=0; %x<5; %x++)
         {
            for (%y=0; %y<5; %y++)
            {
               $board.vaara[%x,%y] = 0;
            }
         }
         
         
         //tutkitaan onko olento vaarassa joutua vihollisen olennon jousiyksikön ammuttavaksi
         %vaarassaHorisontal = 0;
         %vaarassaVertical = 0;
         if ($nappulaSprite[%nappula].hasMoved == 0 && game.getVuorossaOlevaPelaaja() == 1) {

            //echo("tutkitaan vaara...");
            for (%x=0;%x<4;%x++) {
              if ( $board.nappulat[%x,%posy] > 0 ) {
                 if ( $nappulaSprite[$board.nappulat[%x,%posy]].groupid == game.getVuorossaOlevanPelaajanVastustaja()
                 && $nappulaSprite[$board.nappulat[%x,%posy]].missile == "1" ) {
                 
                     %vaarassaHorisontal = 1;
                     
                 
                 }
              }
           
            }
            for (%y=0;%y<4;%y++) {
              if ( $board.nappulat[%posx,%y] > 0 ) {
                 if ($nappulaSprite[$board.nappulat[%posx,%y]].groupid == game.getVuorossaOlevanPelaajanVastustaja()
                 && $nappulaSprite[$board.nappulat[%posx,%y]].missile == "1" ) {
              
                    %vaarassaVertical = 1;

              
                 }
              }
           
            }
         }
      


      
         //Voiko liikkua pohjoiseen?
         if (%north == 0 && %posy > 0) {
            if ($board.obstacles[%posx,%posy-1] == 255 && $nappulaSprite[$board.nappulat[%posx,%posy-1]].groupid == game.getVuorossaOlevanPelaajanVastustaja() ) 
            {
                           

               luoAttackGrid(%posx,%posy-1,0);
               %foundai = %posx SPC %posy-1;
               //echo("found enemy:" @ %foundai);
            }
         }

         //Voiko liikkua etelään?
         if (%south == 0 && %posy < 3) {
            if ($board.obstacles[%posx,%posy+1] == 255 && $nappulaSprite[$board.nappulat[%posx,%posy+1]].groupid == game.getVuorossaOlevanPelaajanVastustaja() )  
            {
                           

               luoAttackGrid(%posx,%posy+1,1);
               %foundai = %posx SPC %posy+1;
               //echo("found enemy:" @ %foundai);

            }
         }

         //Voiko liikkua länteen?
         if (%west == 0 && %posx > 0) {
            if ($board.obstacles[%posx-1,%posy] == 255 && $nappulaSprite[$board.nappulat[%posx-1,%posy]].groupid == game.getVuorossaOlevanPelaajanVastustaja() )  
            {
               
               luoAttackGrid(%posx-1,%posy,2);
               %foundai = %posx-1 SPC %posy;
               //echo("found enemy:" @ %foundai);
            }
         }
      
         //Voiko liikkua itään?
         if (%east == 0 && %posx < 3) {
            if ($board.obstacles[%posx+1,%posy] == 255 && $nappulaSprite[$board.nappulat[%posx+1,%posy]].groupid == game.getVuorossaOlevanPelaajanVastustaja() )  
            {
               

               luoAttackGrid(%posx+1,%posy,3);
               %foundai = %posx+1 SPC %posy;
               //echo("found enemy:" @ %foundai);
            }
         }

      }
      
      //echo("%foundai: " @ %foundai);
      
      //TEKOÄLYSKRIPTI:
      
      if (game.getVuorossaOlevaPelaaja() == 1 ) 
      {
         if ( getWordCount(%foundai) > 0 ) {
            
            debuggi();
            %vihollinen = $nappulaSprite[   $board.nappulat[ getWord(%foundai, 0),  getWord(%foundai, 1) ]   ];
            echo("getWord(%foundai, 0)=" @ getWord(%foundai, 0));
            echo("getWord(%foundai, 1)=" @ getWord(%foundai, 1));
            echo("%vihollinen=" @ %vihollinen);
            echo("$board.nappulat[getWord(%foundai, 0),getWord(%foundai, 1)]=" @ $board.nappulat[ getWord(%foundai, 0),  getWord(%foundai, 1) ]  );
            echo("%foundai=" @ %foundai);
            schedule(500,0, "doAttack", %vihollinen, game.getVuoro());
            echo("doAttack(" @ %vihollinen @ "," @ game.getVuoro() @ ")");
            schedule(1500,0, "katsoMihinRuutuunVoiLiikkua" );

         } else {
            
            if ( $nappulaSprite[%nappula].hasMoved == 0 ) 
            {
               if ( %vaarassaHorisontal == 1 || %vaarassaVertical == 1 ) 
               {

                  for (%x = 0; %x<4; %x++) {
                     for (%y = 0; %y<4; %y++) {
                     
                        //LASKETAAN KUNKIN LAUDAN RUUDUN VAARALLISUUSPISTEET
                        for (%xx = 0; %xx<4; %xx++) {
                           if ( $nappulaSprite[$board.nappulat[%xx,%y]].groupid == game.getVuorossaOlevanPelaajanVastustaja() )
                           {

                           
                              if ( $nappulaSprite[$board.nappulat[%xx,%y]].missile == 1 )
                              {
                              
                                 %value = t2dVectorDistance(%x SPC "0", %xx SPC "0");
                                 if (%value == 1)
                                 {
                                    %value = -1;
                                 }

                                 $board.vaara[%x,%y] = $board.vaara[%x,%y] + %value;
                              
                                 if ( $board.obstacles[%xx ,%y - 1] == 0 )
                                 {
                                    $board.vaara[%x,%y - 1] = $board.vaara[%x,%y - 1] + %value;
                                 }
                                 else if ( $board.obstacles[%xx,%y + 1] == 0 )
                                 {
                                    $board.vaara[%x,%y + 1] = $board.vaara[%x,%y + 1] + %value;
                                 }
                                 
                              
                              }
                              
                           }
                           
                        }

                        for (%yy = 0; %yy<4; %yy++) 
                        {
                           
                           if ( $nappulaSprite[$board.nappulat[%x,%yy]].groupid == game.getVuorossaOlevanPelaajanVastustaja() )
                           {

                           
                              if ( $nappulaSprite[$board.nappulat[%x,%yy]].missile == 1 )
                              {
                              
                                 %value = t2dVectorDistance(%y SPC "0", %yy SPC "0");
                                 if (%value == 1)
                                 {
                                    %value = -1;
                                 }


                                 $board.vaara[%x,%y] = $board.vaara[%x,%y] + %value;
                                 
                                 
                                 if ( $board.obstacles[%x - 1,%yy] == 0 )
                                 {
                                    $board.vaara[%x - 1,%y] = $board.vaara[%x - 1,%y] + %value;
                                 }
                                 else if ( $board.obstacles[%x + 1,%yy] == 0 )
                                 {
                                    $board.vaara[%x + 1,%y] = $board.vaara[%x + 1,%y] + %value;
                                 }
                                 
                              
                              }
                              
                           }
                           
                        }
                        
                     
                     
                     }
                     
                  }
                  //debuggi();
                  
                  %southvaara = $board.vaara[%posx,%posy+1]; 
                  %northvaara = $board.vaara[%posx,%posy-1]; 
                  %eastvaara = $board.vaara[%posx+1,%posy]; 
                  %westvaara = $board.vaara[%posx-1,%posy]; 
                  
                  if (%south == 1 && %north == 1)
                  {
                     if ( %southvaara < %northvaara )
                     {
                        %north = 0;
                     } 
                     else if ( %southvaara > %northvaara )
                     {
                        %south = 0;
                     }
                     else
                     {
                        %south = 0;
                     }
                  }
                  
                  if (%west == 1 && %east == 1)
                  {
                     if ( %westvaara < %eastvaara )
                     {
                        %east = 0;
                     } 
                     else if ( %westvaara > %eastvaara )
                     {
                        %west = 0;
                     }
                     else
                     {
                        %west = 0;
                     }
                  }
                  
                  if (%south == 1 && %east == 1)
                  {
                     if ( %southvaara < %eastvaara )
                     {
                        %east = 0;
                     } 
                     else if ( %southvaara > %eastvaara )
                     {
                        %south = 0;
                     }
                     else
                     {
                        %south = 0;
                     }

                  }
                  
                  if (%south == 1 && %west == 1)
                  {
                     if ( %southvaara < %westvaara )
                     {
                        %west = 0;
                     } 
                     else if ( %southvaara > %westvaara )
                     {
                        %south = 0;
                     }
                     else
                     {
                        %south = 0;
                     }

                  }
                  
                  if (%north == 1 && %east == 1)
                  {
                     if ( %northvaara < %eastvaara )
                     {
                        %east = 0;
                     } 
                     else if ( %northvaara > %eastvaara )
                     {
                        %north = 0;
                     }
                     else
                     {
                        %north = 0;
                     }

                  }
                  
                  if (%north == 1 && %west == 1)
                  {
                     if ( %northvaara < %westvaara )
                     {
                        %west = 0;
                     } 
                     else if ( %northvaara > %westvaara )
                     {
                        %north = 0;
                     }
                     else
                     {
                        %north = 0;
                     }
                  }
                  
                  
               
               
                  if (%south == 1) 
                  {
                     %sankari.setValittuRuutu(%posx SPC %posy+1);
                     doMovement( %posx , %posy + 1 );
                  } 
                  else if (%north == 1)
                  {
                     %sankari.setValittuRuutu(%posx SPC %posy-1);
                     doMovement( %posx , %posy - 1 );
                  }
                  else if (%west == 1) 
                  {
                     %sankari.setValittuRuutu(%posx-1 SPC %posy);
                     doMovement( %posx-1 , %posy );
                  }
                  else if (%east == 1)
                  {
                     %sankari.setValittuRuutu(%posx+1 SPC %posy);
                     doMovement( %posx+1 , %posy );
                  }
                  else
                  {
                     schedule(500,0, "doDefence", game.getVuoro());
                  }
                  
               }
               else
               {
                  schedule(500,0, "doDefence", game.getVuoro());
               }
            }
            else
            {
               schedule(500,0, "doDefence", game.getVuoro());
            }
            
            
         }
      
      } else {
         if ( $nappulaSprite[%nappula].hasMoved == 1 ) {
            if ( $nappulaSprite[%nappula].hasDoneAction == 1 ) 
            {
               //echo("liikuttu on, hyökätty on");
               schedule(1000,0, "doDefence", game.getVuoro());
            } else {
               if ( getWordCount(%foundai) == 0 ) 
               {
                  //echo("liikkumisen jälkeen ei löydetty kohdetta jonka päälle voisi hyökätä" @ %foundai);
                  schedule(1000,0, "doDefence", game.getVuoro());
               }
            }
         } else {
            if ( $nappulaSprite[%nappula].hasDoneAction == 1 ) 
            {
               if ( %north == 0 && %south == 0&& %west == 0 && %east == 0 ) 
               {
                  schedule(1000,0, "doDefence", game.getVuoro());
               }
            } 
            else if ( $nappulaSprite[%nappula].hasDoneAction == 0 && $nappulaSprite[%nappula].hasMoved == 0 )
            {
               if ( !%north && !%south && !%west && !%east ) 
               {
                  schedule(1000,0, "doDefence", game.getVuoro());
               }
            }
         }
      }
   }
}

function piilotaGrids() {

   

}

function luoAttackGrid(%bx,%by,%i)
{
   
      $movementgrid[%i] = new Sprite() { scene = t2dScene; };
      $movementgrid[%i].setImageMap("boardgridgreenImageMap");
      $movementgrid[%i].setPosition(boardToWorld(%bx, %by));
      $movementgrid[%i].setSize("8 8");
      $movementgrid[%i].setFrame("2");

      $movementgrid[%i].setLayer("4");
      
      if (game.getVuorossaOlevaPelaaja() == 1 ) 
      {
               $movementgrid[%i].setVisible("False");
      } else {
               $movementgrid[%i].setVisible("True");
      }
      %alphalk = "1.0";
      $movementgrid[%i].setBlendAlpha(%alphalk);
      $movementgrid[%i].setUseMouseEvents(0);
      $movementgrid[%i].isSelectable = false;  
      $movementgrid[%i].isValintakehys = true;  
      $movementgrid[%i].positionOnBoard = %bx SPC %by;
      $movementgrid[%i].defaultFrame = "2";


}


function luoLiikuGrid(%bx,%by,%i)
{
  
      $movementgrid[%i] = new Sprite() { scene = t2dScene; };
      $movementgrid[%i].setImageMap("boardgridgreenImageMap");
      $movementgrid[%i].setPosition(boardToWorld(%bx, %by));
      $movementgrid[%i].setSize("8 8");
      $movementgrid[%i].setFrame("0");
      $movementgrid[%i].setVisible("True");
      $movementgrid[%i].setLayer("4");

      if (game.getVuorossaOlevaPelaaja() == 1 ) 
      {
               $movementgrid[%i].setVisible("False");
      } else {
               $movementgrid[%i].setVisible("True");
      }
      %alphalk = "1.0";

      $movementgrid[%i].setBlendAlpha(%alphalk);
      $movementgrid[%i].setUseMouseEvents(0);
      $movementgrid[%i].isSelectable = false;  
      $movementgrid[%i].isValintakehys = true;  
      $movementgrid[%i].positionOnBoard = %bx SPC %by;
      $movementgrid[%i].defaultFrame = "0";
  
}

function onkoNappulatElossa()
{
   //echo("onkoNappulatElossa()");
   %pelaajaVector = 0;
   %aiVector = 0;
   for (%i=1; %i <= game.getCountofnappula(); %i++)
   {

      if ( $nappulaSprite[%i].groupid == 0 && $nappulaSprite[%i].alive == 1 ) //nappula kuuluu pelaajalle ja se on laudalla
      {
         if (%pelaajaVector == 0) 
         { 
            %pelaajaVector = %i;
         }
         else
         {
            %pelaajaVector = %pelaajaVector SPC %i;
            
         }
         
      } else {
         //echo("fucking åmål");
      }
      
      
      if ( $nappulaSprite[%i].groupid == 1 && $nappulaSprite[%i].alive == 1) //nappula kuuluu AI:lle ja se on laudalla
      {
         if (%aiVector == 0) 
         {
            %aiVector = %i;
         }
         else
         {
            %aiVector = %aiVector SPC %i;
         }
         
      } else {
         //echo("fucking åmål 2");
      }
      

   }
   
   %aicount=getwordcount(%aiVector);
   %pelaajacount=getwordcount(%pelaajaVector);
   
   if (%aicount == 0 || %pelaajacount == 0) {
      return 0;
   } 
   else
   {
      //echo("AIcount ja PelaajaCount: " SPC %aicount SPC %pelaajacount);
      return 1;
   }
   
   
   
}


function jaaNappuloidenVuorot() 
{
   echo("jaaNappuloidenVuorot()");
   //Tehdään vektorit
   

   %pelaajaVector ="";
   %aiVector ="";
   for (%i=1; %i <= game.getCountofnappula(); %i++)
   {

      if ( $nappulaSprite[%i].groupid == 0 && $nappulaSprite[%i].alive == 1 ) //nappula kuuluu pelaajalle ja se on laudalla
      {
         if (%pelaajaVector == 0) 
         { 
            %pelaajaVector = %i;
         }
         else
         {
            %pelaajaVector = %pelaajaVector SPC %i;
            
         }
         
      }
      
      
      if ( $nappulaSprite[%i].groupid == 1 && $nappulaSprite[%i].alive == 1) //nappula kuuluu AI:lle ja se on laudalla
      {
         if (%aiVector == 0) 
         {
            %aiVector = %i;
         }
         else
         {
            %aiVector = %aiVector SPC %i;
         }
         
      }
      

   }

   %aicount=getwordcount(%aiVector);
   %pelaajacount=getwordcount(%pelaajaVector);
   
   if (%aicount > 0 && %pelaajacount > 0) 
   {
   
      if (%aicount > %pelaajacount) 
      {
         for (%i=0; %i < %aicount - %pelaajacount; %i++)
         {
            %pelaajaVector = %pelaajaVector SPC "0";
         }
      } 
      else if (%aicount < %pelaajacount) 
      {
         for (%i=0; %i < %pelaajacount - %aicount; %i++)
         {
            %aiVector = %aiVector SPC "0";
         }
      }

      %vektorienkoko = getwordcount(%pelaajaVector);
      game.setVuorotVector(getword(%pelaajaVector, 0) SPC getword(%aiVector, 0));
      for (%i = 1; %i < %vektorienkoko; %i++)
      {
         if (getword(%pelaajaVector, %i) == "0") 
         {
            game.setVuorotVector (game.getVuorotVector() SPC getword(%aiVector, %i));
         }
         else if (getword(%aiVector, %i) == "0") 
         {
            game.setVuorotVector( game.getVuorotVector() SPC getword(%pelaajaVector, %i) );
         }
         else
         {
            game.setVuorotVector( game.getVuorotVector() SPC getword(%pelaajaVector, %i) SPC getword(%aiVector, %i) );
         }
      }

   } 
   else 
   {
      setGameState($gameEndState);
      endbattle();
   }

   
}

function endbattle() {


      sceneWindow2D.setUseWindowMouseEvents(false);
      schedule(2000,0,creditsscreen);

}


function poistaNappulanVuoro(%nappula) {

 for (%i=0; %i<getWordCount(game.getVuorotVector()); %i++)
 {


   if (  getWord( game.getVuorotVector(), %i ) == %nappula  ) {
     game.setVuorotVector( removeWord (game.getVuorotVector(), %i) );  
   }
 }
}


function updateActionIcons()
{
   if (getGameState() == $gamePreBattleState || getGameState() == $gamePreBattleTimerState) {
   
      iconAttack1.setVisible(false);
      iconAttack2.setVisible(false);
      iconDefence1.setVisible(false);
      iconDefence2.setVisible(false);
      iconMagic1.setVisible(false);
      iconMagic2.setVisible(false);
      iconMagic3.setVisible(false);
      hero2icon.setVisible(false);
      hero2icongrid.setVisible(false);

   } 

   if (getGameState() == $gameTimerBattleState) {
   
      iconAttack1.setVisible(true);
      iconAttack2.setVisible(true);
      iconDefence1.setVisible(true);
      iconDefence2.setVisible(true);
      iconMagic1.setVisible(true);
      iconMagic2.setVisible(true);
      iconMagic3.setVisible(true);
      hero2icon.setVisible(true);
      hero2icongrid.setVisible(true);
      
      

   } 
  
  
}


function createSetupNappulat()
{
   $nappulaNimi = "warrior archer cleric paladin skeletonwarrior skeletonarcher dreadzombie ancientghost";
   for (%i=0; %i<4; %i++)
   {
         $setupNappulaSprite[%i] = new Sprite() { scene = t2dScene; };
         $setupNappulaSprite[%i].setImageMap(getWord($nappulanimi,%i) @ "StoppedImageMap");
         $setupNappulaSprite[%i].setPosition(0,0);
        $setupNappulaSprite[%i].setSize("8 8");
        $setupNappulaSprite[%i].setBlendAlpha("0.5");
        $setupNappulaSprite[%i].setFrame(4);
        $setupNappulaSprite[%i].setVisible(false);
        $setupNappulaSprite[%i].setLayer("0");
        $setupNappulaSprite[%i].setUseMouseEvents(0);
        $setupNappulaSprite[%i].isSelectable = false;
   }
   
}

function deleteSetupNappulat()
{
   for (%i=0; %i<4; %i++)
   {
      if (isObject($setupNappulaSprite[%i]))
         $setupNappulaSprite[%i].safeDelete();
   }
   for (%i=0; %i<12; %i++)
   {
      if (isObject($icon[%i]))
         $icon[%i].safeDelete();
      if (isObject($grid[%i]))
         $grid[%i].safeDelete();
      if (isObject($gridback[%i]))
         $gridback[%i].safeDelete();
      if (isObject($numb[%i]))
         $numb[%i].safeDelete();
   }
}


function initBoard() 
{
   
   
   debuggi();
   createFloor();
   
   
   $board = new ScriptObject() { };

   for (%i=0;%i<4;%i++) {
    for (%j=0;%j<4;%j++) {
      $board.nappulat[%i,%j] = 0;
      $board.obstacles[%i,%j] = 0;
    }
   }

   debuggi();

   
   %obsx=getRandom(3); //arvotaan luku väliltä [0,3]
   %obsy=getRandom(3); //arvotaan luku väliltä [0,3]
   
   for (%bx=0; %bx<4; %bx++) {
   
      for (%by=0; %by<4; %by++) {
         if (%bx == %obsx && %by == %obsy) {
            $board.obstacles[%bx, %by] = getRandom(2)+1; //arvotaan luku väliltä [1,3]
            createObstacle($board.obstacles[%bx, %by], %bx, %by);
         } else {
            $board.obstacles[%bx, %by] = 0;
         }
         
      
      }
    
   
   }
   
   $floor[0,2].setFrame(1);
   $floor[1,0].setFrame(2);
   
   
}


function createAInappulat() {

   %loop = true;
   %lx = getRandom(3); // [0-3]
   %ly = getRandom(1); // [0-1]
   while (%loop)
   {
      
      %arvonappulalvl = getRandom(3)+1; // [1-4]
      if (HeroGroup.getObject(1).getOlentopisteet() >= %arvonappulalvl ) {
      
         while ($board.obstacles[%lx,%ly] != 0)
         {
           %lx = getRandom(3); // [0-3]
           %ly = getRandom(1); // [0-1]
         } 
         lisaaUusiNappulaLaudalle(%arvonappulalvl, %lx, %ly, 1);         
      
      }
      
      if (HeroGroup.getObject(1).getOlentopisteet() < 1)
      {
         %loop = false;
      }
      
      
      
   }


}

function lisaaUusiNappulaLaudalle(%nappulaLevel, %lx, %ly, %heroid)
{
   // Intercept coordinate assignment: verify target coordinates fall within board array bounds
   if (%lx < 0 || %lx >= 4 || %ly < 0 || %ly >= 4)
   {
      echo("lisaaUusiNappulaLaudalle: rejected - coordinates out of bounds (" @ %lx SPC %ly @ ")");
      return false;
   }

   // Verify tile is not occupied by an existing obstacle or piece
   if ($board.obstacles[%lx, %ly] != 0)
   {
      echo("lisaaUusiNappulaLaudalle: rejected - tile occupied or obstacle (" @ %lx SPC %ly @ ")");
      return false;
   }

   // For player (%heroid == 0), verify target tile corresponds to a valid active green placement tile (rows 2-3)
   if (%heroid == 0)
   {
      if (%ly < 2 || %ly >= 4 || !isObject($green[%lx, %ly]))
      {
         echo("lisaaUusiNappulaLaudalle: rejected - not a valid green placement tile (" @ %lx SPC %ly @ ")");
         return false;
      }
   }
   
   if (%heroid == 1) 
   {
      %animindex = %nappulaLevel+3;
      %nappulaLevel = %nappulaLevel + 4;
      %facing = "s";
   } else 
   {
      %animindex = %nappulaLevel-1;
      %facing = "n";

   }
   
   
   game.addCountOfNappula(1);
   %j = game.getCountOfNappula();
   echo("countof:" @ %j);
   $board.obstacles[%lx,%ly]=255;
   debuggi();
   $board.nappulat[%lx,%ly]=%j;
debuggi();


        $nappulaSprite[%j] = new Sprite() { scene = t2dScene; class = Nappula; };
        $nappulaSprite[%j].state = $olentoStandState[%animindex];

        $nappulaSprite[%j].facing = %facing;
        $nappulaSprite[%j].animationName = $nappulaSprite[%j].state.anim @ $nappulaSprite[%j].facing;
        $nappulaSprite[%j].playAnimation($nappulaSprite[%j].animationName);
        
        $nappulaSprite[%j].setPosition(boardToWorldUnit(%lx, %ly));
        $nappulaSprite[%j].setSize("8 8");
        $nappulaSprite[%j].setBlendAlpha("1");
        $nappulaSprite[%j].setVisible(true);
        $nappulaSprite[%j].setLayer("2");
        $nappulaSprite[%j].setUseMouseEvents(0);
        $nappulaSprite[%j].isSelectable = false;

      %i = %nappulaLevel - 1; 
      $nappulaSprite[%j].hp = UnitProperties.hp[%i];
      $nappulaSprite[%j].hpMAX = UnitProperties.hp[%i];
      $nappulaSprite[%j].attack = UnitProperties.attack[%i];
      $nappulaSprite[%j].attackMAX = UnitProperties.attack[%i];
      $nappulaSprite[%j].defence = UnitProperties.defence[%i];
      $nappulaSprite[%j].defenceMAX = UnitProperties.defence[%i];
      $nappulaSprite[%j].mana = UnitProperties.mana[%i];
      $nappulaSprite[%j].manaMAX = UnitProperties.mana[%i];
      $nappulaSprite[%j].level = UnitProperties.lvl[%i];
      $nappulaSprite[%j].groupid = UnitProperties.groupid[%i];
      $nappulaSprite[%j].creatureID = UnitProperties.lvl[%i] - 1 + $nappulaSprite[%j].groupid*4;
      $nappulaSprite[%j].spells = UnitProperties.spells[%i];
      $nappulaSprite[%j].missile = UnitProperties.missile[%i];
      $nappulaSprite[%j].name = getWord($nappulanimi,%nappulaLevel - 1);
      $nappulaSprite[%j].alive = 1;
      
      $nappulaSprite[%j].boardX = %lx;
      $nappulaSprite[%j].boardY = %ly;
      $nappulaSprite[%j].positionOnBoard = %lx SPC %ly;
      
      $nappulaSprite[%j].hasMoved = 0;
      $nappulaSprite[%j].hasDoneAction = 0;
      $nappulaSprite[%j].index = %j;
      
      
      //echo("luotiin uusi nappula " @ $nappulaSprite[%j] @ "(" @ %j @ ") index(" @ $nappulaSprite[%j].index @
      //" creatureID(" @ $nappulaSprite[%j].creatureID @ ") jonka nimi on " @ $nappulaSprite[%j].name);
      

      
      
              
   //VÄHENNÄ OLENTOPISTEET
   HeroGroup.getObject(%heroid).SubOlentopisteet( $nappulaSprite[%j].level );
   
   //PÄIVITÄ SANKARIN OMISTAMIEN NAPPIEN LUKUMÄÄRÄ
   HeroGroup.getObject(%heroid).AddNappulatlkm(1);

   return true;
}

function updatePisteetGridsIcons()
{

   for(%x=0; %x<4; %x++) {
      for(%y=2; %y<4; %y++) {
         if ($green[%x,%y]) 
         {
            if (isObject($green[%x,%y]))
               $green[%x,%y].safeDelete();     
         }
      
      }
   }

   updateOlentopisteet();
   updateGreenGrids();
   updateOlentoikonit();

   if (HeroGroup.getObject(0).getOlentopisteet() < 1) {

      
      schedule(500,0, jaaKortit);
      heroicon.setPosition("-40 -24");
      heroicongrid.setPosition("-40 -24");

      game.setPeliTimer(5);
      schedule(1000,0, sekuntiLaskuri);
      secondText.setVisible(true);
      secondCounter.setFrame(game.getPeliTimer());
      secondCounter.setVisible(true);
      sijoitaText.setVisible(false);
      olentopisteetcounter.setVisible(false);
   }


}



function sekuntiLaskuri()
{
   game.subPeliTimer(1);
   secondCounter.setFrame(game.getPeliTimer());


   if (game.getPeliTimer() == 0) {
      secondText.setVisible(false);
      secondCounter.setVisible(false);
            // play ambient sound
      setGameState($gameTimerBattleState);
      updateActionIcons();
      InitGameForBattle();
   } else {
      schedule(1000,0, sekuntiLaskuri);
      // play ambient sound
      $secondSound = playArchonSound(ambient4Audio);
   }
   
   
}


function createObstacle(%obstacletype, %ox, %oy) 
{
   
   if (%obstacletype == 1) 
   {
      
      $floor[%ox,%oy].setVisible(False);
   
   } 
   else 
   {
         
        $obstacleSprite = new Sprite() { scene = t2dScene; };
        
        $obstacleSprite.setImageMap("obstaclesImageMap");
        %posX = $floor[%ox,%oy].getPositionX();
        %posY = $floor[%ox,%oy].getPositionY();
        $obstacleSprite.setPosition(%posX, %posY + 4);
        $obstacleSprite.setSize("8 16");
        $obstacleSprite.mountID = "180";
        $obstacleSprite.setFrame(%obstacletype - 2);
        $obstacleSprite.setVisible(True);
        $obstacleSprite.setLayer("1");
        $obstacleSprite.setUseMouseEvents(0);
        $obstacleSprite.isSelectable = false;
        
   }
   
   
   
   
}


function createFloor() 
{
   
   for (%cfx=0;%cfx<4;%cfx++) 
   {
      for (%cfy=0;%cfy<4;%cfy++)
      {

         $floor[%cfx,%cfy] = new Sprite() { scene = t2dScene;  };
         $floor[%cfx,%cfy].setImageMap("boardgridImageMap");
         $floor[%cfx,%cfy].setUseMouseEvents("1");
         $floor[%cfx,%cfy].setPosition(boardToWorld(%cfx, %cfy));
         $floor[%cfx,%cfy].setSize("8 8");
         $floor[%cfx,%cfy].setLayer("7");
         $floor[%cfx,%cfy].setCollisionActiveSend("1");
         $floor[%cfx,%cfy].setCollisionActiveReceive("1");
     
         
      }
   }
   
   

   

}


function updateGreenGrids() 
{
   if (HeroGroup.getObject(0).getOlentoPisteet() > 0) {
      for (%ux=0; %ux<4; %ux++) 
      {
         for (%uy=2; %uy<4; %uy++) 
         {
        
            if ($board.obstacles[%ux,%uy]==0) {
           
               $green[%ux,%uy] = new Sprite() { scene = t2dScene; };
        
               $green[%ux,%uy].setImageMap("boardgridgreenImageMap");
               $green[%ux,%uy].setPosition(boardToWorld(%ux, %uy));
               $green[%ux,%uy].setSize("8 8");
               $green[%ux,%uy].setFrame("0");
               $green[%ux,%uy].setVisible("True");
               $green[%ux,%uy].setLayer("2");
               $green[%ux,%uy].setBlendAlpha("0.9");
               $green[%ux,%uy].setUseMouseEvents(0);
               $green[%ux,%uy].isSelectable = false;  
               $green[%ux,%uy].isValintakehys = true;  
               $green[%ux,%uy].positionOnBoard = %ux SPC %uy;
            
           }
        }
      }
   }

}


function updateOlentopisteet() 
{
   olentopisteetcounter.setFrame(HeroGroup.getObject(0).getOlentopisteet());
   
   
}

function iconSendToFront(%index) {

   $icon[%index].setLayer("1");
   $grid[%index].setLayer("0");

}

function updateOlentoikonit() 
{

   for(%i=0; %i<12; %i++) {
      if (isObject($icon[%i]))
         $icon[%i].safeDelete();
      if (isObject($grid[%i]))
         $grid[%i].safeDelete();
      if (isObject($gridback[%i]))
         $gridback[%i].safeDelete();
      if (isObject($numb[%i]))
         $numb[%i].safeDelete();
   }

   if (HeroGroup.getObject(0).getOlentopisteet() > 0) { 
      

      %x = "-44";
      %lvl1 = HeroGroup.getObject(0).getOlentopisteet();
      %lvl2 = mFloor(HeroGroup.getObject(0).getOlentopisteet()/2);
      %lvl3 = mFloor(HeroGroup.getObject(0).getOlentopisteet()/3);
      %lvl4 = mFloor(HeroGroup.getObject(0).getOlentopisteet()/4);

   
         for (%i=0; %i<12; %i++) {
   
           if (%lvl1 > 0) {
               %frame = "0";
               %lvl1--;
           } else if (%lvl2 > 0) {
               %frame = "1";
               %lvl2--;
           } else if (%lvl3 > 0) {
               %frame = "2";
               %lvl3--;
           } else if (%lvl4 > 0) {
               %frame = "3";
               %lvl4--;
           }
   
         $icon[%i] = new Sprite() { scene = t2dScene; };
        
         $icon[%i].setImageMap("iconsImageMap");
         $icon[%i].setPosition(%x SPC "-28");
         $icon[%i].setSize("8 8");
         $icon[%i].mountID = "100" + %i;
         $icon[%i].setFrame(%frame);
         $icon[%i].setVisible("True");
         $icon[%i].setLayer("6");
         $icon[%i].setUseMouseEvents(0);
         $icon[%i].isSelectable = true;
         $icon[%i].creatureLevel = %frame + 1;
         $icon[%i].index = %i;

         $grid[%i] = new Sprite() { scene = t2dScene; };
        
         $grid[%i].setImageMap("gridsImageMap");
         $grid[%i].setPosition(%x SPC "-28");
         $grid[%i].setSize("8 8");
         $grid[%i].mountID = "120" + %i;
         $grid[%i].setFrame("0");
         $grid[%i].setVisible("True");
         $grid[%i].setLayer("5");
         $grid[%i].setUseMouseEvents(0);
         $grid[%i].isSelectable = false;
         $grid[%i].mount($icon[%i], 0, 0, 0, True, True, True, True);
        
         $gridback[%i] = new Sprite() { scene = t2dScene; };
        
         $gridback[%i].setImageMap("gridsImageMap");
         $gridback[%i].setPosition(%x SPC "-28");
         $gridback[%i].setSize("8 8");
         $gridback[%i].mountID = "140" + %i;
         $gridback[%i].setFrame("1");
         $gridback[%i].setVisible("True");
         $gridback[%i].setLayer("7");
         $gridback[%i].setUseMouseEvents(0);
         $gridback[%i].isSelectable = false;
        
         $numb[%i] = new Sprite() { scene = t2dScene; };
        
         $numb[%i].setImageMap("numbersImageMap");
         $numb[%i].setPosition(%x + 2 SPC "-30");
         $numb[%i].setSize("4 4");
         $numb[%i].mountID = "160" + %i;
         $numb[%i].setFrame(%frame + 1);
         $numb[%i].setVisible("True");
         $numb[%i].setLayer("5");
         $numb[%i].isSelectable = false;
         $numb[%i].setUseMouseEvents(0);
         if (%frame == 2) {
               $numb[%i].setBlending("True", "ZERO");
         }
         $numb[%i].mount($gridback[%i], 0.5, -0.4, 0, True, True, True, True);
         $numb[%i].index = %i;
           

         %x = %x + 8;
        
         if (%lvl1 == 0 && %lvl2 == 0 && %lvl3 == 0 && %lvl4 == 0) 
         {
               %i = 13;
         }
          
      }
   
   }
   
}
   
   
function initStateMachine() {


  $gameIntroState = new ScriptObject() { };
  $gameMainMenuState = new ScriptObject() { };
  $gameCardSelectState = new ScriptObject() { };
  $gamePreBattleState = new ScriptObject() { };
  $gamePreBattleTimerState = new ScriptObject() { };
  $gameTimerBattleState = new ScriptObject() { };
  $gameBattleState = new ScriptObject() { };
  $gameEndState = new ScriptObject() { };  
  
  $gameBattleState.name = "Battle State";
  $gameEndState.name = "End State";

}



function setGameState(%state)
{
   game.setState(%state);
}

function getGameState()
{
   return game.getState();
}

function doAction() {

   if (game.getHighlightedActionIcon() == 1)
   {
      doDefence(game.getVuoro());
   } 
   else if (game.getHighlightedActionIcon() == 2)
   {
   }
   else if (game.getHighlightedActionIcon() == 3)
   {
   }
}

   
function doDefence(%vuoro)
{
   if (%vuoro == game.getVuoro() ) 
   {
   
      
      seuraavaVuoro();
      if (game.getState() != $gameEndState) { katsoMihinRuutuunVoiLiikkua(); }
   }
   
}

function doMagic()
{
}

function doAttack(%vihollinen,%vuoro)

{
   if (%vuoro == game.getVuoro() ) 
   {
         
      $nappulaSprite[game.getNappulaSelected()].hasDoneAction = 1;


      echo("function doAttack("@ %vihollinen @ "," @ %vuoro @ ")");  
      
      
      %pos1 = $nappulaSprite[game.getNappulaSelected()].positionOnBoard;
      %pos2 = %vihollinen.positionOnBoard;

      $nappulaSprite[game.getNappulaSelected()].facing = facing( %pos1, %pos2 );
      %vihollinen.facing = facing ( %pos2, %pos1 );
      
      
      echo("setNappulaState(" @ $nappulaSprite[ game.getNappulaSelected() ] @ "," @ $olentoAttackState[ $nappulaSprite[game.getNappulaSelected() ].creatureID ] @ ")");
      setNappulaState( $nappulaSprite[ game.getNappulaSelected() ], $olentoAttackState[ $nappulaSprite[game.getNappulaSelected() ].creatureID ] );
       
      echo("setNappulaState(" @ %vihollinen @ "," @ $olentoBeenHitState[ %vihollinen.creatureID ] @ ")");
      schedule(50,0,"setNappulaState", %vihollinen, $olentoBeenHitState[ %vihollinen.creatureID ]  );
     
      if ( game.getVuorossaOlevaPelaaja() == 0 ) {
         %result=rollDicesRedAttacks($nappulaSprite[game.getNappulaSelected()], %vihollinen);

         //hyökkäys onnistui      
         if (%result == 1) 
         {  
            echo("vahingoitaNappula(" @ %vihollinen @ ")");
            vahingoitaNappula(%vihollinen);
            $fxbeen2 = playArchonSound(fxBeenHit);
            //$fxsoita = alxPlay(fxSword);
                blood.setPosition(%vihollinen.getPosition());
            schedule(300,0,piilotaveri);
         } else {$fxsoita = playArchonSound(fxSword);}
         
         
      } else {

         %result=rollDicesRedDefends(%vihollinen,$nappulaSprite[game.getNappulaSelected()]);
         
         //puolustus epäonnistui
         if (%result == 0)
         {  
                        echo("vahingoitaNappula(" @ %vihollinen @ ")");
            vahingoitaNappula(%vihollinen);
                $fx2 = playArchonSound(fxBeenHit);
                blood.setPosition(%vihollinen.getPosition());
            schedule(300,0,piilotaveri);
         
         } else {$fxsoita = playArchonSound(fxSword);}
         
      }
   }
   
}

function piilotaveri() {
   blood.setPosition("-50 -80");
}


function rollDicesRedAttacks(%attacker, %defender) {


%att=0;
%def=0;




   for (%i=0; %i<=8; %i++)
{
        if (isObject($noppa[%i])) {
           $noppa[%i].safeDelete();
        }
        if (isObject($noppadef[%i])) {
           $noppadef[%i].safeDelete();
        }
}
   for (%i=0; %i<%attacker.attack; %i++)
   {
   
         
        $noppa[%i] = new Sprite() { scene = t2dScene; };
        $noppa[%i].setImageMap("diceredImageMap");
        $noppa[%i].setPosition( %i * 10 - 24 SPC "30" );
        $noppa[%i].setSize("8 8");
        $noppa[%i].setRotation(getRandom(360));
        $noppa[%i].setAngularVelocity(2 * 36);
        %noppa=getRandom(3);
        $noppa[%i].setFrame(%noppa);
        $noppa[%i].setVisible(true);
        $noppa[%i].setLayer(10);
        $noppa[%i].setUseMouseEvents(0);
        $noppa[%i].isSelectable = false;

        if (%noppa == 0 || %noppa == 2) %att++;
   
   }
   
      for (%i=0; %i<%defender.defence; %i++)
   {
   
   
        $noppadef[%i] = new Sprite() { scene = t2dScene; };
        $noppadef[%i].setImageMap("diceblueImageMap");
        $noppadef[%i].setPosition( %i * 10 - 24 SPC "-30" );
        $noppadef[%i].setSize("8 8");
        $noppadef[%i].setRotation(getRandom(360));
        $noppadef[%i].setAngularVelocity(2 * 36);
        %noppa=getRandom(3);
        $noppadef[%i].setFrame(%noppa);
        $noppadef[%i].setVisible(true);
        $noppadef[%i].setLayer(10);
        $noppadef[%i].setUseMouseEvents(0);
        $noppadef[%i].isSelectable = false;
        
                if (%noppa == 3 ) %def++;


   
   }
   
   
   if (%att>%def) {
      
      return 1 ;
   }
   else 
   {
      return 0;
   }
   

}


function rollDicesRedDefends(%defender, %attacker) {

%att=0;
%def=0;



   for (%i=0; %i<=8; %i++)
{
        if (isObject($noppa[%i])) {
           $noppa[%i].safeDelete();
        }
        if (isObject($noppadef[%i])) {
           $noppadef[%i].safeDelete();
        }
}
   for (%i=0; %i<%defender.defence; %i++)
   {
   
   
        $noppadef[%i] = new Sprite() { scene = t2dScene; };
        $noppadef[%i].setImageMap("diceredImageMap");
        $noppadef[%i].setPosition( %i * 10 - 24 SPC "30" );
        $noppadef[%i].setSize("8 8");
        $noppadef[%i].setRotation(getRandom(360));
        $noppadef[%i].setAngularVelocity(2 * 36);
                %noppa=getRandom(3);
        $noppadef[%i].setFrame(%noppa);
        $noppadef[%i].setVisible(true);
        $noppadef[%i].setLayer(10);
        $noppadef[%i].setUseMouseEvents(0);
        $noppadef[%i].isSelectable = false;
if (%noppa == 1) %def++;
   
   }
      for (%i=0; %i<%attacker.attack; %i++)
   {
   
   
        $noppa[%i] = new Sprite() { scene = t2dScene; };
        $noppa[%i].setImageMap("diceblueImageMap");
        $noppa[%i].setPosition( %i * 10 - 24 SPC "-30" );
        $noppa[%i].setSize("8 8");
        $noppa[%i].setRotation(getRandom(360));
        $noppa[%i].setAngularVelocity(2 * 36);
                %noppa=getRandom(3);
        $noppa[%i].setFrame(%noppa);
        $noppa[%i].setVisible(true);
        $noppa[%i].setLayer(10);
        $noppa[%i].setUseMouseEvents(0);
        $noppa[%i].isSelectable = false;
      if (%noppa <= 1) %att++;
   
   }
   
   
   
      if (%def>=%att) {
      
         return 1 ;
      }
      else 
      {
         return 0;
      }

}



function debuggi() {
/*
   echo("board.obstacles");
   echo($board.obstacles[0,0] SPC $board.obstacles[1,0] SPC $board.obstacles[2,0] SPC $board.obstacles[3,0]);
   echo($board.obstacles[0,1] SPC $board.obstacles[1,1] SPC $board.obstacles[2,1] SPC $board.obstacles[3,1]);
   echo($board.obstacles[0,2] SPC $board.obstacles[1,2] SPC $board.obstacles[2,2] SPC $board.obstacles[3,2]);
   echo($board.obstacles[0,3] SPC $board.obstacles[1,3] SPC $board.obstacles[2,3] SPC $board.obstacles[3,3]);

*/
   echo("board.nappulat");
   echo($board.nappulat[0,0] SPC $board.nappulat[1,0] SPC $board.nappulat[2,0] SPC $board.nappulat[3,0]);
   echo($board.nappulat[0,1] SPC $board.nappulat[1,1] SPC $board.nappulat[2,1] SPC $board.nappulat[3,1]);
   echo($board.nappulat[0,2] SPC $board.nappulat[1,2] SPC $board.nappulat[2,2] SPC $board.nappulat[3,2]);
   echo($board.nappulat[0,3] SPC $board.nappulat[1,3] SPC $board.nappulat[2,3] SPC $board.nappulat[3,3]);
/*
   echo("board.vaara");
   echo($board.vaara[0,0] SPC $board.vaara[1,0] SPC $board.vaara[2,0] SPC $board.vaara[3,0]);
   echo($board.vaara[0,1] SPC $board.vaara[1,1] SPC $board.vaara[2,1] SPC $board.vaara[3,1]);
   echo($board.vaara[0,2] SPC $board.vaara[1,2] SPC $board.vaara[2,2] SPC $board.vaara[3,2]);
   echo($board.vaara[0,3] SPC $board.vaara[1,3] SPC $board.vaara[2,3] SPC $board.vaara[3,3]);




*/
}

function doMovement(%x,%y)
{
   
   %sankari = HeroGroup.getObject(game.getVuorossaOlevaPelaaja());
   
   //HYÖKKÄÄ
   if ($board.obstacles[%x,%y] == 255 && game.getVuorossaOlevaPelaaja() == 0) {
      
         if ( $nappulaSprite[game.getNappulaSelected()].hasDoneAction == 0) 
         {
            
            echo("%sankari=" @ %sankari);
            echo("%sankari.getValittuRuutuX()=" @ %sankari.getValittuRuutuX());
            echo("%sankari.getValittuRuutuY()=" @ %sankari.getValittuRuutuY());
            echo("$board.nappulat[%sankari.getValittuRuutuX(),%sankari.getValittuRuutuY()]=" @ $board.nappulat[ %sankari.getValittuRuutuX() , %sankari.getValittuRuutuY() ] );            
            %vihollinen = $nappulaSprite[   $board.nappulat[ %sankari.getValittuRuutuX() , %sankari.getValittuRuutuY() ]   ];
            echo("%vihollinen=" @ %vihollinen);
            doAttack(%vihollinen, game.getVuoro());
         }
         
         
         
         $nappulaSprite[game.getNappulaSelected()].hasDoneAction = 1;
         
         if (game.getState() != $gameEndState) { katsoMihinRuutuunVoiLiikkua(); }
      
   } else


// KÄVELE

if ($board.obstacles[%x,%y] == 0) {
   
  
   if ( $nappulaSprite[game.getNappulaSelected()].hasMoved == 0) 
   {

      moveNappula(boardToWorldUnit(%sankari.getValittuRuutuX(), %sankari.getValittuRuutuY()),
      %sankari.getValittuRuutuX() SPC %sankari.getValittuRuutuY());
      
    
      deleteGrids(1);
      
    
      %pos = $nappulaSprite[game.getNappulaSelected()].positionOnBoard;
      %posx = getword(%pos,0);
      %posy = getword(%pos,1);

      $board.obstacles[%posx,%posy]=0;
      $board.nappulat[%posx,%posy]=0;
      
      $board.obstacles[%sankari.getValittuRuutuX(),%sankari.getValittuRuutuY()]=255;
      $board.nappulat[%sankari.getValittuRuutuX(),%sankari.getValittuRuutuY()]=game.getNappulaSelected();
      

      $nappulaSprite[game.getNappulaSelected()].BoardX = %sankari.getValittuRuutuX() ;
      $nappulaSprite[game.getNappulaSelected()].BoardY = %sankari.getValittuRuutuY() ;
      $nappulaSprite[game.getNappulaSelected()].positionOnBoard = 
      $nappulaSprite[game.getNappulaSelected()].BoardX SPC $nappulaSprite[game.getNappulaSelected()].BoardY;
            
      $nappulaSprite[game.getNappulaSelected()].hasMoved = 1;
    
    
      
   }
   if (game.getState() != $gameEndState) { 
      if ( game.getVuorossaOlevaPelaaja() == 0 ) 
      {   
        katsoMihinRuutuunVoiLiikkua(); 
      }
      else
      {
        schedule(1000,0,katsoMihinRuutuunVoiLiikkua); 
      }
   }
}

   
      
}

function moveNappula(%pos, %boardpos) 
{
   %nappula = $nappulaSprite[game.getNappulaSelected()];

   %nappula.facing = facing( %nappula.positionOnBoard, %boardpos );
   %nappula.moveto(%pos,10, true, true, true);
   
   setNappulaState( %nappula, $olentoWalkState[ %nappula.creatureID ] );
    $fxfoot = playArchonSound(fxFootStep);
}

function facing(%pos1, %pos2) {

   %fromx = getWord(%pos1,0);
   %fromy = getWord(%pos1,1);
   %targetx = getWord(%pos2,0);
   %targety = getWord(%pos2,1);
   
   if (%targetx > %fromx) 
   {
      return "e";
   } 
   if (%targetx < %fromx)
   {
      return "w";
   }
   if (%targety > %fromy) 
   {
      return "s";
   } 
   if (%targety < %fromy)
   {
      return "n";
   } 
   if (%targety == %fromy && %targetx == %fromx)
   {
      return "";
   }

}

function InitAnimStates() {
   $nappulaNimi = "warrior archer cleric paladin skeletonwarrior skeletonarcher dreadzombie ancientghost";

   for (%i=0; %i<8; %i++)
   {
      %sana = getword($nappulaNimi, %i);
      $olentoStandState[%i] = new ScriptObject() {};
      $olentoStandState[%i].anim = %sana @ "StoppedAnimation_";
      $olentoStandState[%i].restore = 0;
      $olentoStandState[%i].name = "stopped";

      $olentoWalkState[%i] = new ScriptObject() {}; 
      $olentoWalkState[%i].anim = %sana @ "WalkingAnimation_";
      $olentoWalkState[%i].name = "walking";
      $olentoWalkState[%i].restore = 0;

      $olentoBeenHitState[%i] = new ScriptObject() {}; 
      $olentoBeenHitState[%i].anim = %sana @ "BeenHitAnimation_";
      $olentoBeenHitState[%i].restore = 0;
      $olentoBeenHitState[%i].name = "beenhit";

      $olentoAttackState[%i] = new ScriptObject() {}; 
      $olentoAttackState[%i].anim = %sana @ "AttackAnimation_";
      $olentoAttackState[%i].restore = 0;
      $olentoAttackState[%i].name = "attack";

      $olentoDieState[%i] = new ScriptObject() {}; 
      $olentoDieState[%i].anim = %sana @ "TippingOverAnimation_";
      $olentoDieState[%i].restore = 0;
      $olentoDieState[%i].name = "die";
   }
   
}



function setNappulaState(%nappula,%state)
{
   
   if (!%nappula || !%state)
   {
      endgame();
   }
   
   echo("");
   echo("param: %nappula, %state=" @ %nappula @ ", " @ %state @ "(" @ %state.name @ ")");
   echo("setnappulastate, nappulaindex=" @ %nappula.index);
   
   %nappula.state = %state;
   %restore = %nappula.state.restore;
   %playanim = %state.anim @ %nappula.facing;
   
   echo("playanim, nappulaindex=" @ %playanim);
   echo("");
   
   %nappula.playAnimation(%playanim, %restore);
   %nappula.setFrameChangeCallback(1);
   
}

function vahingoitaNappula(%nappula) {
   echo("vahingoitaNappula:" SPC %nappula);
   %nappula.hp = %nappula.hp - 1;
   
   if (%nappula.hp <= 0) {
               
               %nappula.alive = 0;
               //echo(%nappula SPC "(" @ %nappula.index @ ") kuoli");
               schedule(100,0,"setNappulaState", %nappula, $olentoDieState[ %nappula.creatureID ]  );
echo("setNappulaState(" @ %nappula @ "," @ $olentoDieState[ %nappula.creatureID ] @ ")");
               schedule(500,0,piilotaveri);
               blood.setPosition(%nappula.getPosition());
               $soitadie = playArchonSound(fxDie);

               $nappulaSprite[game.getNappulaSelected()].hasMoved = 1;
               $nappulaSprite[game.getNappulaSelected()].hasDoneAction = 1;

     
               $board.obstacles[%nappula.boardX, %nappula.boardY]=0;
               $board.nappulat[%nappula.boardX, %nappula.boardY]=0;
     
               
               poistaNappulanVuoro(%nappula.index);
               //seuraavaVuoro();
              
               if (game.getState() != $gameEndState) 
               {
                  katsoMihinRuutuunVoiLiikkua();
               }
            
   
   }
   
   
}
