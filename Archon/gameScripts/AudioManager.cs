//---------------------------------------------------------------------------------------------
// Archonita - Audio Manager
// Manages continuous background music playback, seamless looping across scenes,
// and audio asset triggers in Torque2D 4.0.
//---------------------------------------------------------------------------------------------

function AudioManager::onAdd(%this)
{
   %this.bgMusic = 0;
   %this.bgMusicSchedule = "";
   %this.loopInterval = 2000;
   %this.isMusicPaused = false;

   // Mute states (restored from persistent preferences if available)
   %this.musicMuted = ($pref::Audio::musicMuted == 1 || $pref::Audio::musicMuted $= "true");
   %this.sfxMuted = ($pref::Audio::sfxMuted == 1 || $pref::Audio::sfxMuted $= "true");
   %this.activeSFXCount = 0;

   // Dice roll audio pool configuration
   %this.diceSoundCount = 9;
   %this.lastDiceRollIndex = 0;
   %this.maxConcurrentDiceSounds = 4;
   %this.activeDiceSoundCount = 0;

   // Preload spell audio assets to eliminate disk I/O lag on trigger
   %this.preloadSpellAudio();
}

function AudioManager::onRemove(%this)
{
   %this.stopBackgroundMusic();
   %this.stopAllDiceSounds();
   %this.stopAllSFXHandles();
}

function AudioManager::isMusicMuted(%this)
{
   return (%this.musicMuted == 1 || %this.musicMuted $= "true");
}

function AudioManager::isMusicPaused(%this)
{
   return (%this.isMusicPaused == true || %this.isMusicPaused $= "true");
}

function AudioManager::isSFXMuted(%this)
{
   return (%this.sfxMuted == 1 || %this.sfxMuted $= "true");
}

function AudioManager::setMusicMuted(%this, %muted)
{
   %this.musicMuted = (%muted == 1 || %muted $= "true") ? true : false;
   $pref::Audio::musicMuted = %this.musicMuted;

   if (%this.musicMuted)
   {
      %this.pauseBackgroundMusic();
   }
   else
   {
      %state = getGameState();
      if ($gameCreditsState $= "" || %state != $gameCreditsState)
         %this.startBackgroundMusic();
   }

   if (isObject($BoardAudioHud))
      $BoardAudioHud.updateVisuals();
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.updateVisuals();
   if (isObject($ResolutionManager))
      $ResolutionManager.savePreferences();
}

function AudioManager::toggleMusicMute(%this)
{
   %this.setMusicMuted(!%this.isMusicMuted());
}

function AudioManager::setSFXMuted(%this, %muted)
{
   %this.sfxMuted = (%muted == 1 || %muted $= "true") ? true : false;
   $pref::Audio::sfxMuted = %this.sfxMuted;

   if (%this.sfxMuted)
   {
      %this.stopAllDiceSounds();
      %this.stopAllSFXHandles();
   }

   if (isObject($BoardAudioHud))
      $BoardAudioHud.updateVisuals();
   if (isObject($OptionsMenuGui))
      $OptionsMenuGui.updateVisuals();
   if (isObject($ResolutionManager))
      $ResolutionManager.savePreferences();
}

function AudioManager::toggleSFXMute(%this)
{
   %this.setSFXMuted(!%this.isSFXMuted());
}

function AudioManager::registerSFXHandle(%this, %handle)
{
   if (%handle == 0)
      return;

   %validCount = 0;
   for (%i = 0; %i < %this.activeSFXCount; %i++)
   {
      %h = %this.activeSFX[%i];
      if (%h != 0 && alxIsPlaying(%h))
      {
         %valid[%validCount] = %h;
         %validCount++;
      }
   }
   for (%i = 0; %i < %validCount; %i++)
      %this.activeSFX[%i] = %valid[%i];

   %this.activeSFX[%validCount] = %handle;
   %this.activeSFXCount = %validCount + 1;
}

function AudioManager::stopAllSFXHandles(%this)
{
   for (%i = 0; %i < %this.activeSFXCount; %i++)
   {
      %h = %this.activeSFX[%i];
      if (%h != 0 && alxIsPlaying(%h))
         alxStop(%h);
      %this.activeSFX[%i] = 0;
   }
   %this.activeSFXCount = 0;
}

function AudioManager::isMusicPlaying(%this)
{
   if (%this.bgMusic != 0 && alxIsPlaying(%this.bgMusic))
      return true;

   if ($introMusic != 0 && alxIsPlaying($introMusic))
      return true;

   return false;
}

