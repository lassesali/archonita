//---------------------------------------------------------------------------------------------
// Archonita - LocalizationManager.cs
// Full multi-language localization engine for Finnish (fi), English (en), French (fr),
// Spanish (es), Brazilian Portuguese (pt-BR), and German (de).
// Supports variable interpolation (%1..%5), language-specific plural rules, live UI refresh,
// font glyph cache warming for European diacritics, and persistence via $pref::Language.
// Conforms strictly to TORQUE_SCRIPT.md: single class ScriptObject, onAdd/onRemove lifecycle.
//---------------------------------------------------------------------------------------------

function LocalizationManager::init(%this)
{
   %this.langCount = 6;
   %this.langCode[0] = "fi";
   %this.langCode[1] = "en";
   %this.langCode[2] = "fr";
   %this.langCode[3] = "es";
   %this.langCode[4] = "pt-BR";
   %this.langCode[5] = "de";

   %this.langLabel[0] = "Suomi";
   %this.langLabel[1] = "English";
   %this.langLabel[2] = "Français";
   %this.langLabel[3] = "Español";
   %this.langLabel[4] = "Português (BR)";
   %this.langLabel[5] = "Deutsch";

   %this.langShort[0] = "FI";
   %this.langShort[1] = "EN";
   %this.langShort[2] = "FR";
   %this.langShort[3] = "ES";
   %this.langShort[4] = "PT-BR";
   %this.langShort[5] = "DE";

   %this.charsetSample = "äöåÄÖÅéèêëàâùûîïôçœÉÈÊËÀÂÙÛÎÏÔÇŒáíóúñü¡¿ÁÍÓÚÑÜãõÃÕß—•";

   %this.populateFinnish();
   %this.populateEnglish();
   %this.populateFrench();
   %this.populateSpanish();
   %this.populatePortugueseBR();
   %this.populateGerman();

   %initialLang = ($pref::Language !$= "") ? %this.normalizeLanguage($pref::Language) : "fi";
   %this.currentLanguage = %initialLang;
   $pref::Language = %initialLang;
}

function LocalizationManager::onAdd(%this)
{
   %this.init();
}

function LocalizationManager::onRemove(%this)
{
   // Pure string catalog; no owned SimObjects or schedules
}

//---------------------------------------------------------------------------------------------
// Language Selection & Normalization
//---------------------------------------------------------------------------------------------
function LocalizationManager::normalizeLanguage(%this, %lang)
{
   %raw = strlwr(trim(%lang));
   if (%raw $= "fi" || %raw $= "fin" || %raw $= "finnish" || %raw $= "suomi")
      return "fi";
   if (%raw $= "en" || %raw $= "eng" || %raw $= "english")
      return "en";
   if (%raw $= "fr" || %raw $= "fra" || %raw $= "fre" || %raw $= "french" || %raw $= "francais" || %raw $= "français")
      return "fr";
   if (%raw $= "es" || %raw $= "spa" || %raw $= "spanish" || %raw $= "espanol" || %raw $= "español")
      return "es";
   if (%raw $= "pt-br" || %raw $= "pt_br" || %raw $= "pt" || %raw $= "por" || %raw $= "portuguese" || %raw $= "portugues")
      return "pt-BR";
   if (%raw $= "de" || %raw $= "deu" || %raw $= "ger" || %raw $= "german" || %raw $= "deutsch")
      return "de";

   return "fi";
}

function LocalizationManager::getLanguage(%this)
{
   if (%this.currentLanguage $= "")
      %this.currentLanguage = "fi";
   return %this.currentLanguage;
}

function LocalizationManager::getLanguageCount(%this)
{
   return (%this.langCount > 0) ? %this.langCount : 6;
}

function LocalizationManager::getLanguageCodeByIndex(%this, %index)
{
   if (%index >= 0 && %index < %this.langCount)
      return %this.langCode[%index];
   return "fi";
}

function LocalizationManager::getLanguageIndex(%this, %lang)
{
   %norm = (%lang !$= "") ? %this.normalizeLanguage(%lang) : %this.getLanguage();
   for (%i = 0; %i < %this.langCount; %i++)
   {
      if (%this.langCode[%i] $= %norm)
         return %i;
   }
   return 0;
}

function LocalizationManager::getLanguageDisplayName(%this, %lang)
{
   %idx = %this.getLanguageIndex(%lang);
   return %this.langLabel[%idx];
}

function LocalizationManager::getLanguageShortCode(%this, %lang)
{
   %idx = %this.getLanguageIndex(%lang);
   return %this.langShort[%idx];
}

function LocalizationManager::setLanguage(%this, %lang, %skipSave)
{
   %norm = %this.normalizeLanguage(%lang);
   %this.currentLanguage = %norm;
   $pref::Language = %norm;

   if (!%skipSave && isObject($ResolutionManager))
      $ResolutionManager.savePreferences();

   %this.prewarmFonts();
   %this.notifyLanguageChanged();
   return %norm;
}

function LocalizationManager::cycleLanguage(%this, %step, %skipSave)
{
   if (%step $= "" || %step == 0)
      %step = 1;

   %idx = %this.getLanguageIndex(%this.getLanguage());
   %next = (%idx + %step) % %this.langCount;
   if (%next < 0)
      %next += %this.langCount;

   return %this.setLanguage(%this.langCode[%next], %skipSave);
}

function LocalizationManager::prewarmFonts(%this)
{
   if (!isFunction("populateFontCacheString") || !isObject(Canvas) || !Canvas.isAwake())
      return;

   %sizes = "10 11 12 13 14 15 16 17 18 20 22 24 26 28 30 32 36 40 44 48";
   %count = getWordCount(%sizes);
   for (%i = 0; %i < %count; %i++)
   {
      %sz = getWord(%sizes, %i);
      populateFontCacheString("Arial", %sz, %this.charsetSample);
   }
}

function LocalizationManager::notifyLanguageChanged(%this)
{
   if (isObject($FactionManager) && $FactionManager.isMethod("refreshLocalization"))
      $FactionManager.refreshLocalization();
   if (isObject($TitleMenuManager) && $TitleMenuManager.isMethod("refreshLocalization"))
      $TitleMenuManager.refreshLocalization();
   if (isObject($OptionsMenuGui) && $OptionsMenuGui.isMethod("refreshLocalization"))
      $OptionsMenuGui.refreshLocalization();
   if (isObject($PauseMenuManager) && $PauseMenuManager.isMethod("refreshLocalization"))
      $PauseMenuManager.refreshLocalization();
   if (isObject($BoardAudioHud) && $BoardAudioHud.isMethod("refreshLocalization"))
      $BoardAudioHud.refreshLocalization();
   if (isObject($FactionSelectGui) && $FactionSelectGui.isMethod("refreshLocalization"))
      $FactionSelectGui.refreshLocalization();
   if (isObject($UnitStatsHud) && $UnitStatsHud.isMethod("refreshLocalization"))
      $UnitStatsHud.refreshLocalization();
   if (isObject($BattleSummaryGui) && $BattleSummaryGui.isMethod("refreshLocalization"))
      $BattleSummaryGui.refreshLocalization();
   if (isObject($VersionConfig) && $VersionConfig.isMethod("refreshLocalization"))
      $VersionConfig.refreshLocalization();
   if (isObject($SceneTextOverlayGui) && $SceneTextOverlayGui.isMethod("refreshAll"))
      $SceneTextOverlayGui.refreshAll();
}

//---------------------------------------------------------------------------------------------
// Translation Lookup, Interpolation & Pluralization
//---------------------------------------------------------------------------------------------
function LocalizationManager::getRawText(%this, %key, %lang)
{
   %targetLang = (%lang !$= "") ? %this.normalizeLanguage(%lang) : %this.getLanguage();
   %val = %this.str[%targetLang, %key];
   if (%val !$= "")
      return %val;

   %val = %this.str["en", %key];
   if (%val !$= "")
      return %val;

   %val = %this.str["fi", %key];
   if (%val !$= "")
      return %val;

   return %key;
}

function LocalizationManager::getText(%this, %key, %arg1, %arg2, %arg3, %arg4, %arg5)
{
   %text = %this.getRawText(%key, "");
   if (%arg5 !$= "")
      %text = strreplace(%text, "%5", %arg5);
   if (%arg4 !$= "")
      %text = strreplace(%text, "%4", %arg4);
   if (%arg3 !$= "")
      %text = strreplace(%text, "%3", %arg3);
   if (%arg2 !$= "")
      %text = strreplace(%text, "%2", %arg2);
   if (%arg1 !$= "")
      %text = strreplace(%text, "%1", %arg1);
   return %text;
}

function LocalizationManager::getTextForLanguage(%this, %lang, %key, %arg1, %arg2, %arg3, %arg4, %arg5)
{
   %text = %this.getRawText(%key, %lang);
   if (%arg5 !$= "")
      %text = strreplace(%text, "%5", %arg5);
   if (%arg4 !$= "")
      %text = strreplace(%text, "%4", %arg4);
   if (%arg3 !$= "")
      %text = strreplace(%text, "%3", %arg3);
   if (%arg2 !$= "")
      %text = strreplace(%text, "%2", %arg2);
   if (%arg1 !$= "")
      %text = strreplace(%text, "%1", %arg1);
   return %text;
}

function LocalizationManager::isSingularCount(%this, %lang, %count)
{
   %n = %count + 0;
   if (%lang $= "fr" || %lang $= "pt-BR")
      return (%n == 0 || %n == 1);
   return (%n == 1);
}

function LocalizationManager::getPluralText(%this, %baseKey, %count, %arg2, %arg3, %arg4, %arg5)
{
   %lang = %this.getLanguage();
   %suffix = %this.isSingularCount(%lang, %count) ? "_one" : "_other";
   return %this.getText(%baseKey @ %suffix, %count, %arg2, %arg3, %arg4, %arg5);
}

function LocalizationManager::getPluralTextForLanguage(%this, %lang, %baseKey, %count, %arg2, %arg3, %arg4, %arg5)
{
   %norm = %this.normalizeLanguage(%lang);
   %suffix = %this.isSingularCount(%norm, %count) ? "_one" : "_other";
   return %this.getTextForLanguage(%norm, %baseKey @ %suffix, %count, %arg2, %arg3, %arg4, %arg5);
}

//---------------------------------------------------------------------------------------------
// Domain Convenience Helpers (Cards & Units)
//---------------------------------------------------------------------------------------------
function LocalizationManager::getCardTitle(%this, %frame)
{
   switch (%frame)
   {
      case 1: return %this.getText("card_warrior_title");
      case 2: return %this.getText("card_wizard_title");
      case 3: return %this.getText("card_sausage_title");
      case 4: return %this.getText("card_magichat_title");
      case 5: return %this.getText("card_lollipop_title");
      case 6: return %this.getText("card_volcano_title");
      case 7: return %this.getText("card_dandelion_title");
      case 8: return %this.getText("card_stomachflu_title");
   }
   return "";
}

function LocalizationManager::getCardDescription(%this, %frame)
{
   switch (%frame)
   {
      case 1: return %this.getText("card_warrior_desc");
      case 2: return %this.getText("card_wizard_desc");
      case 6: return %this.getText("card_volcano_desc");
      case 8: return %this.getText("card_stomachflu_desc");
   }
   return "";
}

