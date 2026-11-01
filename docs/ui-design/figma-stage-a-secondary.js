// Companion editable mockups: HUD, results, profile/loadout, and pause/settings.
const made=[];
const C={ink:'#071018',deep:'#0B1720',panel:'#10232D',panel2:'#142B36',line:'#2A4A53',cyan:'#57D9DB',cyanDark:'#178F9C',paper:'#E8F5F4',muted:'#9CB7BC',amber:'#F5BA61',red:'#F27872',green:'#78D8A8'};
function color(h){return{r:parseInt(h.slice(1,3),16)/255,g:parseInt(h.slice(3,5),16)/255,b:parseInt(h.slice(5,7),16)/255};}
function paint(h,a=1){return{type:'SOLID',color:color(h),opacity:a};}
function base(name,x,y,w,h,hex,parent){const f=figma.createFrame();made.push(f.id);f.name=name;(parent||figma.currentPage).appendChild(f);f.resize(w,h);f.x=x;f.y=y;f.fills=hex?[paint(hex)]:[];return f;}
function block(p,n,x,y,w,h,c,a=1,r=0,stroke=null){const b=base(n,x,y,w,h,null,p);b.fills=[paint(c,a)];b.cornerRadius=r;if(stroke){b.strokes=[paint(stroke)];b.strokeWeight=1;}return b;}
function label(p,n,s,x,y,w,h,size,c,weight='Regular',spacing=0){const t=figma.createText();made.push(t.id);t.name=n;p.appendChild(t);t.fontName={family:'Inter',style:weight};t.fontSize=size;t.resize(w,h);t.textAutoResize='NONE';t.characters=s;t.fills=[paint(c)];if(spacing)t.letterSpacing={unit:'PIXELS',value:spacing};t.x=x;t.y=y;return t;}
function rule(p,n,x,y,w,c=C.line,a=1){return block(p,n,x,y,w,1,c,a);}
function button(p,n,s,x,y,w,h,kind='secondary'){const bg=kind==='primary'?C.cyan:C.panel2,fg=kind==='primary'?C.ink:C.paper;const b=block(p,n,x,y,w,h,bg,1,7,kind==='secondary'?C.line:null);label(b,'Label',s,18,Math.round((h-18)/2),w-40,24,16,fg,'Bold',1);return b;}
function glass(p,n,x,y,w,h){return block(p,n,x,y,w,h,C.panel,0.94,8,C.line);}
function backdrop(p){
  block(p,'Facility backdrop',0,0,1600,900,C.deep);
  for(let i=0;i<8;i++){block(p,'Wall rib '+i,130+i*191,0,7,900,C.line,0.19);rule(p,'Floor path '+i,150+i*145,750+i*12,920-i*95,C.cyan,0.10);}
  block(p,'Door silhouette',590,100,440,655,C.ink,1,8,C.line);
  block(p,'Door glow',620,126,380,590,C.cyanDark,0.10,3);
  block(p,'Door seam',809,126,2,590,C.cyan,0.22);
  block(p,'Floor',0,755,1600,145,C.panel,0.45);
}
await Promise.all(['Regular','Medium','Semi Bold','Bold','Black'].map(style=>figma.loadFontAsync({family:'Inter',style})));

