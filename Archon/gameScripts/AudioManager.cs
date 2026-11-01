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

   // Dice roll audio pool configuration
   %this.diceSoundCount = 9;
   %this.lastDiceRollIndex = 0;
   %this.maxConcurrentDiceSounds = 4;
   %this.activeDiceSoundCount = 0;
}

function AudioManager::onRemove(%this)
{
   %this.stopBackgroundMusic();
   %this.stopAllDiceSounds();
}

function AudioManager::isMusicPlaying(%this)
{
   if (%this.bgMusic != 0 && alxIsPlaying(%this.bgMusic))
      return true;

   if ($introMusic != 0 && alxIsPlaying($introMusic))
      return true;

   return false;
}

function AudioManager::startBackgroundMusic(%this)
{
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

   %state = getGameState();
   if (%state == $gameCreditsState)
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

// Initialize Singleton
if (!isObject(AudioManager))
{
   new ScriptObject(AudioManager) { class = "AudioManager"; };
}
if (!isObject($AudioManager))
{
   $AudioManager = AudioManager;
}
