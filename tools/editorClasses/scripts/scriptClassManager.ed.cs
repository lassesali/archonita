//-----------------------------------------------------------------------------
// Script Class Group Manager.
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Small Helper to safely retrieve a ScriptClass library simGroup
//
// Returns : Script Class Group or 0.
//-----------------------------------------------------------------------------
function ScriptClassManager::GetScriptClassList( %classLibrary )
{
   // Verify Script Class Group Exists
   if( !isObject( ScriptClassGroup ) )
   {
      error("ScriptClassManager::GetScriptClassList - Fatal Error - ScriptClassGroup does not exist!");
      return 0;
   }

   // Return Result
   return ScriptClassGroup.findObjectByInternalName( %classLibrary );   
}