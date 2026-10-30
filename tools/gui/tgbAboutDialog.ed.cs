function TGBAboutFrame::onWake( %this ) 
{
   TGBVersion.setText( getT2DVersion() );
   TGBLBVersion.setText( $LevelBuilder::Version );
}
