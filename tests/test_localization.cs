//-----------------------------------------------------------------------------
// tests/test_localization.cs
// End-to-end verification suite for burned-in text elimination (Objective 1)
// and 6-language runtime localization (fi, en, fr, es, pt-BR, de) (Objective 2).
//-----------------------------------------------------------------------------

$Scripts::ignoreDSOs = true;
setLogMode(1);
$Archon::skipSplash = true;

$locTestPath = pathConcat(getMainDotCsDir(), "tests/localization_test_result.txt");
$locTestFile = new FileObject();
$locTestFile.openForWrite($locTestPath);
$locTestFile.writeLine("LOCALIZATION_TEST_START");

function logLocResult(%msg)
{
   echo(%msg);
   if (isObject($locTestFile))
      $locTestFile.writeLine(%msg);
}

function failLocTest(%msg)
{
   logLocResult("TEST_FAIL: " @ %msg);
   if (isObject($locTestFile))
   {
      $locTestFile.close();
      $locTestFile.delete();
   }
   quit();
}

function passLocTest(%msg)
{
   logLocResult("TEST_PASS: " @ %msg);
}

function assertLocCond(%label, %cond)
{
   if (!%cond)
      failLocTest(%label);
   passLocTest(%label);
}

function makeLocTestUnit(%idx, %groupId, %creatureId, %name, %bx, %by)
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

logLocResult("==========================================");
logLocResult("Archonita - Localization & Asset Suite");
logLocResult("==========================================");

// Save original preference language to restore after persistence test
%savedInitialLang = ($pref::Language !$= "") ? $pref::Language : "fi";

// 1. Singletons & Supported Language Catalog
assertLocCond("$LocalizationManager and $SceneTextOverlayGui singletons exist",
   isObject($LocalizationManager) && isObject($SceneTextOverlayGui));
assertLocCond("6 supported languages registered in exact order (fi, en, fr, es, pt-BR, de)",
   $LocalizationManager.getLanguageCount() == 6 &&
   $LocalizationManager.getLanguageCodeByIndex(0) $= "fi" &&
   $LocalizationManager.getLanguageCodeByIndex(1) $= "en" &&
   $LocalizationManager.getLanguageCodeByIndex(2) $= "fr" &&
   $LocalizationManager.getLanguageCodeByIndex(3) $= "es" &&
   $LocalizationManager.getLanguageCodeByIndex(4) $= "pt-BR" &&
   $LocalizationManager.getLanguageCodeByIndex(5) $= "de");
assertLocCond("Language normalization handles case and alias forms cleanly",
   $LocalizationManager.normalizeLanguage("pt-br") $= "pt-BR" &&
   $LocalizationManager.normalizeLanguage("PT_BR") $= "pt-BR" &&
   $LocalizationManager.normalizeLanguage("Fran\xC3\xA7ais") $= "fr" &&
   $LocalizationManager.normalizeLanguage("English") $= "en" &&
   $LocalizationManager.normalizeLanguage("Deutsch") $= "de");

// 2. Verify full translation key coverage & intact UTF-8 diacritics across all 6 languages
%checkKeys = "banner_select_hero" TAB "banner_place_creatures" TAB "banner_countdown_one" TAB "banner_countdown_other" TAB
             "card_warrior_title" TAB "card_warrior_desc" TAB "card_wizard_title" TAB "card_wizard_desc" TAB
             "card_sausage_title" TAB "card_sausage_stat_one" TAB "card_magichat_title" TAB "card_magichat_stat_one" TAB
             "card_lollipop_title" TAB "card_lollipop_stat_one" TAB "card_volcano_title" TAB "card_volcano_desc" TAB
             "card_dandelion_title" TAB "card_dandelion_stat_one" TAB "card_stomachflu_title" TAB "card_stomachflu_desc" TAB
             "menu_play" TAB "menu_resume" TAB "menu_options" TAB "menu_credits" TAB "menu_quit" TAB
             "options_title" TAB "options_language" TAB "pause_title" TAB "hud_menu" TAB
             "faction_select_title" TAB "faction_undead_name" TAB "faction_dragonfolk_name" TAB "faction_gremlin_name" TAB
             "summary_victory" TAB "summary_defeat" TAB "summary_play_again" TAB "credits_title" TAB "credits_role_designer";

