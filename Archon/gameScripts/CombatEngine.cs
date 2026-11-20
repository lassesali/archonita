//---------------------------------------------------------------------------------------------
// Archonita - CombatEngine.cs
// Unit actions, combat resolution, dice rolling, damage resolution, piece movement, and animation states.
//---------------------------------------------------------------------------------------------

function CombatEngine::onAdd(%this)
{
   %this.dice = new SimSet();
   %this.activeProjectiles = new SimSet();
   %this.hideBloodEvent = 0;
   %this.damagePieceEvent = 0;
   %this.diceRollEvent = 0;
   %this.diceStopEvent = 0;
   %this.rangedLaunchEvent = 0;
   %this.rangedImpactTimeoutEvent = 0;
   %this.pendingAttackId = 0;
   %this.pendingAttackIsRanged = 0;
   %this.pendingRollComplete = 0;
   %this.pendingProjectileImpacted = 0;
   %this.pendingAttackResolved = 0;
   %this.activeCombatProjectile = "";
   %this.inputLocked = false;
   $inputLocked = false;
   $scheduleID = 0;
   $diceRollScheduleID = 0;
}

function CombatEngine::onRemove(%this)
{
   %this.cleanupProjectiles();
   if (isObject(%this.activeProjectiles))
      %this.activeProjectiles.delete();
   if (isObject(%this.dice))
      %this.dice.delete();
   if (isEventPending(%this.hideBloodEvent))
      cancel(%this.hideBloodEvent);
   if (isEventPending(%this.damagePieceEvent))
      cancel(%this.damagePieceEvent);
   if (isEventPending(%this.diceRollEvent))
      cancel(%this.diceRollEvent);
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   if (isEventPending(%this.rangedLaunchEvent))
      cancel(%this.rangedLaunchEvent);
   if (isEventPending(%this.rangedImpactTimeoutEvent))
      cancel(%this.rangedImpactTimeoutEvent);

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i]))
         $noppa[%i].safeDelete();
      if (isObject($noppadef[%i]))
         $noppadef[%i].safeDelete();
   }

   %this.inputLocked = false;
   $inputLocked = false;
   $scheduleID = 0;
   $diceRollScheduleID = 0;
}

function CombatEngine::lockInput(%this)
{
   %this.inputLocked = true;
   $inputLocked = true;
   if (isObject($UnitStatsHud))
      $UnitStatsHud.hideTooltip();
   if (isObject(sceneWindow2D))
   {
      sceneWindow2D.setUseWindowMouseEvents(false);
      sceneWindow2D.objectSelected = false;
   }
}

function CombatEngine::unlockInput(%this)
{
   %this.inputLocked = false;
   $inputLocked = false;
   if (isObject(sceneWindow2D) && isObject(game) && game.getActivePlayer() == 0)
      sceneWindow2D.setUseWindowMouseEvents(true);
}

function CombatEngine::isInputLocked(%this)
{
   return (%this.inputLocked || $inputLocked);
}

function CombatEngine::cleanupProjectiles(%this)
{
   if (isEventPending(%this.rangedLaunchEvent))
   {
      cancel(%this.rangedLaunchEvent);
      %this.rangedLaunchEvent = 0;
   }
   if (isEventPending(%this.rangedImpactTimeoutEvent))
   {
      cancel(%this.rangedImpactTimeoutEvent);
      %this.rangedImpactTimeoutEvent = 0;
   }

   if (!isObject(%this.activeProjectiles))
      return;

   while (%this.activeProjectiles.getCount() > 0)
   {
      %proj = %this.activeProjectiles.getObject(0);
      %this.activeProjectiles.remove(%proj);
      if (isObject(%proj))
         %proj.cleanup("battle_end");
   }
   %this.activeCombatProjectile = "";
}

function CombatEngine::cancelDiceRoll(%this)
{
   if (isEventPending(%this.diceRollEvent))
   {
      cancel(%this.diceRollEvent);
      %this.diceRollEvent = 0;
   }
   if (isEventPending($scheduleID))
   {
      cancel($scheduleID);
      $scheduleID = 0;
   }
   if (isEventPending($diceRollScheduleID))
   {
      cancel($diceRollScheduleID);
      $diceRollScheduleID = 0;
   }
   if (isEventPending(%this.diceStopEvent))
   {
      cancel(%this.diceStopEvent);
      %this.diceStopEvent = 0;
   }
   if (isEventPending(%this.rangedLaunchEvent))
   {
      cancel(%this.rangedLaunchEvent);
      %this.rangedLaunchEvent = 0;
   }
   if (isEventPending(%this.rangedImpactTimeoutEvent))
   {
      cancel(%this.rangedImpactTimeoutEvent);
      %this.rangedImpactTimeoutEvent = 0;
   }
   %this.stopDiceAnimation();
   %this.unlockInput();
}