// Combat HUD: open sightline, separate critical zone event and compact tracker.
const hud=base('03 / Combat HUD — danger zone active',3800,120,1600,900,C.ink);hud.clipsContent=true;backdrop(hud);
const mission=glass(hud,'Objective stack',42,34,425,189);
label(mission,'Kicker','OPERATION 01 / CONTAINMENT BREACH',18,18,390,21,12,C.cyan,'Bold',1.2);
rule(mission,'rule',18,49,389);
label(mission,'Task 1','✓  CLEAR ZONE A',19,66,380,25,17,C.green,'Bold');
label(mission,'Task 2','◉  CLEAR ZONE B',19,105,380,25,17,C.paper,'Bold');
label(mission,'Task 3','○  RECOVER SAMPLE',19,145,380,25,17,C.muted,'Semi Bold');
const squad=glass(hud,'Squad health',1275,33,283,178);
label(squad,'Kicker','SQUAD / 02',17,16,250,22,12,C.cyan,'Bold',1.6);
label(squad,'Player 1','YOU  /  100 HP',17,52,245,24,16,C.paper,'Bold');
block(squad,'You meter',17,81,247,7,C.green,1,4);
label(squad,'Player 2','OPERATOR 02  /  72 HP',17,106,245,22,14,C.paper,'Semi Bold');
block(squad,'Player 2 meter base',17,136,247,7,C.line,1,4);
block(squad,'Player 2 meter',17,136,178,7,C.amber,1,4);
const warning=glass(hud,'Danger zone warning',550,38,500,95);
block(warning,'Warning band',0,0,6,95,C.amber);
label(warning,'Warning title','DANGER ZONE  /  CONTAINMENT',22,15,465,25,18,C.amber,'Bold',1);
label(warning,'Warning detail','HOSTILES INBOUND  •  WAVE 02 / 02',22,51,465,23,16,C.paper,'Semi Bold',0.8);
block(hud,'Crosshair H',785,448,30,2,C.paper,0.95);
block(hud,'Crosshair V',799,434,2,30,C.paper,0.95);
block(hud,'Reticle center',798,447,4,4,C.cyan,1,2);
const zone=glass(hud,'Wave tracker',1110,270,448,100);
label(zone,'Tracker title','ZONE B  /  WAVE 02',18,15,360,22,14,C.amber,'Bold',1);
label(zone,'Tracker count','04',17,38,90,49,40,C.paper,'Black');
label(zone,'Tracker caption','ENEMIES REMAINING',107,54,315,27,16,C.muted,'Semi Bold',1);
const hp=glass(hud,'Health widget',42,781,366,85);
label(hp,'Health label','HEALTH',18,14,175,20,12,C.muted,'Bold',1.3);
label(hp,'Health value','100 / 100',18,38,175,35,28,C.paper,'Black');
block(hp,'Health meter base',201,51,145,8,C.line,1,3);
block(hp,'Health meter',201,51,145,8,C.green,1,3);
const ammo=glass(hud,'Ammo widget',1252,777,306,89);
label(ammo,'Ammo label','ASSAULT RIFLE',17,12,265,20,12,C.muted,'Bold',1.1);
label(ammo,'Ammo value','24  /  90',17,34,270,43,32,C.paper,'Black');
label(hud,'Match microstatus','EXTRACTION LOCKED  •  COMPLETE OBJECTIVES',579,836,500,23,13,C.muted,'Semi Bold',0.8);

// Results: rewarding but immediately actionable, uses actual M6/M9 fields conceptually.
const results=base('04 / Mission Results — success',5600,120,1600,900,C.ink);results.clipsContent=true;
block(results,'Top accent',0,0,1600,6,C.green);
label(results,'Eyebrow','OPERATION 01   /   AFTER ACTION REPORT',58,44,850,25,14,C.cyan,'Bold',2);
label(results,'Heading','MISSION COMPLETE',58,94,980,75,58,C.paper,'Black',2);
label(results,'Outcome','The squad extracted with the research sample.',59,174,850,32,22,C.muted);
const rank=glass(results,'Rank card',1241,45,300,191);
label(rank,'Rank kicker','MISSION RANK',23,18,250,23,13,C.muted,'Bold',1.5);
label(rank,'Rank','A',93,41,150,121,100,C.amber,'Black');
rule(results,'Divider',58,249,1484);
const metrics=[['TEAM SCORE','3,980',C.paper],['COMPLETION','04:32',C.paper],['TEAM KILLS','12',C.paper],['ACCURACY','68.4%',C.paper]];
for(let i=0;i<4;i++){const x=58+i*377;const m=glass(results,'Metric '+i,x,275,350,116);label(m,'Title',metrics[i][0],20,18,305,21,13,C.muted,'Bold',1.5);label(m,'Value',metrics[i][1],20,44,315,55,42,metrics[i][2],'Black');}
const board=glass(results,'Leaderboard',58,420,916,294);
label(board,'Board heading','SQUAD PERFORMANCE',22,17,830,25,17,C.cyan,'Bold',1.3);
rule(board,'rule',22,53,872);
label(board,'Column labels','PLAYER                                   KILLS       HITS       ACCURACY       SCORE',23,67,850,22,12,C.muted,'Bold',0.5);
rule(board,'table rule',22,96,872);
label(board,'Player row 1','01   OPERATOR 01  /  YOU             7             35          70.0%           2,145',23,112,850,36,17,C.paper,'Semi Bold');
rule(board,'row rule',22,160,872);
label(board,'Player row 2','02   OPERATOR 02                       5             29          66.0%           1,835',23,180,850,36,17,C.paper,'Semi Bold');
rule(board,'row rule 2',22,231,872);
label(board,'Score note','OBJECTIVES +1,500    /    TIME +340    /    SURVIVAL +500',23,248,850,22,13,C.muted,'Semi Bold');
const xp=glass(results,'Personal progression',999,420,543,294);
label(xp,'XP heading','YOUR PROGRESSION',22,17,490,25,17,C.cyan,'Bold',1.2);
label(xp,'XP award','+340 XP',22,57,495,59,48,C.paper,'Black');
label(xp,'Level','LEVEL 02',22,131,450,23,17,C.paper,'Bold');
block(xp,'XP base',22,171,495,12,C.line,1,5);
block(xp,'XP progress',22,171,309,12,C.cyan,1,5);
label(xp,'Progress caption','620 / 1,000 XP  •  TEAL FINISH UNLOCKED',22,203,497,37,14,C.green,'Semi Bold');
rule(results,'Footer rule',58,753,1484);
label(results,'Action note','All squad members remain connected while the host chooses the next action.',58,783,820,38,14,C.muted,'Medium');
button(results,'Return lobby','RETURN TO LOBBY',916,779,289,60);
button(results,'Replay','REPLAY MISSION',1223,779,319,60,'primary');

