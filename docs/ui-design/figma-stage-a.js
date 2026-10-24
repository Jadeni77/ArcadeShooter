// Editable Stage A concept for the ArcadeShooter Figma design file.
// Run with Figma use_figma against file 0stgFPzLkSBN7UbWtdlohf.
const made = [];
const C = {
  ink: '#071018', deep: '#0B1720', panel: '#10232D', panel2: '#142B36',
  line: '#2A4A53', cyan: '#57D9DB', cyanDark: '#178F9C', paper: '#E8F5F4',
  muted: '#9CB7BC', amber: '#F5BA61', red: '#F27872', green: '#78D8A8'
};
function color(hex) { return {r:parseInt(hex.slice(1,3),16)/255,g:parseInt(hex.slice(3,5),16)/255,b:parseInt(hex.slice(5,7),16)/255}; }
function paint(hex, opacity=1) { return {type:'SOLID',color:color(hex),opacity}; }
function base(name,x,y,w,h,hex,parent) {
  const f=figma.createFrame(); made.push(f.id); f.name=name;
  (parent||figma.currentPage).appendChild(f); f.resize(w,h); f.x=x; f.y=y;
  f.fills=hex?[paint(hex)]:[]; return f;
}
function block(parent,name,x,y,w,h,hex,opacity=1,radius=0,stroke=null) {
  const b=base(name,x,y,w,h,null,parent); b.fills=[paint(hex,opacity)]; b.cornerRadius=radius;
  if(stroke){b.strokes=[paint(stroke)];b.strokeWeight=1;} return b;
}
function label(parent,name,s,x,y,w,h,size,hex,weight='Regular',spacing=0) {
  const t=figma.createText(); made.push(t.id); t.name=name; parent.appendChild(t);
  t.fontName={family:'Inter',style:weight}; t.fontSize=size; t.resize(w,h);
  t.textAutoResize='NONE'; t.characters=s; t.fills=[paint(hex)];
  if(spacing)t.letterSpacing={unit:'PIXELS',value:spacing}; t.x=x;t.y=y; return t;
}
function rule(parent,name,x,y,w,hex=C.line,opacity=1) { return block(parent,name,x,y,w,1,hex,opacity); }
function button(parent,name,title,x,y,w,h,kind='secondary') {
  const bg=kind==='primary'?C.cyan:kind==='danger'?C.red:C.panel2;
  const fg=kind==='primary'||kind==='danger'?C.ink:C.paper;
  const b=block(parent,name,x,y,w,h,bg,1,7,kind==='secondary'?C.line:null);
  label(b,'Label',title,22,Math.round((h-19)/2),w-60,25,17,fg,'Bold',1.2);
  label(b,'Chevron','›',w-36,Math.round((h-30)/2),20,32,28,fg,'Regular');
  return b;
}
await Promise.all([
  figma.loadFontAsync({family:'Inter',style:'Regular'}),
  figma.loadFontAsync({family:'Inter',style:'Medium'}),
  figma.loadFontAsync({family:'Inter',style:'Semi Bold'}),
  figma.loadFontAsync({family:'Inter',style:'Bold'}),
  figma.loadFontAsync({family:'Inter',style:'Black'})
]);
const page=figma.currentPage; page.name='Stage A / Visual direction';