function CombatEngine::playDiceRollSound(%this, %volume)
{
   return playDiceRollSound(%volume);
}

function CombatEngine::stopDiceAnimation(%this)
{
   if (isEventPending(%this.diceStopEvent))
   {
      cancel(%this.diceStopEvent);
      %this.diceStopEvent = 0;
   }

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i]))
      {
         if (!$noppa[%i].isStaticFrameProvider()) $noppa[%i].stopAnimation();
         if ($noppa[%i].dieType $= "red")
            $noppa[%i].setImage("Archon:diceredImageMap");
         else
            $noppa[%i].setImage("Archon:diceblueImageMap");
         %frame = ($noppa[%i].rollResult < 4) ? $noppa[%i].rollResult : ($noppa[%i].rollResult % 4);
         $noppa[%i].setImageFrame(%frame);
         $noppa[%i].setSize("8 8");
         $noppa[%i].setAngle(0);
         $noppa[%i].setAngularVelocity(0);
      }
      if (isObject($noppadef[%i]))
      {
         if (!$noppadef[%i].isStaticFrameProvider()) $noppadef[%i].stopAnimation();
         if ($noppadef[%i].dieType $= "red")
            $noppadef[%i].setImage("Archon:diceredImageMap");
         else
            $noppadef[%i].setImage("Archon:diceblueImageMap");
         %frame = ($noppadef[%i].rollResult < 4) ? $noppadef[%i].rollResult : ($noppadef[%i].rollResult % 4);
         $noppadef[%i].setImageFrame(%frame);
         $noppadef[%i].setSize("8 8");
         $noppadef[%i].setAngle(0);
         $noppadef[%i].setAngularVelocity(0);
      }
   }
}

function CombatEngine::doAction(%this)
{
   if (%this.isInputLocked())
      return;

   if (game.getHighlightedActionIcon() == 1)
   {
      %this.doDefence(game.getTurn());
   } 
   else if (game.getHighlightedActionIcon() == 2)
   {
      %this.doMagic();
   } 
   else if (game.getHighlightedActionIcon() == 3)
   {
   }
}

function CombatEngine::doDefence(%this, %turn)
{
   if (%this.isInputLocked())
      return;

   if (%turn == game.getTurn()) 
   {
      nextTurn();
      if (game.getState() != $gameEndState) { 
         evaluateTacticalMoves(); 
      }
   }
}

function CombatEngine::doMagic(%this)
{
   if (%this.isInputLocked() || getGameState() != $gameBattleState)
      return;

   if (game.getActivePlayer() != 0 || !game.getCanCastSpell() || $topCardIndex < 1)
   {
      game.setSelectedAction(0);
      game.setHighlightedActionIcon(0);
      updateActionIconsVisualState();
      %this.updateMagicButtonParticles();
      return;
   }

   %topCard = $specialcard[$topCardIndex];
   if (isObject(%topCard) && !game.getSpellRevealed())
   {
      %topCard.setFrame(%topCard.spellFrame);
      game.setSpellRevealed(true);
      playSpellRevealSound();
      %this.updateMagicButtonParticles();
      if (isObject($SceneTextOverlayGui))
         $SceneTextOverlayGui.refreshAll();
   }
}

function CombatEngine::getSpellTargetCategory(%this, %spellFrame)
{
   switch (%spellFrame)
   {
      case 3: return "all";
      case 4: return "avatar";
      case 5: return "avatar";
      case 6: return "all";
      case 7: return "avatar";
      case 8: return "unit";
   }
   return "";
}

