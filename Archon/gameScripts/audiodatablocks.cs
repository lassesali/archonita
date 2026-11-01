//---------------------------------------------------------------------------------------------
// Archonita Audio Assets Mapping for Torque2D 4.0
// Audio profiles are declared as AudioAssets in Archon/data/audio/*.asset.taml
//---------------------------------------------------------------------------------------------
$ambient1Audio = "Archon:ambient1Audio";
$ambient4Audio = "Archon:ambient4Audio";
$fxSword = "Archon:fxSword";
$fxBeenHit = "Archon:fxBeenHit";
$fxFootStep = "Archon:fxFootStep";
$fxDie = "Archon:fxDie";

for (%i = 1; %i <= 9; %i++)
{
   $diceRollAudio[%i] = "Archon:diceroll" @ %i;
}

// Spell Reveal & Spell Casting Audio Assets
$sfxSpellReveal = "Archon:sfx_spell_reveal";
$sfxSpellFrame[3] = "Archon:sfx_spell_frame3_taikamakkara";
$sfxSpellFrame[4] = "Archon:sfx_spell_frame4_taikahattu";
$sfxSpellFrame[5] = "Archon:sfx_spell_frame5_taikatikkari";
$sfxSpellFrame[6] = "Archon:sfx_spell_frame6_tulivuori";
$sfxSpellFrame[7] = "Archon:sfx_spell_frame7_voikukan_juuri";
$sfxSpellFrame[8] = "Archon:sfx_spell_frame8_vatsatauti";
