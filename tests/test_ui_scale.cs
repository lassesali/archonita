//-----------------------------------------------------------------------------
// tests/test_ui_scale.cs
// End-to-end verification suite for the UI Scale option (75% - 150%, with 75%
// as the 1.0x baseline), OptionsMenuGui preset/step controls, 6-language
// localization, viewport-safe scaling across all UI panels/HUDs/overlays,
// zero-drift round-trip restoration, and Archon/preferences.cs persistence.
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

$uiScaleTestPath = pathConcat(getMainDotCsDir(), "tests/ui_scale_test_result.txt");
$uiScaleTestFile = new FileObject();
$uiScaleTestFile.openForWrite($uiScaleTestPath);
$uiScaleTestFile.writeLine("UI_SCALE_TEST_START");

function logScaleResult(%msg)
{
   echo(%msg);
   if (isObject($uiScaleTestFile))
      $uiScaleTestFile.writeLine(%msg);
}

function failScaleTest(%msg)
{
   logScaleResult("TEST_FAIL: " @ %msg);
   if (isObject($uiScaleTestFile))
   {
      $uiScaleTestFile.close();
      $uiScaleTestFile.delete();
   }
   quit();
}

function passScaleTest(%msg)
{
   logScaleResult("TEST_PASS: " @ %msg);
}

function assertScaleCond(%label, %cond)
{
   if (!%cond)
   {
      failScaleTest(%label);
      return;
   }
   passScaleTest(%label);
}

function makeScaleTestUnit(%idx, %groupId, %creatureId, %name, %bx, %by)
{
   if (isObject($nappulaSprite[%idx]))
      $nappulaSprite[%idx].delete();

   $nappulaSprite[%idx] = new Sprite()
   {
      scene = t2dScene;
      class = "Piece";
      superclass = "Nappula";
      Position = boardToWorldUnit(%bx, %by);
      Size = "8 8";
      SceneLayer = 2;
      index = %idx;
      groupid = %groupId;
      creatureID = %creatureId;
      name = %name;
      level = UnitProperties.lvl[%creatureId];
      hp = UnitProperties.hp[%creatureId];
      hpMAX = UnitProperties.hp[%creatureId];
      attack = UnitProperties.attack[%creatureId];
      attackMAX = UnitProperties.attack[%creatureId];
      defence = UnitProperties.defence[%creatureId];
      defenceMAX = UnitProperties.defence[%creatureId];
      mana = UnitProperties.mana[%creatureId];
      manaMAX = UnitProperties.mana[%creatureId];
      spells = UnitProperties.spells[%creatureId];
      missile = UnitProperties.missile[%creatureId];
      alive = 1;
      hasMoved = 0;
      hasDoneAction = 0;
      skipNextTurn = 0;
      facing = (%groupId == 0 ? "n" : "s");
      boardX = %bx;
      boardY = %by;
      positionOnBoard = %bx SPC %by;
   };
   return $nappulaSprite[%idx];
}

setCompanyAndProduct("Torque Game Engines", "Archonita");
ModuleDatabase.EchoInfo = false;
AssetDatabase.EchoInfo = false;

ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "library"));
ModuleDatabase.scanModules(pathConcat(getMainDotCsDir(), "Archon"));
ModuleDatabase.LoadExplicit("AppCore");

logScaleResult("==========================================");
logScaleResult("Archonita - UI Scale Verification Suite");
logScaleResult("==========================================");

// Save original preference state to restore cleanly at exit
%savedInitialScale = ($pref::UI::Scale !$= "") ? $pref::UI::Scale : 75;
%savedInitialLang = ($pref::Language !$= "") ? $pref::Language : "fi";

// 1. Default baseline & normalization/clamping
assertScaleCond("Default UI scale is 75% (1.0x relative scale factor)",
   $ResolutionManager.getUiScalePercent() == 75 &&
   mAbs($ResolutionManager.getUiScaleFactor() - 1.0) < 0.001);

assertScaleCond("normalizeUiScale clamps to [75, 150] and defaults empty to 75",
   $ResolutionManager.normalizeUiScale("") == 75 &&
   $ResolutionManager.normalizeUiScale(50) == 75 &&
   $ResolutionManager.normalizeUiScale(75) == 75 &&
   $ResolutionManager.normalizeUiScale(100) == 100 &&
   $ResolutionManager.normalizeUiScale(150) == 150 &&
   $ResolutionManager.normalizeUiScale(225) == 150);