function LocalizationManager::getCardStatText(%this, %frame)
{
   switch (%frame)
   {
      case 3: return %this.getPluralText("card_sausage_stat", 1);
      case 4: return %this.getPluralText("card_magichat_stat", 1);
      case 5: return %this.getPluralText("card_lollipop_stat", 1);
      case 7: return %this.getPluralText("card_dandelion_stat", 1);
   }
   return "";
}

function LocalizationManager::isStatCard(%this, %frame)
{
   return (%frame == 3 || %frame == 4 || %frame == 5 || %frame == 7);
}

function LocalizationManager::getUnitName(%this, %creatureId, %fallbackName)
{
   if (%creatureId !$= "" && %creatureId >= 0 && %creatureId <= 15)
      return %this.getText("unit_name_" @ %creatureId);

   %raw = strlwr(trim(%fallbackName));
   switch$ (%raw)
   {
      case "warrior":         return %this.getText("unit_name_0");
      case "archer":          return %this.getText("unit_name_1");
      case "cleric":          return %this.getText("unit_name_2");
      case "paladin":         return %this.getText("unit_name_3");
      case "skeletonwarrior": return %this.getText("unit_name_4");
      case "skeletonarcher":  return %this.getText("unit_name_5");
      case "dreadzombie":     return %this.getText("unit_name_6");
      case "ancientghost":    return %this.getText("unit_name_7");
      case "dragonfolk":      return %this.getText("unit_name_8");
      case "reddragon":       return %this.getText("unit_name_9");
      case "airdragon":       return %this.getText("unit_name_10");
      case "golddragon":      return %this.getText("unit_name_11");
      case "gremlin":         return %this.getText("unit_name_12");
      case "hornedoneye":     return %this.getText("unit_name_13");
      case "mistress":        return %this.getText("unit_name_14");
      case "demon":           return %this.getText("unit_name_15");
   }

   if (%fallbackName !$= "")
      return %fallbackName;
   return %this.getText("unit_fallback");
}

//---------------------------------------------------------------------------------------------
// Finnish (fi) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populateFinnish(%this)
{
   %l = "fi";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Valitse sankari";
   %this.str[%l, "banner_place_creatures"] = "Sijoita olennot pelilaudalle";
   %this.str[%l, "banner_countdown_one"] = "Peli alkaa %1 sekunnin kuluttua...";
   %this.str[%l, "banner_countdown_other"] = "Peli alkaa %1 sekunnin kuluttua...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Soturi";
   %this.str[%l, "card_warrior_desc"] = "Soturi on taitava taistelija.";
   %this.str[%l, "card_wizard_title"] = "Velho";
   %this.str[%l, "card_wizard_desc"] = "Velho osaa loitsia.";
   %this.str[%l, "card_sausage_title"] = "Taikamakkara";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 olentojen puolustusheitto";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 olentojen puolustusheittoa";
   %this.str[%l, "card_magichat_title"] = "Taikahattu";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 loitsupiste";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 loitsupistettä";
   %this.str[%l, "card_lollipop_title"] = "Taikatikkari";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 loitsuvahinko";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 loitsuvahinkoa";
   %this.str[%l, "card_volcano_title"] = "Tulivuori";
   %this.str[%l, "card_volcano_desc"] = "Mahtava tulivuori tuhoaa vihollisen armeijan.";
   %this.str[%l, "card_dandelion_title"] = "Voikukan juuri";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 loitsupiste";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 loitsupistettä";
   %this.str[%l, "card_stomachflu_title"] = "Vatsatauti";
   %this.str[%l, "card_stomachflu_desc"] = "Vatsatautiin sairastunut olento ei voi tehdä mitään seuraavalla vuorollaan.";

   // Title Menu
   %this.str[%l, "menu_play"] = "ALOITA PELI";
   %this.str[%l, "menu_resume"] = "JATKA PELIÄ";
   %this.str[%l, "menu_resume_none"] = "JATKA PELIÄ (EI PELIÄ)";
   %this.str[%l, "menu_options"] = "ASETUKSET";
   %this.str[%l, "menu_credits"] = "TEKIJÄT";
   %this.str[%l, "menu_quit"] = "LOPETA";
   %this.str[%l, "menu_lang_btn"] = "KIELI: SUOMI (FI)";
   %this.str[%l, "menu_confirm_title"] = "ALOITETAANKO UUSI PELI?";
   %this.str[%l, "menu_confirm_body"] = "Peli on kesken. Uuden pelin aloittaminen poistaa sen.";
   %this.str[%l, "menu_confirm_yes"] = "UUSI PELI";
   %this.str[%l, "menu_confirm_no"] = "PERUUTA";

   // Options Menu
   %this.str[%l, "options_title"] = "ASETUKSET JA RYHMITTYMÄ";
   %this.str[%l, "options_aspect"] = "Kuvasuhde:";
   %this.str[%l, "options_resolution"] = "Resoluutio:";
   %this.str[%l, "options_mode"] = "Näyttötila:";
   %this.str[%l, "options_windowed"] = "IKKUNOITU";
   %this.str[%l, "options_fullscreen"] = "KOKORUUTU";
   %this.str[%l, "options_audio"] = "Ääniasetukset:";
   %this.str[%l, "options_faction"] = "Vihollisryhmittymä:";
   %this.str[%l, "options_language"] = "Kieli:";
   %this.str[%l, "options_ui_scale"] = "Käyttöliittymän skaala: %1%";
   %this.str[%l, "options_apply"] = "OTA KÄYTTÖÖN";
   %this.str[%l, "options_close"] = "SULJE";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "PELI TAUOLLA";
   %this.str[%l, "pause_resume"] = "JATKA PELIÄ";
   %this.str[%l, "pause_options"] = "ASETUKSET";
   %this.str[%l, "pause_title_screen"] = "PALAA PÄÄVALIKKOON";
   %this.str[%l, "pause_quit"] = "LOPETA PELI";
   %this.str[%l, "hud_menu"] = "VALIKKO";
   %this.str[%l, "hud_sfx_on"] = "ÄÄNET: PÄÄLLÄ";
   %this.str[%l, "hud_sfx_muted"] = "ÄÄNET: MYK.";
   %this.str[%l, "hud_music_on"] = "MUSIIKKI: PÄÄLLÄ";
   %this.str[%l, "hud_music_muted"] = "MUSIIKKI: MYK.";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Ennakkojulkaisu";
   %this.str[%l, "watermark_version"] = "Versio: %1";
   %this.str[%l, "watermark_build"] = "Koontiversio: %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Ihmisten ritarikunta";
   %this.str[%l, "faction_human_order_upper"] = "IHMISTEN RITARIKUNTA";
   %this.str[%l, "faction_short_undead"] = "EPÄKUOLLEET";
   %this.str[%l, "faction_short_dragonfolk"] = "LOHIKÄÄRMEET";
   %this.str[%l, "faction_short_gremlin"] = "RIUVAT";
   %this.str[%l, "faction_undead_name"] = "Epäkuolleiden legioona";
   %this.str[%l, "faction_undead_upper"] = "EPÄKUOLLEIDEN LEGIOONA";
   %this.str[%l, "faction_undead_subtitle"] = "Haudasta nousseet";
   %this.str[%l, "faction_undead_desc"] = "Kirotut luurankosoturit ja kostonhimoiset aaveet ikivanhoista holveista. Käyttävät aavekosketusta ja tauteja.";
   %this.str[%l, "faction_undead_spec"] = "Nekroottiset kiroukset: Tauti (-1 HYÖ) ja Aavekosketus (-2 PUO).";
   %this.str[%l, "faction_dragonfolk_name"] = "Lohikäärmekansa";
   %this.str[%l, "faction_dragonfolk_upper"] = "LOHIKÄÄRMEKANSA";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "Suomujen klaani";
   %this.str[%l, "faction_dragonfolk_desc"] = "Ylpeät matelijasoturit, tulenhenkäiset liskot ja ylhäiset kultalohikäärmeet elementaalivoimineen.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Suomujen raivo: Korkea kestävyys, liekinhenkäys ja salama-iskut.";
   %this.str[%l, "faction_gremlin_name"] = "Maanalainen lauma";
   %this.str[%l, "faction_gremlin_upper"] = "MAANALAINEN LAUMA";
   %this.str[%l, "faction_gremlin_subtitle"] = "Syvyyksien riivat";
   %this.str[%l, "faction_gremlin_desc"] = "Ovelat maanalaiset riivat, kyklooppisilmien säteet ja mahtavat helvetin demonit syvyyksistä.";
   %this.str[%l, "faction_gremlin_spec"] = "Syvyyksien uhka: Ilkeä parvi, silmäsäteet ja manalan demoni.";

   %this.str[%l, "faction_select_title"] = "VALITSE VIHOLLISRYHMITTYMÄ";
   %this.str[%l, "faction_select_subtitle"] = "Valitse, mikä vastustajaryhmittymä kohtaa sinut taistelulaudalla";
   %this.str[%l, "faction_select_btn"] = "VALITSE";
   %this.str[%l, "faction_selected_btn"] = "[ VALITTU ]";
   %this.str[%l, "faction_change_hero_btn"] = "VAIHDA SANKARIA";
   %this.str[%l, "faction_begin_battle_btn"] = "ALOITA TAISTELU";

   // Units
   %this.str[%l, "unit_fallback"] = "Yksikkö";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (Taso %2)";
   %this.str[%l, "unit_name_0"] = "Soturi";
   %this.str[%l, "unit_name_1"] = "Jousimies";
   %this.str[%l, "unit_name_2"] = "Pappi";
   %this.str[%l, "unit_name_3"] = "Paladiini";
   %this.str[%l, "unit_name_4"] = "Luurankosoturi";
   %this.str[%l, "unit_name_5"] = "Luurankojousimies";
   %this.str[%l, "unit_name_6"] = "Kauhuzombi";
   %this.str[%l, "unit_name_7"] = "Muinainen aave";
   %this.str[%l, "unit_name_8"] = "Lohikäärmesotilas";
   %this.str[%l, "unit_name_9"] = "Punalohikäärme";
   %this.str[%l, "unit_name_10"] = "Ilmalohikäärme";
   %this.str[%l, "unit_name_11"] = "Kultalohikäärme";
   %this.str[%l, "unit_name_12"] = "Riiva";
   %this.str[%l, "unit_name_13"] = "Sarvikyklooppi";
   %this.str[%l, "unit_name_14"] = "Valtiatar";
   %this.str[%l, "unit_name_15"] = "Demoni";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "OHI";
   %this.str[%l, "badge_atk_prefix"] = "H";
   %this.str[%l, "badge_def_prefix"] = "P";
   %this.str[%l, "hud_role_melee"] = "Lähitaistelu";
   %this.str[%l, "hud_role_ranged"] = "Kaukotaistelu";
   %this.str[%l, "tooltip_header"] = "%1 (Taso %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "KP: %1 / %2";
   %this.str[%l, "stat_attack"] = "Hyökkäys";
   %this.str[%l, "stat_defence"] = "Puolustus";
   %this.str[%l, "dice_stat_one"] = "%1: %2 noppa";
   %this.str[%l, "dice_stat_other"] = "%1: %2 noppaa";
   %this.str[%l, "dice_stat_mod_one"] = "%1: %2 noppa (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1: %2 noppaa (%3)";
   %this.str[%l, "status_sick"] = "Tila: Sairas (ohittaa vuoron)";
   %this.str[%l, "status_turn_both"] = "Vuoro: Liike ja toiminto valmis";
   %this.str[%l, "status_turn_action"] = "Vuoro: Toiminto valmis";
   %this.str[%l, "status_turn_move"] = "Vuoro: Liike valmis";
   %this.str[%l, "status_turn_finishing"] = "Vuoro: Päättymässä";
   %this.str[%l, "trait_ghostly_touch"] = "Kyky: Aavekosketus";
   %this.str[%l, "trait_disease"] = "Kyky: Tauti";
   %this.str[%l, "status_mana"] = "Mana: %1";
   %this.str[%l, "status_enemy_unit"] = "Vihollisyksikkö";
   %this.str[%l, "status_allied_unit"] = "Liittolaisyksikkö";
   %this.str[%l, "status_recruit_cost"] = "Hinta: %1 OP";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "VOITTO";
   %this.str[%l, "summary_defeat"] = "TAPPIO";
   %this.str[%l, "summary_sub_victory"] = "%1 on lyöty!";
   %this.str[%l, "summary_sub_defeat"] = "Joukkosi kukistuivat (%1).";
   %this.str[%l, "hero_class_warrior"] = "Soturi";
   %this.str[%l, "hero_class_mage"] = "Velho";
   %this.str[%l, "summary_turns_one"] = "%1 vuoro";
   %this.str[%l, "summary_turns_other"] = "%1 vuoroa";
   %this.str[%l, "summary_meta"] = "Sankari: %1  |  Kesto: %2";
   %this.str[%l, "summary_player_header"] = "PELAAJAN JOUKOT (%1)";
   %this.str[%l, "summary_enemy_header"] = "VIHOLLISEN JOUKOT (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 yksikkö elossa";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 yksikköä elossa";
   %this.str[%l, "summary_detail"] = "KP jäljellä: %1  |  Kaatuneet: %2";
   %this.str[%l, "summary_roster_header"] = "ELOONJÄÄNEET YKSIKÖT";
   %this.str[%l, "summary_none_survived"] = "Yksikään yksikkö ei selvinnyt taistelusta.";
   %this.str[%l, "summary_none_short"] = "Ei yhtään";
   %this.str[%l, "summary_more_units_one"] = "... ja %1 muu eloonjäänyt yksikkö";
   %this.str[%l, "summary_more_units_other"] = "... ja %1 muuta eloonjäänyttä yksikköä";
   %this.str[%l, "summary_unit_row"] = "• %1 (Taso %2) — KP: %3  |  HYÖ: %4  |  PUO: %5";
   %this.str[%l, "summary_play_again"] = "PELAA UUDELLEEN";
   %this.str[%l, "summary_main_menu"] = "PALAA PÄÄVALIKKOON";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 puolustusheitto";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 puolustusheittoa";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 mana";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 manaa";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 loitsuvahinko";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 loitsuvahinkoa";
   %this.str[%l, "fct_hp_loss"] = "-%1 KP";
   %this.str[%l, "fct_skip_turn"] = "Vuoro ohitetaan!";

   // Credits
   %this.str[%l, "credits_title"] = "TEKIJÄT";
   %this.str[%l, "credits_role_designer"] = "PELISUUNNITTELU";
   %this.str[%l, "credits_role_programmer"] = "OHJELMOINTI";
   %this.str[%l, "credits_role_artist"] = "2D- JA 3D-TAIDE";
   %this.str[%l, "credits_role_music"] = "MUSIIKKI JA ÄÄNET";
   %this.str[%l, "credits_role_level"] = "KENTTÄSUUNNITTELU";
   %this.str[%l, "credits_role_qa"] = "LAADUNVARMISTUS";
   %this.str[%l, "credits_role_thanks"] = "ERITYISKIITOKSET";
}

