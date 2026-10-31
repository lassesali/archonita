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
}

function AudioManager::onRemove(%this)
{
   %this.stopBackgroundMusic();
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
// Global Convenience Wrappers
//---------------------------------------------------------------------------------------------

function startBackgroundMusic()
{
   if (isObject($AudioManager))
      return $AudioManager.startBackgroundMusic();
   return 0;
}

function stopBackgroundMusic()
{
   if (isObject($AudioManager))
      $AudioManager.stopBackgroundMusic();
}

function checkBackgroundMusicLoop()
{
   if (isObject($AudioManager))
      $AudioManager.checkBackgroundMusicLoop();
}