// 2. OptionsMenuGui controls, preset/step handlers, and bounds gating
$LocalizationManager.setLanguage("en", true);
$OptionsMenuGui.show(true);
assertScaleCond("OptionsMenuGui UI Scale controls exist with 75% selected and '-' disabled at floor",
   isObject(OptionsMenuUiScaleLabel) &&
   isObject(OptionsMenuUiScaleDownBtn) &&
   isObject(OptionsMenuUiScaleUpBtn) &&
   isObject(OptionsMenuUiScaleBtn_75) &&
   isObject(OptionsMenuUiScaleBtn_100) &&
   isObject(OptionsMenuUiScaleBtn_125) &&
   isObject(OptionsMenuUiScaleBtn_150) &&
   OptionsMenuUiScaleLabel.getText() $= "UI Scale: 75%" &&
   OptionsMenuUiScaleBtn_75.Profile.getId() == $OptionsMenuGui.btnSelectedProfile.getId() &&
   OptionsMenuUiScaleBtn_100.Profile.getId() == $OptionsMenuGui.btnNormalProfile.getId() &&
   !OptionsMenuUiScaleDownBtn.isActive() &&
   OptionsMenuUiScaleUpBtn.isActive());

$OptionsMenuGui.onAdjustUiScale(5);
assertScaleCond("Step '+' adjusts UI scale 75% -> 80% and enables '-' button",
   $ResolutionManager.getUiScalePercent() == 80 &&
   OptionsMenuUiScaleLabel.getText() $= "UI Scale: 80%" &&
   OptionsMenuUiScaleBtn_75.Profile.getId() == $OptionsMenuGui.btnNormalProfile.getId() &&
   OptionsMenuUiScaleDownBtn.isActive() &&
   OptionsMenuUiScaleUpBtn.isActive());

$OptionsMenuGui.onAdjustUiScale(-5);
assertScaleCond("Step '-' adjusts UI scale 80% -> 75% and disables '-' button",
   $ResolutionManager.getUiScalePercent() == 75 &&
   OptionsMenuUiScaleLabel.getText() $= "UI Scale: 75%" &&
   OptionsMenuUiScaleBtn_75.Profile.getId() == $OptionsMenuGui.btnSelectedProfile.getId() &&
   !OptionsMenuUiScaleDownBtn.isActive());

$OptionsMenuGui.onSelectUiScale(150);
assertScaleCond("Preset 150% selects 150% button and disables '+' button at ceiling",
   $ResolutionManager.getUiScalePercent() == 150 &&
   OptionsMenuUiScaleLabel.getText() $= "UI Scale: 150%" &&
   OptionsMenuUiScaleBtn_150.Profile.getId() == $OptionsMenuGui.btnSelectedProfile.getId() &&
   OptionsMenuUiScaleDownBtn.isActive() &&
   !OptionsMenuUiScaleUpBtn.isActive());

$OptionsMenuGui.onSelectUiScale(100);
assertScaleCond("Preset 100% updates UI scale label and highlights 100% button",
   $ResolutionManager.getUiScalePercent() == 100 &&
   OptionsMenuUiScaleLabel.getText() $= "UI Scale: 100%" &&
   OptionsMenuUiScaleBtn_100.Profile.getId() == $OptionsMenuGui.btnSelectedProfile.getId());

// 3. Localization of options_ui_scale across all 6 languages
$LocalizationManager.setLanguage("fi", true);
%fiScaleLbl = OptionsMenuUiScaleLabel.getText();
$LocalizationManager.setLanguage("fr", true);
%frScaleLbl = OptionsMenuUiScaleLabel.getText();
$LocalizationManager.setLanguage("es", true);
%esScaleLbl = OptionsMenuUiScaleLabel.getText();
$LocalizationManager.setLanguage("pt-BR", true);
%ptScaleLbl = OptionsMenuUiScaleLabel.getText();
$LocalizationManager.setLanguage("de", true);
%deScaleLbl = OptionsMenuUiScaleLabel.getText();
assertScaleCond("OptionsMenuUiScaleLabel localizes cleanly across fi/fr/es/pt-BR/de",
   %fiScaleLbl $= "K\xC3\xA4ytt\xC3\xB6liittym\xC3\xA4n skaala: 100%" &&
   %frScaleLbl $= "\xC3\x89chelle de l'interface : 100%" &&
   %esScaleLbl $= "Escala de interfaz: 100%" &&
   %ptScaleLbl $= "Escala da interface: 100%" &&
   %deScaleLbl $= "UI-Skalierung: 100%");
