// Stage A review renderer. Draws original concept art and UI overlays directly with AppKit.
// These are approval mockups, not Unity assets or a browser preview.
import AppKit

let root = URL(fileURLWithPath: #filePath).deletingLastPathComponent().path
let width = 1600.0, height = 900.0
func color(_ hex: UInt32, _ alpha: CGFloat = 1) -> NSColor {
    NSColor(calibratedRed: CGFloat((hex >> 16) & 255) / 255,
            green: CGFloat((hex >> 8) & 255) / 255,
            blue: CGFloat(hex & 255) / 255, alpha: alpha)
}
let white = color(0xeff4f2), soft = color(0xbfcbc9), dim = color(0x8b9a9c)
let amber = color(0xe3ad55), cyan = color(0x7ad7db), green = color(0xa8d797), red = color(0xeb776c)
func box(_ x: Double, _ top: Double, _ w: Double, _ h: Double, _ c: NSColor) {
    c.setFill(); NSRect(x: x, y: height-top-h, width: w, height: h).fill()
}
func stroke(_ x: Double, _ top: Double, _ w: Double, _ h: Double, _ c: NSColor, _ size: CGFloat = 1) {
    c.setStroke(); let p = NSBezierPath(rect: NSRect(x:x,y:height-top-h,width:w,height:h)); p.lineWidth=size; p.stroke()
}
func line(_ x: Double, _ top: Double, _ xx: Double, _ ttop: Double, _ c: NSColor, _ size: CGFloat = 1) {
    c.setStroke(); let p=NSBezierPath(); p.move(to:NSPoint(x:x,y:height-top)); p.line(to:NSPoint(x:xx,y:height-ttop)); p.lineWidth=size; p.stroke()
}
func font(_ size: CGFloat, display: Bool = false) -> NSFont {
    if display { return NSFont(name:"DINCondensed-Bold", size:size) ?? NSFont(name:"AvenirNextCondensed-DemiBold",size:size) ?? NSFont.boldSystemFont(ofSize:size) }
    return NSFont(name:"AvenirNext-Medium",size:size) ?? NSFont.systemFont(ofSize:size)
}
@discardableResult func text(_ s:String, _ x:Double, _ top:Double, _ size:CGFloat, _ c:NSColor=white, display:Bool=false, kern:CGFloat=0) -> CGSize {
    let a = NSAttributedString(string:s, attributes:[.font:font(size,display:display), .foregroundColor:c, .kern:kern])
    let sz=a.size(); a.draw(at:NSPoint(x:x,y:height-top-Double(sz.height))); return sz
}
func right(_ s:String, _ x:Double, _ top:Double, _ size:CGFloat, _ c:NSColor=white, display:Bool=false, kern:CGFloat=0) {
    let attrs:[NSAttributedString.Key:Any] = [.font:font(size,display:display),.foregroundColor:c,.kern:kern]
    let a=NSAttributedString(string:s,attributes:attrs); let sz=a.size(); a.draw(at:NSPoint(x:x-Double(sz.width),y:height-top-Double(sz.height)))
}
func gradient(_ x:Double,_ top:Double,_ w:Double,_ h:Double,_ a:NSColor,_ b:NSColor,_ angle:CGFloat=0) {
    NSGradient(starting:a,ending:b)?.draw(in:NSRect(x:x,y:height-top-h,width:w,height:h),angle:angle)
}
func background(_ name:String) {
    let path="\(root)/concept-art/\(name).png"
    NSImage(contentsOfFile:path)?.draw(in:NSRect(x:0,y:0,width:width,height:height), from:.zero, operation:.sourceOver, fraction:1)
}
func small(_ s:String,_ x:Double,_ t:Double,_ c:NSColor=soft,_ size:CGFloat=16) { text(s,x,t,size,c,display:true,kern:1.5) }
func bar(_ top:Double=0,_ h:Double=94) { gradient(0,top,1600,h,color(0x02080c,0.96),color(0x02080c,0.45),90); line(0,top+h,1600,top+h,color(0xf0f5f2,0.22)) }
func footer(_ s:String) { box(0,850,1600,50,color(0x01070a,0.89)); line(0,850,1600,850,color(0xf0f5f2,0.25)); small(s,62,867,soft,14) }
func panel(_ x:Double,_ t:Double,_ w:Double,_ h:Double,_ accent:NSColor=cyan) {
    box(x,t,w,h,color(0x020b0f,0.79)); box(x,t,w,3,accent)
}

func menu() {
    background("facility-menu")
    gradient(0,0,900,900,color(0x02080c,0.88),color(0x02080c,0),0)
    gradient(0,615,1600,285,color(0x02070a,0.83),color(0x02070a,0),90)
    bar(); text("ARCADE / SHOOTER",62,24,32,white,display:true,kern:2)
    right("OPERATOR STATUS  /  ONLINE",1542,35,16,soft,display:true,kern:1.5)
    box(70,143,3,565,amber)
    small("RESEARCH FACILITY  //  INCIDENT 01",100,141,cyan,18)
    text("CONTAINMENT",100,178,101,white,display:true,kern:1)
    text("BREACH",100,258,101,white,display:true,kern:1)
    text("The facility is lost. The squad is not.",103,363,22,soft)
    small("OPERATIONS  /  01",103,421,amber,17)
    let options=["PLAY / MULTIPLAYER","PLAYER PROFILE","SETTINGS","QUIT GAME"]
    for i in 0..<4 {
        let t=456.0+Double(i)*67
        if i==0 { gradient(98,t,495,67,color(0xe3ad55,0.22),color(0xe3ad55,0),0); box(98,t,4,67,amber) }
        line(100,t+66,581,t+66,color(0xf0f5f2,0.23))
        text(String(format:"%02d",i+1),119,t+20,17,i==0 ? amber:dim,display:true)
        text(options[i],176,t+9,31,white,display:true,kern:1)
        if i==0 { text("›",547,t+9,32,amber,display:true) }
    }
    panel(1100,654,430,177,amber); small("ACTIVE OPERATION  /  01",1125,670,amber,16)
    text("CONTAINMENT BREACH",1125,697,32,white,display:true,kern:1)
    text("Clear both danger zones. Recover the research",1125,739,15,soft)
    text("sample. Extract as a team.",1125,758,15,soft)
    line(1125,792,1504,792,color(0xf0f5f2,0.25)); small("CO-OP  2–4",1125,799,cyan,15); right("SECTOR 03",1503,799,15,soft,display:true)
    footer("ESC  BACK     /     ENTER  SELECT"); right("BUILD 0.10  •  MACOS",1537,867,14,soft,display:true,kern:1)
}

func lobby() {
    background("squad-lobby"); bar(0,113)
    text("FIRETEAM / HANGAR 03",47,18,42,white,display:true,kern:1.5)
    small("OPERATION 01   /   CONTAINMENT BREACH",49,72,amber,18)
    text("04 / 04 OPERATORS",732,32,24,white,display:true,kern:1)
    box(1224,13,332,85,color(0x020a0d,0.72)); box(1224,13,3,85,cyan)
    small("PRIVATE SESSION / JOIN CODE",1242,24,soft,13)
    text("AX7R42",1242,46,30,white,display:true,kern:2); right("COPY ↗",1536,57,17,cyan,display:true)
    for i in 1..<4 { line(Double(i)*400,114,Double(i)*400,672,color(0xffffff,0.2)) }
    for i in 0..<4 { text(String(format:"%02d",i+1),Double(i)*400+28,615,30,white,display:true); box(Double(i)*400+28,657,220,2,cyan) }
    gradient(0,668,1600,232,color(0x02080b,0.98),color(0x02080b,0.66),90)
    box(0,668,1600,2,amber)
    let names=["ECHO","ROOK","VEX","ASH"], sub=["HOST / LEVEL 03","LEVEL 02","LEVEL 01","LEVEL 02"], finishes=["PATHFINDER","STANDARD","STANDARD","TEAL FINISH"]
    for i in 0..<4 {
        let x=Double(i)*400+28
        if i>0 { line(Double(i)*400,670,Double(i)*400,849,color(0xf0f5f2,0.18)) }
        text(names[i],x,680,39,white,display:true,kern:1)
        small(sub[i],x,724,soft,16); small(finishes[i],x,749,cyan,15)
        for j in 0..<3 { stroke(x+Double(j)*47,772,38,30,color(0xf0f5f2,0.48)); text("◇",x+Double(j)*47+10,774,19,soft,display:true) }
        box(x,810,340,2,green); small("READY",x,816,green,17)
    }
    footer("ESC  LEAVE LOBBY     •     NORMAL DIFFICULTY")
    box(1260,856,278,38,amber); text("START MISSION  ›",1282,858,27,color(0x071114),display:true,kern:1)
}

func hud() {
    background("facility-combat")
    gradient(0,0,1600,250,color(0x010609,0.56),color(0x010609,0),90)
    gradient(0,605,1600,295,color(0x010609,0.76),color(0x010609,0),90)
    small("OPERATION 01 / CONTAINMENT BREACH",42,32,cyan,17)
    text("✓   CLEAR ZONE A",42,69,21,green,display:true)
    text("◉   CLEAR ZONE B",42,102,21,white,display:true)
    text("○   RECOVER SAMPLE",42,135,21,soft,display:true)
    gradient(41,194,320,139,color(0x01080a,0.7),color(0x01080a,0),0); box(41,194,2,139,cyan)
    small("SQUAD STATUS",58,201,cyan,14)
    let squad=[("ECHO / YOU","100",green),("ROOK","072",amber),("VEX","095",green)]
    for (i,p) in squad.enumerated(){let t=229.0+Double(i)*31;text(p.0,58,t,17,white,display:true);right(p.1,328,t,17,p.2,display:true)}
    gradient(545,35,520,82,color(0x02080a,0.74),color(0x02080a,0.74),0); box(545,35,520,2,amber)
    text("!   CONTAINMENT BREACH",616,44,27,amber,display:true)
    small("HOSTILES INBOUND   /   ZONE B",634,82,white,16)
    box(1554,34,2,75,amber); right("ZONE B / WAVE 02",1537,37,16,amber,display:true,kern:1)
    right("04   REMAINING",1537,61,35,white,display:true)
    stroke(797,447,12,12,white); line(803,438,803,470,white,1); line(787,453,819,453,white,1)
    small("HEALTH / ECHO",42,779,soft,16); text("100",42,801,42,white,display:true)
    for i in 0..<10 { box(42+Double(i)*31,854,28,10,white) }
    right("ASSAULT RIFLE / AUTO",1555,785,17,soft,display:true,kern:1)
    right("24  / 90",1555,810,57,white,display:true)
    small("EXTRACTION LOCKED   •   COMPLETE OBJECTIVES",592,857,soft,15)
}

func results() {
    background("extraction-results")
    gradient(0,0,1120,900,color(0x02080d,0.83),color(0x02080d,0.1),0)
    gradient(0,575,1600,325,color(0x02070a,0.81),color(0x02070a,0),90)
    small("OPERATION 01 / AFTER ACTION REPORT",66,46,cyan,19)
    text("MISSION",64,83,90,white,display:true,kern:1)
    text("COMPLETE",64,160,90,white,display:true,kern:1)
    text("The squad extracted with the research sample.",68,258,20,soft)
    panel(1250,44,280,264,amber); small("MISSION RANK",1300,61,soft,16)
    text("A",1328,78,191,amber,display:true)
    small("TEAM SCORE",67,319,soft,17); text("3,980",67,342,74,white,display:true)
    text("04:32   COMPLETION TIME",309,383,20,soft,display:true)
    panel(65,598,665,190,cyan); small("SQUAD PERFORMANCE",90,616,cyan,17)
    let cols=[90.0,281,391,481,583]
    let rows:[[String]]=[["OPERATOR","KILLS","HITS","ACC.","SCORE"],["ECHO / YOU","07","35","70%","2,145"],["ROOK","05","29","66%","1,835"]]
    for i in 0..<3 { let t=648.0+Double(i)*37; for j in 0..<5 { text(rows[i][j],cols[j],t,i==0 ? 16:21,i==0 ? soft:white,display:true) } }
    line(90,687,705,687,color(0xffffff,0.2))
    line(90,755,705,755,color(0xffffff,0.2)); small("OBJECTIVES +1,500   /   TIME +340   /   SURVIVAL +500",90,764,soft,13)
    panel(1100,603,430,185,amber); small("PERSONAL PROGRESSION",1125,621,amber,17)
    text("+340 XP",1125,646,59,white,display:true)
    small("LEVEL 02   /   620 OF 1,000 XP",1125,708,soft,15)
    box(1125,739,379,8,color(0xffffff,0.2)); box(1125,739,250,8,cyan)
    small("TEAL FINISH UNLOCKED",1125,754,green,16)
    footer("SQUAD REMAINS CONNECTED   •   HOST CHOOSES NEXT ACTION")
    text("RETURN TO LOBBY",1005,859,23,white,display:true)
    box(1290,856,241,38,amber); text("REPLAY MISSION  ›",1305,859,23,color(0x081012),display:true)
}

func profile() {
    background("squad-lobby")
    gradient(0,0,1100,900,color(0x01080b,0.92),color(0x01080b,0.13),0)
    gradient(1100,0,500,900,color(0x01080b,0.12),color(0x01080b,0.72),0)
    bar(); text("ARCADE / SHOOTER",65,25,32,white,display:true,kern:2)
    right("OPERATOR PROFILE / LOADOUT",1534,36,16,cyan,display:true,kern:1)
    small("LOCAL PROFILE / 01",70,135,cyan,17); text("ECHO",70,167,78,white,display:true)
    small("PATHFINDER / EQUIPPED",73,258,amber,18)
    box(70,321,410,2,cyan); small("CURRENT LEVEL",70,341,soft,17)
    text("03",70,369,70,white,display:true)
    box(70,459,410,9,color(0xffffff,0.2)); box(70,459,234,9,cyan)
    small("570 / 1,200 XP TO NEXT LEVEL",70,479,soft,15)
    text("MISSIONS COMPLETED     18",70,692,21,white,display:true)
    text("SUCCESSFUL EXTRACTIONS     12",70,728,21,white,display:true)
    text("PRIMARY WEAPON     ASSAULT RIFLE",70,764,21,white,display:true)
    panel(1160,650,370,174,cyan); small("CURRENT LOADOUT / PRIMARY",1183,667,cyan,16)
    text("ASSAULT RIFLE",1183,690,37,white,display:true)
    small("DAMAGE 20   •   8 ROUNDS / SEC   •   MAG 30",1183,735,soft,12)
    line(1183,771,1506,771,color(0xffffff,0.2)); small("TEAL FINISH / EQUIPPED",1183,781,green,16)
    footer("ESC  BACK TO LOBBY   /   COSMETICS SAVED LOCALLY")
}

func pause() {
    background("facility-combat"); box(0,0,1600,900,color(0x02070a,0.82)); bar()
    text("OPERATIONS PAUSED",70,104,81,white,display:true)
    text("Your online session continues while this menu is open.",73,217,19,amber)
    let entries=["RESUME MISSION","SETTINGS","LEAVE MATCH","RETURN TO MENU"]
    for i in 0..<4 { let t=332.0+Double(i)*79
        if i==0 { gradient(70,t,475,76,color(0xe3ad55,0.2),color(0xe3ad55,0),0);box(70,t,4,76,amber) }
        text(String(format:"%02d",i+1),91,t+14,36,i==0 ? amber:soft,display:true)
        text(entries[i],153,t+14,36,white,display:true)
        line(70,t+75,545,t+75,color(0xffffff,0.22))
    }
    line(684,278,684,789,color(0xffffff,0.26))
    small("AUDIO / CONTROLS / VIDEO",724,294,cyan,17)
    let settings=[("MASTER VOLUME",0.83,"83%"),("MUSIC VOLUME",0.58,"58%"),("SFX VOLUME",0.77,"77%"),("MOUSE SENSITIVITY",0.43,"43%")]
    for (i,p) in settings.enumerated(){let t=352.0+Double(i)*72
        text(p.0,724,t,21,white,display:true); box(977,t+16,381,6,color(0xffffff,0.23));box(977,t+16,381*p.1,6,cyan);right(p.2,1452,t,21,white,display:true)
    }
    line(724,665,1490,665,color(0xffffff,0.25))
    text("DISPLAY MODE",724,695,21,white,display:true)
    text("FULLSCREEN / 2560 × 1440",977,695,21,white,display:true)
    footer("ESC  RESUME   •   SETTINGS SAVED LOCALLY   •   LEAVE MATCH REQUIRES CONFIRMATION")
}

let renderers:[(String,()->Void)] = [("main-menu-v2",menu),("lobby-v2",lobby),("combat-hud-v2",hud),("mission-results-v2",results),("profile-loadout-v2",profile),("pause-settings-v2",pause)]
for (name, draw) in renderers {
    guard let bitmap=NSBitmapImageRep(bitmapDataPlanes:nil,pixelsWide:Int(width),pixelsHigh:Int(height),bitsPerSample:8,samplesPerPixel:4,hasAlpha:true,isPlanar:false,colorSpaceName:.deviceRGB,bytesPerRow:0,bitsPerPixel:0), let context=NSGraphicsContext(bitmapImageRep:bitmap) else { fatalError("Could not allocate bitmap") }
    NSGraphicsContext.saveGraphicsState(); NSGraphicsContext.current=context
    NSColor.black.setFill(); NSRect(x:0,y:0,width:width,height:height).fill()
    draw(); context.flushGraphics(); NSGraphicsContext.restoreGraphicsState()
    guard let data=bitmap.representation(using:.png,properties:[:]) else { fatalError("Could not encode PNG") }
    try data.write(to:URL(fileURLWithPath:"\(root)/mockups/\(name).png"))
    print("Rendered \(name)")
}
