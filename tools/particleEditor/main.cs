//-----------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializeParticleEditor()
{   
   echo( "\n--------- Initializing Particle Editor ---------" );

   exec("./scripts/newParticleEditor.ed.cs");
   exec("./scripts/ParticleEditorEffectFunctions.ed.cs");
   exec("./scripts/ParticleEditorEmitterFunctions.ed.cs");
   exec("./scripts/undo.ed.cs");
}

function destroyParticleEditor()
{
   // Export Preferences.
   echo("Exporting Particle Editor preferences.");
   export("$particleEditor::*", "./Preferences/prefs.cs", false);
}
