//---------------------------------------------------------------------------------------------
// Archonita - TacticalAI.cs
// AI army drafting, board tactical evaluation, threat calculation, and action decision-making.
//---------------------------------------------------------------------------------------------

function TacticalAI::onAdd(%this)
{
   %this.aiAttackEvent = 0;
   %this.aiDefenseEvent = 0;
   %this.aiMoveFollowupEvent = 0;
}

function TacticalAI::onRemove(%this)
{
   if (isEventPending(%this.aiAttackEvent))
      cancel(%this.aiAttackEvent);
   if (isEventPending(%this.aiDefenseEvent))
      cancel(%this.aiDefenseEvent);
   if (isEventPending(%this.aiMoveFollowupEvent))
      cancel(%this.aiMoveFollowupEvent);
}

function TacticalAI::createAIPieces(%this)
{
   %loop = true;
   %rangeY = BoardConfig.aiPlacementMaxY - BoardConfig.aiPlacementMinY + 1;
   %lx = getRandom(BoardConfig.width - 1);
   %ly = getRandom(%rangeY - 1) + BoardConfig.aiPlacementMinY;

   while (%loop)
   {
      %randomPieceLevel = getRandom(3) + 1; // [1-4]
      if (HeroGroup.getObject(1).getCreaturePoints() >= %randomPieceLevel) {
         while ($board.obstacles[%lx, %ly] != 0)
         {
            %lx = getRandom(BoardConfig.width - 1);
            %ly = getRandom(%rangeY - 1) + BoardConfig.aiPlacementMinY;
         } 
         spawnPieceOnBoard(%randomPieceLevel, %lx, %ly, 1);         
      }
      
      if (HeroGroup.getObject(1).getCreaturePoints() < 1)
      {
         %loop = false;
      }
   }
}