%allKeysPresent = true;
%keyCount = getFieldCount(%checkKeys);
for (%li = 0; %li < 6; %li++)
{
   %lc = $LocalizationManager.getLanguageCodeByIndex(%li);
   for (%ki = 0; %ki < %keyCount; %ki++)
   {
      %kName = getField(%checkKeys, %ki);
      %rawVal = $LocalizationManager.str[%lc, %kName];
      if (%rawVal $= "" || %rawVal $= %kName)
      {
         echo("Missing key for lang=" @ %lc @ " key=" @ %kName);
         %allKeysPresent = false;
      }
   }
}
assertLocCond("All core translation keys exist natively in all 6 languages without missing fallback", %allKeysPresent);

// Verify diacritics are preserved natively (no replacement '?' or mojibake)
assertLocCond("Diacritics preserved natively across fi/fr/es/pt-BR/de",
   strpos($LocalizationManager.getTextForLanguage("fi", "card_magichat_stat_other", 2), "\xC3\xA4") != -1 &&
   strpos($LocalizationManager.getTextForLanguage("fr", "card_warrior_title"), "\xC3\xA8") != -1 &&
   strpos($LocalizationManager.getTextForLanguage("es", "card_dandelion_title"), "\xC3\xAD") != -1 &&
   strpos($LocalizationManager.getTextForLanguage("pt-BR", "card_volcano_title"), "\xC3\xA3") != -1 &&
   strpos($LocalizationManager.getTextForLanguage("de", "card_dandelion_title"), "\xC3\xB6") != -1);

// 3. Dynamic interpolation & singular/plural rules (0, 1, 2, 5)
assertLocCond("English pluralization separates 1 second vs 0 and 5 seconds",
   $LocalizationManager.getPluralTextForLanguage("en", "banner_countdown", 1) $= "Game starts in 1 second..." &&
   $LocalizationManager.getPluralTextForLanguage("en", "banner_countdown", 5) $= "Game starts in 5 seconds..." &&
   $LocalizationManager.getPluralTextForLanguage("en", "banner_countdown", 0) $= "Game starts in 0 seconds...");
assertLocCond("French & Portuguese (BR) treat 0 and 1 as singular and 2+ as plural",
   $LocalizationManager.getPluralTextForLanguage("fr", "banner_countdown", 0) $= "La partie commence dans 0 seconde..." &&
   $LocalizationManager.getPluralTextForLanguage("fr", "banner_countdown", 1) $= "La partie commence dans 1 seconde..." &&
   $LocalizationManager.getPluralTextForLanguage("fr", "banner_countdown", 5) $= "La partie commence dans 5 secondes..." &&
   $LocalizationManager.getPluralTextForLanguage("pt-BR", "card_magichat_stat", 1) $= "+1 ponto de magia" &&
   $LocalizationManager.getPluralTextForLanguage("pt-BR", "card_magichat_stat", 3) $= "+3 pontos de magia");
assertLocCond("Spanish, German & Finnish pluralize stat cards and turns accurately",
   $LocalizationManager.getPluralTextForLanguage("es", "card_magichat_stat", 1) $= "+1 punto de hechizo" &&
   $LocalizationManager.getPluralTextForLanguage("es", "card_magichat_stat", 4) $= "+4 puntos de hechizo" &&
   $LocalizationManager.getPluralTextForLanguage("de", "card_sausage_stat", 1) $= "+1 Verteidigungswurf (Kreaturen)" &&
   $LocalizationManager.getPluralTextForLanguage("de", "card_sausage_stat", 2) $= "+2 Verteidigungsw\xC3\xBCrfe (Kreaturen)" &&
   $LocalizationManager.getPluralTextForLanguage("fi", "summary_turns", 1) $= "1 vuoro" &&
   $LocalizationManager.getPluralTextForLanguage("fi", "summary_turns", 7) $= "7 vuoroa");