// Profile and loadout: existing single rifle and two earned cosmetic choices.
const profile=base('05 / Profile and Loadout',7400,120,1600,900,C.ink);profile.clipsContent=true;
block(profile,'Top bar',0,0,1600,94,C.deep);
label(profile,'Brand','ARCADE / SHOOTER',50,25,570,47,31,C.paper,'Black',2);
label(profile,'Section','PROFILE   /   LOADOUT',1120,33,420,25,16,C.cyan,'Bold',1.8);
label(profile,'Heading','Operator profile',55,135,780,60,49,C.paper,'Bold');
label(profile,'Back','‹  BACK TO LOBBY',55,210,520,27,15,C.muted,'Semi Bold',1);
const identity=glass(profile,'Operator identity',55,279,395,545);
block(identity,'Avatar field',29,29,335,185,C.deep,1,8,C.line);
block(identity,'Avatar circle',139,67,118,118,C.cyanDark,0.7,59);
label(identity,'Avatar initials','01',171,102,77,57,42,C.paper,'Black');
label(identity,'Callsign','OPERATOR 01',28,244,330,45,27,C.paper,'Black');
label(identity,'Title','PATHFINDER  /  EQUIPPED',29,292,330,25,14,C.cyan,'Bold',1);
rule(identity,'Divider',28,334,337);
label(identity,'Level','LEVEL 03',29,351,320,33,24,C.paper,'Bold');
block(identity,'XP base',29,399,335,12,C.line,1,5);
block(identity,'XP fill',29,399,183,12,C.cyan,1,5);
label(identity,'XP','570 / 1,200 XP TO NEXT LEVEL',29,421,335,22,13,C.muted,'Medium');
label(identity,'Lifetime','MISSIONS  18     EXTRACTIONS  12',29,483,345,22,13,C.muted,'Semi Bold');
const weapon=glass(profile,'Weapon preview',474,279,646,545);
label(weapon,'Kicker','ASSAULT RIFLE  /  PRIMARY',25,22,580,25,14,C.cyan,'Bold',1.5);
block(weapon,'Preview stage',25,66,596,284,C.deep,1,7,C.line);
rule(weapon,'Stage guide 1',44,111,560,C.cyan,0.1);rule(weapon,'Stage guide 2',44,208,560,C.cyan,0.1);
block(weapon,'Rifle stock',93,188,94,16,C.muted,0.8,2);
block(weapon,'Rifle receiver',184,169,245,43,C.paper,0.82,3);
block(weapon,'Rifle barrel',427,179,131,15,C.paper,0.8,2);
block(weapon,'Rifle sight',317,156,80,11,C.cyan,0.9,2);
block(weapon,'Rifle grip',335,210,31,83,C.muted,0.9,3);
block(weapon,'Rifle magazine',250,210,39,89,C.muted,0.9,3);
label(weapon,'Preview caption','DRAG TO ROTATE  /  PROJECT MODEL PREVIEW',42,317,558,22,11,C.muted,'Semi Bold',1.1);
label(weapon,'Stats title','WEAPON PROFILE',26,375,540,28,17,C.paper,'Bold',1.1);
label(weapon,'Stats','DAMAGE   20         RATE   8 / SEC         MAGAZINE   30',26,415,586,26,15,C.muted,'Semi Bold');
block(weapon,'Damage bar',26,470,330,8,C.cyan,1,4);
block(weapon,'Rate bar',380,470,204,8,C.amber,1,4);
const cosmetics=glass(profile,'Cosmetics',1144,279,399,545);
label(cosmetics,'Kicker','FINISHES  /  EQUIPMENT',23,22,360,24,14,C.cyan,'Bold',1.1);
rule(cosmetics,'Divider',23,58,353);
const standard=block(cosmetics,'Standard finish',22,80,354,112,C.panel2,1,6,C.line);
label(standard,'Name','STANDARD',18,16,320,30,21,C.paper,'Bold');
label(standard,'Caption','DEFAULT FINISH  •  AVAILABLE',18,53,320,23,13,C.muted,'Semi Bold');
const teal=block(cosmetics,'Teal finish',22,210,354,112,C.panel2,1,6,C.cyanDark);
label(teal,'Name','TEAL',18,16,320,30,21,C.paper,'Bold');
label(teal,'Caption','LEVEL 2 REWARD  •  EQUIPPED',18,53,320,23,13,C.green,'Semi Bold');
button(cosmetics,'Equip button','EQUIPPED',22,345,354,54);
label(cosmetics,'Unlock hint','Only unlocked finishes can be equipped.\nSelection is saved on this computer.',22,429,348,71,14,C.muted);