// Main menu: purposeful facility geometry acts as a low-cost animated backdrop.
const menu=base('01 / Main Menu — 1600 × 900',200,120,1600,900,C.ink); menu.clipsContent=true;
block(menu,'Facility depth / rear wall',560,0,1040,900,C.deep);
for(let i=0;i<8;i++){
  block(menu,'Corridor rib '+(i+1),610+i*135,0,6,900,C.line,0.18);
  rule(menu,'Ceiling trace '+(i+1),600+i*120,124,70,C.cyan,0.12);
}
block(menu,'Horizon light',710,206,790,430,C.cyanDark,0.075,30);
block(menu,'Facility doorway',1005,110,365,670,C.ink,1,9,C.line);
block(menu,'Facility doorway inner',1050,150,275,590,C.deep,1,4,C.cyanDark);
block(menu,'Door center seam',1188,150,2,590,C.cyan,0.3);
block(menu,'Door warning stripe',1050,150,275,7,C.amber,0.8);
block(menu,'Floor plane',510,745,1090,155,C.panel,0.55);
for(let i=0;i<6;i++)rule(menu,'Floor guide '+(i+1),590+i*150,765+i*19,730-i*120,C.cyan,0.11);
block(menu,'Menu atmosphere',0,0,625,900,C.ink,0.93);
block(menu,'Menu edge',620,0,2,900,C.cyan,0.33);
label(menu,'Eyebrow','ARES DIVISION  /  COOPERATIVE OPERATIONS',72,65,470,20,13,C.cyan,'Bold',2.3);
label(menu,'Brand / first','ARCADE',68,121,500,82,74,C.paper,'Black',4);
label(menu,'Brand / second','SHOOTER',68,204,500,82,74,C.paper,'Black',4);
block(menu,'Brand accent',71,304,146,5,C.cyan);
label(menu,'Story marker','OPERATION 01  ·  CONTAINMENT BREACH',72,344,495,26,15,C.amber,'Semi Bold',1.5);
label(menu,'Story copy','Enter the facility. Hold the line.\nExtract as a team.',72,384,480,76,24,C.muted,'Regular');
button(menu,'Primary / Multiplayer','PLAY  /  MULTIPLAYER',72,510,475,70,'primary');
button(menu,'Secondary / Profile','PLAYER PROFILE',72,594,475,58);
button(menu,'Secondary / Settings','SETTINGS',72,665,475,58);
button(menu,'Secondary / Quit','QUIT GAME',72,736,475,58);
label(menu,'Location marker','SECTOR 03  //  RESEARCH FACILITY',1390,62,530,24,14,C.muted,'Semi Bold',1.5).rotation=90;
label(menu,'Footer','BUILD 0.10     •     PRIVATE SQUAD OPERATIONS     •     2–4 PLAYERS',72,849,800,25,12,C.muted,'Medium',1);
label(menu,'Scene caption','CONTAINMENT / 01',1022,682,350,36,27,C.paper,'Bold',2);
label(menu,'Scene subcaption','RESEARCH FACILITY',1024,718,350,22,13,C.cyan,'Semi Bold',2);