// 4. Objective 1 & 2 Live Screen Overlays: Hero Select (card_0, card_1, teksti)
TurnManager.initStateMachine();
CombatEngine.initAnimStates();

$LocalizationManager.setLanguage("en", true);
selectHeroScreen();

assertLocCond("Hero select screen displays localized 'teksti' banner and both hero cards",
   $SceneTextOverlayGui.isBannerVisible() &&
   $SceneTextOverlayGui.getBannerText() $= "Choose a Hero" &&
   $SceneTextOverlayGui.isCardSlotVisible(0) &&
   $SceneTextOverlayGui.getCardSlotTitleText(0) $= "Warrior" &&
   $SceneTextOverlayGui.isCardSlotVisible(1) &&
   $SceneTextOverlayGui.getCardSlotTitleText(1) $= "Wizard");

// Verify card title and body rects sit strictly inside card overlay bounds
%card0Size = $SceneTextOverlayGui.cardSlotCtrl[0].getExtent();
%card0W = getWord(%card0Size, 0);
%card0H = getWord(%card0Size, 1);
%titleBounds0 = $SceneTextOverlayGui.getCardSlotTitleBounds(0);
%bodyBounds0 = $SceneTextOverlayGui.getCardSlotBodyBounds(0);
assertLocCond("Hero card title & description GUI bounds fit inside card_0 frame",
   getWord(%titleBounds0, 0) >= 0 && getWord(%titleBounds0, 1) >= 0 &&
   (getWord(%titleBounds0, 0) + getWord(%titleBounds0, 2)) <= %card0W &&
   getWord(%bodyBounds0, 0) >= 0 &&
   (getWord(%bodyBounds0, 0) + getWord(%bodyBounds0, 2)) <= %card0W &&
   (getWord(%bodyBounds0, 1) + getWord(%bodyBounds0, 3)) <= %card0H);

// Switch language dynamically on Hero Select across all remaining 5 locales
$LocalizationManager.setLanguage("fr", true);
assertLocCond("Hero select updates immediately to French (fr)",
   $SceneTextOverlayGui.getBannerText() $= "Choisissez un h\xC3\xA9ros" &&
   $SceneTextOverlayGui.getCardSlotTitleText(0) $= "Guerri\xC3\xA8re" &&
   $SceneTextOverlayGui.getCardSlotTitleText(1) $= "Magicienne");

$LocalizationManager.setLanguage("de", true);
assertLocCond("Hero select updates immediately to German (de)",
   $SceneTextOverlayGui.getBannerText() $= "W\xC3\xA4hle einen Helden" &&
   $SceneTextOverlayGui.getCardSlotTitleText(0) $= "Kriegerin" &&
   $SceneTextOverlayGui.getCardSlotTitleText(1) $= "Zauberin");
Canvas.repaint();

// Check FactionSelectGui modal blocks underlying card text cleanly & updates live
$FactionSelectGui.show(true);
assertLocCond("Opening FactionSelectGui updates text in German and hides underlying SceneTextOverlayRoot",
   $FactionSelectGui.isVisible() &&
   $FactionSelectGui.headerLabel.getText() $= "GEGNERISCHE FRAKTION W\xC3\x84HLEN" &&
   !$SceneTextOverlayGui.overlayRoot.isVisible());
$LocalizationManager.setLanguage("es", true);
assertLocCond("Live switch to Spanish (es) updates open FactionSelectGui immediately",
   $FactionSelectGui.headerLabel.getText() $= "ELIGE LA FACCI\xC3\xB3N ENEMIGA" ||
   $FactionSelectGui.headerLabel.getText() $= "ELIGE LA FACCI\xC3\x93N ENEMIGA");
