//---------------------------------------------------------------------------------------------
// Archonita - BoardManager.cs
// Board creation, tile grid coordinates, obstacles, piece placement, and visual markers.
//---------------------------------------------------------------------------------------------

if (!isObject(BoardConfig))
{
   new ScriptObject(BoardConfig)
   {
      width = 4;
      height = 4;
      tileSize = 8;
      
      aiPlacementMinY = 0;
      aiPlacementMaxY = 1;
      playerPlacementMinY = 2;
      playerPlacementMaxY = 3;
      
      unitOffsetY = 2;
      obstacleOffsetY = 4;
   };
}

//---------------------------------------------------------------------------------------------
// BoardManager Lifecycle & Methods
//---------------------------------------------------------------------------------------------
function BoardManager::onAdd(%this)
{
   %this.pieces = new SimSet();
   %this.movementGrids = new SimSet();
}

function BoardManager::onRemove(%this)
{
   if (isObject(%this.pieces))
      %this.pieces.delete();
   if (isObject(%this.movementGrids))
      %this.movementGrids.delete();
}

function BoardManager::showBoardScreen(%this)
{
   setGameState($gamePreBattleState);
   dismountobject();
   startGame("Archon/data/levels/board.t2d");  
   
   InitNewGame();
}

function BoardManager::initBoard(%this)
{
   %this.dumpBoardDebug();
   %this.createFloor();
   
   $board = new ScriptObject() { };

   for (%i = 0; %i < BoardConfig.width; %i++) {
      for (%j = 0; %j < BoardConfig.height; %j++) {
         $board.nappulat[%i, %j] = 0;
         $board.pieces[%i, %j] = 0;
         $board.obstacles[%i, %j] = 0;
      }
   }

   %this.dumpBoardDebug();
   
   %obsx = getRandom(BoardConfig.width - 1);
   %obsy = getRandom(BoardConfig.height - 1);
   
   for (%bx = 0; %bx < BoardConfig.width; %bx++) {
      for (%by = 0; %by < BoardConfig.height; %by++) {
         if (%bx == %obsx && %by == %obsy) {
            $board.obstacles[%bx, %by] = getRandom(2) + 1;
            %this.createObstacle($board.obstacles[%bx, %by], %bx, %by);
         } else {
            $board.obstacles[%bx, %by] = 0;
         }
      }
   }
   
   if (isObject($floor[0, 2])) $floor[0, 2].setFrame(1);
   if (isObject($floor[1, 0])) $floor[1, 0].setFrame(2);
}

function BoardManager::createFloor(%this)
{
   for (%cfx = 0; %cfx < BoardConfig.width; %cfx++) 
   {
      for (%cfy = 0; %cfy < BoardConfig.height; %cfy++)
      {
         $floor[%cfx, %cfy] = new Sprite() { scene = t2dScene; };
         $floor[%cfx, %cfy].setImageMap("boardgridImageMap");
         $floor[%cfx, %cfy].setUseMouseEvents("1");
         $floor[%cfx, %cfy].setPosition(boardToWorld(%cfx, %cfy));
         $floor[%cfx, %cfy].setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
         $floor[%cfx, %cfy].setLayer("7");
         $floor[%cfx, %cfy].setCollisionActiveSend("1");
         $floor[%cfx, %cfy].setCollisionActiveReceive("1");
      }
   }
}

function BoardManager::createObstacle(%this, %obstacletype, %ox, %oy)
{
   if (%obstacletype == 1) 
   {
      $floor[%ox, %oy].setVisible(false);
   } 
   else 
   {
      $obstacleSprite = new Sprite() { scene = t2dScene; };
      $obstacleSprite.setImageMap("obstaclesImageMap");
      %posX = $floor[%ox, %oy].getPositionX();
      %posY = $floor[%ox, %oy].getPositionY();
      $obstacleSprite.setPosition(%posX, %posY + BoardConfig.obstacleOffsetY);
      $obstacleSprite.setSize(BoardConfig.tileSize SPC (BoardConfig.tileSize * 2));
      $obstacleSprite.mountID = "180";
      $obstacleSprite.setFrame(%obstacletype - 2);
      $obstacleSprite.setVisible(true);
      $obstacleSprite.setLayer("1");
      $obstacleSprite.setUseMouseEvents(0);
      $obstacleSprite.isSelectable = false;
   }
}

