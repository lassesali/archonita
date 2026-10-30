function ABFont::buildText(%this, %text, %size, %boldFirst, %boldFrames)
{ 
   %this.textCount = 0;
   
   %charCount = strLen(%text);

   %width = getWord(%size, 0);
   %height = getWord(%size, 1);
   
   %pos = %this.getPosition();
   %posX = getWord(%pos, 0);
   %posY = getWord(%pos, 1);
   
   %startX = %posX - (%width / 2);

   %spacing = -0.4;
   
   %charWidth = (%width / %charCount) - %spacing;

   %totalSpacer = 0;

   for(%i=0;%i<%charCount;%i++)
   {
      %char = getSubStr(%text, %i, 1);
      
      %frame = %this.returnFrame(%char);
      %spacer = %this.returnSpacer(%char);
		    
		    if(%i + 1 < %charCount)
		    {
		       %nextChar = getSubStr(%text, %i + 1, 1);
		       %nextSpacer = %spacer;
		    } else
		    {
		       %nextSpacer = 0; 
		    } 
		    
		    if(%i <= %boldFrames && %boldFirst)
		    {
         %textWidth = %charWidth * 1.3;
         %textHeight = %height * 1.3;
         %color = "0 0 0 1";
		    } else
		    {
         %textWidth = %charWidth;
         %textHeight = %height;
         %color = "1 0 0 1";
		    }
		    
      %textSprite = new t2dStaticSprite()
      {
         scenegraph = %this.scenegraph;  		      
         imageMap = ABFontImageMap;  
         frame = %frame;
         size = %textWidth SPC %textHeight;
         blendColor = %color;
         layer = $AnimationBuilder::layers::scrollContent;
      };

      if(%i > 0)
         %newX = (%startX + (%i * (%charWidth + %spacing))) - (%totalSpacer + %nextSpacer);
      else
         %newX = %startX;
   
      %totalSpacer += %spacer;
      %totalSpacer += %nextSpacer;
   
      //%textSprite.setDebugOn(1, 5);
      %textSprite.mount(%this, %this.getLocalPoint(%newX, %posY));
		   
      %this.textArray[%i] = %textSprite;
      %this.textCount++;
      
      $AnimationBuilder::StoryboardGroup.add(%textSprite);
   }	      
}

function ABFont::clearText(%this)
{
   for(%i=0;%i<%this.textCount;%i++)
   {
      %this.textArray[%i].safeDelete();      
   }

   %this.safeDelete();
}

function ABFont::returnFrame(%this, %char)
{
   return getWord($ABFontArray[%char], 0);   
}

function ABFont::returnSpacer(%this, %char)
{
   %spacer = getWord($ABFontArray[%char], 1);
   
   if(%spacer $= "")
      %spacer = 0;
   
   return %spacer;   
}

$ABFontArray[" "] = "0 0.15";
$ABFontArray["!"] = "1 0.15";
$ABFontArray["QUOTE"] = 2;
$ABFontArray["#"] = 3;
$ABFontArray["$"] = 4;
$ABFontArray["%"] = 5;
$ABFontArray["&"] = 6;
$ABFontArray["'"] = "7 0.15";
$ABFontArray["("] = "8 0.15";
$ABFontArray[")"] = "9 0.15";
$ABFontArray["*"] = "10 0.15";
$ABFontArray["+"] = "11 0.15";
$ABFontArray[","] = "12 0.15";
$ABFontArray["-"] = "13 0.15";
$ABFontArray["."] = "14 0.15";
$ABFontArray["SLASH"] = 15;
$ABFontArray["0"] = 16;
$ABFontArray["1"] = 17;
$ABFontArray["2"] = 18;
$ABFontArray["3"] = 19;
$ABFontArray["4"] = 20;
$ABFontArray["5"] = 21;
$ABFontArray["6"] = 22;
$ABFontArray["7"] = 23;
$ABFontArray["8"] = 24;
$ABFontArray["9"] = 25;
$ABFontArray[":"] = "26 0.15";
$ABFontArray[";"] = 27;
$ABFontArray["<"] = 28;
$ABFontArray["="] = 29;
$ABFontArray[">"] = 30;
$ABFontArray["?"] = 31;
$ABFontArray["@"] = 32;
$ABFontArray["A"] = 33;
$ABFontArray["B"] = 34;
$ABFontArray["C"] = 35;
$ABFontArray["D"] = 36;
$ABFontArray["E"] = 37;
$ABFontArray["F"] = 38;
$ABFontArray["G"] = 39;
$ABFontArray["H"] = 40;
$ABFontArray["I"] = 41;
$ABFontArray["J"] = 42;
$ABFontArray["K"] = 43;
$ABFontArray["L"] = 44;
$ABFontArray["M"] = 45;
$ABFontArray["N"] = 46;
$ABFontArray["O"] = 47;
$ABFontArray["P"] = 48;
$ABFontArray["Q"] = 49;
$ABFontArray["R"] = 50;
$ABFontArray["S"] = 51;
$ABFontArray["T"] = 52;
$ABFontArray["U"] = 53;
$ABFontArray["V"] = 54;
$ABFontArray["W"] = 55;
$ABFontArray["X"] = 56;
$ABFontArray["Y"] = 57;
$ABFontArray["Z"] = 58;
$ABFontArray["["] = "59 0.15";
$ABFontArray["BACKSLASH"] = 60;
$ABFontArray["]"] = "61 0.15";
$ABFontArray["^"] = "62 0.15";
$ABFontArray["_"] = 63;
$ABFontArray["`"] = "64 0.15";
$ABFontArray["a"] = 65;
$ABFontArray["b"] = 66;
$ABFontArray["c"] = 67;
$ABFontArray["d"] = 68;
$ABFontArray["e"] = 69;
$ABFontArray["f"] = "70 0.17";
$ABFontArray["g"] = 71;
$ABFontArray["h"] = 72;
$ABFontArray["i"] = "73 0.15";
$ABFontArray["j"] = 74;
$ABFontArray["k"] = 75;
$ABFontArray["l"] = "76 0.15";
$ABFontArray["m"] = 77;
$ABFontArray["n"] = 78;
$ABFontArray["o"] = 79;
$ABFontArray["p"] = 80;
$ABFontArray["q"] = 81;
$ABFontArray["r"] = "82 0.17";
$ABFontArray["s"] = 83;
$ABFontArray["t"] = 84;
$ABFontArray["u"] = 85;
$ABFontArray["v"] = 86;
$ABFontArray["w"] = 87;
$ABFontArray["x"] = 88;
$ABFontArray["y"] = 89;
$ABFontArray["z"] = 90;