function CombatEngine::castRevealedSpell(%this, %clickedCategory, %targetPiece)
{
   if (!game.getCanCastSpell() || !game.getSpellRevealed() || $topCardIndex < 1)
      return false;

   %topCard = $specialcard[$topCardIndex];
   if (!isObject(%topCard))
      return false;

   %spellFrame = %topCard.spellFrame;
   %requiredCategory = %this.getSpellTargetCategory(%spellFrame);

   if (%requiredCategory $= "avatar" && %clickedCategory !$= "avatar") return false;
   if (%requiredCategory $= "unit" && (%clickedCategory !$= "unit" || !isObject(%targetPiece) || %targetPiece.alive == 0)) return false;
   if (%requiredCategory $= "all" && (%clickedCategory !$= "board" && %clickedCategory !$= "unit")) return false;

   // Play spell resolution SFX upon valid target confirmation
   playSpellCastSound(%spellFrame);

   // Trigger spell particle effect before lethal damage can mark units dead
   if (isObject(SpellEffectManager))
      SpellEffectManager.playSpellEffect(%spellFrame, %targetPiece);

   %hero = HeroGroup.getObject(0);

   switch (%spellFrame)
   {
      case 3: // Taikamakkara (+1 creature defense roll)
         %fctDef = isObject($LocalizationManager) ? $LocalizationManager.getPluralText("fct_def_bonus", 1) : "+1 Defence Roll";
         for (%i = 1; %i <= game.getPieceCount(); %i++)
         {
            if (isObject($nappulaSprite[%i]) && $nappulaSprite[%i].alive == 1 && $nappulaSprite[%i].groupid == 0)
            {
               $nappulaSprite[%i].defence += 1;
               $nappulaSprite[%i].defenceMAX += 1;
               FloatingTextManager.spawnAbovePiece($nappulaSprite[%i], %fctDef, "77 208 225");
            }
         }
      case 4: // Taikahattu (+1 spell points)
         %hero.addMana(1);
         %fctMana = isObject($LocalizationManager) ? $LocalizationManager.getPluralText("fct_mana_bonus", 1) : "+1 Mana";
         FloatingTextManager.spawnAboveAvatar(heroicon, %fctMana, "100 181 246");
      case 5: // Taikatikkari (+1 spell damage)
         %hero.addSpellDamage(1);
         %fctDmg = isObject($LocalizationManager) ? $LocalizationManager.getPluralText("fct_spell_dmg", 1) : "+1 Spell Damage";
         FloatingTextManager.spawnAboveAvatar(heroicon, %fctDmg, "255 183 77");
      case 6: // Tulivuori (Volcano damages enemy army)
         %dmg = 1 + %hero.getSpellDamage();
         %curPiece = game.getSelectedPiece();
         %savedMoved = (isObject($nappulaSprite[%curPiece])) ? $nappulaSprite[%curPiece].hasMoved : 0;
         %savedAction = (isObject($nappulaSprite[%curPiece])) ? $nappulaSprite[%curPiece].hasDoneAction : 0;
         %fctHpLoss = isObject($LocalizationManager) ? $LocalizationManager.getText("fct_hp_loss", %dmg) : ("-" @ %dmg @ " HP");
         for (%i = 1; %i <= game.getPieceCount(); %i++)
         {
            %unit = $nappulaSprite[%i];
            if (isObject(%unit) && %unit.alive == 1 && %unit.groupid == 1)
            {
               FloatingTextManager.spawnAbovePiece(%unit, %fctHpLoss, "229 57 53");
               for (%d = 0; %d < %dmg; %d++)
               {
                  if (%unit.alive == 1)
                     %this.damagePiece(%unit);
               }
            }
         }
         if (isObject($nappulaSprite[%curPiece]))
         {
            $nappulaSprite[%curPiece].hasMoved = %savedMoved;
            $nappulaSprite[%curPiece].hasDoneAction = %savedAction;
         }
      case 7: // Voikukan juuri (+1 spell points)
         %hero.addMana(1);
         %fctMana2 = isObject($LocalizationManager) ? $LocalizationManager.getPluralText("fct_mana_bonus", 1) : "+1 Mana";
         FloatingTextManager.spawnAboveAvatar(heroicon, %fctMana2, "100 181 246");
      case 8: // Vatsatauti (Target skips next turn)
         %targetPiece.skipNextTurn = 1;
         %fctSkip = isObject($LocalizationManager) ? $LocalizationManager.getText("fct_skip_turn") : "Skip Turn!";
         FloatingTextManager.spawnAbovePiece(%targetPiece, %fctSkip, "174 213 129");
   }

   if (%topCard.isMethod("discardAfterCast"))
      %topCard.discardAfterCast();
   else
      Card::discardAfterCast(%topCard);
   $topCardIndex--;

   game.setCanCastSpell(false);
   game.setSpellRevealed(false);
   game.setSelectedAction(0);
   game.setHighlightedActionIcon(0);
   updateActionIconsVisualState();
   %this.updateMagicButtonParticles();
   if (isObject($UnitStatsHud))
      $UnitStatsHud.refreshAll();
   if (isObject($SceneTextOverlayGui))
      $SceneTextOverlayGui.refreshAll();
   return true;
}