//---------------------------------------------------------------------------------------------
// English (en) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populateEnglish(%this)
{
   %l = "en";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Choose a Hero";
   %this.str[%l, "banner_place_creatures"] = "Place creatures on the board";
   %this.str[%l, "banner_countdown_one"] = "Game starts in %1 second...";
   %this.str[%l, "banner_countdown_other"] = "Game starts in %1 seconds...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Warrior";
   %this.str[%l, "card_warrior_desc"] = "The Warrior is a skilled fighter.";
   %this.str[%l, "card_wizard_title"] = "Wizard";
   %this.str[%l, "card_wizard_desc"] = "The Wizard knows how to cast spells.";
   %this.str[%l, "card_sausage_title"] = "Magic Sausage";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 creature defence roll";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 creature defence rolls";
   %this.str[%l, "card_magichat_title"] = "Magic Hat";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 spell point";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 spell points";
   %this.str[%l, "card_lollipop_title"] = "Magic Lollipop";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 spell damage point";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 spell damage points";
   %this.str[%l, "card_volcano_title"] = "Volcano";
   %this.str[%l, "card_volcano_desc"] = "A mighty volcano destroys the enemy army.";
   %this.str[%l, "card_dandelion_title"] = "Dandelion Root";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 spell point";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 spell points";
   %this.str[%l, "card_stomachflu_title"] = "Stomach Flu";
   %this.str[%l, "card_stomachflu_desc"] = "A sickened creature cannot act on its next turn.";

   // Title Menu
   %this.str[%l, "menu_play"] = "PLAY";
   %this.str[%l, "menu_resume"] = "RESUME GAME";
   %this.str[%l, "menu_resume_none"] = "RESUME GAME (NONE)";
   %this.str[%l, "menu_options"] = "OPTIONS";
   %this.str[%l, "menu_credits"] = "CREDITS";
   %this.str[%l, "menu_quit"] = "QUIT";
   %this.str[%l, "menu_lang_btn"] = "LANGUAGE: ENGLISH (EN)";
   %this.str[%l, "menu_confirm_title"] = "START NEW GAME?";
   %this.str[%l, "menu_confirm_body"] = "A game is in progress. Starting a new game will discard it.";
   %this.str[%l, "menu_confirm_yes"] = "NEW GAME";
   %this.str[%l, "menu_confirm_no"] = "CANCEL";

   // Options Menu
   %this.str[%l, "options_title"] = "SETTINGS & FACTION OPTIONS";
   %this.str[%l, "options_aspect"] = "Aspect Ratio:";
   %this.str[%l, "options_resolution"] = "Resolution:";
   %this.str[%l, "options_mode"] = "Display Mode:";
   %this.str[%l, "options_windowed"] = "WINDOWED";
   %this.str[%l, "options_fullscreen"] = "FULLSCREEN";
   %this.str[%l, "options_audio"] = "Audio Controls:";
   %this.str[%l, "options_faction"] = "Enemy Faction:";
   %this.str[%l, "options_language"] = "Language:";
   %this.str[%l, "options_ui_scale"] = "UI Scale: %1%";
   %this.str[%l, "options_apply"] = "APPLY VIDEO SETTINGS";
   %this.str[%l, "options_close"] = "CLOSE";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "GAME PAUSED";
   %this.str[%l, "pause_resume"] = "RESUME GAME";
   %this.str[%l, "pause_options"] = "OPTIONS";
   %this.str[%l, "pause_title_screen"] = "RETURN TO TITLE SCREEN";
   %this.str[%l, "pause_quit"] = "QUIT GAME";
   %this.str[%l, "hud_menu"] = "MENU";
   %this.str[%l, "hud_sfx_on"] = "SFX: ON";
   %this.str[%l, "hud_sfx_muted"] = "SFX: MUTED";
   %this.str[%l, "hud_music_on"] = "MUSIC: ON";
   %this.str[%l, "hud_music_muted"] = "MUSIC: MUTED";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Early Access";
   %this.str[%l, "watermark_version"] = "Version: %1";
   %this.str[%l, "watermark_build"] = "Build: %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Human Order";
   %this.str[%l, "faction_human_order_upper"] = "HUMAN ORDER";
   %this.str[%l, "faction_short_undead"] = "UNDEAD";
   %this.str[%l, "faction_short_dragonfolk"] = "DRAGONFOLK";
   %this.str[%l, "faction_short_gremlin"] = "GREMLINS";
   %this.str[%l, "faction_undead_name"] = "Undead Legion";
   %this.str[%l, "faction_undead_upper"] = "UNDEAD LEGION";
   %this.str[%l, "faction_undead_subtitle"] = "The Risen Dead";
   %this.str[%l, "faction_undead_desc"] = "Cursed skeletal warriors and vengeful spectres arising from ancient crypts. Wields Ghostly Touch and Disease afflictions.";
   %this.str[%l, "faction_undead_spec"] = "Necrotic afflictions: Disease (-1 ATK) and Ghostly Touch (-2 DEF).";
   %this.str[%l, "faction_dragonfolk_name"] = "Dragonfolk Faction";
   %this.str[%l, "faction_dragonfolk_upper"] = "DRAGONFOLK FACTION";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "The Scaled Clan";
   %this.str[%l, "faction_dragonfolk_desc"] = "Proud reptilian warriors, flame-breathing drakes, and sovereign gold dragons commanding elemental breath and draconic might.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Scaled fury: High durability, blazing breath & lightning strikes.";
   %this.str[%l, "faction_gremlin_name"] = "Gremlin Faction";
   %this.str[%l, "faction_gremlin_upper"] = "GREMLIN FACTION";
   %this.str[%l, "faction_gremlin_subtitle"] = "The Underworld Horde";
   %this.str[%l, "faction_gremlin_desc"] = "Crafty underworld imps, cyclopean eye-beam horrors, whips, and towering infernal demons surging from the subterranean abyss.";
   %this.str[%l, "faction_gremlin_spec"] = "Underworld menace: Vicious swarm, eye-beams & infernal demon lord.";

   %this.str[%l, "faction_select_title"] = "CHOOSE ENEMY FACTION";
   %this.str[%l, "faction_select_subtitle"] = "Select which opposing legion will confront you on the board";
   %this.str[%l, "faction_select_btn"] = "SELECT";
   %this.str[%l, "faction_selected_btn"] = "[ SELECTED ]";
   %this.str[%l, "faction_change_hero_btn"] = "CHANGE HERO";
   %this.str[%l, "faction_begin_battle_btn"] = "BEGIN BATTLE";

   // Units
   %this.str[%l, "unit_fallback"] = "Unit";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (Lvl %2)";
   %this.str[%l, "unit_name_0"] = "Warrior";
   %this.str[%l, "unit_name_1"] = "Archer";
   %this.str[%l, "unit_name_2"] = "Cleric";
   %this.str[%l, "unit_name_3"] = "Paladin";
   %this.str[%l, "unit_name_4"] = "Skeleton Warrior";
   %this.str[%l, "unit_name_5"] = "Skeleton Archer";
   %this.str[%l, "unit_name_6"] = "Dread Zombie";
   %this.str[%l, "unit_name_7"] = "Ancient Ghost";
   %this.str[%l, "unit_name_8"] = "Dragon Folk";
   %this.str[%l, "unit_name_9"] = "Red Dragon";
   %this.str[%l, "unit_name_10"] = "Air Dragon";
   %this.str[%l, "unit_name_11"] = "Gold Dragon";
   %this.str[%l, "unit_name_12"] = "Gremlin";
   %this.str[%l, "unit_name_13"] = "Horned One-Eye";
   %this.str[%l, "unit_name_14"] = "Mistress";
   %this.str[%l, "unit_name_15"] = "Demon";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "SKIP";
   %this.str[%l, "badge_atk_prefix"] = "A";
   %this.str[%l, "badge_def_prefix"] = "D";
   %this.str[%l, "hud_role_melee"] = "Melee";
   %this.str[%l, "hud_role_ranged"] = "Ranged";
   %this.str[%l, "tooltip_header"] = "%1 (Lvl %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "HP: %1 / %2";
   %this.str[%l, "stat_attack"] = "Attack";
   %this.str[%l, "stat_defence"] = "Defence";
   %this.str[%l, "dice_stat_one"] = "%1: %2 die";
   %this.str[%l, "dice_stat_other"] = "%1: %2 dice";
   %this.str[%l, "dice_stat_mod_one"] = "%1: %2 die (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1: %2 dice (%3)";
   %this.str[%l, "status_sick"] = "Status: Sick (Skips Next Turn)";
   %this.str[%l, "status_turn_both"] = "Active Turn: Move & Action Ready";
   %this.str[%l, "status_turn_action"] = "Active Turn: Action Ready";
   %this.str[%l, "status_turn_move"] = "Active Turn: Move Ready";
   %this.str[%l, "status_turn_finishing"] = "Active Turn: Finishing";
   %this.str[%l, "trait_ghostly_touch"] = "Trait: Ghostly Touch";
   %this.str[%l, "trait_disease"] = "Trait: Disease";
   %this.str[%l, "status_mana"] = "Mana: %1";
   %this.str[%l, "status_enemy_unit"] = "Enemy Unit";
   %this.str[%l, "status_allied_unit"] = "Allied Unit";
   %this.str[%l, "status_recruit_cost"] = "Recruit Cost: %1 CP";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "VICTORY";
   %this.str[%l, "summary_defeat"] = "DEFEAT";
   %this.str[%l, "summary_sub_victory"] = "The %1 has been defeated!";
   %this.str[%l, "summary_sub_defeat"] = "Your forces fell to the %1.";
   %this.str[%l, "hero_class_warrior"] = "Warrior";
   %this.str[%l, "hero_class_mage"] = "Mage";
   %this.str[%l, "summary_turns_one"] = "Turns Played: %1";
   %this.str[%l, "summary_turns_other"] = "Turns Played: %1";
   %this.str[%l, "summary_meta"] = "Hero: %1  |  %2";
   %this.str[%l, "summary_player_header"] = "PLAYER FORCES (%1)";
   %this.str[%l, "summary_enemy_header"] = "ENEMY FORCES (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 Unit Surviving";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 Units Surviving";
   %this.str[%l, "summary_detail"] = "Remaining HP: %1  |  Fallen: %2";
   %this.str[%l, "summary_roster_header"] = "SURVIVING UNIT TALLY";
   %this.str[%l, "summary_none_survived"] = "No units survived the battle.";
   %this.str[%l, "summary_none_short"] = "None";
   %this.str[%l, "summary_more_units_one"] = "... and %1 more surviving unit";
   %this.str[%l, "summary_more_units_other"] = "... and %1 more surviving units";
   %this.str[%l, "summary_unit_row"] = "• %1 (Lvl %2) — HP: %3  |  ATK: %4  |  DEF: %5";
   %this.str[%l, "summary_play_again"] = "PLAY AGAIN";
   %this.str[%l, "summary_main_menu"] = "RETURN TO MAIN MENU";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 Defence Roll";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 Defence Rolls";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 Mana";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 Mana";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 Spell Damage";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 Spell Damage";
   %this.str[%l, "fct_hp_loss"] = "-%1 HP";
   %this.str[%l, "fct_skip_turn"] = "Skip Turn!";

   // Credits
   %this.str[%l, "credits_title"] = "CREDITS";
   %this.str[%l, "credits_role_designer"] = "GAME DESIGNER";
   %this.str[%l, "credits_role_programmer"] = "PROGRAMMER";
   %this.str[%l, "credits_role_artist"] = "2D & 3D ARTIST";
   %this.str[%l, "credits_role_music"] = "MUSIC & SOUND";
   %this.str[%l, "credits_role_level"] = "LEVEL DESIGNER";
   %this.str[%l, "credits_role_qa"] = "QA & TESTING";
   %this.str[%l, "credits_role_thanks"] = "SPECIAL THANKS";
}

