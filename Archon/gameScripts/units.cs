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
  
   };
  
  