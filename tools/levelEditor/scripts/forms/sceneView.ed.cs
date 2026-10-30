//-----------------------------------------------------------------------------
// Gui SceneGraphEditCtrl Form Content
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Register Form Content.
//-----------------------------------------------------------------------------
$LBSceneViewContent = GuiFormManager::AddFormContent( "LevelBuilder", "Scene View", "LBSceneWindow::CreateForm", "LBSceneWindow::SaveForm", 2 );

//-----------------------------------------------------------------------------
// Form Content Creation Function
//-----------------------------------------------------------------------------
function LBSceneWindow::CreateForm( %formCtrl )
{    
   %base = new LevelBuilderSceneWindow() 
   {
      class = "LevelBuilderSceneView";
      canSaveDynamicFields = "0";
      Profile = "LBSceneWindow";
      HorizSizing = "width";
      VertSizing = "height";
      position = "0 32";
      Extent = "668 543";
      MinExtent = "10 10";
      canSave = "0";
      visible = "1";
      internalName = "LevelBuilderSceneView";
      tooltipprofile = "GuiInputCtrlProfile";
      hovertime = "0";
      lockMouse = "0";
      useWindowMouseEvents = "1";
      useObjectMouseEvents = "0";
      bob = "your uncle";
   };
   %formCtrl.add(%base);
   %formCtrl.canSaveDynamicFields = 0;

   // Propagate Saved State Settings.
   if( %formCtrl.lastCameraZoom !$= "" )
      %base.setCurrentCameraZoom( %formCtrl.lastCameraZoom );
   if( %formCtrl.lastCameraPosition !$= "" )
      %base.setCurrentCameraPosition( %formCtrl.lastCameraPosition );

   if( %formCtrl.lastSceneGraph !$= "" && isObject( %formCtrl.lastSceneGraph ) && %formCtrl.lastSceneGraph.getClassName() $= "t2dSceneGraph" )
      %base.setSceneGraph( %formCtrl.lastSceneGraph );
   else if (isObject($currentSceneGraph))
      %base.setSceneGraph($currentSceneGraph);
   else
      // Request Notification of Active SceneGraph
      GuiFormManager::SendContentMessage( $LBSceneViewContent, %base, "getSceneGraph" );
      
  if (!isObject(ToolManager.getActiveTool()))
      LevelBuilderToolManager::setTool(LevelEditorSelectionTool);

      
   // Resize as appropriate.
   if( %formCtrl.isMethod("sizeContentsToFit") )
      %formctrl.sizeContentsToFit(%base, %formCtrl.contentID.margin);

   //*** Return back the base control to indicate we were successful
   return %base;

}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LBSceneWindow::SaveForm( %formCtrl, %contentObj )
{
   %formCtrl.lastCameraZoom = %contentObj.getCurrentCameraZoom();
   %formCtrl.lastCameraPosition = %contentObj.getCurrentCameraPosition();
   %formCtrl.lastSceneGraph = %contentObj.getSceneGraph();
}

//-----------------------------------------------------------------------------
// Form Content Save Function
//-----------------------------------------------------------------------------
function LevelBuilderSceneView::onContentMessage( %this, %sender, %message )
{

   %command = GetWord( %message, 0 );
   %value   = GetWord( %message, 1 );

   switch$( %command )
   {
      case "updateScenegraph":
         // Update scenegraph to %value's scenegraph.
         if( isObject( %value ) )
            %this.setSceneGraph( %value );
      case "getSceneGraph":
         // Update sibling view with our scenegraph.
         if( %this.getSceneGraph() )
            %sender.onContentMessage( %this, "updateScenegraph " @ %this.getSceneGraph() );
      case "playScene":
         %this.testScene();
      case "pauseScene":
         %this.endTestScene();
   }
}

//-----------------------------------------------------------------------------
// Form Content Functionality
//-----------------------------------------------------------------------------
function LevelBuilderSceneView::onAdd(%this)
{
   if (!isObject(ToolManager))
      error("LevelBuilderSceneWindow::onAdd() - No ToolManager Found!");
   else
      %this.setSceneEdit(ToolManager);
}

