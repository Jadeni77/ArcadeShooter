// Stage A cinematic revision. Run in the Figma Plugin API through use_figma.
// The four full-bleed image target frames are populated with original concept art
// using upload_assets after this script returns their IDs.
const created=[];
const P={white:'#F1F5F1',soft:'#BFC8C8',dim:'#849294',ink:'#071116',cyan:'#78D9DF',amber:'#E5AF54',red:'#EA6D66',green:'#A4D69B',line:'#667F83'};
function col(h){return{r:parseInt(h.slice(1,3),16)/255,g:parseInt(h.slice(3,5),16)/255,b:parseInt(h.slice(5,7),16)/255};}
function fill(h,a=1){return{type:'SOLID',color:col(h),opacity:a};}
function frame(p,n,x,y,w,h,c=null,a=1){const q=figma.createFrame();created.push(q.id);q.name=n;(p||figma.currentPage).appendChild(q);q.resize(w,h);q.x=x;q.y=y;q.fills=c?[fill(c,a)]:[];return q;}
function box(p,n,x,y,w,h,c,a=1){return frame(p,n,x,y,w,h,c,a);}
function line(p,n,x,y,w,h,c=P.line,a=1){return box(p,n,x,y,w,h,c,a);}
function txt(p,n,s,x,y,w,h,z,c=P.white,style='Medium',family='Barlow Condensed',space=0){const t=figma.createText();created.push(t.id);t.name=n;p.appendChild(t);t.fontName={family,style};t.fontSize=z;t.resize(w,h);t.textAutoResize='NONE';t.characters=s;t.fills=[fill(c)];if(space)t.letterSpacing={unit:'PIXELS',value:space};t.x=x;t.y=y;return t;}
function img(p,n){return frame(p,n,0,0,1600,900,P.ink);}
function border(p,n,x,y,w,h,c=P.cyan,a=0.6){line(p,n+' top',x,y,w,2,c,a);line(p,n+' bottom',x,y+h-2,w,2,c,a);line(p,n+' left',x,y,2,h,c,a);line(p,n+' right',x+w-2,y,2,h,c,a);}
await Promise.all([
 ['Barlow Condensed','Black'],['Barlow Condensed','Bold'],['Barlow Condensed','SemiBold'],
 ['Barlow Condensed','Medium'],['Barlow Condensed','Regular'],['Inter','Regular'],['Inter','Medium']
].map(([family,style])=>figma.loadFontAsync({family,style})));
const old=figma.currentPage;old.name='Archive / first visual pass';
const page=figma.createPage();page.name='Stage A / Cinematic revision';
await figma.setCurrentPageAsync(page);

// 01: Menu. Architecture is the hero; the interface is a thin command layer.
const menu=frame(null,'01 / Main Menu — cinematic',120,120,1600,900,P.ink);menu.clipsContent=true;
const menuImage=img(menu,'IMAGE FILL / original facility concept');
box(menu,'Legibility veil / left',0,0,560,900,P.ink,0.38);
box(menu,'Legibility veil / footer',0,810,1600,90,P.ink,0.72);
line(menu,'Top rule',0,76,1600,1,P.white,0.22);
line(menu,'Left accent',54,118,3,711,P.amber,0.82);
txt(menu,'Brand mark','ARCADE  /  SHOOTER',78,25,680,43,34,P.white,'Black','Barlow Condensed',2.1);
txt(menu,'Top right status','OPERATOR STATUS  •  ONLINE',1252,35,320,28,17,P.soft,'SemiBold','Barlow Condensed',1.5);
txt(menu,'Series label','RESEARCH FACILITY  //  INCIDENT 01',79,142,655,24,18,P.cyan,'Bold','Barlow Condensed',2.3);
txt(menu,'Main title line 1','CONTAINMENT',77,180,670,95,78,P.white,'Black','Barlow Condensed',2.2);
txt(menu,'Main title line 2','BREACH',77,265,590,95,78,P.white,'Black','Barlow Condensed',2.2);
line(menu,'Title underline',80,370,259,5,P.amber);
txt(menu,'Narrative','The facility is lost. The squad is not.',79,391,585,40,22,P.soft,'Regular','Inter');
txt(menu,'Menu kicker','OPERATIONS  /  01',80,464,480,24,16,P.amber,'Bold','Barlow Condensed',2);
const menuItems=[['01','PLAY  /  MULTIPLAYER'],['02','PLAYER PROFILE'],['03','SETTINGS'],['04','QUIT GAME']];
for(let i=0;i<menuItems.length;i++){
  let y=502+i*75;
  if(i===0){box(menu,'Selected action field',72,y-5,474,66,P.ink,0.64);line(menu,'Selected action bar',72,y-5,5,66,P.amber);}
  txt(menu,'Action index '+i,menuItems[i][0],91,y+7,57,30,18,i===0?P.amber:P.dim,'Bold','Barlow Condensed',1.3);
  txt(menu,'Action label '+i,menuItems[i][1],151,y,370,47,32,i===0?P.white:P.soft,i===0?'Bold':'SemiBold','Barlow Condensed',1.6);
  if(i===0)txt(menu,'Action arrow','›',505,y-3,40,42,35,P.amber,'Regular','Barlow Condensed');
  else line(menu,'Action divider '+i,88,y+56,443,1,P.white,0.17);
}
const dossier=box(menu,'Operation dossier veil',1071,596,457,211,P.ink,0.75);
line(dossier,'Dossier top accent',0,0,457,3,P.amber);
txt(dossier,'Dossier eyebrow','ACTIVE OPERATION  /  01',23,17,420,23,16,P.amber,'Bold','Barlow Condensed',1.6);
txt(dossier,'Dossier title','CONTAINMENT BREACH',23,48,422,43,32,P.white,'Bold','Barlow Condensed',1.1);
txt(dossier,'Dossier copy','Clear the danger zones. Recover the sample.\nExtract as a team.',23,100,420,62,17,P.soft,'Regular','Inter');
line(dossier,'Dossier rule',23,174,413,1,P.white,0.23);
txt(dossier,'Dossier meta','CO-OP 2–4    •    SECTOR 03',23,181,415,20,14,P.cyan,'SemiBold','Barlow Condensed',1.1);
txt(menu,'Footer left','ESC  BACK     /     ENTER  SELECT',79,849,530,29,15,P.soft,'SemiBold','Barlow Condensed',1.5);
txt(menu,'Footer right','BUILD 0.10    •    MACOS',1330,849,250,29,15,P.soft,'SemiBold','Barlow Condensed',1.5);