//---------------------------------------------------------------------------------------------
// French (fr) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populateFrench(%this)
{
   %l = "fr";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Choisissez un héros";
   %this.str[%l, "banner_place_creatures"] = "Placez vos créatures sur le plateau";
   %this.str[%l, "banner_countdown_one"] = "La partie commence dans %1 seconde...";
   %this.str[%l, "banner_countdown_other"] = "La partie commence dans %1 secondes...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Guerrière";
   %this.str[%l, "card_warrior_desc"] = "La guerrière est une combattante aguerrie.";
   %this.str[%l, "card_wizard_title"] = "Magicienne";
   %this.str[%l, "card_wizard_desc"] = "La magicienne maîtrise l'art des sortilèges.";
   %this.str[%l, "card_sausage_title"] = "Saucisse magique";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 dé de défense (créatures)";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 dés de défense (créatures)";
   %this.str[%l, "card_magichat_title"] = "Chapeau magique";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 point de sort";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 points de sort";
   %this.str[%l, "card_lollipop_title"] = "Sucette magique";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 dégât de sort";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 dégâts de sort";
   %this.str[%l, "card_volcano_title"] = "Volcan";
   %this.str[%l, "card_volcano_desc"] = "Un puissant volcan ravage l'armée ennemie.";
   %this.str[%l, "card_dandelion_title"] = "Racine de pissenlit";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 point de sort";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 points de sort";
   %this.str[%l, "card_stomachflu_title"] = "Gastro-entérite";
   %this.str[%l, "card_stomachflu_desc"] = "La créature malade ne peut rien faire à son prochain tour.";

   // Title Menu
   %this.str[%l, "menu_play"] = "JOUER";
   %this.str[%l, "menu_resume"] = "REPRENDRE LA PARTIE";
   %this.str[%l, "menu_resume_none"] = "REPRENDRE (AUCUNE)";
   %this.str[%l, "menu_options"] = "OPTIONS";
   %this.str[%l, "menu_credits"] = "CRÉDITS";
   %this.str[%l, "menu_quit"] = "QUITTER";
   %this.str[%l, "menu_lang_btn"] = "LANGUE : FRANÇAIS (FR)";
   %this.str[%l, "menu_confirm_title"] = "NOUVELLE PARTIE ?";
   %this.str[%l, "menu_confirm_body"] = "Une partie est en cours. En lancer une nouvelle l'effacera.";
   %this.str[%l, "menu_confirm_yes"] = "NOUVELLE PARTIE";
   %this.str[%l, "menu_confirm_no"] = "ANNULER";

   // Options Menu
   %this.str[%l, "options_title"] = "PARAMÈTRES ET FACTIONS";
   %this.str[%l, "options_aspect"] = "Format d'image :";
   %this.str[%l, "options_resolution"] = "Résolution :";
   %this.str[%l, "options_mode"] = "Mode d'affichage :";
   %this.str[%l, "options_windowed"] = "FENÊTRÉ";
   %this.str[%l, "options_fullscreen"] = "PLEIN ÉCRAN";
   %this.str[%l, "options_audio"] = "Contrôles audio :";
   %this.str[%l, "options_faction"] = "Faction ennemie :";
   %this.str[%l, "options_language"] = "Langue :";
   %this.str[%l, "options_ui_scale"] = "Échelle de l'interface : %1%";
   %this.str[%l, "options_apply"] = "APPLIQUER LA VIDÉO";
   %this.str[%l, "options_close"] = "FERMER";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "JEU EN PAUSE";
   %this.str[%l, "pause_resume"] = "REPRENDRE";
   %this.str[%l, "pause_options"] = "OPTIONS";
   %this.str[%l, "pause_title_screen"] = "ÉCRAN TITRE";
   %this.str[%l, "pause_quit"] = "QUITTER LE JEU";
   %this.str[%l, "hud_menu"] = "MENU";
   %this.str[%l, "hud_sfx_on"] = "EFFETS : ON";
   %this.str[%l, "hud_sfx_muted"] = "EFFETS : MUET";
   %this.str[%l, "hud_music_on"] = "MUSIQUE : ON";
   %this.str[%l, "hud_music_muted"] = "MUSIQUE : MUET";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Accès anticipé";
   %this.str[%l, "watermark_version"] = "Version : %1";
   %this.str[%l, "watermark_build"] = "Build : %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Ordre humain";
   %this.str[%l, "faction_human_order_upper"] = "ORDRE HUMAIN";
   %this.str[%l, "faction_short_undead"] = "MORTS-VIVANTS";
   %this.str[%l, "faction_short_dragonfolk"] = "DRACONIENS";
   %this.str[%l, "faction_short_gremlin"] = "GREMLINS";
   %this.str[%l, "faction_undead_name"] = "Légion des Morts-Vivants";
   %this.str[%l, "faction_undead_upper"] = "LÉGION DES MORTS-VIVANTS";
   %this.str[%l, "faction_undead_subtitle"] = "Les Ressuscités";
   %this.str[%l, "faction_undead_desc"] = "Squelettes maudits et spectres vengeurs surgis des cryptes anciennes. Infligent Contact spectral et Maladie.";
   %this.str[%l, "faction_undead_spec"] = "Fléaux nécrotiques : Maladie (-1 ATQ) et Contact spectral (-2 DÉF).";
   %this.str[%l, "faction_dragonfolk_name"] = "Peuple Draconien";
   %this.str[%l, "faction_dragonfolk_upper"] = "PEUPLE DRACONIEN";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "Le Clan Écaillé";
   %this.str[%l, "faction_dragonfolk_desc"] = "Fiers guerriers reptiliens, dragons cracheurs de feu et souverains dorés maniant les éléments.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Fureur draconique : Haute endurance, souffle de feu et éclairs.";
   %this.str[%l, "faction_gremlin_name"] = "Horde des Gremlins";
   %this.str[%l, "faction_gremlin_upper"] = "HORDE DES GREMLINS";
   %this.str[%l, "faction_gremlin_subtitle"] = "La Horde Souterraine";
   %this.str[%l, "faction_gremlin_desc"] = "Lutins fourbes des abysses, cyclopes à rayon oculaire et démons infernaux jaillissant des profondeurs.";
   %this.str[%l, "faction_gremlin_spec"] = "Menace abyssale : Essaim vicieux, rayons oculaires et seigneur démon.";

   %this.str[%l, "faction_select_title"] = "CHOISIR LA FACTION ENNEMIE";
   %this.str[%l, "faction_select_subtitle"] = "Sélectionnez la légion adverse que vous affronterez sur le plateau";
   %this.str[%l, "faction_select_btn"] = "CHOISIR";
   %this.str[%l, "faction_selected_btn"] = "[ SÉLECTIONNÉ ]";
   %this.str[%l, "faction_change_hero_btn"] = "CHANGER DE HÉROS";
   %this.str[%l, "faction_begin_battle_btn"] = "LANCER LE COMBAT";

   // Units
   %this.str[%l, "unit_fallback"] = "Unité";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (Niv. %2)";
   %this.str[%l, "unit_name_0"] = "Guerrier";
   %this.str[%l, "unit_name_1"] = "Archer";
   %this.str[%l, "unit_name_2"] = "Clerc";
   %this.str[%l, "unit_name_3"] = "Paladin";
   %this.str[%l, "unit_name_4"] = "Guerrier squelette";
   %this.str[%l, "unit_name_5"] = "Archer squelette";
   %this.str[%l, "unit_name_6"] = "Zombie sinistre";
   %this.str[%l, "unit_name_7"] = "Spectre ancien";
   %this.str[%l, "unit_name_8"] = "Homme-dragon";
   %this.str[%l, "unit_name_9"] = "Dragon rouge";
   %this.str[%l, "unit_name_10"] = "Dragon des airs";
   %this.str[%l, "unit_name_11"] = "Dragon d'or";
   %this.str[%l, "unit_name_12"] = "Gremlin";
   %this.str[%l, "unit_name_13"] = "Cyclope cornu";
   %this.str[%l, "unit_name_14"] = "Maîtresse";
   %this.str[%l, "unit_name_15"] = "Démon";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "PASS";
   %this.str[%l, "badge_atk_prefix"] = "A";
   %this.str[%l, "badge_def_prefix"] = "D";
   %this.str[%l, "hud_role_melee"] = "Mêlée";
   %this.str[%l, "hud_role_ranged"] = "À distance";
   %this.str[%l, "tooltip_header"] = "%1 (Niv. %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "PV : %1 / %2";
   %this.str[%l, "stat_attack"] = "Attaque";
   %this.str[%l, "stat_defence"] = "Défense";
   %this.str[%l, "dice_stat_one"] = "%1 : %2 dé";
   %this.str[%l, "dice_stat_other"] = "%1 : %2 dés";
   %this.str[%l, "dice_stat_mod_one"] = "%1 : %2 dé (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1 : %2 dés (%3)";
   %this.str[%l, "status_sick"] = "État : Malade (passe son tour)";
   %this.str[%l, "status_turn_both"] = "Tour actif : Mouvement et action";
   %this.str[%l, "status_turn_action"] = "Tour actif : Action prête";
   %this.str[%l, "status_turn_move"] = "Tour actif : Mouvement prêt";
   %this.str[%l, "status_turn_finishing"] = "Tour actif : Fin du tour";
   %this.str[%l, "trait_ghostly_touch"] = "Trait : Contact spectral";
   %this.str[%l, "trait_disease"] = "Trait : Maladie";
   %this.str[%l, "status_mana"] = "Mana : %1";
   %this.str[%l, "status_enemy_unit"] = "Unité ennemie";
   %this.str[%l, "status_allied_unit"] = "Unité alliée";
   %this.str[%l, "status_recruit_cost"] = "Coût : %1 PC";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "VICTOIRE";
   %this.str[%l, "summary_defeat"] = "DÉFAITE";
   %this.str[%l, "summary_sub_victory"] = "L'armée (%1) a été vaincue !";
   %this.str[%l, "summary_sub_defeat"] = "Vos forces ont succombé face à : %1.";
   %this.str[%l, "hero_class_warrior"] = "Guerrière";
   %this.str[%l, "hero_class_mage"] = "Magicienne";
   %this.str[%l, "summary_turns_one"] = "%1 tour joué";
   %this.str[%l, "summary_turns_other"] = "%1 tours joués";
   %this.str[%l, "summary_meta"] = "Héros : %1  |  %2";
   %this.str[%l, "summary_player_header"] = "FORCES DU JOUEUR (%1)";
   %this.str[%l, "summary_enemy_header"] = "FORCES ENNEMIES (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 survivant";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 survivants";
   %this.str[%l, "summary_detail"] = "PV restants : %1  |  Pertes : %2";
   %this.str[%l, "summary_roster_header"] = "BILAN DES SURVIVANTS";
   %this.str[%l, "summary_none_survived"] = "Aucune unité n'a survécu au combat.";
   %this.str[%l, "summary_none_short"] = "Aucun";
   %this.str[%l, "summary_more_units_one"] = "... et %1 autre unité survivante";
   %this.str[%l, "summary_more_units_other"] = "... et %1 autres unités survivantes";
   %this.str[%l, "summary_unit_row"] = "• %1 (Niv. %2) — PV : %3  |  ATQ : %4  |  DÉF : %5";
   %this.str[%l, "summary_play_again"] = "REJOUER";
   %this.str[%l, "summary_main_menu"] = "MENU PRINCIPAL";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 jet de défense";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 jets de défense";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 Mana";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 Mana";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 dégât magique";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 dégâts magiques";
   %this.str[%l, "fct_hp_loss"] = "-%1 PV";
   %this.str[%l, "fct_skip_turn"] = "Passe son tour !";

   // Credits
   %this.str[%l, "credits_title"] = "CRÉDITS";
   %this.str[%l, "credits_role_designer"] = "CONCEPTION DU JEU";
   %this.str[%l, "credits_role_programmer"] = "PROGRAMMATION";
   %this.str[%l, "credits_role_artist"] = "ART 2D ET 3D";
   %this.str[%l, "credits_role_music"] = "MUSIQUE ET SON";
   %this.str[%l, "credits_role_level"] = "CONCEPTION NIVEAUX";
   %this.str[%l, "credits_role_qa"] = "TESTS ET QUALITÉ";
   %this.str[%l, "credits_role_thanks"] = "REMERCIEMENTS";
}

