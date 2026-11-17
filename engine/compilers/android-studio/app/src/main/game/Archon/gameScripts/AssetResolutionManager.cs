//-----------------------------------------------------------------------------
// Archonita - Asset Resolution Manager
// Torque2D 4.0 Asset Dynamic Cell Scaling
//-----------------------------------------------------------------------------

function AssetResolutionManager::onAdd(%this)
{
   %this.normalizeAssets();
}

function AssetResolutionManager::onRemove(%this)
{
   // Cleanup if needed
}

function AssetResolutionManager::normalizeAssets(%this)
{
   %query = new AssetQuery();
   AssetDatabase.findAssetType(%query, "ImageAsset");
   %count = %query.getCount();
   
   for (%i = 0; %i < %count; %i++)
   {
      %assetId = %query.getAsset(%i);
      if (getSubStr(%assetId, 0, 7) $= "Archon:")
      {
         %this.normalizeImageAsset(%assetId);
      }
   }
   %query.delete();
}

function AssetResolutionManager::normalizeImageAsset(%this, %assetId)
{
   %asset = AssetDatabase.acquireAsset(%assetId);
   if (!isObject(%asset))
      return;

   %assetName = getUnit(%assetId, 1, ":");
   
   // Special case: texts.png has a 4px padding strip at the bottom (512x196 with 3 frames of 64px)
   if (%assetName $= "textsImageMap")
   {
      AssetDatabase.releaseAsset(%assetId);
      return;
   }

   %cx = %asset.getCellCountX();
   %cy = %asset.getCellCountY();
   
   // Only cell-sliced sprite sheets require dynamic cell dimension adjustment
   if (%cx > 0 && %cy > 0)
   {
      %imgW = %asset.getImageWidth();
      %imgH = %asset.getImageHeight();
      if (%imgW > 0 && %imgH > 0)
      {
         %expectedW = mFloor(%imgW / %cx);
         %expectedH = mFloor(%imgH / %cy);
         
         if (%asset.getCellWidth() != %expectedW || %asset.getCellHeight() != %expectedH)
         {
            echo("AssetResolutionManager: Adapting " @ %assetName @ 
                 " (" @ %imgW @ "x" @ %imgH @ ") cells from " @ 
                 %asset.getCellWidth() @ "x" @ %asset.getCellHeight() @ 
                 " to " @ %expectedW @ "x" @ %expectedH);
            %asset.setCellWidth(%expectedW);
            %asset.setCellHeight(%expectedH);
            %asset.refreshAsset();
         }
      }
   }
   AssetDatabase.releaseAsset(%assetId);
}
