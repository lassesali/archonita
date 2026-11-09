   // 1=ghostly touch, 2=disease
   
   /*
   new t2dSceneObjectDatablock(Nappula) {
      
      hp = 0;
      hpMAX = 0;
      attack = 0;
      attackMAX = 0;
      defence = 0;
      defenceMAX = 0;
      mana = 0;
      manaMAX = 0;
      lvl = 0;
      id = 0;
      spells = "";

   };
   
*/   
   
   
   new ScriptObject(Hero) {
      //Human:
      mana[0]=0;
      manaMAX[0]=0;
      spells[0]="";
      olentopisteet[0]=5;
      ammatti[0]=0;
      
      //AI:
      mana[1]=0;
      manaMAX[1]=0;
      spells[1]="";
      olentopisteet[1]=5;
      ammatti[1]=0;
  
   };
   
   
   
   new ScriptObject(UnitProperties) {

      //warrior: 
      lvl[0]=1; 
      hp[0]=1; 
      mana[0]=0;
      attack[0]=2; 
      defence[0]=3;
      spells[0]="";
      groupid[0]=0;
      missile[0]=0;
      
      //archer: 
      lvl[1]=2; 
      hp[1]=1; 
      mana[1]=0;
      attack[1]=2; 
      defence[1]=2;
      spells[1]="";
      groupid[1]=0;
      missile[1]=1;
      
      //cleric: 
      lvl[2]=3; 
      hp[2]=1; 
      mana[2]=1; 
      attack[2]=2; 
      defence[2]=4;
      spells[2]="";
      groupid[2]=0;
      missile[2]=0;
      
      //paladin:
      lvl[3]=4; 
      hp[3]=3; 
      mana[3]=1; 
      attack[3]=4; 
      defence[3]=5;
      spells[3]="";
      groupid[3]=0;
      missile[3]=0;

      
      //skeleton warrior: 
      lvl[4]=1; 
      hp[4]=1; 
      mana[4]=0;
      attack[4]=2; 
      defence[4]=3;
      spells[4]="";
      groupid[4]=1;
      missile[4]=0;
     
      //skeleton archer: 
      lvl[5]=2; 
      hp[5]=2; 
      mana[5]=0;
      attack[5]=1; 
      defence[5]=2;
      spells[5]="";
      groupid[5]=1;
      missile[5]=1;
             
      //dread zombie: 
      lvl[6]=3; 
      hp[6]=3; 
      mana[6]=1; 
      attack[6]=3; 
      defence[6]=3; 
      spells[6]="2"; 
      groupid[6]=1;
      missile[6]=0;

      // spell=disease (-1 attack)
      
      //ancient ghost: 
      lvl[7]=4; 
      hp[7]=3; 
      mana[7]=1; 
      attack[7]=3; 
      defence[7]=6; 
      spells[7]="1"; 
      groupid[7]=1;
      missile[7]=0;

      // spell=ghostly touch (-2 defence)
  

      // ==========================================
      // Dragonfolk Faction (groupid 1 for AI enemy)
      // ==========================================

      // dragonfolk (Lvl 1 Melee):
      lvl[8]=1;
      hp[8]=2;
      mana[8]=0;
      attack[8]=2;
      defence[8]=3;
      spells[8]="";
      groupid[8]=1;
      missile[8]=0;

      // reddragon (Lvl 2 Ranged / Fire Breath):
      lvl[9]=2;
      hp[9]=2;
      mana[9]=0;
      attack[9]=2;
      defence[9]=3;
      spells[9]="";
      groupid[9]=1;
      missile[9]=1;

      // airdragon (Lvl 3 Flying / Lightning Caster):
      lvl[10]=3;
      hp[10]=3;
      mana[10]=1;
      attack[10]=3;
      defence[10]=4;
      spells[10]="1";
      groupid[10]=1;
      missile[10]=0;

      // golddragon (Lvl 4 Majestic Sovereign Boss):
      lvl[11]=4;
      hp[11]=4;
      mana[11]=1;
      attack[11]=4;
      defence[11]=5;
      spells[11]="2";
      groupid[11]=1;
      missile[11]=0;

      // ==========================================
      // Gremlin Faction (groupid 1 for AI enemy)
      // ==========================================

      // gremlin (Lvl 1 Fast Melee):
      lvl[12]=1;
      hp[12]=1;
      mana[12]=0;
      attack[12]=2;
      defence[12]=2;
      spells[12]="";
      groupid[12]=1;
      missile[12]=0;

      // hornedoneye (Lvl 2 Eye-Beam Ranged):
      lvl[13]=2;
      hp[13]=2;
      mana[13]=0;
      attack[13]=2;
      defence[13]=2;
      spells[13]="";
      groupid[13]=1;
      missile[13]=1;

      // mistress (Lvl 3 Sorceress / Dark Caster):
      lvl[14]=3;
      hp[14]=2;
      mana[14]=1;
      attack[14]=3;
      defence[14]=4;
      spells[14]="1";
      groupid[14]=1;
      missile[14]=0;

      // demon (Lvl 4 Infernal Fiend Boss):
      lvl[15]=4;
      hp[15]=4;
      mana[15]=1;
      attack[15]=4;
      defence[15]=5;
      spells[15]="2";
      groupid[15]=1;
      missile[15]=0;

   };