//---------------------------------------------------------------------------------------------
// Spanish (es) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populateSpanish(%this)
{
   %l = "es";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Elige un héroe";
   %this.str[%l, "banner_place_creatures"] = "Coloca tus criaturas en el tablero";
   %this.str[%l, "banner_countdown_one"] = "La partida comienza en %1 segundo...";
   %this.str[%l, "banner_countdown_other"] = "La partida comienza en %1 segundos...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Guerrera";
   %this.str[%l, "card_warrior_desc"] = "La guerrera es una combatiente hábil.";
   %this.str[%l, "card_wizard_title"] = "Hechicera";
   %this.str[%l, "card_wizard_desc"] = "La hechicera domina el arte de la magia.";
   %this.str[%l, "card_sausage_title"] = "Salchicha mágica";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 dado de defensa (criaturas)";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 dados de defensa (criaturas)";
   %this.str[%l, "card_magichat_title"] = "Sombrero mágico";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 punto de hechizo";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 puntos de hechizo";
   %this.str[%l, "card_lollipop_title"] = "Piruleta mágica";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 punto de daño mágico";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 puntos de daño mágico";
   %this.str[%l, "card_volcano_title"] = "Volcán";
   %this.str[%l, "card_volcano_desc"] = "Un poderoso volcán arrasa al ejército enemigo.";
   %this.str[%l, "card_dandelion_title"] = "Raíz de diente de león";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 punto de hechizo";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 puntos de hechizo";
   %this.str[%l, "card_stomachflu_title"] = "Gripe estomacal";
   %this.str[%l, "card_stomachflu_desc"] = "La criatura enferma no podrá actuar en su próximo turno.";

   // Title Menu
   %this.str[%l, "menu_play"] = "JUGAR";
   %this.str[%l, "menu_resume"] = "REANUDAR PARTIDA";
   %this.str[%l, "menu_resume_none"] = "REANUDAR (NINGUNA)";
   %this.str[%l, "menu_options"] = "OPCIONES";
   %this.str[%l, "menu_credits"] = "CRÉDITOS";
   %this.str[%l, "menu_quit"] = "SALIR";
   %this.str[%l, "menu_lang_btn"] = "IDIOMA: ESPAÑOL (ES)";
   %this.str[%l, "menu_confirm_title"] = "¿INICIAR NUEVA PARTIDA?";
   %this.str[%l, "menu_confirm_body"] = "Hay una partida en curso. Empezar una nueva la descartará.";
   %this.str[%l, "menu_confirm_yes"] = "NUEVA PARTIDA";
   %this.str[%l, "menu_confirm_no"] = "CANCELAR";

   // Options Menu
   %this.str[%l, "options_title"] = "AJUSTES Y FACCIONES";
   %this.str[%l, "options_aspect"] = "Relación de aspecto:";
   %this.str[%l, "options_resolution"] = "Resolución:";
   %this.str[%l, "options_mode"] = "Modo de pantalla:";
   %this.str[%l, "options_windowed"] = "VENTANA";
   %this.str[%l, "options_fullscreen"] = "PANTALLA COMPLETA";
   %this.str[%l, "options_audio"] = "Controles de audio:";
   %this.str[%l, "options_faction"] = "Facción enemiga:";
   %this.str[%l, "options_language"] = "Idioma:";
   %this.str[%l, "options_ui_scale"] = "Escala de interfaz: %1%";
   %this.str[%l, "options_apply"] = "APLICAR VÍDEO";
   %this.str[%l, "options_close"] = "CERRAR";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "JUEGO EN PAUSA";
   %this.str[%l, "pause_resume"] = "REANUDAR";
   %this.str[%l, "pause_options"] = "OPCIONES";
   %this.str[%l, "pause_title_screen"] = "VOLVER AL MENÚ";
   %this.str[%l, "pause_quit"] = "SALIR DEL JUEGO";
   %this.str[%l, "hud_menu"] = "MENÚ";
   %this.str[%l, "hud_sfx_on"] = "EFECTOS: SÍ";
   %this.str[%l, "hud_sfx_muted"] = "EFECTOS: NO";
   %this.str[%l, "hud_music_on"] = "MÚSICA: SÍ";
   %this.str[%l, "hud_music_muted"] = "MÚSICA: NO";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Acceso anticipado";
   %this.str[%l, "watermark_version"] = "Versión: %1";
   %this.str[%l, "watermark_build"] = "Compilación: %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Orden Humana";
   %this.str[%l, "faction_human_order_upper"] = "ORDEN HUMANA";
   %this.str[%l, "faction_short_undead"] = "NO MUERTOS";
   %this.str[%l, "faction_short_dragonfolk"] = "DRACÓNIDOS";
   %this.str[%l, "faction_short_gremlin"] = "GREMLINS";
   %this.str[%l, "faction_undead_name"] = "Legión No Muerta";
   %this.str[%l, "faction_undead_upper"] = "LEGIÓN NO MUERTA";
   %this.str[%l, "faction_undead_subtitle"] = "Los Alzados";
   %this.str[%l, "faction_undead_desc"] = "Esqueletos malditos y espectros vengativos surgidos de criptas antiguas. Usan Toque espectral y Enfermedad.";
   %this.str[%l, "faction_undead_spec"] = "Aflicciones necróticas: Enfermedad (-1 ATQ) y Toque espectral (-2 DEF).";
   %this.str[%l, "faction_dragonfolk_name"] = "Clan Dracónido";
   %this.str[%l, "faction_dragonfolk_upper"] = "CLAN DRACÓNIDO";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "El Clan Escamado";
   %this.str[%l, "faction_dragonfolk_desc"] = "Orgullosos guerreros reptiles, dragones de fuego y dragones dorados que dominan el aliento elemental.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Furia escamada: Gran resistencia, aliento ardiente y rayos.";
   %this.str[%l, "faction_gremlin_name"] = "Horda Gremlin";
   %this.str[%l, "faction_gremlin_upper"] = "HORDA GREMLIN";
   %this.str[%l, "faction_gremlin_subtitle"] = "La Horda Subterránea";
   %this.str[%l, "faction_gremlin_desc"] = "Astutos trasgos del subsuelo, cíclopes de rayo ocular y demonios infernales del abismo.";
   %this.str[%l, "faction_gremlin_spec"] = "Amenaza abismal: Enjambre feroz, rayos oculares y señor demoníaco.";

   %this.str[%l, "faction_select_title"] = "ELIGE LA FACCIÓN ENEMIGA";
   %this.str[%l, "faction_select_subtitle"] = "Selecciona qué legión rival te enfrentará en el tablero";
   %this.str[%l, "faction_select_btn"] = "ELEGIR";
   %this.str[%l, "faction_selected_btn"] = "[ ELEGIDA ]";
   %this.str[%l, "faction_change_hero_btn"] = "CAMBIAR HÉROE";
   %this.str[%l, "faction_begin_battle_btn"] = "COMENZAR BATALLA";

   // Units
   %this.str[%l, "unit_fallback"] = "Unidad";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (Niv. %2)";
   %this.str[%l, "unit_name_0"] = "Guerrero";
   %this.str[%l, "unit_name_1"] = "Arquera";
   %this.str[%l, "unit_name_2"] = "Clérigo";
   %this.str[%l, "unit_name_3"] = "Paladín";
   %this.str[%l, "unit_name_4"] = "Guerrero esqueleto";
   %this.str[%l, "unit_name_5"] = "Arquero esqueleto";
   %this.str[%l, "unit_name_6"] = "Zombi temible";
   %this.str[%l, "unit_name_7"] = "Espectro antiguo";
   %this.str[%l, "unit_name_8"] = "Dracónido";
   %this.str[%l, "unit_name_9"] = "Dragón rojo";
   %this.str[%l, "unit_name_10"] = "Dragón aéreo";
   %this.str[%l, "unit_name_11"] = "Dragón dorado";
   %this.str[%l, "unit_name_12"] = "Gremlin";
   %this.str[%l, "unit_name_13"] = "Cíclope cornudo";
   %this.str[%l, "unit_name_14"] = "Dominadora";
   %this.str[%l, "unit_name_15"] = "Demonio";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "PASA";
   %this.str[%l, "badge_atk_prefix"] = "A";
   %this.str[%l, "badge_def_prefix"] = "D";
   %this.str[%l, "hud_role_melee"] = "Cuerpo a cuerpo";
   %this.str[%l, "hud_role_ranged"] = "A distancia";
   %this.str[%l, "tooltip_header"] = "%1 (Niv. %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "PS: %1 / %2";
   %this.str[%l, "stat_attack"] = "Ataque";
   %this.str[%l, "stat_defence"] = "Defensa";
   %this.str[%l, "dice_stat_one"] = "%1: %2 dado";
   %this.str[%l, "dice_stat_other"] = "%1: %2 dados";
   %this.str[%l, "dice_stat_mod_one"] = "%1: %2 dado (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1: %2 dados (%3)";
   %this.str[%l, "status_sick"] = "Estado: Enfermo (pierde turno)";
   %this.str[%l, "status_turn_both"] = "Turno activo: Mover y actuar";
   %this.str[%l, "status_turn_action"] = "Turno activo: Acción lista";
   %this.str[%l, "status_turn_move"] = "Turno activo: Movimiento listo";
   %this.str[%l, "status_turn_finishing"] = "Turno activo: Finalizando";
   %this.str[%l, "trait_ghostly_touch"] = "Rasgo: Toque espectral";
   %this.str[%l, "trait_disease"] = "Rasgo: Enfermedad";
   %this.str[%l, "status_mana"] = "Maná: %1";
   %this.str[%l, "status_enemy_unit"] = "Unidad enemiga";
   %this.str[%l, "status_allied_unit"] = "Unidad aliada";
   %this.str[%l, "status_recruit_cost"] = "Coste: %1 PC";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "VICTORIA";
   %this.str[%l, "summary_defeat"] = "DERROTA";
   %this.str[%l, "summary_sub_victory"] = "¡%1 ha sido derrotada!";
   %this.str[%l, "summary_sub_defeat"] = "Tus fuerzas cayeron ante: %1.";
   %this.str[%l, "hero_class_warrior"] = "Guerrera";
   %this.str[%l, "hero_class_mage"] = "Hechicera";
   %this.str[%l, "summary_turns_one"] = "%1 turno";
   %this.str[%l, "summary_turns_other"] = "%1 turnos";
   %this.str[%l, "summary_meta"] = "Héroe: %1  |  %2";
   %this.str[%l, "summary_player_header"] = "FUERZAS JUGADOR (%1)";
   %this.str[%l, "summary_enemy_header"] = "FUERZAS ENEMIGAS (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 superviviente";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 supervivientes";
   %this.str[%l, "summary_detail"] = "PS restantes: %1  |  Bajas: %2";
   %this.str[%l, "summary_roster_header"] = "UNIDADES SUPERVIVIENTES";
   %this.str[%l, "summary_none_survived"] = "Ninguna unidad sobrevivió a la batalla.";
   %this.str[%l, "summary_none_short"] = "Ninguna";
   %this.str[%l, "summary_more_units_one"] = "... y %1 unidad superviviente más";
   %this.str[%l, "summary_more_units_other"] = "... y %1 unidades supervivientes más";
   %this.str[%l, "summary_unit_row"] = "• %1 (Niv. %2) — PS: %3  |  ATQ: %4  |  DEF: %5";
   %this.str[%l, "summary_play_again"] = "JUGAR DE NUEVO";
   %this.str[%l, "summary_main_menu"] = "MENÚ PRINCIPAL";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 dado de defensa";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 dados de defensa";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 Maná";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 Maná";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 daño mágico";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 daño mágico";
   %this.str[%l, "fct_hp_loss"] = "-%1 PS";
   %this.str[%l, "fct_skip_turn"] = "¡Pierde turno!";

   // Credits
   %this.str[%l, "credits_title"] = "CRÉDITOS";
   %this.str[%l, "credits_role_designer"] = "DISEÑO DEL JUEGO";
   %this.str[%l, "credits_role_programmer"] = "PROGRAMACIÓN";
   %this.str[%l, "credits_role_artist"] = "ARTE 2D Y 3D";
   %this.str[%l, "credits_role_music"] = "MÚSICA Y SONIDO";
   %this.str[%l, "credits_role_level"] = "DISEÑO DE NIVELES";
   %this.str[%l, "credits_role_qa"] = "PRUEBAS Y CALIDAD";
   %this.str[%l, "credits_role_thanks"] = "AGRADECIMIENTOS";
}

