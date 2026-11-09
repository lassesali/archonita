//---------------------------------------------------------------------------------------------
// Archonita - PauseMenuManager.cs
// Manages in-game Pause Menu modal, session suspension to Title Screen, and match resumption.
//---------------------------------------------------------------------------------------------

function PauseMenuManager::onAdd(%this)
{
   %this.isPaused = false;
   %this.wasCountdownRunning = false;
   %this.savedGameState = "";
   %this.savedUseWindowMouse = true;

   %this.borderProfile = "";
   %this.backdropProfile = "";
   %this.panelProfile = "";
   %this.headerProfile = "";
   %this.btnProfile = "";

   %this.backdropCtrl = "";
   %this.panelCtrl = "";
   %this.headerLabel = "";
   %this.btnResume = "";
   %this.btnOptions = "";
   %this.btnMainMenu = "";
   %this.btnQuit = "";

   %this.createProfiles();
   %this.createControls();
}

function PauseMenuManager::onRemove(%this)
{
   if (isObject(savedBoardScene))
   {
      savedBoardScene.clear();
      savedBoardScene.delete();
   }
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();
   if (isObject(%this.btnProfile))
      %this.btnProfile.delete();
   if (isObject(%this.headerProfile))
      %this.headerProfile.delete();
   if (isObject(%this.panelProfile))
      %this.panelProfile.delete();
   if (isObject(%this.backdropProfile))
      %this.backdropProfile.delete();
   if (isObject(%this.borderProfile))
      %this.borderProfile.delete();
}