function CombatEngine::updateMagicButtonParticles(%this)
{
   if (!isObject(magicButtonEffect))
   {
      if (isObject(t2dScene))
      {
         new ParticlePlayer(magicButtonEffect)
         {
            scene = t2dScene;
            Particle = "Archon:magic_button_glow";
            Position = "40 -15.1";
            Size = "16 16";
            SceneLayer = 29;
         };
         if (isObject(magicButtonEffect))
         {
            magicButtonEffect.stop(false, false);
         }
      }
      else
      {
         return;
      }
   }

   %isEligible = (isObject(game) && 
                  game.getCanCastSpell() && 
                  $topCardIndex >= 1 && 
                  game.getActivePlayer() == 0 && 
                  getGameState() == $gameBattleState);

   %baseDensity = isObject($ResolutionManager)
      ? $ResolutionManager.getParticleDensity()
      : (($pref::T2D::ParticlePlayerEmissionRateScale !$= "") ? $pref::T2D::ParticlePlayerEmissionRateScale : 1.0);

   if (%isEligible)
   {
      if (!magicButtonEffect.getIsPlaying() || (magicButtonEffect.isMethod("getEmitterPaused") && magicButtonEffect.getEmitterPaused(0)))
         magicButtonEffect.play(false);

      %rateScale = %baseDensity * (game.getSpellRevealed() ? 1.7 : 1.0);
      magicButtonEffect.setEmissionRateScale(%rateScale);
   }
   else
   {
      if (magicButtonEffect.getIsPlaying() && !(magicButtonEffect.isMethod("getEmitterPaused") && magicButtonEffect.getEmitterPaused(0)))
         magicButtonEffect.stop(true, false);
   }
}

function CombatEngine::doAttack(%this, %targetEnemy, %turn)
{
   if (%turn != game.getTurn() || %this.isInputLocked())
      return;

   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   %attacker = $nappulaSprite[%selectedPiece];
   %attacker.hasDoneAction = 1;

   echo("function doAttack(" @ %targetEnemy @ "," @ %turn @ ")");  
   
   %pos1 = %attacker.positionOnBoard;
   %pos2 = %targetEnemy.positionOnBoard;

   %attacker.facing = %this.facing(%pos1, %pos2);
   %targetEnemy.facing = %this.facing(%pos2, %pos1);
   
   echo("setPieceState(" @ %attacker @ "," @ $olentoAttackState[%attacker.creatureID] @ ")");
   %this.setPieceState(%attacker, $olentoAttackState[%attacker.creatureID]);

   // Lock player input & clear selection grids during the roll / projectile window
   %this.lockInput();
   deleteGrids(1);

   if (game.getActivePlayer() == 0) {
      %result = %this.rollAttackDice(%attacker, %targetEnemy);
   } else {
      %result = %this.rollDefenseDice(%targetEnemy, %attacker);
   }

   %this.pendingAttackId++;
   %attackId = %this.pendingAttackId;
   %this.pendingAttackAttacker = %attacker;
   %this.pendingAttackTarget = %targetEnemy;
   %this.pendingAttackResult = %result;
   %this.pendingAttackTurn = %turn;
   %this.pendingRollComplete = 0;
   %this.pendingProjectileImpacted = 0;
   %this.pendingAttackResolved = 0;
   %this.activeCombatProjectile = "";

   if (%attacker.missile == 1)
   {
      %this.pendingAttackIsRanged = 1;
      %this.launchRangedAttackProjectile(%attacker, %targetEnemy, %attackId);
   }
   else
   {
      %this.pendingAttackIsRanged = 0;
      echo("setPieceState(" @ %targetEnemy @ "," @ $olentoBeenHitState[%targetEnemy.creatureID] @ ")");
      schedule(50, 0, "setPieceState", %targetEnemy, $olentoBeenHitState[%targetEnemy.creatureID]);
   }

   // Defer combat damage / outcome resolution by 500ms
   if (isEventPending(%this.diceRollEvent))
      cancel(%this.diceRollEvent);
   if (isEventPending(%this.rangedImpactTimeoutEvent))
   {
      cancel(%this.rangedImpactTimeoutEvent);
      %this.rangedImpactTimeoutEvent = 0;
   }

   %this.diceRollEvent = %this.schedule(500, "onAttackRollComplete", %targetEnemy, %result, %turn);
   $scheduleID = %this.diceRollEvent;
   $diceRollScheduleID = %this.diceRollEvent;
}

function CombatEngine::launchRangedAttackProjectile(%this, %attacker, %targetEnemy, %attackId)
{
   if (!isObject(%attacker) || !isObject(%targetEnemy))
      return "";

   %startPos = %attacker.getPosition();
   %targetPos = %targetEnemy.getPosition();
   %tx = getWord(%targetPos, 0);
   %ty = getWord(%targetPos, 1);

   %dist = VectorDist(%startPos, %targetPos);
   %flightSpeed = (%dist > 0.1) ? (%dist / 0.35) : 45.0;
   if (%flightSpeed < 35.0)
      %flightSpeed = 35.0;
   if (%flightSpeed > 120.0)
      %flightSpeed = 120.0;

   %savedSpeed = %attacker.projectileSpeed;
   %attacker.projectileSpeed = %flightSpeed;
   %proj = RangedUnit::fireProjectile(%attacker, %tx, %ty, 1, %targetEnemy);
   %attacker.projectileSpeed = %savedSpeed;
   if (isObject(%proj))
   {
      %proj.combatEngine = %this;
      %proj.isCombatAttack = 1;
      %proj.combatAttackId = %attackId;
      %proj.trackTargetUnit = 1;
      %this.activeCombatProjectile = %proj;
   }
   return %proj;
}