//---------------------------------------------------------------------------------------------
// Brazilian Portuguese (pt-BR) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populatePortugueseBR(%this)
{
   %l = "pt-BR";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Escolha seu herói";
   %this.str[%l, "banner_place_creatures"] = "Posicione as criaturas no tabuleiro";
   %this.str[%l, "banner_countdown_one"] = "A partida começa em %1 segundo...";
   %this.str[%l, "banner_countdown_other"] = "A partida começa em %1 segundos...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Guerreira";
   %this.str[%l, "card_warrior_desc"] = "A guerreira é uma combatente habilidosa.";
   %this.str[%l, "card_wizard_title"] = "Feiticeira";
   %this.str[%l, "card_wizard_desc"] = "A feiticeira domina a arte dos feitiços.";
   %this.str[%l, "card_sausage_title"] = "Salsicha Mágica";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 dado de defesa (criaturas)";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 dados de defesa (criaturas)";
   %this.str[%l, "card_magichat_title"] = "Chapéu Mágico";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 ponto de magia";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 pontos de magia";
   %this.str[%l, "card_lollipop_title"] = "Pirulito Mágico";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 ponto de dano mágico";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 pontos de dano mágico";
   %this.str[%l, "card_volcano_title"] = "Vulcão";
   %this.str[%l, "card_volcano_desc"] = "Um poderoso vulcão devasta o exército inimigo.";
   %this.str[%l, "card_dandelion_title"] = "Raiz de Dente-de-Leão";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 ponto de magia";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 pontos de magia";
   %this.str[%l, "card_stomachflu_title"] = "Dor de Barriga";
   %this.str[%l, "card_stomachflu_desc"] = "A criatura doente não pode agir no próximo turno.";

   // Title Menu
   %this.str[%l, "menu_play"] = "JOGAR";
   %this.str[%l, "menu_resume"] = "CONTINUAR JOGO";
   %this.str[%l, "menu_resume_none"] = "CONTINUAR (NENHUM)";
   %this.str[%l, "menu_options"] = "OPÇÕES";
   %this.str[%l, "menu_credits"] = "CRÉDITOS";
   %this.str[%l, "menu_quit"] = "SAIR";
   %this.str[%l, "menu_lang_btn"] = "IDIOMA: PORTUGUÊS (BR)";
   %this.str[%l, "menu_confirm_title"] = "INICIAR NOVO JOGO?";
   %this.str[%l, "menu_confirm_body"] = "Há um jogo em andamento. Começar outro irá descartá-lo.";
   %this.str[%l, "menu_confirm_yes"] = "NOVO JOGO";
   %this.str[%l, "menu_confirm_no"] = "CANCELAR";

   // Options Menu
   %this.str[%l, "options_title"] = "CONFIGURAÇÕES E FACÇÕES";
   %this.str[%l, "options_aspect"] = "Proporção:";
   %this.str[%l, "options_resolution"] = "Resolução:";
   %this.str[%l, "options_mode"] = "Modo de exibição:";
   %this.str[%l, "options_windowed"] = "JANELA";
   %this.str[%l, "options_fullscreen"] = "TELA CHEIA";
   %this.str[%l, "options_audio"] = "Controles de áudio:";
   %this.str[%l, "options_faction"] = "Facção inimiga:";
   %this.str[%l, "options_language"] = "Idioma:";
   %this.str[%l, "options_ui_scale"] = "Escala da interface: %1%";
   %this.str[%l, "options_apply"] = "APLICAR VÍDEO";
   %this.str[%l, "options_close"] = "FECHAR";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "JOGO PAUSADO";
   %this.str[%l, "pause_resume"] = "CONTINUAR";
   %this.str[%l, "pause_options"] = "OPÇÕES";
   %this.str[%l, "pause_title_screen"] = "VOLTAR AO MENU";
   %this.str[%l, "pause_quit"] = "SAIR DO JOGO";
   %this.str[%l, "hud_menu"] = "MENU";
   %this.str[%l, "hud_sfx_on"] = "SONS: LIG.";
   %this.str[%l, "hud_sfx_muted"] = "SONS: MUDO";
   %this.str[%l, "hud_music_on"] = "MÚSICA: LIG.";
   %this.str[%l, "hud_music_muted"] = "MÚSICA: MUDO";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Acesso Antecipado";
   %this.str[%l, "watermark_version"] = "Versão: %1";
   %this.str[%l, "watermark_build"] = "Compilação: %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Ordem Humana";
   %this.str[%l, "faction_human_order_upper"] = "ORDEM HUMANA";
   %this.str[%l, "faction_short_undead"] = "MORTOS-VIVOS";
   %this.str[%l, "faction_short_dragonfolk"] = "DRACONATOS";
   %this.str[%l, "faction_short_gremlin"] = "GREMLINS";
   %this.str[%l, "faction_undead_name"] = "Legião dos Mortos-Vivos";
   %this.str[%l, "faction_undead_upper"] = "LEGIÃO DOS MORTOS-VIVOS";
   %this.str[%l, "faction_undead_subtitle"] = "Os Reerguidos";
   %this.str[%l, "faction_undead_desc"] = "Guerreiros esqueléticos amaldiçoados e espectros vingativos das criptas antigas. Usam Toque Espectral e Peste.";
   %this.str[%l, "faction_undead_spec"] = "Pragas necróticas: Peste (-1 ATQ) e Toque Espectral (-2 DEF).";
   %this.str[%l, "faction_dragonfolk_name"] = "Clã Draconato";
   %this.str[%l, "faction_dragonfolk_upper"] = "CLÃ DRACONATO";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "O Clã Escamado";
   %this.str[%l, "faction_dragonfolk_desc"] = "Orgulhosos guerreiros reptilianos, dragões flamejantes e dragões dourados soberanos dos elementos.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Fúria escamada: Alta resistência, sopro flamejante e relâmpagos.";
   %this.str[%l, "faction_gremlin_name"] = "Horda dos Gremlins";
   %this.str[%l, "faction_gremlin_upper"] = "HORDA DOS GREMLINS";
   %this.str[%l, "faction_gremlin_subtitle"] = "A Horda do Submundo";
   %this.str[%l, "faction_gremlin_desc"] = "Diabretes astutos do subsolo, ciclopes de raio ocular e demônios infernais surgidos do abismo.";
   %this.str[%l, "faction_gremlin_spec"] = "Ameaça abissal: Enxame feroz, raios oculares e lorde demoníaco.";

   %this.str[%l, "faction_select_title"] = "ESCOLHA A FACÇÃO INIMIGA";
   %this.str[%l, "faction_select_subtitle"] = "Selecione qual legião adversária enfrentará você no tabuleiro";
   %this.str[%l, "faction_select_btn"] = "SELECIONAR";
   %this.str[%l, "faction_selected_btn"] = "[ SELECIONADA ]";
   %this.str[%l, "faction_change_hero_btn"] = "TROCAR HERÓI";
   %this.str[%l, "faction_begin_battle_btn"] = "INICIAR BATALHA";

   // Units
   %this.str[%l, "unit_fallback"] = "Unidade";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (Nív. %2)";
   %this.str[%l, "unit_name_0"] = "Guerreiro";
   %this.str[%l, "unit_name_1"] = "Arqueira";
   %this.str[%l, "unit_name_2"] = "Clérigo";
   %this.str[%l, "unit_name_3"] = "Paladino";
   %this.str[%l, "unit_name_4"] = "Guerreiro Esqueleto";
   %this.str[%l, "unit_name_5"] = "Arqueiro Esqueleto";
   %this.str[%l, "unit_name_6"] = "Zumbi Medonho";
   %this.str[%l, "unit_name_7"] = "Espectro Antigo";
   %this.str[%l, "unit_name_8"] = "Draconato";
   %this.str[%l, "unit_name_9"] = "Dragão Vermelho";
   %this.str[%l, "unit_name_10"] = "Dragão do Ar";
   %this.str[%l, "unit_name_11"] = "Dragão Dourado";
   %this.str[%l, "unit_name_12"] = "Gremlin";
   %this.str[%l, "unit_name_13"] = "Ciclope Chifrudo";
   %this.str[%l, "unit_name_14"] = "Senhora Infernal";
   %this.str[%l, "unit_name_15"] = "Demônio";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "PULA";
   %this.str[%l, "badge_atk_prefix"] = "A";
   %this.str[%l, "badge_def_prefix"] = "D";
   %this.str[%l, "hud_role_melee"] = "Corpo a corpo";
   %this.str[%l, "hud_role_ranged"] = "À distância";
   %this.str[%l, "tooltip_header"] = "%1 (Nív. %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "PV: %1 / %2";
   %this.str[%l, "stat_attack"] = "Ataque";
   %this.str[%l, "stat_defence"] = "Defesa";
   %this.str[%l, "dice_stat_one"] = "%1: %2 dado";
   %this.str[%l, "dice_stat_other"] = "%1: %2 dados";
   %this.str[%l, "dice_stat_mod_one"] = "%1: %2 dado (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1: %2 dados (%3)";
   %this.str[%l, "status_sick"] = "Estado: Doente (pula o turno)";
   %this.str[%l, "status_turn_both"] = "Turno ativo: Mover e agir";
   %this.str[%l, "status_turn_action"] = "Turno ativo: Ação pronta";
   %this.str[%l, "status_turn_move"] = "Turno ativo: Movimento pronto";
   %this.str[%l, "status_turn_finishing"] = "Turno ativo: Finalizando";
   %this.str[%l, "trait_ghostly_touch"] = "Traço: Toque Espectral";
   %this.str[%l, "trait_disease"] = "Traço: Peste";
   %this.str[%l, "status_mana"] = "Mana: %1";
   %this.str[%l, "status_enemy_unit"] = "Unidade inimiga";
   %this.str[%l, "status_allied_unit"] = "Unidade aliada";
   %this.str[%l, "status_recruit_cost"] = "Custo: %1 PC";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "VITÓRIA";
   %this.str[%l, "summary_defeat"] = "DERROTA";
   %this.str[%l, "summary_sub_victory"] = "O exército (%1) foi derrotado!";
   %this.str[%l, "summary_sub_defeat"] = "Suas forças caíram diante de: %1.";
   %this.str[%l, "hero_class_warrior"] = "Guerreira";
   %this.str[%l, "hero_class_mage"] = "Feiticeira";
   %this.str[%l, "summary_turns_one"] = "%1 turno jogado";
   %this.str[%l, "summary_turns_other"] = "%1 turnos jogados";
   %this.str[%l, "summary_meta"] = "Herói: %1  |  %2";
   %this.str[%l, "summary_player_header"] = "FORÇAS DO JOGADOR (%1)";
   %this.str[%l, "summary_enemy_header"] = "FORÇAS INIMIGAS (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 sobrevivente";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 sobreviventes";
   %this.str[%l, "summary_detail"] = "PV restantes: %1  |  Baixas: %2";
   %this.str[%l, "summary_roster_header"] = "UNIDADES SOBREVIVENTES";
   %this.str[%l, "summary_none_survived"] = "Nenhuma unidade sobreviveu à batalha.";
   %this.str[%l, "summary_none_short"] = "Nenhuma";
   %this.str[%l, "summary_more_units_one"] = "... e mais %1 unidade sobrevivente";
   %this.str[%l, "summary_more_units_other"] = "... e mais %1 unidades sobreviventes";
   %this.str[%l, "summary_unit_row"] = "• %1 (Nív. %2) — PV: %3  |  ATQ: %4  |  DEF: %5";
   %this.str[%l, "summary_play_again"] = "JOGAR NOVAMENTE";
   %this.str[%l, "summary_main_menu"] = "MENU PRINCIPAL";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 dado de defesa";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 dados de defesa";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 Mana";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 Mana";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 dano mágico";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 de dano mágico";
   %this.str[%l, "fct_hp_loss"] = "-%1 PV";
   %this.str[%l, "fct_skip_turn"] = "Perde o turno!";

   // Credits
   %this.str[%l, "credits_title"] = "CRÉDITOS";
   %this.str[%l, "credits_role_designer"] = "DESIGN DO JOGO";
   %this.str[%l, "credits_role_programmer"] = "PROGRAMAÇÃO";
   %this.str[%l, "credits_role_artist"] = "ARTE 2D E 3D";
   %this.str[%l, "credits_role_music"] = "MÚSICA E SOM";
   %this.str[%l, "credits_role_level"] = "DESIGN DE FASES";
   %this.str[%l, "credits_role_qa"] = "TESTES E QA";
   %this.str[%l, "credits_role_thanks"] = "AGRADECIMENTOS";
}

