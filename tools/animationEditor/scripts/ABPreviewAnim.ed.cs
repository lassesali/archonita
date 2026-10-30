function ABPreviewAnim::onFrameChange(%this, %frame)
{
   %slider = AnimationBuilderPreviewSlider;

   %slider.setValue(%frame);   
}

function ABPreviewAnim::onAnimationEnd(%this)
{
   %slider = AnimationBuilderPreviewSlider;
   
   %frame = %this.getAnimationFrame();
   
   %frame--;

   //%slider.setValue(%frame);   
}