function BoardManager::updatePlacementGrids(%this)
{
   if (HeroGroup.getObject(0).getCreaturePoints() > 0) {
      for (%ux = 0; %ux < BoardConfig.width; %ux++) 
      {
         for (%uy = BoardConfig.playerPlacementMinY; %uy <= BoardConfig.playerPlacementMaxY; %uy++) 
         {
            if ($board.obstacles[%ux, %uy] == 0) {
               $green[%ux, %uy] = new Sprite() { scene = t2dScene; };
               $green[%ux, %uy].setImageMap("boardgridgreenImageMap");
               $green[%ux, %uy].setPosition(boardToWorld(%ux, %uy));
               $green[%ux, %uy].setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
               $green[%ux, %uy].setFrame("0");
               $green[%ux, %uy].setVisible("True");
               $green[%ux, %uy].setLayer("2");
               $green[%ux, %uy].setBlendAlpha("0.9");
               $green[%ux, %uy].setUseMouseEvents(0);
               $green[%ux, %uy].isSelectable = false;  
               $green[%ux, %uy].isValintakehys = true;
               $green[%ux, %uy].isSelectionFrame = true;
               $green[%ux, %uy].positionOnBoard = %ux SPC %uy;
            }
         }
      }
   }
}

function BoardManager::createSetupPieces(%this)
{
   $unitNames = "warrior archer cleric paladin skeletonwarrior skeletonarcher dreadzombie ancientghost";
   $nappulaNimi = $unitNames;

   for (%i = 0; %i < 4; %i++)
   {
      $setupNappulaSprite[%i] = new Sprite() { scene = t2dScene; };
      $setupNappulaSprite[%i].setImageMap(getWord($unitNames, %i) @ "StoppedImageMap");
      $setupNappulaSprite[%i].setPosition(0, 0);
      $setupNappulaSprite[%i].setSize("8 8");
      $setupNappulaSprite[%i].setBlendAlpha("0.5");
      $setupNappulaSprite[%i].setFrame(4);
      $setupNappulaSprite[%i].setVisible(false);
      $setupNappulaSprite[%i].setLayer("0");
      $setupNappulaSprite[%i].setUseMouseEvents(0);
      $setupNappulaSprite[%i].isSelectable = false;
   }
}