function AudioManager::pauseBackgroundMusic(%this)
{
   if (%this.bgMusicSchedule !$= "")
   {
      cancel(%this.bgMusicSchedule);
      %this.bgMusicSchedule = "";
   }

   if (%this.bgMusic != 0)
   {
      alxPause(%this.bgMusic);
      %this.isMusicPaused = true;
   }

   if ($introMusic != 0 && $introMusic != %this.bgMusic)
   {
      alxPause($introMusic);
   }
}

function AudioManager::resumeBackgroundMusic(%this)
{
   return %this.startBackgroundMusic();
}

function AudioManager::startBackgroundMusic(%this)
{
   if (%this.isMusicMuted())
   {
      %this.pauseBackgroundMusic();
      return 0;
   }

   if (%this.isMusicPaused && %this.bgMusic != 0)
   {
      alxUnpause(%this.bgMusic);
      if ($introMusic != 0 && $introMusic != %this.bgMusic)
         alxUnpause($introMusic);
      %this.isMusicPaused = false;

      if (%this.isMusicPlaying())
      {
         if (%this.bgMusicSchedule !$= "")
            cancel(%this.bgMusicSchedule);
         %this.bgMusicSchedule = %this.schedule(%this.loopInterval, checkBackgroundMusicLoop);
         return %this.bgMusic;
      }

      %this.bgMusic = 0;
   }

   if (%this.isMusicPlaying())
   {
      if (%this.bgMusic == 0 && $introMusic != 0)
         %this.bgMusic = $introMusic;
      $introMusic = %this.bgMusic;

      if (%this.bgMusicSchedule $= "")
         %this.bgMusicSchedule = %this.schedule(%this.loopInterval, checkBackgroundMusicLoop);
      return %this.bgMusic;
   }

   // Play ambient1Audio
   %this.bgMusic = playArchonSound(ambient1Audio);
   $introMusic = %this.bgMusic;
   %this.isMusicPaused = false;

   if (%this.bgMusicSchedule !$= "")
   {
      cancel(%this.bgMusicSchedule);
      %this.bgMusicSchedule = "";
   }

   %this.bgMusicSchedule = %this.schedule(%this.loopInterval, checkBackgroundMusicLoop);
   return %this.bgMusic;
}

function AudioManager::checkBackgroundMusicLoop(%this)
{
   %this.bgMusicSchedule = "";

   if (%this.isMusicMuted())
      return;

   %state = getGameState();
   if ($gameCreditsState !$= "" && %state == $gameCreditsState)
      return;

   if (!%this.isMusicPlaying())
   {
      %this.bgMusic = playArchonSound(ambient1Audio);
      $introMusic = %this.bgMusic;
   }

   %this.bgMusicSchedule = %this.schedule(%this.loopInterval, checkBackgroundMusicLoop);
}

function AudioManager::stopBackgroundMusic(%this)
{
   if (%this.bgMusicSchedule !$= "")
   {
      cancel(%this.bgMusicSchedule);
      %this.bgMusicSchedule = "";
   }

   if (%this.bgMusic != 0)
   {
      alxStop(%this.bgMusic);
      %this.bgMusic = 0;
   }

   if ($introMusic != 0)
   {
      alxStop($introMusic);
      $introMusic = 0;
   }

   %this.isMusicPaused = false;
}

//---------------------------------------------------------------------------------------------
// Dice Roll Audio Pool & Playback Logic
//---------------------------------------------------------------------------------------------

function AudioManager::getRandomDiceRollIndex(%this)
{
   %count = (%this.diceSoundCount > 0) ? %this.diceSoundCount : 9;
   if (%count <= 1)
      return 1;

   %roll = getRandom(1, %count);
   // No immediate repeats check: ensure the same clip isn't picked back-to-back
   if (%roll == %this.lastDiceRollIndex)
   {
      %offset = getRandom(1, %count - 1);
      %roll = ((%this.lastDiceRollIndex - 1 + %offset) % %count) + 1;
   }
   %this.lastDiceRollIndex = %roll;
   return %roll;
}