function CombatEngine::onProjectileImpact(%this, %projectile)
{
   if (!isObject(%projectile) || %projectile.combatAttackId != %this.pendingAttackId)
      return;

   %this.pendingProjectileImpacted = 1;
   %targetEnemy = %this.pendingAttackTarget;

   if (isObject(%targetEnemy) && %targetEnemy.alive != 0)
   {
      echo("setPieceState(" @ %targetEnemy @ "," @ $olentoBeenHitState[%targetEnemy.creatureID] @ ")");
      %this.setPieceState(%targetEnemy, $olentoBeenHitState[%targetEnemy.creatureID]);
   }

   if (%this.pendingRollComplete && !%this.pendingAttackResolved)
      %this.finalizeCombatAttack(%this.pendingAttackTarget, %this.pendingAttackResult, %this.pendingAttackTurn);
}

function CombatEngine::onAttackRollComplete(%this, %targetEnemy, %result, %turn)
{
   %this.diceRollEvent = 0;
   $scheduleID = 0;
   $diceRollScheduleID = 0;
   %this.pendingRollComplete = 1;

   // Stop dice animation and set static rolled frame
   %this.stopDiceAnimation();

   if (%this.pendingAttackIsRanged && !%this.pendingProjectileImpacted && isObject(%this.activeCombatProjectile))
   {
      // Wait for projectile impact to apply damage/hit reaction (with safety timeout)
      %this.rangedImpactTimeoutEvent = %this.schedule(800, "finalizeCombatAttack", %targetEnemy, %result, %turn);
      return;
   }

   %this.finalizeCombatAttack(%targetEnemy, %result, %turn);
}

function CombatEngine::finalizeCombatAttack(%this, %targetEnemy, %result, %turn)
{
   if (%this.pendingAttackResolved)
      return;
   %this.pendingAttackResolved = 1;

   if (isEventPending(%this.rangedImpactTimeoutEvent))
   {
      cancel(%this.rangedImpactTimeoutEvent);
      %this.rangedImpactTimeoutEvent = 0;
   }

   // Unlock input
   %this.unlockInput();

   if (%turn != game.getTurn() || !isObject(%targetEnemy) || game.getState() == $gameEndState)
      return;

   if (game.getActivePlayer() == 0) {
      // Attack succeeded
      if (%result == 1) 
      {  
         echo("damagePiece(" @ %targetEnemy @ ")");
         %this.damagePiece(%targetEnemy);
         $fxbeen2 = playArchonSound(fxBeenHit);
         %this.triggerBloodEffect(%targetEnemy.getPosition(), 300);
      } else {
         $fxsoita = playArchonSound(fxSword);
      }
   } else {
      // Defense failed
      if (%result == 0)
      {  
         echo("damagePiece(" @ %targetEnemy @ ")");
         %this.damagePiece(%targetEnemy);
         $fx2 = playArchonSound(fxBeenHit);
         %this.triggerBloodEffect(%targetEnemy.getPosition(), 300);
      } else {
         $fxsoita = playArchonSound(fxSword);
      }
   }

   if (game.getState() != $gameEndState) { 
      evaluateTacticalMoves(); 
   }
}

function CombatEngine::triggerBloodEffect(%this, %position, %durationMs)
{
   if (!isObject(blood))
      return;

   if (isEventPending(%this.hideBloodEvent))
      cancel(%this.hideBloodEvent);

   blood.setPosition(%position);
   if (blood.isMethod("play"))
      blood.play(false);

   %this.hideBloodEvent = %this.schedule(%durationMs, "hideBlood");
}

function CombatEngine::hideBlood(%this)
{
   %this.hideBloodEvent = 0;
   if (!isObject(blood))
      return;

   if (blood.isMethod("stop"))
      blood.stop(true, false);
   else
      blood.setPosition("-50 -80");
}