function TacticalAI::evaluateTacticalMoves(%this)
{
   deleteGrids(0);
  
   %pieceId = game.getSelectedPiece();
   %hero = HeroGroup.getObject(game.getActivePlayer());

   if (%pieceId > 0) {
      %foundTarget = ""; 
      %pos = $nappulaSprite[%pieceId].positionOnBoard;
      %posx = getword(%pos, 0);
      %posy = getword(%pos, 1);
      %north = 0;
      %south = 0;
      %west = 0;
      %east = 0;

      if ($nappulaSprite[%pieceId].hasMoved == 0) 
      {
         // Can piece move North?
         if (%north == 0 && %posy > 0) {
            if ($board.obstacles[%posx, %posy - 1] == 0) 
            {
               createMovementGrid(%posx, %posy - 1, 0);
               %north = 1;
            }
         }

         // Can piece move South?
         if (%south == 0 && %posy < (BoardConfig.height - 1)) {
            if ($board.obstacles[%posx, %posy + 1] == 0)  
            {
               createMovementGrid(%posx, %posy + 1, 1);
               %south = 1;
            }
         }

         // Can piece move West?
         if (%west == 0 && %posx > 0) {
            if ($board.obstacles[%posx - 1, %posy] == 0)  
            {
               createMovementGrid(%posx - 1, %posy, 2);
               %west = 1;
            }
         }
      
         // Can piece move East?
         if (%east == 0 && %posx < (BoardConfig.width - 1)) {
            if ($board.obstacles[%posx + 1, %posy] == 0)  
            {
               createMovementGrid(%posx + 1, %posy, 3);
               %east = 1;
            }
         }
      }

      if ($nappulaSprite[%pieceId].hasDoneAction == 0) 
      {
         if ($nappulaSprite[%pieceId].hasMoved == 1) {
            selectPiece(%pieceId);
         }
      
         // Missile ranged targeting along straight horizontal and vertical lines
         if ($nappulaSprite[%pieceId].missile == 1) {
            for (%x = 0; %x < BoardConfig.width; %x++) {
               if ($board.nappulat[%x, %posy] > 0) {
                  if ($nappulaSprite[$board.nappulat[%x, %posy]].groupid == game.getActiveOpponent()) {
                     createAttackGrid(%x, %posy, 4 + %x);
                     %foundTarget = %x SPC %posy;
                  }
               }
            }
            for (%y = 0; %y < BoardConfig.height; %y++) {
               if ($board.nappulat[%posx, %y] > 0) {
                  if ($nappulaSprite[$board.nappulat[%posx, %y]].groupid == game.getActiveOpponent()) {
                     createAttackGrid(%posx, %y, 8 + %y);
                     %foundTarget = %posx SPC %y;
                  }
               }
            }
         }
         
         // Clear threat matrix
         for (%x = 0; %x < BoardConfig.width; %x++)
         {
            for (%y = 0; %y < BoardConfig.height; %y++)
            {
               $board.threat[%x, %y] = 0;
               $board.vaara[%x, %y] = 0;
            }
         }
         
         // Evaluate if piece is in danger from opponent ranged units
         %inDangerHorizontal = 0;
         %inDangerVertical = 0;
         if ($nappulaSprite[%pieceId].hasMoved == 0 && game.getActivePlayer() == 1) {
            for (%x = 0; %x < BoardConfig.width; %x++) {
               if ($board.nappulat[%x, %posy] > 0) {
                  if ($nappulaSprite[$board.nappulat[%x, %posy]].groupid == game.getActiveOpponent()
                     && $nappulaSprite[$board.nappulat[%x, %posy]].missile == "1") {
                     %inDangerHorizontal = 1;
                  }
               }
            }
            for (%y = 0; %y < BoardConfig.height; %y++) {
               if ($board.nappulat[%posx, %y] > 0) {
                  if ($nappulaSprite[$board.nappulat[%posx, %y]].groupid == game.getActiveOpponent()
                     && $nappulaSprite[$board.nappulat[%posx, %y]].missile == "1") {
                     %inDangerVertical = 1;
                  }
               }
            }
         }
      
         // Melee attack opportunities: North
         if (%north == 0 && %posy > 0) {
            if ($board.obstacles[%posx, %posy - 1] == 255 && $nappulaSprite[$board.nappulat[%posx, %posy - 1]].groupid == game.getActiveOpponent()) 
            {
               createAttackGrid(%posx, %posy - 1, 0);
               %foundTarget = %posx SPC %posy - 1;
            }
         }

         // Melee attack opportunities: South
         if (%south == 0 && %posy < (BoardConfig.height - 1)) {
            if ($board.obstacles[%posx, %posy + 1] == 255 && $nappulaSprite[$board.nappulat[%posx, %posy + 1]].groupid == game.getActiveOpponent())  
            {
               createAttackGrid(%posx, %posy + 1, 1);
               %foundTarget = %posx SPC %posy + 1;
            }
         }

         // Melee attack opportunities: West
         if (%west == 0 && %posx > 0) {
            if ($board.obstacles[%posx - 1, %posy] == 255 && $nappulaSprite[$board.nappulat[%posx - 1, %posy]].groupid == game.getActiveOpponent())  
            {
               createAttackGrid(%posx - 1, %posy, 2);
               %foundTarget = %posx - 1 SPC %posy;
            }
         }
      
         // Melee attack opportunities: East
         if (%east == 0 && %posx < (BoardConfig.width - 1)) {
            if ($board.obstacles[%posx + 1, %posy] == 255 && $nappulaSprite[$board.nappulat[%posx + 1, %posy]].groupid == game.getActiveOpponent())  
            {
               createAttackGrid(%posx + 1, %posy, 3);
               %foundTarget = %posx + 1 SPC %posy;
            }
         }
      }
      
      // Tactical AI Decision Tree
      if (game.getActivePlayer() == 1) 
      {
         if (getWordCount(%foundTarget) > 0) {
            dumpBoardDebug();
            %targetEnemy = $nappulaSprite[$board.nappulat[getWord(%foundTarget, 0), getWord(%foundTarget, 1)]];
            echo("Target tile: (" @ getWord(%foundTarget, 0) SPC getWord(%foundTarget, 1) @ ")");
            echo("Target enemy: " @ %targetEnemy);
            %this.aiAttackEvent = schedule(500, 0, "doAttack", %targetEnemy, game.getTurn());
            %this.aiMoveFollowupEvent = schedule(1500, 0, "evaluateTacticalMoves");
         } else {
            if ($nappulaSprite[%pieceId].hasMoved == 0) 
            {
               if (%inDangerHorizontal == 1 || %inDangerVertical == 1) 
               {
                  for (%x = 0; %x < BoardConfig.width; %x++) {
                     for (%y = 0; %y < BoardConfig.height; %y++) {
                        // Calculate danger points per tile
                        for (%xx = 0; %xx < BoardConfig.width; %xx++) {
                           if ($nappulaSprite[$board.nappulat[%xx, %y]].groupid == game.getActiveOpponent())
                           {
                              if ($nappulaSprite[$board.nappulat[%xx, %y]].missile == 1)
                              {
                                 %value = t2dVectorDistance(%x SPC "0", %xx SPC "0");
                                 if (%value == 1)
                                 {
                                    %value = -1;
                                 }

                                 $board.threat[%x, %y] = $board.threat[%x, %y] + %value;
                                 $board.vaara[%x, %y] = $board.threat[%x, %y];
                              
                                 if ($board.obstacles[%xx, %y - 1] == 0)
                                 {
                                    $board.threat[%x, %y - 1] = $board.threat[%x, %y - 1] + %value;
                                    $board.vaara[%x, %y - 1] = $board.threat[%x, %y - 1];
                                 }
                                 else if ($board.obstacles[%xx, %y + 1] == 0)
                                 {
                                    $board.threat[%x, %y + 1] = $board.threat[%x, %y + 1] + %value;
                                    $board.vaara[%x, %y + 1] = $board.threat[%x, %y + 1];
                                 }
                              }
                           }
                        }

                        for (%yy = 0; %yy < BoardConfig.height; %yy++) 
                        {
                           if ($nappulaSprite[$board.nappulat[%x, %yy]].groupid == game.getActiveOpponent())
                           {
                              if ($nappulaSprite[$board.nappulat[%x, %yy]].missile == 1)
                              {
                                 %value = t2dVectorDistance(%y SPC "0", %yy SPC "0");
                                 if (%value == 1)
                                 {
                                    %value = -1;
                                 }

                                 $board.threat[%x, %y] = $board.threat[%x, %y] + %value;
                                 $board.vaara[%x, %y] = $board.threat[%x, %y];
                                 
                                 if ($board.obstacles[%x - 1, %yy] == 0)
                                 {
                                    $board.threat[%x - 1, %y] = $board.threat[%x - 1, %y] + %value;
                                    $board.vaara[%x - 1, %y] = $board.threat[%x - 1, %y];
                                 }
                                 else if ($board.obstacles[%x + 1, %yy] == 0)
                                 {
                                    $board.threat[%x + 1, %y] = $board.threat[%x + 1, %y] + %value;
                                    $board.vaara[%x + 1, %y] = $board.threat[%x + 1, %y];
                                 }
                              }
                           }
                        }
                     }
                  }
                  
                  %southThreat = $board.threat[%posx, %posy + 1]; 
                  %northThreat = $board.threat[%posx, %posy - 1]; 
                  %eastThreat  = $board.threat[%posx + 1, %posy]; 
                  %westThreat  = $board.threat[%posx - 1, %posy]; 
                  
                  if (%south == 1 && %north == 1)
                  {
                     if (%southThreat < %northThreat)
                     {
                        %north = 0;
                     } 
                     else if (%southThreat > %northThreat)
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
                     if (%westThreat < %eastThreat)
                     {
                        %east = 0;
                     } 
                     else if (%westThreat > %eastThreat)
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
                     if (%southThreat < %eastThreat)
                     {
                        %east = 0;
                     } 
                     else if (%southThreat > %eastThreat)
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
                     if (%southThreat < %westThreat)
                     {
                        %west = 0;
                     } 
                     else if (%southThreat > %westThreat)
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
                     if (%northThreat < %eastThreat)
                     {
                        %east = 0;
                     } 
                     else if (%northThreat > %eastThreat)
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
                     if (%northThreat < %westThreat)
                     {
                        %west = 0;
                     } 
                     else if (%northThreat > %westThreat)
                     {
                        %west = 0;
                     }
                     else
                     {
                        %north = 0;
                     }
                  }
               
                  if (%south == 1) 
                  {
                     %hero.setSelectedTile(%posx SPC %posy + 1);
                     doMovement(%posx, %posy + 1);
                  } 
                  else if (%north == 1)
                  {
                     %hero.setSelectedTile(%posx SPC %posy - 1);
                     doMovement(%posx, %posy - 1);
                  }
                  else if (%west == 1) 
                  {
                     %hero.setSelectedTile(%posx - 1 SPC %posy);
                     doMovement(%posx - 1, %posy);
                  }
                  else if (%east == 1)
                  {
                     %hero.setSelectedTile(%posx + 1 SPC %posy);
                     doMovement(%posx + 1, %posy);
                  }
                  else
                  {
                     %this.aiDefenseEvent = schedule(500, 0, "doDefence", game.getTurn());
                  }
               }
               else
               {
                  %this.aiDefenseEvent = schedule(500, 0, "doDefence", game.getTurn());
               }
            }
            else
            {
               %this.aiDefenseEvent = schedule(500, 0, "doDefence", game.getTurn());
            }
         }
      } else {
         if ($nappulaSprite[%pieceId].hasMoved == 1) {
            if ($nappulaSprite[%pieceId].hasDoneAction == 1) 
            {
               %this.aiDefenseEvent = schedule(1000, 0, "doDefence", game.getTurn());
            } else {
               if (getWordCount(%foundTarget) == 0) 
               {
                  %this.aiDefenseEvent = schedule(1000, 0, "doDefence", game.getTurn());
               }
            }
         } else {
            if ($nappulaSprite[%pieceId].hasDoneAction == 1) 
            {
               if (%north == 0 && %south == 0 && %west == 0 && %east == 0) 
               {
                  %this.aiDefenseEvent = schedule(1000, 0, "doDefence", game.getTurn());
               }
            } 
            else if ($nappulaSprite[%pieceId].hasDoneAction == 0 && $nappulaSprite[%pieceId].hasMoved == 0)
            {
               if (!%north && !%south && !%west && !%east) 
               {
                  %this.aiDefenseEvent = schedule(1000, 0, "doDefence", game.getTurn());
               }
            }
         }
      }
   }
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function createAIPieces()            { TacticalAI.createAIPieces(); }
function createAInappulat()          { TacticalAI.createAIPieces(); }
function evaluateTacticalMoves()     { TacticalAI.evaluateTacticalMoves(); }
function katsoMihinRuutuunVoiLiikkua(){ TacticalAI.evaluateTacticalMoves(); }

// Initialize Singleton
if (!isObject(TacticalAI))
{
   new ScriptObject(TacticalAI) { };
}