// 02: Lobby. Keep the operators visible; each lane identifies one squad member.
const lobby=frame(null,'02 / Multiplayer Lobby — fireteam',1920,120,1600,900,P.ink);lobby.clipsContent=true;
const lobbyImage=img(lobby,'IMAGE FILL / original fireteam concept');
box(lobby,'Top readability rail',0,0,1600,119,P.ink,0.80);
box(lobby,'Lower roster rail',0,664,1600,236,P.ink,0.86);
line(lobby,'Top line',0,118,1600,1,P.white,0.25);
line(lobby,'Roster top line',0,664,1600,2,P.amber,0.65);
txt(lobby,'Section label','FIRETEAM  /  HANGAR 03',48,22,600,44,36,P.white,'Black','Barlow Condensed',2);
txt(lobby,'Mission title','OPERATION 01    CONTAINMENT BREACH',49,75,680,27,18,P.amber,'Bold','Barlow Condensed',1.3);
txt(lobby,'Player count','04 / 04 OPERATORS',725,35,370,31,22,P.soft,'Bold','Barlow Condensed',1.7);
const code=box(lobby,'Invite code rail',1192,17,362,81,P.ink,0.54);
line(code,'Invite border',0,0,4,81,P.cyan);
txt(code,'Code eyebrow','PRIVATE SESSION  /  JOIN CODE',18,11,325,20,14,P.soft,'Bold','Barlow Condensed',1);
txt(code,'Code value','AX7R42',18,33,195,38,28,P.white,'Black','Barlow Condensed',2);
txt(code,'Copy interaction','COPY  ↗',237,40,107,29,16,P.cyan,'Bold','Barlow Condensed',1);
for(let i=1;i<4;i++)line(lobby,'Squad lane '+i,400*i,130,1,534,P.white,0.17);
const operators=[
 ['01','ECHO','HOST  /  LEVEL 03','PATHFINDER','READY',P.green],
 ['02','ROOK','LEVEL 02','STANDARD','READY',P.green],
 ['03','VEX','LEVEL 01','STANDARD','READY',P.green],
 ['04','ASH','LEVEL 02','TEAL FINISH','READY',P.green]
];
for(let i=0;i<4;i++){
  const x=i*400;
  txt(lobby,'Portrait number '+i,operators[i][0],x+30,607,160,46,31,P.white,'Black','Barlow Condensed',1);
  line(lobby,'Portrait registration '+i,x+31,645,235,2,P.cyan,0.68);
  txt(lobby,'Operator name '+i,operators[i][1],x+31,682,315,52,41,P.white,'Black','Barlow Condensed',1.7);
  txt(lobby,'Operator title '+i,operators[i][2],x+32,736,338,22,15,P.soft,'SemiBold','Barlow Condensed',1);
  txt(lobby,'Cosmetic '+i,operators[i][3],x+32,763,338,22,14,P.cyan,'SemiBold','Barlow Condensed',1);
  for(let j=0;j<3;j++){const q=box(lobby,'Equipment slot '+i+'-'+j,x+32+j*48,779,39,31,P.white,0.14);border(q,'Slot border',0,0,39,31,P.white,0.24);}
  line(lobby,'Readiness bar '+i,x+30,818,340,2,operators[i][5],0.88);
  txt(lobby,'Readiness '+i,operators[i][4],x+31,824,336,24,17,operators[i][5],'Bold','Barlow Condensed',1.6);
}
box(lobby,'Launch action',1287,852,266,39,P.amber,0.94);
txt(lobby,'Launch label','START MISSION  ›',1307,855,233,30,21,P.ink,'Bold','Barlow Condensed',1.1);
txt(lobby,'Leave label','ESC  LEAVE LOBBY',50,869,550,26,14,P.soft,'SemiBold','Barlow Condensed',1.5);
return {createdNodeIds:created,mutatedNodeIds:[old.id],pageId:page.id,menuId:menu.id,lobbyId:lobby.id,imageTargetIds:[menuImage.id,lobbyImage.id],nodeCount:created.length};
