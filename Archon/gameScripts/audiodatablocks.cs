new AudioDescription(AudioLooping)
{
   volume   = 1.0;
   isLooping= true;
   is3D     = false;
   type     = $GuiAudioType;
};

new AudioDescription(AudioNonLooping)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = $GuiAudioType;
};

new AudioProfile(ambient1Audio)
{
	filename = "~/data/audio/music.ogg";
	description = "AudioLooping";
	preload = false;
};




new AudioProfile(ambient4Audio)
{
	filename = "~/data/audio/popclick.wav";
	description = "AudioNonLooping";
	preload = false;
};

new AudioProfile(fxSword)
{
	filename = "~/data/audio/sword.wav";
	description = "AudioNonLooping";
	preload = false;
};

new AudioProfile(fxBeenHit)
{
	filename = "~/data/audio/beenhit.wav";
	description = "AudioNonLooping";
	preload = false;
};


new AudioProfile(fxFootStep)
{
	filename = "~/data/audio/footstep.wav";
	description = "AudioNonLooping";
	preload = false;
};

new AudioProfile(fxDie)
{
	filename = "~/data/audio/die.wav";
	description = "AudioNonLooping";
	preload = false;
};