// Lobby: four slot squad board, a clear invite control, and one host-owned launch area.
const lobby=base('02 / Multiplayer Lobby — 1600 × 900',2000,120,1600,900,C.ink); lobby.clipsContent=true;
block(lobby,'Top bar',0,0,1600,104,C.deep);
rule(lobby,'Top divider',0,103,1600,C.line);
label(lobby,'Brand','ARCADE / SHOOTER',54,30,650,55,35,C.paper,'Black',2.4);
label(lobby,'Section','SQUAD LOBBY   /   OPERATION 01',997,41,550,25,16,C.cyan,'Bold',1.6);
label(lobby,'Page heading','Prepare for deployment',56,139,960,65,46,C.paper,'Bold');
label(lobby,'Status','PRIVATE SESSION  •  2 / 4 OPERATORS CONNECTED',58,201,850,23,14,C.muted,'Semi Bold',1.3);
// Invitation control.
const code=block(lobby,'Join code card',1115,142,428,101,C.panel,1,8,C.line);
label(code,'Card caption','JOIN CODE',20,13,220,24,12,C.muted,'Bold',1.6);
label(code,'Code','AX7R42',20,40,220,43,31,C.paper,'Black',3);
block(code,'Copy control',285,24,124,54,C.cyan,1,5);
label(code,'Copy label','COPY CODE',300,41,103,22,13,C.ink,'Bold',0.7);
// Roster.
const roster=block(lobby,'Squad panel',56,286,650,485,C.panel,1,10,C.line);
label(roster,'Panel kicker','01  /  SQUAD',25,24,300,25,14,C.cyan,'Bold',2);
label(roster,'Panel count','2 OF 4',520,24,105,25,14,C.muted,'Bold',1);
rule(roster,'Header rule',25,63,600);
const slots=[
  ['01','OPERATOR 01','HOST · YOU','READY',C.green],
  ['02','OPERATOR 02','SQUAD MEMBER','NOT READY',C.amber],
  ['03','AWAITING OPERATOR','OPEN SLOT','INVITE',C.muted],
  ['04','AWAITING OPERATOR','OPEN SLOT','INVITE',C.muted]
];
for(let i=0;i<4;i++){
  const y=82+i*94; const s=block(roster,'Roster slot '+(i+1),25,y,600,78,i<2?C.panel2:C.deep,1,6,i<2?C.line:null);
  block(s,'Index badge',13,15,48,48,i<2?C.cyanDark:C.panel,1,5);
  label(s,'Index',slots[i][0],22,25,35,27,19,i<2?C.paper:C.muted,'Bold');
  label(s,'Name',slots[i][1],77,14,323,29,18,i<2?C.paper:C.muted,'Bold');
  label(s,'Role',slots[i][2],77,44,330,20,12,C.muted,'Semi Bold',1.1);
  block(s,'State dot',463,35,8,8,slots[i][4],1,4);
  label(s,'State',slots[i][3],481,29,110,24,12,slots[i][4],'Bold',0.5);
}
// Mission briefing and loadout.
const briefing=block(lobby,'Mission briefing',730,286,813,319,C.panel,1,10,C.line);
label(briefing,'Panel kicker','02  /  MISSION BRIEFING',25,24,500,25,14,C.cyan,'Bold',2);
rule(briefing,'Header rule',25,63,763);
block(briefing,'Mission map silhouette',25,83,302,206,C.deep,1,6);
for(let i=0;i<6;i++)rule(briefing,'Map line '+(i+1),40,104+i*31,272,C.cyan,0.14);
block(briefing,'Map room one',66,126,76,88,C.panel2,1,2,C.line);
block(briefing,'Map room two',159,126,65,88,C.panel2,1,2,C.line);
block(briefing,'Map room three',241,126,53,88,C.panel2,1,2,C.line);
block(briefing,'Map route one',143,165,16,7,C.cyan,0.8);
block(briefing,'Map route two',224,165,17,7,C.cyan,0.8);
label(briefing,'Map label','SECTOR 03  /  RESEARCH FACILITY',43,260,275,19,11,C.muted,'Bold',0.8);
label(briefing,'Mission title','CONTAINMENT BREACH',352,88,425,35,27,C.paper,'Black',1);
label(briefing,'Mission summary','Clear both danger zones. Recover the\nresearch sample. Extract together.',352,137,430,70,17,C.muted,'Regular');
label(briefing,'Difficulty caption','DIFFICULTY',352,220,200,20,12,C.muted,'Bold',1.3);
block(briefing,'Normal difficulty',352,247,131,40,C.cyan,1,4);
label(briefing,'Normal label','NORMAL',371,257,115,23,14,C.ink,'Bold');
block(briefing,'Hard difficulty',496,247,114,40,C.deep,1,4,C.line);
label(briefing,'Hard label','HARD',522,257,80,23,14,C.paper,'Bold');
label(briefing,'Host control','HOST CONTROL',650,256,130,21,11,C.amber,'Bold',1);
const loadout=block(lobby,'Loadout card',730,624,813,147,C.panel,1,10,C.line);
label(loadout,'Panel kicker','03  /  LOADOUT',25,17,300,22,14,C.cyan,'Bold',1.5);
label(loadout,'Weapon','ASSAULT RIFLE',25,51,315,30,23,C.paper,'Bold');
label(loadout,'Finish','STANDARD FINISH  •  LEVEL 1',25,88,370,22,13,C.muted,'Semi Bold',0.8);
block(loadout,'Rifle silhouette / stock',510,77,68,10,C.muted,0.72,2);
block(loadout,'Rifle silhouette / body',575,61,151,24,C.muted,0.8,2);
block(loadout,'Rifle silhouette / barrel',719,67,66,9,C.muted,0.8,2);
block(loadout,'Rifle silhouette / magazine',618,84,21,40,C.muted,0.72,2);
// Footer actions.
rule(lobby,'Footer divider',0,805,1600,C.line);
button(lobby,'Leave lobby','LEAVE LOBBY',56,825,265,54);
label(lobby,'Launch note','Only the host can launch. Waiting for all connected players to be ready.',750,843,510,42,13,C.muted,'Medium');
button(lobby,'Start mission','START MISSION',1280,825,263,54,'primary');
return {createdNodeIds:made,menuId:menu.id,lobbyId:lobby.id,nodeCount:made.length,
  frames:[{name:menu.name,x:menu.x,y:menu.y,w:menu.width,h:menu.height},
          {name:lobby.name,x:lobby.x,y:lobby.y,w:lobby.width,h:lobby.height}]};