$LocalizationManager.setLanguage("en", true);
$OptionsMenuGui.show(false);

// 4. Full 150% (2.0x) scaling across all GUIs at 1920x1080
%origMainExtent = mainScreenGui.extent;
mainScreenGui.Extent = "1920 1080";
TurnManager.initStateMachine();
CombatEngine.initAnimStates();

if (isObject(t2dScene)) { t2dScene.clear(); t2dScene.delete(); }
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
$ResolutionManager.applyCameraAspect(sceneWindow2D);
game.reset();
game.setState($gameBattleState);
game.setTurn(1);
game.setPieceCount(1);
makeScaleTestUnit(1, 0, 3, "paladin", 1, 2);
game.setSelectedPiece(1);

$TitleMenuManager.show(true);
$OptionsMenuGui.show(true);
$PauseMenuManager.openPauseMenu();
$PauseMenuManager.onResumeClicked();
$BoardAudioHud.show(true);
$FactionSelectGui.show(true);
$BattleSummaryGui.show(true);
$VersionConfig.showWatermark(true);

$ResolutionManager.setUiScale(150, true);
$FloatingTextManager.spawnText(0, 0, "ScaleTest", "255 255 255", 500);
%fctCount = $FloatingTextManager.activeTexts.getCount();
%fctInst = (%fctCount > 0) ? $FloatingTextManager.activeTexts.getObject(0) : "";
$SceneTextOverlayGui.setBannerTextAndPos("ScaleBanner", 0, 20, 86);

assertScaleCond("150% (2.0x) at 1920x1080 scales all menus, modals, HUDs, watermark, and FPS overlay accurately",
   TitleMenuPanel.extent $= "580 644" &&
   TitleMenuConfirmPanel.extent $= "800 352" &&
   OptionsMenuPanel.extent $= "1280 892" &&
   mAbs(OptionsMenuUiScaleLabel.fontSizeAdjust - 2.0) < 0.001 &&
   mAbs(OptionsMenuApplyBtn.fontSizeAdjust - 2.0) < 0.001 &&
   PauseMenuPanel.extent $= "640 528" &&
   mAbs(PauseMenuResumeBtn.fontSizeAdjust - 2.0) < 0.001 &&
   BoardAudioHudContainer.extent $= "696 68" &&
   FactionSelectPanel.extent $= "1500 1032" &&
   mAbs($FactionSelectGui.descLabel[0].fontSizeAdjust - 2.0) < 0.001 &&
   BattleSummaryPanel.extent $= "1120 900" &&
   MainMenuWatermark.extent $= "528 120" &&
   FpsCounterOverlay.extent $= "240 52" &&
   isObject(%fctInst) &&
   isObject(%fctInst.container) &&
   %fctInst.container.extent $= "480 64" &&
   %fctInst.fgProfile.fontSize == 36 &&
   mAbs(SceneTextBannerLabel.fontSizeAdjust - 2.0) < 0.001 &&
   getWord(SceneTextBannerLabel.extent, 1) == 88);

// Dismiss modals so UnitStatsHud can display tooltip and micro-badge at 150%
$BattleSummaryGui.show(false);
$FactionSelectGui.show(false);
$OptionsMenuGui.show(false);
$TitleMenuManager.show(false);
$UnitStatsHud.showBadges(true);
enableUnitBadges();
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());

assertScaleCond("150% (2.0x) at 1920x1080 scales UnitStatsHud tooltip (1040x540) and micro-badge (152x44)",
   $UnitStatsHud.isTooltipVisible() &&
   UnitStatsTooltipPanel.extent $= "1040 540" &&
   UnitStatsTooltipHpTrack.extent $= "976 24" &&
   mAbs(UnitStatsTooltipHeader.fontSizeAdjust - 2.0) < 0.001 &&
   $UnitStatsHud.isBadgeVisible(1) &&
   $UnitStatsHud.badgeCtrl[1].extent $= "152 44");

// 5. Cycle 75 -> 100 -> 125 -> 150 -> 75 and verify zero rounding drift
$ResolutionManager.setUiScale(75, true);
$ResolutionManager.setUiScale(100, true);
$ResolutionManager.setUiScale(125, true);
$ResolutionManager.setUiScale(150, true);
$ResolutionManager.setUiScale(75, true);