function PauseMenuManager::createProfiles(%this)
{
   if (!isObject(%this.borderProfile))
   {
      %this.borderProfile = new GuiBorderProfile(PauseMenuBorderProfile)
      {
         border = 2;
         borderHL = 2;
         borderSL = 2;
         borderNA = 2;
         borderColor = "195 160 95 230";
         borderColorHL = "250 220 140 255";
         borderColorSL = "255 240 175 255";
         borderColorNA = "110 110 110 150";
         underfill = true;
      };
   }

   if (!isObject(%this.backdropProfile))
   {
      %this.backdropProfile = new GuiControlProfile(PauseMenuBackdropProfile : GuiDefaultProfile)
      {
         fillColor = "0 0 0 155";
         border = false;
         useInput = true;
      };
   }

   if (!isObject(%this.panelProfile))
   {
      %this.panelProfile = new GuiControlProfile(PauseMenuPanelProfile : GuiDefaultProfile)
      {
         fillColor = "16 20 30 242";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }

   if (!isObject(%this.headerProfile))
   {
      %this.headerProfile = new GuiControlProfile(PauseMenuHeaderProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 20;
         fontColor = "255 225 145 255";
         align = "center";
         vAlign = "middle";
         fillColor = "0 0 0 0";
         border = false;
         useInput = false;
      };
   }

   if (!isObject(%this.btnProfile))
   {
      %this.btnProfile = new GuiControlProfile(PauseMenuButtonProfile : GuiDefaultProfile)
      {
         fontType = "Arial";
         fontSize = 15;
         fillColor = "32 42 62 235";
         fillColorHL = "52 68 98 250";
         fillColorSL = "70 92 130 255";
         fillColorNA = "28 30 36 150";
         fontColor = "245 235 205 255";
         fontColorHL = "255 250 220 255";
         fontColorSL = "255 255 255 255";
         fontColorNA = "125 125 125 160";
         align = "center";
         vAlign = "middle";
         borderLeft = %this.borderProfile;
         borderRight = %this.borderProfile;
         borderTop = %this.borderProfile;
         borderBottom = %this.borderProfile;
         useInput = true;
      };
   }
}

function PauseMenuManager::createControls(%this)
{
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.delete();

   %mainW = 1024;
   %mainH = 768;
   if (isObject(mainScreenGui))
   {
      %mainW = getWord(mainScreenGui.extent, 0);
      %mainH = getWord(mainScreenGui.extent, 1);
   }

   %panelW = 320;
   %panelH = 264;
   %panelX = mFloor((%mainW - %panelW) / 2);
   %panelY = mFloor((%mainH - %panelH) / 2);

   %this.backdropCtrl = new GuiControl(PauseMenuBackdrop)
   {
      Profile = %this.backdropProfile;
      HorizSizing = "width";
      VertSizing = "height";
      Position = "0 0";
      Extent = %mainW SPC %mainH;
      Visible = "0";
   };

   %this.panelCtrl = new GuiControl(PauseMenuPanel)
   {
      Profile = %this.panelProfile;
      HorizSizing = "center";
      VertSizing = "center";
      Position = %panelX SPC %panelY;
      Extent = %panelW SPC %panelH;
      Visible = "1";
   };

   %this.headerLabel = new GuiControl()
   {
      Profile = %this.headerProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "10 16";
      Extent = "300 32";
      Text = "GAME PAUSED";
   };

   %this.btnResume = new GuiButtonCtrl(PauseMenuResumeBtn)
   {
      Profile = %this.btnProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "30 62";
      Extent = "260 38";
      Text = "RESUME GAME";
      Command = "$PauseMenuManager.onResumeClicked();";
   };

   %this.btnOptions = new GuiButtonCtrl(PauseMenuOptionsBtn)
   {
      Profile = %this.btnProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "30 110";
      Extent = "260 38";
      Text = "OPTIONS";
      Command = "$PauseMenuManager.onOptionsClicked();";
   };

   %this.btnMainMenu = new GuiButtonCtrl(PauseMenuTitleBtn)
   {
      Profile = %this.btnProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "30 158";
      Extent = "260 38";
      Text = "RETURN TO TITLE SCREEN";
      Command = "$PauseMenuManager.onMainMenuClicked();";
   };

   %this.btnQuit = new GuiButtonCtrl(PauseMenuQuitBtn)
   {
      Profile = %this.btnProfile;
      HorizSizing = "width";
      VertSizing = "bottom";
      Position = "30 206";
      Extent = "260 38";
      Text = "QUIT GAME";
      Command = "$PauseMenuManager.onQuitClicked();";
   };

   %this.panelCtrl.add(%this.headerLabel);
   %this.panelCtrl.add(%this.btnResume);
   %this.panelCtrl.add(%this.btnOptions);
   %this.panelCtrl.add(%this.btnMainMenu);
   %this.panelCtrl.add(%this.btnQuit);
   %this.backdropCtrl.add(%this.panelCtrl);

   if (isObject(mainScreenGui))
   {
      mainScreenGui.add(%this.backdropCtrl);
      mainScreenGui.pushToBack(%this.backdropCtrl);
   }
}

function PauseMenuManager::reanchor(%this)
{
   if (!isObject(%this.backdropCtrl) || !isObject(mainScreenGui))
      return;

   %mainW = getWord(mainScreenGui.extent, 0);
   %mainH = getWord(mainScreenGui.extent, 1);
   %this.backdropCtrl.Position = "0 0";
   %this.backdropCtrl.Extent = %mainW SPC %mainH;

   if (isObject(%this.panelCtrl))
   {
      %panelW = getWord(%this.panelCtrl.extent, 0);
      %panelH = getWord(%this.panelCtrl.extent, 1);
      %this.panelCtrl.Position = mFloor((%mainW - %panelW) / 2) SPC mFloor((%mainH - %panelH) / 2);
   }

   if (%this.backdropCtrl.isVisible())
      mainScreenGui.pushToBack(%this.backdropCtrl);
}

function PauseMenuManager::isBoardState(%this, %state)
{
   if (%state $= "")
      %state = getGameState();

   if (%state == $gamePreBattleState ||
       %state == $gamePreBattleTimerState ||
       %state == $gameTimerBattleState ||
       %state == $gameBattleState)
   {
      return true;
   }
   return false;
}

function PauseMenuManager::togglePauseMenu(%this)
{
   if (isObject($OptionsMenuGui) && $OptionsMenuGui.isVisible())
   {
      $OptionsMenuGui.show(false);
      return;
   }

   if (!%this.isBoardState(getGameState()) && !%this.isPaused)
      return;

   if (%this.isPaused)
      %this.onResumeClicked();
   else
      %this.openPauseMenu();
}

function PauseMenuManager::openPauseMenu(%this)
{
   if (!isObject(%this.backdropCtrl))
      %this.createControls();

   %this.isPaused = true;

   if (isObject(TurnManager) && isEventPending(TurnManager.turnTimerEvent))
   {
      cancel(TurnManager.turnTimerEvent);
      TurnManager.turnTimerEvent = 0;
      %this.wasCountdownRunning = true;
   }

   if (isObject(t2dScene))
      t2dScene.setScenePause(true);

   %this.reanchor();
   %this.backdropCtrl.setVisible(true);
   mainScreenGui.pushToBack(%this.backdropCtrl);
}

function PauseMenuManager::onResumeClicked(%this)
{
   playArchonSound(fxSword);
   %this.isPaused = false;

   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.setVisible(false);

   if (isObject(t2dScene))
      t2dScene.setScenePause(false);

   if (%this.wasCountdownRunning && isObject(TurnManager))
   {
      %this.wasCountdownRunning = false;
      TurnManager.turnTimerEvent = TurnManager.schedule(1000, "updateCountdownTimer");
   }
}

function PauseMenuManager::onOptionsClicked(%this)
{
   playArchonSound(fxSword);
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.show(true);
}

function PauseMenuManager::onMainMenuClicked(%this)
{
   playArchonSound(fxSword);
   %this.suspendAndReturnToTitle();
}

function PauseMenuManager::onQuitClicked(%this)
{
   playArchonSound(fxSword);
   %this.isPaused = false;
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.setVisible(false);
   %this.discardSavedSession();
   creditsScreen();
}

function PauseMenuManager::suspendAndReturnToTitle(%this)
{
   %this.isPaused = false;
   if (isObject(%this.backdropCtrl))
      %this.backdropCtrl.setVisible(false);

   %this.savedGameState = getGameState();
   if (isObject(sceneWindow2D))
      %this.savedUseWindowMouse = sceneWindow2D.getUseWindowMouseEvents();

   // Pause and rename active board scene so loading mainmenu.t2d does not destroy it
   if (isObject(mouseTrail))
      mouseTrail.setName("savedBoardMouseTrail");

   if (isObject(t2dScene))
   {
      t2dScene.setScenePause(true);
      t2dScene.setName("savedBoardScene");
   }

   if (isObject($BoardAudioHud))
      $BoardAudioHud.show(false);

   $GameSession::hasActiveSession = true;
   mainmenuScreen();
}

function PauseMenuManager::hasSavedSession(%this)
{
   return ($GameSession::hasActiveSession && isObject(savedBoardScene));
}

function PauseMenuManager::resumeToBoardFromTitle(%this)
{
   if (!%this.hasSavedSession())
      return false;

   if ($bubbleSchedule !$= "")
   {
      cancel($bubbleSchedule);
      $bubbleSchedule = "";
   }
   if ($heroSelectSchedule !$= "")
   {
      cancel($heroSelectSchedule);
      $heroSelectSchedule = "";
   }

   if (isObject($TitleMenuManager))
      $TitleMenuManager.show(false);
   showVersionWatermark(false);

   // Delete temporary mainmenu scene before restoring savedBoardScene
   if (isObject(t2dScene) && t2dScene.getId() != savedBoardScene.getId())
   {
      t2dScene.clear();
      t2dScene.delete();
   }

   savedBoardScene.setName("t2dScene");
   if (isObject(savedBoardMouseTrail))
      savedBoardMouseTrail.setName("mouseTrail");

   Canvas.setContent(mainScreenGui);
   Canvas.setCursor(DefaultCursor);
   Canvas.showCursor();

   sceneWindow2D.setScene(t2dScene);
   if (isObject($ResolutionManager))
   {
      $ResolutionManager.applyCameraAspect(sceneWindow2D);
      $ResolutionManager.adaptSceneBackground();
   }

   t2dScene.setScenePause(false);
   initMouse();

   if (%this.savedGameState !$= "")
      setGameState(%this.savedGameState);
   else
      setGameState($gameBattleState);

   sceneWindow2D.setUseWindowMouseEvents(%this.savedUseWindowMouse);

   if (%this.wasCountdownRunning && isObject(TurnManager))
   {
      %this.wasCountdownRunning = false;
      TurnManager.turnTimerEvent = TurnManager.schedule(1000, "updateCountdownTimer");
   }

   if (isObject(BoardManager))
      BoardManager.bindBoardHotkeys();

   if (isObject($BoardAudioHud))
      $BoardAudioHud.show(true);

   return true;
}

function PauseMenuManager::discardSavedSession(%this)
{
   if (isObject(savedBoardMouseTrail))
      savedBoardMouseTrail.setName("mouseTrail");

   if (isObject(savedBoardScene))
   {
      savedBoardScene.clear();
      savedBoardScene.delete();
   }

   %this.wasCountdownRunning = false;
   %this.savedGameState = "";
   $GameSession::hasActiveSession = false;
}

if (!isObject($PauseMenuManager))
{
   $PauseMenuManager = new ScriptObject(PauseMenuManager) { };
}
