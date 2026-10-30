//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

function updateOptions()
{
   $levelEditor::ShowGrid = T2DLVLShowGrid.getValue();
   $levelEditor::SnapX = T2DLVLGridCheckX.getValue();
   $levelEditor::SnapY = T2DLVLGridCheckY.getValue();
   $levelEditor::GridSizeX = T2DLVLGridUnitX.getText();
   $levelEditor::GridSizeY = T2DLVLGridUnitY.getText();
   $levelEditor::BackgroundColor = T2DBGColorControl.getValue();
   $levelEditor::BackgroundColorPos = T2DBGColorControl.getSelectorPos();

   if(T2DObjectLibraryColorLight.getValue())
      $levelEditor::ObjectLibraryBackgroundColor = "Light";
   else if(T2DObjectLibraryColorMedium.getValue())
      $levelEditor::ObjectLibraryBackgroundColor = "Medium";
   else if(T2DObjectLibraryColorDark.getValue())
      $levelEditor::ObjectLibraryBackgroundColor = "Dark";

   $levelEditor::GridColor = T2DGridColorControl.getValue();
   $levelEditor::GridColorPos = T2DGridColorControl.getSelectorPos();
   $levelEditor::SnapThreshold = T2DSnapThresholdText.getText();
   $levelEditor::FullContainSelection = T2DFullContainToggle.getValue();
   $levelEditor::UndoSelections = T2DUndoSelectionsToggle.getValue();
   $levelEditor::DesignResolutionX = T2DDesignResolutionXText.getText();
   $levelEditor::DesignResolutionY = T2DDesignResolutionYText.getText();
   $levelEditor::ShowCamera = T2DShowCameraToggle.getValue();
   $levelEditor::ShowGuides = T2DShowGuidesToggle.getValue();
   $levelEditor::DisplayScriptErrors = T2DDisplayErrorsToggle.getValue();
   
   applyOptions();
}

function applyOptions()
{
   T2DLVLShowGrid.setValue($levelEditor::ShowGrid);
   T2DLVLGridCheckX.setValue($levelEditor::SnapX);
   T2DLVLGridCheckY.setValue($levelEditor::SnapY);
   T2DLVLGridUnitX.setText($levelEditor::GridSizeX);
   T2DLVLGridUnitY.setText($levelEditor::GridSizeY);
   T2DBGColorControl.setSelectorPos($levelEditor::BackgroundColorPos);
   T2DGridColorControl.setSelectorPos($levelEditor::GridColorPos);
   T2DBGColorControl.updateColor();
   T2DGridColorControl.updateColor();
   
   T2DObjectLibraryColorLight.setValue(false);
   T2DObjectLibraryColorMedium.setValue(false);
   T2DObjectLibraryColorDark.setValue(false);
   switch$ ($levelEditor::ObjectLibraryBackgroundColor)
   {
      case "Light":
         T2DObjectLibraryColorLight.setValue(true);
      
      case "Medium":
         T2DObjectLibraryColorMedium.setValue(true);
      
      case "Dark":
         T2DObjectLibraryColorDark.setValue(true);
   }
   
   T2DSnapThresholdText.setText($levelEditor::SnapThreshold);
   T2DFullContainToggle.setValue($levelEditor::FullContainSelection);
   T2DUndoSelectionsToggle.setValue($levelEditor::UndoSelections);
   T2DDesignResolutionXText.setText($levelEditor::DesignResolutionX);
   T2DDesignResolutionYText.setText($levelEditor::DesignResolutionY);
   T2DShowCameraToggle.setValue($levelEditor::ShowCamera);
   T2DShowGuidesToggle.setValue($levelEditor::ShowGuides);
     
   ToolManager.setGridVisibility($levelEditor::ShowGrid);

   ToolManager.setSnapToGridX($levelEditor::SnapX);
   ToolManager.setGridSizeX($levelEditor::GridSizeX);     
   ToolManager.setSnapToGridY($levelEditor::SnapY);
   ToolManager.setGridSizeY($levelEditor::GridSizeY);
   
   ToolManager.setFillColor(mFloor(getWord($levelEditor::BackgroundColor,0)*255),
                            mFloor(getWord($levelEditor::BackgroundColor,1)*255), 
                            mFloor(getWord($levelEditor::BackgroundColor,2)*255));
   
   ToolManager.setGridColor(mFloor(getWord($levelEditor::GridColor,0)*255),
                            mFloor(getWord($levelEditor::GridColor,1)*255), 
                            mFloor(getWord($levelEditor::GridColor,2)*255), 60);
   
   GuiFormManager::SendContentMessage($LBCreateSiderBar, "", "changeProfiles" SPC $levelEditor::ObjectLibraryBackgroundColor);
                            
   ToolManager.setDesignResolution($levelEditor::DesignResolutionX, $levelEditor::DesignResolutionY);
   ToolManager.setCameraVisibility($levelEditor::ShowCamera);
   ToolManager.setGuidesVisibility($levelEditor::ShowGuides);
                                       
   LevelEditorSelectionTool.setFullContainSelect($levelEditor::FullContainSelection);
   LevelEditorSelectionTool.setUndoSelections($levelEditor::UndoSelections);
   ToolManager.setSnapThreshold($levelEditor::SnapThreshold);
   ToolManager.setRotationSnap($levelEditor::rotationSnap);
   ToolManager.setRotationSnapAngle($levelEditor::rotationSnapAngle);
   ToolManager.setRotationSnapThreshold($levelEditor::rotationSnapThreshold);
   T2DDisplayErrorsToggle.setValue($levelEditor::DisplayScriptErrors);
}
