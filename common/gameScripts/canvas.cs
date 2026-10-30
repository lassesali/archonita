//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// initializeCanvas
// Constructs and initializes the default canvas window.
//---------------------------------------------------------------------------------------------
$canvasCreated = false;
function initializeCanvas(%windowName)
{
   // We must note this before we create the canvas for the first time
   // otherwise it will set the default res
   if( $pref::Video::windowedRes $= "" && $pref::Video::fullScreen != true )
      %doSetMaxRes = true;
   else
      %doSetMaxRes = false;
      
   
   // Don't duplicate the canvas.
   if($canvasCreated)
   {
      error("Cannot instantiate more than one canvas!");
      return;
   }
   
   videoSetGammaCorrection($pref::OpenGL::gammaCorrection);
   
   if (!createCanvas(%windowName))
   {
      error("Canvas creation failed. Shutting down.");
      quit();
   }
   
   
   if( %doSetMaxRes )
   {
      // Get the list of valid resolutions.
      %resList = getResolutionList( $pref::Video::displayDevice );
      %resCount = getFieldCount(%resList);
      %deskRes = getDesktopResolution();
        
      // Loop through all the valid resolutions.
      for (%i = (%resCount - 1); %i >= 0; %i-- )
      {
         %res = getWords(getField(%resList, %i), 0, 2);
               
         if (firstWord(%res) >= firstWord(%deskRes))
            continue;
         if (getWord(%res, 1) >= getWord(%deskRes, 1))
            continue;
            
         $pref::Video::windowedRes = %res;
         
         setScreenMode( GetWord( %res, 0), GetWord( %res,1), GetWord( %res,2), false );
         
         %i = 0;
         
      }      
   }


   $canvasCreated = true;
}

//---------------------------------------------------------------------------------------------
// resetCanvas
// Forces the canvas to redraw itself.
//---------------------------------------------------------------------------------------------
function resetCanvas()
{
   if (isObject(Canvas))
      Canvas.repaint();
}