function LevelBuilderSceneView::onRightMouseDown(%this, %modifier, %position, %clicks)
{
   %this.rightMouseDownPosition = %this.getWindowPoint(%position);
}

function LevelBuilderSceneView::onRightMouseDragged(%this, %modifier, %position, %clicks)
{
   // It may seem wierd that we are converting to window coords, then back to scene coords... and it is. But,
   // for some reason I couldn't get it to work without these conversions and I have no idea why.
   %pos = %this.getWindowPoint(%position);
   %zoom = %this.getCurrentCameraZoom();
   %width = getWord(%this.getCurrentCameraArea(), 2) - getWord(%this.getCurrentCameraArea(), 0) + 1;
   %height = getWord(%this.getCurrentCameraArea(), 3) - getWord(%this.getCurrentCameraArea(), 1) + 1;
   %xSize = (%width / getWord(%this.extent, 0)) / %zoom;
   %ySize = (%height / getWord(%this.extent, 1)) / %zoom;

   // Get the position of the camera in scene space
   %x = getWord(%this.getCurrentCameraPosition(), 0);
   %y = getWord(%this.getCurrentCameraPosition(), 1);

   // Find the difference in mouse position since the last update
   // and scale it by the scene window to screen res ratio
   %x += (getWord(%this.rightMouseDownPosition, 0) - getWord(%pos, 0)) * %xSize;
   %y += (getWord(%this.rightMouseDownPosition, 1) - getWord(%pos, 1)) * %ySize;

   // Update the previous position
   %this.rightMouseDownPosition = %pos;

   // Update the camera position
   %this.setCurrentCameraPosition(%x SPC %y);
}

function LevelBuilderSceneView::onMouseWheelDown(%this, %modifier, %position, %clicks)
{
   %zoom = %this.getCurrentCameraZoom();
   %amount = -120 * 0.0005 * %zoom;
   %this.setCurrentCameraZoom(%zoom + %amount);
}

function LevelBuilderSceneView::onMouseWheelUp(%this, %modifier, %position, %clicks)
{
   %zoom = %this.getCurrentCameraZoom();
   %amount = 120 * 0.0005 * %zoom;
   %this.setCurrentCameraZoom(%zoom + %amount);
}

function LevelBuilderSceneView::onControlDropped(%this, %control, %position)
{
   if (isObject(%control.sceneObject))
   {
      
      %datablockName = %control.datablockName;
      %toolType      = %control.toolType;
   
      switch$( %toolType )
      {
         case "t2dStaticSprite":
               %object = %control.getSceneObject();
               %imageMode = %datablockName.getImageMapMode();
               if( (%imageMode $= "CELL") || (%imageMode $= "LINK") || (%imageMode $= "KEY") )
               {
                  %currentFrame = %object.getFrame();
                  levelEditorStaticSpriteTool.setImageMap( %datablockName, %currentFrame );   
               }    
               else
                  levelEditorStaticSpriteTool.setImageMap( %datablockName, 0 );
               $selectedStaticSprite = %datablockName;
         case "t2dAnimatedSprite":
            $selectedAnimatedSprite = %datablockName;
            levelEditorAnimatedSpriteTool.setAnimation( %datablockName );
         case "t2dChunkedSprite":
            $selectedChunkedSprite = %datablockName;
            levelEditorChunkedSpriteTool.setImageMap( %datablockName );        
         case "t2dScroller":
            $selectedScroller = %datablockName;
            levelEditorScrollerTool.setImageMap( %datablockName );
         case "t2dTileLayer":
            $selectedTileMap = %datablockName;
            levelEditorTileMapTool.setTileLayerFile( %datablockName );
         case "t2dParticleEffect":
            $selectedParticleEffect = %datablockName;
            levelEditorParticleTool.setEffect( %datablockName );
         case "t2dShape3D":
            $selectedShape3D = %datablockName;
            levelEditor3DShapeTool.setShape( %datablockName );
         case "t2dSceneObject":
         case "t2dTrigger":
      }
   
      // Set the proper tool for this object.
      LevelBuilderToolManager::setCreateTool( %toolType );
      
      ToolManager.getActiveTool().createObject(%this, %this.getWorldPoint(t2dVectorSub(%position, %this.getGlobalPosition())));
   }
}
