//-----------------------------------------------------------------------------
// Torque2D Packaging Utility
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function initializePackagingUtility()
{
   echo( "\n--------- Initializing Packaging Utility ---------" );
   
   exec( "./packagingPrefs.ed.cs" );
   exec( "./packagingUtility.ed.cs" );
   exec( "./htmlReport.ed.cs" );
   exec( "./guiReport.ed.cs" );
   exec( "./fileRefList.ed.cs" );
   exec( "./fileRefObject.ed.cs" );
   exec( "./loadSave.ed.cs" );
   exec( "./ui/packagingUtilityDlg.ed.gui" );
   exec( "./ui/packagingOptionsDlg.ed.gui" );
   exec( "./ui/packagingStatusDlg.ed.gui"  );
   exec( "./ui/packagingExtensionsDlg.ed.gui" );

   GlobalActionMap.bindCmd(keyboard, "f6", "togglePackagingUtility();", "");
  
}

function destroyPackagingUtility()
{
}