//---------------------------------------------------------------------------------------------
// German (de) Translations
//---------------------------------------------------------------------------------------------
function LocalizationManager::populateGerman(%this)
{
   %l = "de";

   // Scene banners
   %this.str[%l, "banner_select_hero"] = "Wähle einen Helden";
   %this.str[%l, "banner_place_creatures"] = "Platziere die Kreaturen auf dem Feld";
   %this.str[%l, "banner_countdown_one"] = "Das Spiel beginnt in %1 Sekunde...";
   %this.str[%l, "banner_countdown_other"] = "Das Spiel beginnt in %1 Sekunden...";

   // Cards
   %this.str[%l, "card_warrior_title"] = "Kriegerin";
   %this.str[%l, "card_warrior_desc"] = "Die Kriegerin ist eine geschickte Kämpferin.";
   %this.str[%l, "card_wizard_title"] = "Zauberin";
   %this.str[%l, "card_wizard_desc"] = "Die Zauberin beherrscht die hohe Magie.";
   %this.str[%l, "card_sausage_title"] = "Zauberwurst";
   %this.str[%l, "card_sausage_stat_one"] = "+%1 Verteidigungswurf (Kreaturen)";
   %this.str[%l, "card_sausage_stat_other"] = "+%1 Verteidigungswürfe (Kreaturen)";
   %this.str[%l, "card_magichat_title"] = "Zauberhut";
   %this.str[%l, "card_magichat_stat_one"] = "+%1 Zauberpunkt";
   %this.str[%l, "card_magichat_stat_other"] = "+%1 Zauberpunkte";
   %this.str[%l, "card_lollipop_title"] = "Zauberlolli";
   %this.str[%l, "card_lollipop_stat_one"] = "+%1 Zauberschaden";
   %this.str[%l, "card_lollipop_stat_other"] = "+%1 Zauberschaden";
   %this.str[%l, "card_volcano_title"] = "Vulkan";
   %this.str[%l, "card_volcano_desc"] = "Ein mächtiger Vulkan vernichtet die feindliche Armee.";
   %this.str[%l, "card_dandelion_title"] = "Löwenzahnwurzel";
   %this.str[%l, "card_dandelion_stat_one"] = "+%1 Zauberpunkt";
   %this.str[%l, "card_dandelion_stat_other"] = "+%1 Zauberpunkte";
   %this.str[%l, "card_stomachflu_title"] = "Magen-Darm-Grippe";
   %this.str[%l, "card_stomachflu_desc"] = "Die erkrankte Kreatur setzt in ihrem nächsten Zug aus.";

   // Title Menu
   %this.str[%l, "menu_play"] = "SPIELEN";
   %this.str[%l, "menu_resume"] = "FORTSETZEN";
   %this.str[%l, "menu_resume_none"] = "FORTSETZEN (KEIN SPIEL)";
   %this.str[%l, "menu_options"] = "OPTIONEN";
   %this.str[%l, "menu_credits"] = "MITWIRKENDE";
   %this.str[%l, "menu_quit"] = "BEENDEN";
   %this.str[%l, "menu_lang_btn"] = "SPRACHE: DEUTSCH (DE)";
   %this.str[%l, "menu_confirm_title"] = "NEUES SPIEL STARTEN?";
   %this.str[%l, "menu_confirm_body"] = "Eine Partie läuft bereits. Ein Neustart verwirft sie.";
   %this.str[%l, "menu_confirm_yes"] = "NEUES SPIEL";
   %this.str[%l, "menu_confirm_no"] = "ABBRECHEN";

   // Options Menu
   %this.str[%l, "options_title"] = "EINSTELLUNGEN & FRAKTION";
   %this.str[%l, "options_aspect"] = "Seitenverhältnis:";
   %this.str[%l, "options_resolution"] = "Auflösung:";
   %this.str[%l, "options_mode"] = "Anzeigemodus:";
   %this.str[%l, "options_windowed"] = "FENSTER";
   %this.str[%l, "options_fullscreen"] = "VOLLBILD";
   %this.str[%l, "options_audio"] = "Audiosteuerung:";
   %this.str[%l, "options_faction"] = "Gegnerische Fraktion:";
   %this.str[%l, "options_language"] = "Sprache:";
   %this.str[%l, "options_ui_scale"] = "UI-Skalierung: %1%";
   %this.str[%l, "options_apply"] = "GRAFIK ANWENDEN";
   %this.str[%l, "options_close"] = "SCHLIESSEN";

   // Pause Menu & Board HUD
   %this.str[%l, "pause_title"] = "SPIEL PAUSIERT";
   %this.str[%l, "pause_resume"] = "FORTSETZEN";
   %this.str[%l, "pause_options"] = "OPTIONEN";
   %this.str[%l, "pause_title_screen"] = "ZUM HAUPTMENÜ";
   %this.str[%l, "pause_quit"] = "SPIEL BEENDEN";
   %this.str[%l, "hud_menu"] = "MENÜ";
   %this.str[%l, "hud_sfx_on"] = "TON: AN";
   %this.str[%l, "hud_sfx_muted"] = "TON: STUMM";
   %this.str[%l, "hud_music_on"] = "MUSIK: AN";
   %this.str[%l, "hud_music_muted"] = "MUSIK: AUS";

   // Version watermark
   %this.str[%l, "watermark_stage_ea"] = "Früher Zugang";
   %this.str[%l, "watermark_version"] = "Version: %1";
   %this.str[%l, "watermark_build"] = "Build: %1";

   // Factions & Hero classes
   %this.str[%l, "faction_human_order"] = "Orden der Menschen";
   %this.str[%l, "faction_human_order_upper"] = "ORDEN DER MENSCHEN";
   %this.str[%l, "faction_short_undead"] = "UNTOTE";
   %this.str[%l, "faction_short_dragonfolk"] = "DRACHENVOLK";
   %this.str[%l, "faction_short_gremlin"] = "GREMLINS";
   %this.str[%l, "faction_undead_name"] = "Untoten-Legion";
   %this.str[%l, "faction_undead_upper"] = "UNTOTEN-LEGION";
   %this.str[%l, "faction_undead_subtitle"] = "Die Erhobenen Toten";
   %this.str[%l, "faction_undead_desc"] = "Verfluchte Skelettkrieger und rachsüchtige Geister aus alten Grüften. Nutzen Geisterberührung und Seuche.";
   %this.str[%l, "faction_undead_spec"] = "Nekrotische Flüche: Seuche (-1 ANG) und Geisterberührung (-2 VTD).";
   %this.str[%l, "faction_dragonfolk_name"] = "Drachenvolk";
   %this.str[%l, "faction_dragonfolk_upper"] = "DRACHENVOLK";
   %this.str[%l, "faction_dragonfolk_subtitle"] = "Der Schuppenclan";
   %this.str[%l, "faction_dragonfolk_desc"] = "Stolze Echsenkrieger, feuerspeiende Drachen und erhabene Golddrachen mit elementarem Atem.";
   %this.str[%l, "faction_dragonfolk_spec"] = "Schuppenzorn: Hohe Zähigkeit, Feueratem & Blitzschläge.";
   %this.str[%l, "faction_gremlin_name"] = "Gremlin-Horde";
   %this.str[%l, "faction_gremlin_upper"] = "GREMLIN-HORDE";
   %this.str[%l, "faction_gremlin_subtitle"] = "Die Unterwelthorde";
   %this.str[%l, "faction_gremlin_desc"] = "Listige Kobolde der Unterwelt, Zyklopen mit Augenstrahlen und höllische Dämonen aus dem Abgrund.";
   %this.str[%l, "faction_gremlin_spec"] = "Abgrundgefahr: Bösartiger Schwarm, Augenstrahlen & Dämonenfürst.";

   %this.str[%l, "faction_select_title"] = "GEGNERISCHE FRAKTION WÄHLEN";
   %this.str[%l, "faction_select_subtitle"] = "Wähle die Legion, der du auf dem Schlachtfeld gegenübertrittst";
   %this.str[%l, "faction_select_btn"] = "WÄHLEN";
   %this.str[%l, "faction_selected_btn"] = "[ GEWÄHLT ]";
   %this.str[%l, "faction_change_hero_btn"] = "HELD WECHSELN";
   %this.str[%l, "faction_begin_battle_btn"] = "SCHLACHT BEGINNEN";

   // Units
   %this.str[%l, "unit_fallback"] = "Einheit";
   %this.str[%l, "unit_lvl_fmt"] = "%1 (St. %2)";
   %this.str[%l, "unit_name_0"] = "Krieger";
   %this.str[%l, "unit_name_1"] = "Bogenschützin";
   %this.str[%l, "unit_name_2"] = "Kleriker";
   %this.str[%l, "unit_name_3"] = "Paladin";
   %this.str[%l, "unit_name_4"] = "Skelettkrieger";
   %this.str[%l, "unit_name_5"] = "Skelettschütze";
   %this.str[%l, "unit_name_6"] = "Schreckenszombie";
   %this.str[%l, "unit_name_7"] = "Uralter Geist";
   %this.str[%l, "unit_name_8"] = "Drachenkrieger";
   %this.str[%l, "unit_name_9"] = "Roter Drache";
   %this.str[%l, "unit_name_10"] = "Luftdrache";
   %this.str[%l, "unit_name_11"] = "Golddrache";
   %this.str[%l, "unit_name_12"] = "Gremlin";
   %this.str[%l, "unit_name_13"] = "Gehörnter Zyklop";
   %this.str[%l, "unit_name_14"] = "Herrin";
   %this.str[%l, "unit_name_15"] = "Dämon";

   // Unit Stats HUD & Tooltips
   %this.str[%l, "badge_skip"] = "AUS";
   %this.str[%l, "badge_atk_prefix"] = "A";
   %this.str[%l, "badge_def_prefix"] = "V";
   %this.str[%l, "hud_role_melee"] = "Nahkampf";
   %this.str[%l, "hud_role_ranged"] = "Fernkampf";
   %this.str[%l, "tooltip_header"] = "%1 (St. %2)";
   %this.str[%l, "tooltip_team_role"] = "%1  |  %2";
   %this.str[%l, "tooltip_hp"] = "LP: %1 / %2";
   %this.str[%l, "stat_attack"] = "Angriff";
   %this.str[%l, "stat_defence"] = "Verteidigung";
   %this.str[%l, "dice_stat_one"] = "%1: %2 Würfel";
   %this.str[%l, "dice_stat_other"] = "%1: %2 Würfel";
   %this.str[%l, "dice_stat_mod_one"] = "%1: %2 Würfel (%3)";
   %this.str[%l, "dice_stat_mod_other"] = "%1: %2 Würfel (%3)";
   %this.str[%l, "status_sick"] = "Status: Krank (setzt aus)";
   %this.str[%l, "status_turn_both"] = "Zug aktiv: Bewegung & Aktion";
   %this.str[%l, "status_turn_action"] = "Zug aktiv: Aktion bereit";
   %this.str[%l, "status_turn_move"] = "Zug aktiv: Bewegung bereit";
   %this.str[%l, "status_turn_finishing"] = "Zug aktiv: Beendet";
   %this.str[%l, "trait_ghostly_touch"] = "Merkmal: Geisterberührung";
   %this.str[%l, "trait_disease"] = "Merkmal: Seuche";
   %this.str[%l, "status_mana"] = "Mana: %1";
   %this.str[%l, "status_enemy_unit"] = "Gegnerische Einheit";
   %this.str[%l, "status_allied_unit"] = "Verbündete Einheit";
   %this.str[%l, "status_recruit_cost"] = "Kosten: %1 KP";

   // Battle Summary Modal
   %this.str[%l, "summary_victory"] = "SIEG";
   %this.str[%l, "summary_defeat"] = "NIEDERLAGE";
   %this.str[%l, "summary_sub_victory"] = "Die Fraktion (%1) wurde besiegt!";
   %this.str[%l, "summary_sub_defeat"] = "Deine Truppen erlagen: %1.";
   %this.str[%l, "hero_class_warrior"] = "Kriegerin";
   %this.str[%l, "hero_class_mage"] = "Zauberin";
   %this.str[%l, "summary_turns_one"] = "%1 Zug gespielt";
   %this.str[%l, "summary_turns_other"] = "%1 Züge gespielt";
   %this.str[%l, "summary_meta"] = "Held: %1  |  %2";
   %this.str[%l, "summary_player_header"] = "SPIELERTRUPPEN (%1)";
   %this.str[%l, "summary_enemy_header"] = "GEGNERTRUPPEN (%1)";
   %this.str[%l, "summary_surviving_one"] = "%1 / %2 überlebend";
   %this.str[%l, "summary_surviving_other"] = "%1 / %2 überlebend";
   %this.str[%l, "summary_detail"] = "Restliche LP: %1  |  Verluste: %2";
   %this.str[%l, "summary_roster_header"] = "ÜBERLEBENDE EINHEITEN";
   %this.str[%l, "summary_none_survived"] = "Keine Einheit hat die Schlacht überlebt.";
   %this.str[%l, "summary_none_short"] = "Keine";
   %this.str[%l, "summary_more_units_one"] = "... und %1 weitere überlebende Einheit";
   %this.str[%l, "summary_more_units_other"] = "... und %1 weitere überlebende Einheiten";
   %this.str[%l, "summary_unit_row"] = "• %1 (St. %2) — LP: %3  |  ANG: %4  |  VTD: %5";
   %this.str[%l, "summary_play_again"] = "ERNEUT SPIELEN";
   %this.str[%l, "summary_main_menu"] = "ZUM HAUPTMENÜ";

   // Floating Combat Text
   %this.str[%l, "fct_def_bonus_one"] = "+%1 Verteidigungswurf";
   %this.str[%l, "fct_def_bonus_other"] = "+%1 Verteidigungswürfe";
   %this.str[%l, "fct_mana_bonus_one"] = "+%1 Mana";
   %this.str[%l, "fct_mana_bonus_other"] = "+%1 Mana";
   %this.str[%l, "fct_spell_dmg_one"] = "+%1 Zauberschaden";
   %this.str[%l, "fct_spell_dmg_other"] = "+%1 Zauberschaden";
   %this.str[%l, "fct_hp_loss"] = "-%1 LP";
   %this.str[%l, "fct_skip_turn"] = "Setzt aus!";

   // Credits
   %this.str[%l, "credits_title"] = "MITWIRKENDE";
   %this.str[%l, "credits_role_designer"] = "SPIELDESIGN";
   %this.str[%l, "credits_role_programmer"] = "PROGRAMMIERUNG";
   %this.str[%l, "credits_role_artist"] = "2D- & 3D-GRAFIK";
   %this.str[%l, "credits_role_music"] = "MUSIK & SOUND";
   %this.str[%l, "credits_role_level"] = "LEVELDESIGN";
   %this.str[%l, "credits_role_qa"] = "QS & TESTS";
   %this.str[%l, "credits_role_thanks"] = "BESONDERER DANK";
}

// Initialize Singleton
if (!isObject(LocalizationManager))
{
   new ScriptObject(LocalizationManager) { };
}
if (!isObject($LocalizationManager))
{
   $LocalizationManager = LocalizationManager;
}