function BoardManager::deleteSetupPieces(%this)
{
   for (%i = 0; %i < 4; %i++)
   {
      if (isObject($setupNappulaSprite[%i]))
         $setupNappulaSprite[%i].safeDelete();
   }
   for (%i = 0; %i < 12; %i++)
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

function BoardManager::updateCreatureIcons(%this)
{
   for (%i = 0; %i < 12; %i++) {
      if (isObject($icon[%i]))
         $icon[%i].safeDelete();
      if (isObject($grid[%i]))
         $grid[%i].safeDelete();
      if (isObject($gridback[%i]))
         $gridback[%i].safeDelete();
      if (isObject($numb[%i]))
         $numb[%i].safeDelete();
   }

   if (HeroGroup.getObject(0).getCreaturePoints() > 0) {
      %x = "-44";
      %lvl1 = HeroGroup.getObject(0).getCreaturePoints();
      %lvl2 = mFloor(HeroGroup.getObject(0).getCreaturePoints() / 2);
      %lvl3 = mFloor(HeroGroup.getObject(0).getCreaturePoints() / 3);
      %lvl4 = mFloor(HeroGroup.getObject(0).getCreaturePoints() / 4);

      for (%i = 0; %i < 12; %i++) {
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
         $grid[%i].mount($icon[%i], 0, 0, 0, true, true, true, true);

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
         $numb[%i].mount($gridback[%i], 0.5, -0.4, 0, true, true, true, true);
         $numb[%i].index = %i;

         %x = %x + 8;

         if (%lvl1 == 0 && %lvl2 == 0 && %lvl3 == 0 && %lvl4 == 0) 
         {
            %i = 13;
         }
      }
   }
}

function BoardManager::iconSendToFront(%this, %index)
{
   $icon[%index].setLayer("1");
   $grid[%index].setLayer("0");
}

function BoardManager::updateCreaturePoints(%this)
{
   olentopisteetcounter.setFrame(HeroGroup.getObject(0).getCreaturePoints());
}

function BoardManager::updatePlacementHUD(%this)
{
   for (%x = 0; %x < BoardConfig.width; %x++) {
      for (%y = BoardConfig.playerPlacementMinY; %y <= BoardConfig.playerPlacementMaxY; %y++) {
         if ($green[%x, %y]) 
         {
            if (isObject($green[%x, %y]))
               $green[%x, %y].safeDelete();     
         }
      }
   }

   %this.updateCreaturePoints();
   %this.updatePlacementGrids();
   %this.updateCreatureIcons();

   if (HeroGroup.getObject(0).getCreaturePoints() < 1) {
      schedule(500, 0, dealCards);
      heroicon.setPosition("-40 -24");
      heroicongrid.setPosition("-40 -24");

      game.setGameTimer(5);
      schedule(1000, 0, updateCountdownTimer);
      secondText.setVisible(true);
      secondCounter.setFrame(game.getGameTimer());
      secondCounter.setVisible(true);
      sijoitaText.setVisible(false);
      olentopisteetcounter.setVisible(false);
   }
}

function BoardManager::spawnPieceOnBoard(%this, %pieceLevel, %lx, %ly, %heroid)
{
   // Verify target coordinates fall within dynamic board array bounds
   if (%lx < 0 || %lx >= BoardConfig.width || %ly < 0 || %ly >= BoardConfig.height)
   {
      echo("spawnPieceOnBoard: rejected - coordinates out of bounds (" @ %lx SPC %ly @ ")");
      return false;
   }

   // Verify tile is not occupied by an existing obstacle or piece
   if ($board.obstacles[%lx, %ly] != 0)
   {
      echo("spawnPieceOnBoard: rejected - tile occupied or obstacle (" @ %lx SPC %ly @ ")");
      return false;
   }

   // For player (%heroid == 0), verify target tile corresponds to valid active green placement rows
   if (%heroid == 0)
   {
      if (%ly < BoardConfig.playerPlacementMinY || %ly > BoardConfig.playerPlacementMaxY || !isObject($green[%lx, %ly]))
      {
         echo("spawnPieceOnBoard: rejected - not a valid green placement tile (" @ %lx SPC %ly @ ")");
         return false;
      }
   }
   
   if (%heroid == 1) 
   {
      %animindex = %pieceLevel + 3;
      %pieceLevel = %pieceLevel + 4;
      %facing = "s";
   } else 
   {
      %animindex = %pieceLevel - 1;
      %facing = "n";
   }
   
   game.addPieceCount(1);
   %j = game.getPieceCount();
   echo("countof:" @ %j);
   $board.obstacles[%lx, %ly] = 255;
   %this.dumpBoardDebug();
   $board.nappulat[%lx, %ly] = %j;
   $board.pieces[%lx, %ly] = %j;
   %this.dumpBoardDebug();

   $nappulaSprite[%j] = new Sprite() { scene = t2dScene; class = Nappula; };
   $nappulaSprite[%j].state = $olentoStandState[%animindex];

   $nappulaSprite[%j].facing = %facing;
   $nappulaSprite[%j].animationName = $nappulaSprite[%j].state.anim @ $nappulaSprite[%j].facing;
   $nappulaSprite[%j].playAnimation($nappulaSprite[%j].animationName);
   
   $nappulaSprite[%j].setPosition(boardToWorldUnit(%lx, %ly));
   $nappulaSprite[%j].setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
   $nappulaSprite[%j].setBlendAlpha("1");
   $nappulaSprite[%j].setVisible(true);
   $nappulaSprite[%j].setLayer("2");
   $nappulaSprite[%j].setUseMouseEvents(0);
   $nappulaSprite[%j].isSelectable = false;

   %i = %pieceLevel - 1; 
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
   $nappulaSprite[%j].creatureID = UnitProperties.lvl[%i] - 1 + $nappulaSprite[%j].groupid * 4;
   $nappulaSprite[%j].spells = UnitProperties.spells[%i];
   $nappulaSprite[%j].missile = UnitProperties.missile[%i];
   $nappulaSprite[%j].name = getWord($unitNames, %pieceLevel - 1);
   $nappulaSprite[%j].alive = 1;
   
   $nappulaSprite[%j].boardX = %lx;
   $nappulaSprite[%j].boardY = %ly;
   $nappulaSprite[%j].positionOnBoard = %lx SPC %ly;
   
   $nappulaSprite[%j].hasMoved = 0;
   $nappulaSprite[%j].hasDoneAction = 0;
   $nappulaSprite[%j].index = %j;
           
   // Deduct creature points from hero
   HeroGroup.getObject(%heroid).subCreaturePoints($nappulaSprite[%j].level);
   
   // Update piece count owned by hero
   HeroGroup.getObject(%heroid).addPieceCount(1);

   return true;
}

function BoardManager::selectPiece(%this, %pieceId)
{
   $nappulaselectedgrid = new Sprite() { scene = t2dScene; };
   $nappulaselectedgrid.setImageMap("boardgridgreenImageMap");
   $nappulaselectedgrid.setPosition(boardToWorld($nappulaSprite[%pieceId].BoardX, $nappulaSprite[%pieceId].BoardY));
   $nappulaselectedgrid.setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
   $nappulaselectedgrid.setFrame("1");
   $nappulaselectedgrid.setVisible("True");
   $nappulaselectedgrid.setLayer("4");
   $nappulaselectedgrid.setBlendAlpha("0.9");
   $nappulaselectedgrid.setUseMouseEvents(0);
   $nappulaselectedgrid.isSelectable = false;  
   $nappulaselectedgrid.isValintakehys = true;
   $nappulaselectedgrid.isSelectionFrame = true;
   $nappulaselectedgrid.SrcBlendFactor = "ONE_MINUS_DST_COLOR";
   $nappulaselectedgrid.positionOnBoard = $nappulaSprite[%pieceId].positionOnBoard;
}

function BoardManager::deleteGrids(%this, %i)
{
   if (%i == 1) { 
      if (isObject($nappulaselectedgrid)) {
         $nappulaselectedgrid.safeDelete();
      }
   }
   for (%j = 0; %j < 16; %j++)
   {
      if (isObject($movementgrid[%j])) {
         $movementgrid[%j].safeDelete();
      }
   }
}

function BoardManager::createAttackGrid(%this, %bx, %by, %i)
{
   $movementgrid[%i] = new Sprite() { scene = t2dScene; };
   $movementgrid[%i].setImageMap("boardgridgreenImageMap");
   $movementgrid[%i].setPosition(boardToWorld(%bx, %by));
   $movementgrid[%i].setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
   $movementgrid[%i].setFrame("2");
   $movementgrid[%i].setLayer("4");
   
   if (game.getActivePlayer() == 1) 
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
   $movementgrid[%i].isSelectionFrame = true;
   $movementgrid[%i].positionOnBoard = %bx SPC %by;
   $movementgrid[%i].defaultFrame = "2";
}

function BoardManager::createMovementGrid(%this, %bx, %by, %i)
{
   $movementgrid[%i] = new Sprite() { scene = t2dScene; };
   $movementgrid[%i].setImageMap("boardgridgreenImageMap");
   $movementgrid[%i].setPosition(boardToWorld(%bx, %by));
   $movementgrid[%i].setSize(BoardConfig.tileSize SPC BoardConfig.tileSize);
   $movementgrid[%i].setFrame("0");
   $movementgrid[%i].setVisible("True");
   $movementgrid[%i].setLayer("4");

   if (game.getActivePlayer() == 1) 
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
   $movementgrid[%i].isSelectionFrame = true;
   $movementgrid[%i].positionOnBoard = %bx SPC %by;
   $movementgrid[%i].defaultFrame = "0";
}

function BoardManager::hideGrids(%this)
{
}

function BoardManager::dumpBoardDebug(%this)
{
   echo("board.nappulat (" @ BoardConfig.width @ "x" @ BoardConfig.height @ ")");
   for (%y = 0; %y < BoardConfig.height; %y++) {
      %line = "";
      for (%x = 0; %x < BoardConfig.width; %x++) {
         %line = (%line $= "") ? $board.nappulat[%x, %y] : (%line SPC $board.nappulat[%x, %y]);
      }
      echo(%line);
   }
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function showBoardScreen()                         { BoardManager.showBoardScreen(); }
function BoardScreen()                             { BoardManager.showBoardScreen(); }
function initBoard()                               { BoardManager.initBoard(); }
function createFloor()                             { BoardManager.createFloor(); }
function createObstacle(%type, %x, %y)             { BoardManager.createObstacle(%type, %x, %y); }
function updatePlacementGrids()                    { BoardManager.updatePlacementGrids(); }
function updateGreenGrids()                        { BoardManager.updatePlacementGrids(); }
function createSetupPieces()                       { BoardManager.createSetupPieces(); }
function createSetupNappulat()                     { BoardManager.createSetupPieces(); }
function deleteSetupPieces()                       { BoardManager.deleteSetupPieces(); }
function deleteSetupNappulat()                     { BoardManager.deleteSetupPieces(); }
function updateCreatureIcons()                     { BoardManager.updateCreatureIcons(); }
function updateOlentoikonit()                      { BoardManager.updateCreatureIcons(); }
function iconSendToFront(%idx)                     { BoardManager.iconSendToFront(%idx); }
function updateCreaturePoints()                    { BoardManager.updateCreaturePoints(); }
function updateOlentopisteet()                     { BoardManager.updateCreaturePoints(); }
function updatePlacementHUD()                      { BoardManager.updatePlacementHUD(); }
function updatePisteetGridsIcons()                 { BoardManager.updatePlacementHUD(); }
function spawnPieceOnBoard(%lvl, %x, %y, %hero)    { return BoardManager.spawnPieceOnBoard(%lvl, %x, %y, %hero); }
function lisaaUusiNappulaLaudalle(%l, %x, %y, %h)  { return BoardManager.spawnPieceOnBoard(%l, %x, %y, %h); }
function selectPiece(%id)                          { BoardManager.selectPiece(%id); }
function selectedNappula(%id)                      { BoardManager.selectPiece(%id); }
function deleteGrids(%i)                           { BoardManager.deleteGrids(%i); }
function createAttackGrid(%x, %y, %i)              { BoardManager.createAttackGrid(%x, %y, %i); }
function luoAttackGrid(%x, %y, %i)                 { BoardManager.createAttackGrid(%x, %y, %i); }
function createMovementGrid(%x, %y, %i)            { BoardManager.createMovementGrid(%x, %y, %i); }
function luoLiikuGrid(%x, %y, %i)                  { BoardManager.createMovementGrid(%x, %y, %i); }
function hideGrids()                               { BoardManager.hideGrids(); }
function piilotaGrids()                            { BoardManager.hideGrids(); }
function dumpBoardDebug()                          { BoardManager.dumpBoardDebug(); }
function debuggi()                                 { BoardManager.dumpBoardDebug(); }

// Initialize Singleton
if (!isObject(BoardManager))
{
   new ScriptObject(BoardManager) { };
}