// Pause/settings: online play stays active; separate visual confirmation for leaving.
const pause=base('06 / Pause and Settings',9200,120,1600,900,C.ink);pause.clipsContent=true;backdrop(pause);
block(pause,'Dim',0,0,1600,900,C.ink,0.72);
const pausePanel=glass(pause,'Pause panel',325,91,950,718);
label(pausePanel,'Kicker','OPERATIONS MENU',38,29,830,22,14,C.cyan,'Bold',2);
label(pausePanel,'Heading','Paused',38,68,570,71,53,C.paper,'Black');
label(pausePanel,'Online note','Your session continues while this menu is open.',41,147,835,30,18,C.amber,'Medium');
rule(pausePanel,'Divider',39,192,872);
button(pausePanel,'Resume','RESUME MISSION',39,222,350,62,'primary');
button(pausePanel,'Leave','LEAVE MATCH',39,300,350,62);
label(pausePanel,'Leave hint','Leaving an active match asks for confirmation.',41,371,360,54,13,C.muted);
rule(pausePanel,'Vertical divider',418,224,1,C.line);
label(pausePanel,'Settings heading','SETTINGS',451,223,442,34,25,C.paper,'Bold',1.5);
label(pausePanel,'Audio heading','AUDIO',451,279,440,23,13,C.cyan,'Bold',1.2);
const settings=[['MASTER VOLUME',0.83],['MUSIC VOLUME',0.58],['SFX VOLUME',0.77],['MOUSE SENSITIVITY',0.43]];
for(let i=0;i<settings.length;i++){const y=316+i*71;label(pausePanel,'Setting '+i,settings[i][0],451,y,385,22,14,C.paper,'Semi Bold');block(pausePanel,'Track '+i,451,y+30,381,8,C.line,1,4);block(pausePanel,'Fill '+i,451,y+30,Math.round(381*settings[i][1]),8,C.cyan,1,4);}
label(pausePanel,'Video heading','VIDEO',451,615,425,23,13,C.cyan,'Bold',1.2);
label(pausePanel,'Video setting','FULLSCREEN   /   2560 × 1440',451,649,446,25,16,C.paper,'Semi Bold');
label(pausePanel,'Footer','ESC  TO RESUME                                  SETTINGS SAVED LOCALLY',39,676,870,22,13,C.muted,'Semi Bold',1);
return {createdNodeIds:made,nodeCount:made.length,frames:[hud,results,profile,pause].map(f=>({id:f.id,name:f.name,x:f.x,y:f.y,w:f.width,h:f.height}))};