function AudioManager::pruneActiveDiceSounds(%this)
{
   %validCount = 0;
   for (%i = 0; %i < %this.activeDiceSoundCount; %i++)
   {
      %h = %this.activeDiceSound[%i];
      if (%h != 0 && alxIsPlaying(%h))
      {
         %valid[%validCount] = %h;
         %validCount++;
      }
   }
   for (%i = 0; %i < %validCount; %i++)
      %this.activeDiceSound[%i] = %valid[%i];
   for (%i = %validCount; %i < %this.activeDiceSoundCount; %i++)
      %this.activeDiceSound[%i] = 0;

   %this.activeDiceSoundCount = %validCount;
}

function AudioManager::removeActiveDiceSoundIndex(%this, %index)
{
   if (%index < 0 || %index >= %this.activeDiceSoundCount)
      return;

   for (%i = %index; %i < %this.activeDiceSoundCount - 1; %i++)
      %this.activeDiceSound[%i] = %this.activeDiceSound[%i + 1];

   %this.activeDiceSound[%this.activeDiceSoundCount - 1] = 0;
   %this.activeDiceSoundCount--;
}

function AudioManager::stopAllDiceSounds(%this)
{
   for (%i = 0; %i < %this.activeDiceSoundCount; %i++)
   {
      %handle = %this.activeDiceSound[%i];
      if (%handle != 0 && alxIsPlaying(%handle))
      {
         alxStop(%handle);
      }
      %this.activeDiceSound[%i] = 0;
   }
   %this.activeDiceSoundCount = 0;
}

function AudioManager::playDiceRollSound(%this, %volume)
{
   if (%this.isSFXMuted())
      return 0;

   // Prune finished sounds from active channel pool
   %this.pruneActiveDiceSounds();

   // Channel pooling: if max concurrent channels are in use, gracefully reclaim the oldest
   %maxVoices = (%this.maxConcurrentDiceSounds > 0) ? %this.maxConcurrentDiceSounds : 4;
   while (%this.activeDiceSoundCount >= %maxVoices)
   {
      %oldest = %this.activeDiceSound[0];
      if (%oldest != 0 && alxIsPlaying(%oldest))
         alxStop(%oldest);
      %this.removeActiveDiceSoundIndex(0);
   }

   // Pick randomized index (1..9) with no immediate back-to-back repeats
   %index = %this.getRandomDiceRollIndex();
   %assetName = "Archon:diceroll" @ %index;

   // Update timestamp for multi-die burst coordination
   $lastDiceAnimationSoundTick = getSimTime();

   // Play sound
   %handle = playArchonSound(%assetName);
   if (%handle == 0 || !alxIsPlaying(%handle))
      return 0;

   // Volume balancing: prevent clipping when multiple dice roll sounds play concurrently
   if (%volume !$= "")
   {
      %gain = mClamp(%volume, 0, 1);
      alxSourcef(%handle, AL_GAIN_LINEAR, %gain);
   }
   else if (%this.activeDiceSoundCount > 0)
   {
      // Slightly attenuate concurrent voices so overlapping sounds mix cleanly
      alxSourcef(%handle, AL_GAIN_LINEAR, 0.85);
   }

   // Register in active pool for concurrent tracking
   %this.activeDiceSound[%this.activeDiceSoundCount] = %handle;
   %this.activeDiceSoundCount++;

   return %handle;
}

//---------------------------------------------------------------------------------------------
// Spell Audio Preloading & Playback
//---------------------------------------------------------------------------------------------

function AudioManager::preloadSpellAudio(%this)
{
   if (isObject(AssetDatabase))
   {
      AssetDatabase.acquireAsset("Archon:sfx_spell_reveal");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame3_taikamakkara");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame4_taikahattu");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame5_taikatikkari");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame6_tulivuori");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame7_voikukan_juuri");
      AssetDatabase.acquireAsset("Archon:sfx_spell_frame8_vatsatauti");
   }
}

function AudioManager::playSpellRevealSound(%this)
{
   return playArchonSound("Archon:sfx_spell_reveal");
}

function AudioManager::playSpellCastSound(%this, %spellFrame)
{
   %asset = $sfxSpellFrame[%spellFrame];
   if (%asset $= "")
   {
      switch (%spellFrame)
      {
         case 3: %asset = "Archon:sfx_spell_frame3_taikamakkara";
         case 4: %asset = "Archon:sfx_spell_frame4_taikahattu";
         case 5: %asset = "Archon:sfx_spell_frame5_taikatikkari";
         case 6: %asset = "Archon:sfx_spell_frame6_tulivuori";
         case 7: %asset = "Archon:sfx_spell_frame7_voikukan_juuri";
         case 8: %asset = "Archon:sfx_spell_frame8_vatsatauti";
         default: return 0;
      }
   }
   return playArchonSound(%asset);
}