$FactionSelectGui.show(false);
assertLocCond("Closing FactionSelectGui restores Spanish hero card overlay",
   $SceneTextOverlayGui.overlayRoot.isVisible() &&
   $SceneTextOverlayGui.getBannerText() $= "Elige un h\xC3\xA9roe" &&
   $SceneTextOverlayGui.getCardSlotTitleText(0) $= "Guerrera" &&
   $SceneTextOverlayGui.getCardSlotTitleText(1) $= "Hechicera");

// 5. Board Screen Overlays: Place creatures, Dynamic Countdown, Face-down vs Revealed Spell Cards
$selectedHero = 1;
$sankarivalittu = 1;
showBoardScreen();

assertLocCond("Board pre-battle phase displays localized 'Place creatures' banner and HUD",
   $SceneTextOverlayGui.isBannerVisible() &&
   $SceneTextOverlayGui.getBannerText() $= "Coloca tus criaturas en el tablero" &&
   BoardHudLangBtn.getText() $= "ES");

// Trigger countdown state (5 -> 1 second) and verify sijoitaText hidden, dynamic secondText active, secondCounter alpha 0
game.setState($gamePreBattleTimerState);
sijoitaText.setVisible(false);
secondText.setVisible(true);
secondCounter.setVisible(true);
game.setGameTimer(5);
secondCounter.setFrame(5);
$LocalizationManager.setLanguage("en", true);
assertLocCond("Countdown timer hides legacy secondCounter bitmap and renders dynamic English 5s text",
   secondCounter.getBlendAlpha() == 0 &&
   $SceneTextOverlayGui.isBannerVisible() &&
   $SceneTextOverlayGui.getBannerText() $= "Game starts in 5 seconds...");

TurnManager.updateCountdownTimer(); // 4
TurnManager.updateCountdownTimer(); // 3
TurnManager.updateCountdownTimer(); // 2
TurnManager.updateCountdownTimer(); // 1
assertLocCond("Countdown timer updates smoothly to singular 1 second form",
   $SceneTextOverlayGui.getBannerText() $= "Game starts in 1 second...");

// Cancel countdown schedule so it doesn't fire asynchronously
if (isEventPending(TurnManager.turnTimerEvent))
{
   cancel(TurnManager.turnTimerEvent);
   TurnManager.turnTimerEvent = 0;
}

// Verify spell card deck: face-down hides text, revealed frame 3..8 renders dynamic card text & fits bounds
game.setState($gameBattleState);
secondText.setVisible(false);
$SceneTextOverlayGui.refreshAll();

if (!isObject($specialcard[1]))
{
   $specialcard[1] = new Sprite() { scene = t2dScene; class = Card; superclass = Kortti; };
   $specialcard[1].setImageMap("cardsImageMap");
   $specialcard[1].setPosition("-40 0");
   $specialcard[1].setSize("24 24");
   $specialcard[1].setVisible(true);
}
$specialcard[1].setAngle(0);
$specialcard[1].setBlendAlpha(1);
$specialcard[1].setFrame(0);
$SceneTextOverlayGui.refreshAll();
assertLocCond("Face-down spell card (frame 0) shows no card text overlay",
   !$SceneTextOverlayGui.isCardSlotVisible(0));