assertScaleCond("Returning to 75% restores exact 1.0x baseline extents and fontSizeAdjust with zero drift",
   TitleMenuPanel.extent $= "290 322" &&
   OptionsMenuPanel.extent $= "640 446" &&
   PauseMenuPanel.extent $= "320 264" &&
   BoardAudioHudContainer.extent $= "348 34" &&
   FactionSelectPanel.extent $= "750 516" &&
   BattleSummaryPanel.extent $= "560 450" &&
   UnitStatsTooltipPanel.extent $= "520 270" &&
   UnitStatsTooltipHpTrack.extent $= "488 12" &&
   MainMenuWatermark.extent $= "264 60" &&
   FpsCounterOverlay.extent $= "120 26" &&
   $UnitStatsHud.badgeCtrl[1].extent $= "76 22" &&
   mAbs($FactionSelectGui.descLabel[0].fontSizeAdjust - 1.0) < 0.001 &&
   mAbs(OptionsMenuApplyBtn.fontSizeAdjust - 1.0) < 0.001 &&
   mAbs(UnitStatsTooltipHeader.fontSizeAdjust - 1.0) < 0.001);

// 6. Viewport-safe clamping at 1280x720
mainScreenGui.Extent = "1280 720";
$ResolutionManager.setUiScale(100, true);
assertScaleCond("At 1280x720 and 100% scale, OptionsMenuPanel scales unclamped to 853x595",
   OptionsMenuPanel.extent $= "853 595");

$ResolutionManager.setUiScale(150, true);
assertScaleCond("At 1280x720 and 150% scale, large modals clamp safely within 1256x696 while smaller GUIs remain 2.0x",
   getWord(OptionsMenuPanel.extent, 0) <= 1256 &&
   getWord(OptionsMenuPanel.extent, 1) <= 696 &&
   getWord(FactionSelectPanel.extent, 0) <= 1256 &&
   getWord(FactionSelectPanel.extent, 1) <= 696 &&
   getWord(BattleSummaryPanel.extent, 0) <= 1256 &&
   getWord(BattleSummaryPanel.extent, 1) <= 696 &&
   TitleMenuPanel.extent $= "580 644" &&
   PauseMenuPanel.extent $= "640 528" &&
   BoardAudioHudContainer.extent $= "696 68");

// 6b. Hero Select & Board Spell Card Title/Description Scaling (SceneTextOverlayGui)
$LocalizationManager.setLanguage("en", true);
mainScreenGui.Extent = "1280 720";
sceneWindow2D.Extent = "1280 720";
$ResolutionManager.applyCameraAspect(sceneWindow2D);
selectHeroScreen();
$SceneTextOverlayGui.stopTracking();

$ResolutionManager.setUiScale(75, true);
%hTitle75 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
%hBody75 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(100, true);
%hTitle100 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
%hBody100 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(150, true);
%hTitle150 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
%hBody150 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
%hExt = $SceneTextOverlayGui.cardSlotCtrl[0].getExtent();
%hTb = $SceneTextOverlayGui.getCardSlotTitleBounds(0);
%hBb = $SceneTextOverlayGui.getCardSlotBodyBounds(0);
$ResolutionManager.setUiScale(75, true);
%hBodyReset = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;

assertScaleCond("Hero select card titles & descriptions scale strictly across 75% -> 100% -> 150% and stay inside bounds",
   $SceneTextOverlayGui.isCardSlotVisible(0) &&
   mAbs(%hTitle75 - 1.0) < 0.01 &&
   mAbs(%hBody75 - 1.0) < 0.01 &&
   %hTitle100 > %hTitle75 &&
   %hTitle150 > %hTitle100 &&
   %hBody100 > %hBody75 &&
   %hBody150 > %hBody100 &&
   mAbs(%hBody150 - 2.0) < 0.01 &&
   mAbs(%hBodyReset - 1.0) < 0.01 &&
   (getWord(%hTb, 0) + getWord(%hTb, 2)) <= getWord(%hExt, 0) &&
   (getWord(%hBb, 1) + getWord(%hBb, 3)) <= getWord(%hExt, 1));