function CombatEngine::rollAttackDice(%this, %attacker, %defender)
{
   %att = 0;
   %def = 0;

   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   for (%i = 0; %i < %attacker.attack; %i++)
   {
      $noppa[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppa[%i].playAnimation("Archon:redDiceRollAnimation");
      $noppa[%i].setAnimationFrame(getRandom(5));
      $noppa[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].rollResult = %roll;
      $noppa[%i].dieType = "red";
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;

      if (%roll == 0 || %roll == 2) %att++;
   }
   
   for (%i = 0; %i < %defender.defence; %i++)
   {
      $noppadef[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppadef[%i].playAnimation("Archon:blueDiceRollAnimation");
      $noppadef[%i].setAnimationFrame(getRandom(5));
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].rollResult = %roll;
      $noppadef[%i].dieType = "blue";
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      
      if (%roll == 3) %def++;
   }
   
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");

   if (%att > %def) {
      return 1;
   }
   else 
   {
      return 0;
   }
}

function CombatEngine::rollDefenseDice(%this, %defender, %attacker)
{
   %att = 0;
   %def = 0;

   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   for (%i = 0; %i < %defender.defence; %i++)
   {
      $noppadef[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppadef[%i].playAnimation("Archon:redDiceRollAnimation");
      $noppadef[%i].setAnimationFrame(getRandom(5));
      $noppadef[%i].setPosition(%i * 10 - 24 SPC "30");
      $noppadef[%i].setSize("8 8");
      $noppadef[%i].setRotation(getRandom(360));
      $noppadef[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppadef[%i].rollResult = %roll;
      $noppadef[%i].dieType = "red";
      $noppadef[%i].setVisible(true);
      $noppadef[%i].setLayer(10);
      $noppadef[%i].setUseMouseEvents(0);
      $noppadef[%i].isSelectable = false;
      if (%roll == 1) %def++;
   }

   for (%i = 0; %i < %attacker.attack; %i++)
   {
      $noppa[%i] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
      $noppa[%i].playAnimation("Archon:blueDiceRollAnimation");
      $noppa[%i].setAnimationFrame(getRandom(5));
      $noppa[%i].setPosition(%i * 10 - 24 SPC "-30");
      $noppa[%i].setSize("8 8");
      $noppa[%i].setRotation(getRandom(360));
      $noppa[%i].setAngularVelocity(2 * 36);
      %roll = getRandom(3);
      $noppa[%i].rollResult = %roll;
      $noppa[%i].dieType = "blue";
      $noppa[%i].setVisible(true);
      $noppa[%i].setLayer(10);
      $noppa[%i].setUseMouseEvents(0);
      $noppa[%i].isSelectable = false;
      if (%roll <= 1) %att++;
   }
   
   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");

   if (%def >= %att) {
      return 1;
   }
   else 
   {
      return 0;
   }
}

function CombatEngine::rollMovementDice(%this, %player)
{
   playDiceRollSound();

   for (%i = 0; %i <= 8; %i++)
   {
      if (isObject($noppa[%i])) {
         $noppa[%i].safeDelete();
      }
      if (isObject($noppadef[%i])) {
         $noppadef[%i].safeDelete();
      }
   }

   $noppa[0] = new Sprite() { class = "DiceSprite"; scene = t2dScene; };
   if (%player == 0)
   {
      $noppa[0].playAnimation("Archon:blueDiceRollAnimation");
      $noppa[0].dieType = "blue";
      $noppa[0].setPosition("-24 30");
   }
   else
   {
      $noppa[0].playAnimation("Archon:redDiceRollAnimation");
      $noppa[0].dieType = "red";
      $noppa[0].setPosition("24 30");
   }
   $noppa[0].setAnimationFrame(getRandom(5));
   $noppa[0].setSize("8 8");
   $noppa[0].setRotation(getRandom(360));
   $noppa[0].setAngularVelocity(2 * 36);
   %roll = getRandom(5);
   $noppa[0].rollResult = %roll;
   $noppa[0].setVisible(true);
   $noppa[0].setLayer(10);
   $noppa[0].setUseMouseEvents(0);
   $noppa[0].isSelectable = false;

   if (isEventPending(%this.diceStopEvent))
      cancel(%this.diceStopEvent);
   %this.diceStopEvent = %this.schedule(500, "stopDiceAnimation");
   return %roll;
}

function CombatEngine::doMovement(%this, %x, %y)
{
   if (%this.isInputLocked())
      return;

   %hero = HeroGroup.getObject(game.getActivePlayer());
   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   // Attack target if tile contains enemy
   if ($board.obstacles[%x, %y] == 255 && game.getActivePlayer() == 0) {
      if ($nappulaSprite[%selectedPiece].hasDoneAction == 0) 
      {
         echo("%hero=" @ %hero);
         echo("%hero.getSelectedTileX()=" @ %hero.getSelectedTileX());
         echo("%hero.getSelectedTileY()=" @ %hero.getSelectedTileY());
         echo("$board.nappulat[%hero.getSelectedTileX(),%hero.getSelectedTileY()]=" @ $board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]);            
         %enemy = $nappulaSprite[$board.nappulat[%hero.getSelectedTileX(), %hero.getSelectedTileY()]];
         echo("%enemy=" @ %enemy);
         %this.doAttack(%enemy, game.getTurn());
      }
      return;
   } else

   // Walk to empty tile
   if ($board.obstacles[%x, %y] == 0) {
      if ($nappulaSprite[%selectedPiece].hasMoved == 0) 
      {
         %piece = $nappulaSprite[%selectedPiece];
         %this.movePiece(boardToWorldUnit(%x, %y), %x SPC %y);
         
         deleteGrids(1);
         
         %pos = %piece.positionOnBoard;
         %posx = getWord(%pos, 0);
         %posy = getWord(%pos, 1);

         $board.obstacles[%posx, %posy] = 0;
         $board.nappulat[%posx, %posy] = 0;
         $board.pieces[%posx, %posy] = 0;
         
         $board.obstacles[%x, %y] = 255;
         $board.nappulat[%x, %y] = %selectedPiece;
         $board.pieces[%x, %y] = %selectedPiece;
         
         %piece.BoardX = %x;
         %piece.BoardY = %y;
         %piece.positionOnBoard = %x SPC %y;
               
         %piece.hasMoved = 1;

         if (game.getState() != $gameEndState) { 
            if (game.getActivePlayer() == 0) 
            {   
               evaluateTacticalMoves(); 
            }
            else
            {
               if (isObject(TacticalAI) && isEventPending(TacticalAI.aiMoveFollowupEvent))
                  cancel(TacticalAI.aiMoveFollowupEvent);
               %followup = schedule(1000, 0, evaluateTacticalMoves);
               if (isObject(TacticalAI))
                  TacticalAI.aiMoveFollowupEvent = %followup;
            }
         }
      }
   }
}

function CombatEngine::movePiece(%this, %pos, %boardpos)
{
   %selectedPiece = game.getSelectedPiece();
   if (%selectedPiece $= "" || !isObject($nappulaSprite[%selectedPiece]))
      return;

   %piece = $nappulaSprite[%selectedPiece];

   %piece.setLayer("2");
   %piece.setSceneLayerDepthFront();

   %piece.facing = %this.facing(%piece.positionOnBoard, %boardpos);
   %piece.moveto(%pos, 10, true, true, 0.1);
   
   %this.setPieceState(%piece, $olentoWalkState[%piece.creatureID]);
   $fxfoot = playArchonSound(fxFootStep);
   if (isObject($UnitStatsHud))
      $UnitStatsHud.startMovementTracking(1100);
}

function CombatEngine::facing(%this, %pos1, %pos2)
{
   %fromx = getWord(%pos1, 0);
   %fromy = getWord(%pos1, 1);
   %targetx = getWord(%pos2, 0);
   %targety = getWord(%pos2, 1);
   
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

function CombatEngine::initAnimStates(%this)
{
   $unitNames = "warrior archer cleric paladin skeletonwarrior skeletonarcher dreadzombie ancientghost dragonfolk reddragon airdragon golddragon gremlin hornedoneye mistress demon";
   $nappulaNimi = $unitNames;

   for (%i = 0; %i < 16; %i++)
   {
      %name = getword($unitNames, %i);
      $olentoStandState[%i] = new ScriptObject() {};
      $olentoStandState[%i].anim = %name @ "StoppedAnimation_";
      $olentoStandState[%i].restore = 0;
      $olentoStandState[%i].name = "stopped";

      $olentoWalkState[%i] = new ScriptObject() {}; 
      $olentoWalkState[%i].anim = %name @ "WalkingAnimation_";
      $olentoWalkState[%i].name = "walking";
      $olentoWalkState[%i].restore = 0;

      $olentoBeenHitState[%i] = new ScriptObject() {}; 
      $olentoBeenHitState[%i].anim = %name @ "BeenHitAnimation_";
      $olentoBeenHitState[%i].restore = 0;
      $olentoBeenHitState[%i].name = "beenhit";

      $olentoAttackState[%i] = new ScriptObject() {}; 
      $olentoAttackState[%i].anim = %name @ "AttackAnimation_";
      $olentoAttackState[%i].restore = 0;
      $olentoAttackState[%i].name = "attack";

      $olentoDieState[%i] = new ScriptObject() {}; 
      $olentoDieState[%i].anim = %name @ "TippingOverAnimation_";
      $olentoDieState[%i].restore = 0;
      $olentoDieState[%i].name = "die";
   }
}

function CombatEngine::setPieceState(%this, %piece, %state)
{
   if (!%piece || !%state)
   {
      endgame();
   }
   
   echo("");
   echo("param: %piece, %state=" @ %piece @ ", " @ %state @ "(" @ %state.name @ ")");
   echo("setPieceState, pieceIndex=" @ %piece.index);
   
   %piece.state = %state;
   %restore = %piece.state.restore;
   %playanim = %state.anim @ %piece.facing;
   
   echo("playanim, pieceIndex=" @ %playanim);
   echo("");
   
   %piece.playAnimation(%playanim, %restore);
   %piece.setFrameChangeCallback(1);

   if (%state.name $= "die" || %state == $olentoDieState[%piece.creatureID])
   {
      %piece.setLayer("3");
      %piece.setSceneLayerDepthBack();
   }
   else if (%piece.alive)
   {
      %piece.setLayer("2");
      %piece.setSceneLayerDepthFront();
   }
}

function CombatEngine::damagePiece(%this, %piece)
{
   echo("damagePiece:" SPC %piece);
   %piece.hp = %piece.hp - 1;
   
   if (%piece.hp <= 0) {
      %piece.alive = 0;
      %piece.setLayer("3");
      %piece.setSceneLayerDepthBack();
      schedule(100, 0, "setPieceState", %piece, $olentoDieState[%piece.creatureID]);
      echo("setPieceState(" @ %piece @ "," @ $olentoDieState[%piece.creatureID] @ ")");
      %this.triggerBloodEffect(%piece.getPosition(), 500);
      $soitadie = playArchonSound(fxDie);

      %selectedPiece = game.getSelectedPiece();
      if (%selectedPiece !$= "" && isObject($nappulaSprite[%selectedPiece]))
      {
         $nappulaSprite[%selectedPiece].hasMoved = 1;
         $nappulaSprite[%selectedPiece].hasDoneAction = 1;
      }

      $board.obstacles[%piece.boardX, %piece.boardY] = 0;
      $board.nappulat[%piece.boardX, %piece.boardY] = 0;
      $board.pieces[%piece.boardX, %piece.boardY] = 0;

      removePieceTurn(%piece.index);

      if (game.getState() != $gameEndState) 
      {
         evaluateTacticalMoves();
      }
   }
   if (isObject($UnitStatsHud))
      $UnitStatsHud.refreshAll();
}

//---------------------------------------------------------------------------------------------
// Global Delegation Wrapper Stubs (Backward Compatibility)
//---------------------------------------------------------------------------------------------
function doAction()                                { CombatEngine.doAction(); }
function doDefence(%turn)                          { CombatEngine.doDefence(%turn); }
function doDefense(%turn)                          { CombatEngine.doDefence(%turn); }
function doMagic()                                 { CombatEngine.doMagic(); }
function getSpellTargetCategory(%spellFrame)       { return CombatEngine.getSpellTargetCategory(%spellFrame); }
function castRevealedSpell(%clickedCategory, %piece) { return CombatEngine.castRevealedSpell(%clickedCategory, %piece); }
function updateMagicButtonParticleState()          { CombatEngine.updateMagicButtonParticles(); }
function doAttack(%target, %turn)                  { CombatEngine.doAttack(%target, %turn); }
function hideBlood()                               { CombatEngine.hideBlood(); }
function piilotaveri()                             { CombatEngine.hideBlood(); }
function rollAttackDice(%att, %def)                { return CombatEngine.rollAttackDice(%att, %def); }
function rollDicesRedAttacks(%att, %def)           { return CombatEngine.rollAttackDice(%att, %def); }
function rollDefenseDice(%def, %att)               { return CombatEngine.rollDefenseDice(%def, %att); }
function rollDicesRedDefends(%def, %att)           { return CombatEngine.rollDefenseDice(%def, %att); }
function rollMovementDice(%player)                 { return CombatEngine.rollMovementDice(%player); }
function doMovement(%x, %y)                        { CombatEngine.doMovement(%x, %y); }
function movePiece(%pos, %bpos)                    { CombatEngine.movePiece(%pos, %bpos); }
function moveNappula(%pos, %bpos)                  { CombatEngine.movePiece(%pos, %bpos); }
function facing(%p1, %p2)                          { return CombatEngine.facing(%p1, %p2); }
function InitAnimStates()                          { CombatEngine.initAnimStates(); }
function setPieceState(%piece, %state)             { CombatEngine.setPieceState(%piece, %state); }
function setNappulaState(%piece, %state)           { CombatEngine.setPieceState(%piece, %state); }
function damagePiece(%piece)                       { CombatEngine.damagePiece(%piece); }
function vahingoitaNappula(%piece)                 { CombatEngine.damagePiece(%piece); }
function cancelDiceRoll()                          { CombatEngine.cancelDiceRoll(); }
function stopDiceAnimation()                       { CombatEngine.stopDiceAnimation(); }
function isInputLocked()                           { return CombatEngine.isInputLocked(); }
function lockInput()                               { CombatEngine.lockInput(); }
function unlockInput()                             { CombatEngine.unlockInput(); }

// Initialize Singleton
if (!isObject(CombatEngine))
{
   new ScriptObject(CombatEngine) { };
}