%allSpellFramesFit = true;
for (%f = 3; %f <= 8; %f++)
{
   $specialcard[1].setFrame(%f);
   for (%li = 0; %li < 6; %li++)
   {
      %lc = $LocalizationManager.getLanguageCodeByIndex(%li);
      $LocalizationManager.setLanguage(%lc, true);
      %expectedTitle = $LocalizationManager.getCardTitle(%f);
      %slotTitle = $SceneTextOverlayGui.getCardSlotTitleText(0);
      %slotBody = $SceneTextOverlayGui.getCardSlotBodyText(0);
      %cExt = $SceneTextOverlayGui.cardSlotCtrl[0].getExtent();
      %tb = $SceneTextOverlayGui.getCardSlotTitleBounds(0);
      %bb = $SceneTextOverlayGui.getCardSlotBodyBounds(0);
      if (!$SceneTextOverlayGui.isCardSlotVisible(0) || %slotTitle !$= %expectedTitle || %slotBody $= "" ||
          (getWord(%tb, 0) + getWord(%tb, 2)) > getWord(%cExt, 0) ||
          (getWord(%bb, 1) + getWord(%bb, 3)) > getWord(%cExt, 1))
      {
         echo("Card layout fail frame=" @ %f @ " lang=" @ %lc @ " title=" @ %slotTitle);
         %allSpellFramesFit = false;
      }
   }
}
assertLocCond("All 6 spell cards (frames 3..8) render localized title & body inside bounds for all 6 languages",
   %allSpellFramesFit);

// Tilted / discarding card hides dynamic card text cleanly
$specialcard[1].setFrame(6);
$specialcard[1].setAngle(45);
$SceneTextOverlayGui.refreshAll();
assertLocCond("Tilted/discarding spell card suppresses card overlay cleanly",
   !$SceneTextOverlayGui.isCardSlotVisible(0));
$specialcard[1].setAngle(0);

// BoardAudioHud quick language cycler + PauseMenuGui live switch
$BoardAudioHud.onCycleLanguage();
%afterHudCycle = $LocalizationManager.getLanguage();
assertLocCond("BoardAudioHud.onCycleLanguage advances language and updates HUD button",
   %afterHudCycle !$= "de" &&
   BoardHudLangBtn.getText() $= $LocalizationManager.getLanguageShortCode());

$PauseMenuManager.openPauseMenu();
$LocalizationManager.setLanguage("pt-BR", true);
assertLocCond("PauseMenuManager updates live to Brazilian Portuguese and hides board overlay behind modal",
   $PauseMenuManager.backdropCtrl.isVisible() &&
   PauseMenuResumeBtn.getText() $= "CONTINUAR" &&
   PauseMenuTitleBtn.getText() $= "VOLTAR AO MENU" &&
   !$SceneTextOverlayGui.overlayRoot.isVisible());
$PauseMenuManager.onResumeClicked();
Canvas.repaint();

// 6. Credits Screen Dynamic Overlays
creditsScreen();
if ($creditsReturnSchedule !$= "") { cancel($creditsReturnSchedule); $creditsReturnSchedule = ""; }
if ($creditsFadeSchedule !$= "") { cancel($creditsFadeSchedule); $creditsFadeSchedule = ""; }
if ($creditsExplosionSchedule !$= "") { cancel($creditsExplosionSchedule); $creditsExplosionSchedule = ""; }

$LocalizationManager.setLanguage("pt-BR", true);
assertLocCond("Credits screen renders localized Brazilian Portuguese title and all 7 roles",
   $SceneTextOverlayGui.getCreditsTitleText() $= "CR\xC3\x89DITOS" &&
   $SceneTextOverlayGui.getCreditsRoleText(0) $= "DESIGN DO JOGO" &&
   $SceneTextOverlayGui.getCreditsRoleText(6) $= "AGRADECIMENTOS");

$LocalizationManager.setLanguage("fi", true);
assertLocCond("Credits screen switches live to Finnish (fi)",
   $SceneTextOverlayGui.getCreditsTitleText() $= "TEKIJ\xC3\x84T" &&
   $SceneTextOverlayGui.getCreditsRoleText(0) $= "PELISUUNNITTELU" &&
   $SceneTextOverlayGui.getCreditsRoleText(6) $= "ERITYISKIITOKSET");