// Spell card descriptions (Volcano frame 6, Stomach Flu frame 8) and stat text (Taikamakkara frame 3)
if (isObject(t2dScene)) { t2dScene.clear(); t2dScene.delete(); }
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
$ResolutionManager.applyCameraAspect(sceneWindow2D);
game.reset();
game.setState($gameBattleState);
if (isObject(secondText)) secondText.setVisible(false);
$specialcard[1] = new Sprite() { scene = t2dScene; class = Card; superclass = Kortti; };
$specialcard[1].setImageMap("cardsImageMap");
$specialcard[1].setPosition("-40 0");
$specialcard[1].setSize("24 24");
$specialcard[1].setAngle(0);
$specialcard[1].setBlendAlpha(1);
$specialcard[1].setVisible(true);

// Frame 6 (Volcano description) across 75% -> 100% -> 150% at 720p and 150% at 1080p
$specialcard[1].setFrame(6);
$ResolutionManager.setUiScale(75, true);
%s6Body75 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
%s6Title75 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(100, true);
%s6Body100 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
%s6Title100 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(150, true);
%s6Body150 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
%s6Title150 = $SceneTextOverlayGui.cardTitleCtrl[0].fontSizeAdjust;
%s6Ext = $SceneTextOverlayGui.cardSlotCtrl[0].getExtent();
%s6Bb = $SceneTextOverlayGui.getCardSlotBodyBounds(0);

// Frame 3 (stat card) and Frame 8 (long description) at 75% vs 150%
$specialcard[1].setFrame(3);
$ResolutionManager.setUiScale(75, true);
%s3Body75 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(150, true);
%s3Body150 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;

$specialcard[1].setFrame(8);
$ResolutionManager.setUiScale(75, true);
%s8Body75 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
$ResolutionManager.setUiScale(150, true);
%s8Body150 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;

// Larger rendered card dimensions (equivalent to 1920x1080 card pixel extent) also scale descriptions higher cleanly
$specialcard[1].setFrame(6);
$specialcard[1].setSize("36 36");
$SceneTextOverlayGui.reanchor();
%s6Body1080_150 = $SceneTextOverlayGui.cardBodyCtrl[0].fontSizeAdjust;
%s6Ext1080 = $SceneTextOverlayGui.cardSlotCtrl[0].getExtent();
%s6Bb1080 = $SceneTextOverlayGui.getCardSlotBodyBounds(0);
$specialcard[1].setSize("24 24");

assertScaleCond("Spell card titles, stat lines, and descriptions scale across UI Scale (75% -> 100% -> 150%) and card extent inside bounds",
   %s6Title100 > %s6Title75 &&
   %s6Title150 > %s6Title100 &&
   %s6Body100 > %s6Body75 &&
   %s6Body150 > %s6Body100 &&
   %s3Body150 > %s3Body75 &&
   %s8Body150 > %s8Body75 &&
   %s6Body1080_150 > %s6Body150 &&
   (getWord(%s6Bb, 1) + getWord(%s6Bb, 3)) <= getWord(%s6Ext, 1) &&
   (getWord(%s6Bb1080, 1) + getWord(%s6Bb1080, 3)) <= getWord(%s6Ext1080, 1));

// 7. Persistence round-trip via Archon/preferences.cs
mainScreenGui.Extent = %origMainExtent;
sceneWindow2D.Extent = %origMainExtent;
$ResolutionManager.applyCameraAspect(sceneWindow2D);
$ResolutionManager.setUiScale(125, false);
$ResolutionManager.setUiScale(75, true); // Flip in memory only without saving
%beforeReload = $ResolutionManager.getUiScalePercent();
$ResolutionManager.loadPreferences();
assertScaleCond("ResolutionManager::loadPreferences restores persisted $pref::UI::Scale (125)",
   %beforeReload == 75 &&
   $ResolutionManager.getUiScalePercent() == 125 &&
   $pref::UI::Scale == 125);

// Restore clean initial preferences before exit
disableUnitBadges();
$UnitStatsHud.clearBadges();
$BoardAudioHud.show(false);
$SceneTextOverlayGui.hideBanner();
$LocalizationManager.setLanguage(%savedInitialLang, true);
$ResolutionManager.setUiScale(%savedInitialScale, false);

if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}
stopBackgroundMusic();
alxStopAll();

logScaleResult("==========================================");
logScaleResult("ALL UI SCALE TESTS PASSED!");
logScaleResult("==========================================");

if (isObject($uiScaleTestFile))
{
   $uiScaleTestFile.close();
   $uiScaleTestFile.delete();
}

quit();