//---------------------------------------------------------------------------------------------
// Global Convenience Wrappers
//---------------------------------------------------------------------------------------------

function startBackgroundMusic()
{
   if (isObject($AudioManager))
      return $AudioManager.startBackgroundMusic();
   else if (isObject(AudioManager))
      return AudioManager.startBackgroundMusic();
   return 0;
}

function stopBackgroundMusic()
{
   if (isObject($AudioManager))
      $AudioManager.stopBackgroundMusic();
   else if (isObject(AudioManager))
      AudioManager.stopBackgroundMusic();
}

function pauseBackgroundMusic()
{
   if (isObject($AudioManager))
      $AudioManager.pauseBackgroundMusic();
   else if (isObject(AudioManager))
      AudioManager.pauseBackgroundMusic();
}

function resumeBackgroundMusic()
{
   if (isObject($AudioManager))
      return $AudioManager.resumeBackgroundMusic();
   else if (isObject(AudioManager))
      return AudioManager.resumeBackgroundMusic();
   return 0;
}

function checkBackgroundMusicLoop()
{
   if (isObject($AudioManager))
      $AudioManager.checkBackgroundMusicLoop();
   else if (isObject(AudioManager))
      AudioManager.checkBackgroundMusicLoop();
}

function getRandomDiceRollIndex()
{
   if (isObject($AudioManager))
      return $AudioManager.getRandomDiceRollIndex();
   else if (isObject(AudioManager))
      return AudioManager.getRandomDiceRollIndex();
   return getRandom(1, 9);
}

function playDiceRollSound(%volume)
{
   if (isObject($AudioManager))
      return $AudioManager.playDiceRollSound(%volume);
   else if (isObject(AudioManager))
      return AudioManager.playDiceRollSound(%volume);
   return 0;
}

function stopAllDiceSounds()
{
   if (isObject($AudioManager))
      $AudioManager.stopAllDiceSounds();
   else if (isObject(AudioManager))
      AudioManager.stopAllDiceSounds();
}

function playSpellRevealSound()
{
   if (isObject($AudioManager))
      return $AudioManager.playSpellRevealSound();
   else if (isObject(AudioManager))
      return AudioManager.playSpellRevealSound();
   return playArchonSound("Archon:sfx_spell_reveal");
}

function playSpellCastSound(%spellFrame)
{
   if (isObject($AudioManager))
      return $AudioManager.playSpellCastSound(%spellFrame);
   else if (isObject(AudioManager))
      return AudioManager.playSpellCastSound(%spellFrame);
   return 0;
}

function toggleMusicMute()
{
   if (isObject($AudioManager))
      $AudioManager.toggleMusicMute();
   else if (isObject(AudioManager))
      AudioManager.toggleMusicMute();
}

function toggleSFXMute()
{
   if (isObject($AudioManager))
      $AudioManager.toggleSFXMute();
   else if (isObject(AudioManager))
      AudioManager.toggleSFXMute();
}

function setMusicMuted(%muted)
{
   if (isObject($AudioManager))
      $AudioManager.setMusicMuted(%muted);
   else if (isObject(AudioManager))
      AudioManager.setMusicMuted(%muted);
}

function setSFXMuted(%muted)
{
   if (isObject($AudioManager))
      $AudioManager.setSFXMuted(%muted);
   else if (isObject(AudioManager))
      AudioManager.setSFXMuted(%muted);
}

function isMusicMuted()
{
   if (isObject($AudioManager))
      return $AudioManager.isMusicMuted();
   else if (isObject(AudioManager))
      return AudioManager.isMusicMuted();
   return false;
}

function isMusicPaused()
{
   if (isObject($AudioManager))
      return $AudioManager.isMusicPaused();
   else if (isObject(AudioManager))
      return AudioManager.isMusicPaused();
   return false;
}

function isSFXMuted()
{
   if (isObject($AudioManager))
      return $AudioManager.isSFXMuted();
   else if (isObject(AudioManager))
      return AudioManager.isSFXMuted();
   return false;
}

// Initialize Singleton
if (!isObject(AudioManager))
{
   new ScriptObject(AudioManager) { };
}
if (!isObject($AudioManager))
{
   $AudioManager = AudioManager;
}