// 7. Live Switching across TitleMenu, OptionsMenuGui, PauseMenuGui, UnitStatsHud, BattleSummaryGui
mainmenuScreen();
assertLocCond("Main menu displays Finnish buttons & cycles through OptionsMenuGui",
   $TitleMenuManager.menuPanel.isVisible() &&
   TitleMenuPlayBtn.getText() $= "ALOITA PELI" &&
   TitleMenuOptionsBtn.getText() $= "ASETUKSET" &&
   TitleMenuLanguageBtn.getText() $= "KIELI: SUOMI (FI)");

$OptionsMenuGui.show(true);
$OptionsMenuGui.onSelectLanguage("fr");
assertLocCond("Selecting 'fr' in OptionsMenuGui updates both OptionsMenuGui & TitleMenuManager live",
   OptionsMenuApplyBtn.getText() $= "APPLIQUER LA VID\xC3\x89O" &&
   TitleMenuPlayBtn.getText() $= "JOUER" &&
   TitleMenuLanguageBtn.getText() $= "LANGUE : FRAN\xC3\x87AIS (FR)");
Canvas.repaint();
$OptionsMenuGui.show(false);
$TitleMenuManager.onLanguageClicked();
assertLocCond("Clicking TitleMenuManager language button cycles 'fr' -> 'es' live",
   $LocalizationManager.getLanguage() $= "es" &&
   TitleMenuPlayBtn.getText() $= "JUGAR");

// Test BattleSummaryGui & UnitStatsHud live switch
if (isObject(t2dScene)) { t2dScene.clear(); t2dScene.delete(); }
new Scene(t2dScene);
sceneWindow2D.setScene(t2dScene);
$ResolutionManager.applyCameraAspect(sceneWindow2D);
game.reset();
game.setState($gameBattleState);
game.setTurn(3);
game.setPieceCount(2);
makeLocTestUnit(1, 0, 3, "paladin", 1, 2);
makeLocTestUnit(2, 1, 4, "skeletonwarrior", 2, 1);

$UnitStatsHud.showBadges(true);
$UnitStatsHud.updateHover($nappulaSprite[1].getPosition());
$LocalizationManager.setLanguage("de", true);
assertLocCond("UnitStatsHud tooltip updates live to German (de)",
   $UnitStatsHud.isTooltipVisible() &&
   $UnitStatsHud.getTooltipHeaderText() $= "Paladin (St. 4)" &&
   $UnitStatsHud.getTooltipAttackText() $= "Angriff: 4 W\xC3\xBCrfel");

$nappulaSprite[2].alive = 0;
TurnManager.endBattle();
$LocalizationManager.setLanguage("es", true);
assertLocCond("BattleSummaryGui updates live to Spanish (es) on VICTORY modal",
   $BattleSummaryGui.isVisible() &&
   $BattleSummaryGui.getBannerTitleText() $= "VICTORIA" &&
   BattleSummaryPlayAgainBtn.getText() $= "JUGAR DE NUEVO");
$BattleSummaryGui.show(false);

// 8. Persistence Test via preferences.cs
$LocalizationManager.setLanguage("pt-BR", false);
$LocalizationManager.setLanguage("en", true); // Temporarily flip in memory without saving
$ResolutionManager.loadPreferences();
assertLocCond("ResolutionManager::loadPreferences restores persisted $pref::Language ('pt-BR')",
   $LocalizationManager.getLanguage() $= "pt-BR" &&
   $pref::Language $= "pt-BR");

// Restore clean initial language preference before exiting
$LocalizationManager.setLanguage(%savedInitialLang, false);

// Final Cleanup
if ($bubbleSchedule !$= "") { cancel($bubbleSchedule); $bubbleSchedule = ""; }
stopBackgroundMusic();
alxStopAll();
if (isObject($SceneTextOverlayGui))
   $SceneTextOverlayGui.stopTracking();
if (isObject(t2dScene))
{
   t2dScene.clear();
   t2dScene.delete();
}

logLocResult("==========================================");
logLocResult("ALL LOCALIZATION TESTS PASSED!");
logLocResult("==========================================");

if (isObject($locTestFile))
{
   $locTestFile.close();
   $locTestFile.delete();
}

quit();
