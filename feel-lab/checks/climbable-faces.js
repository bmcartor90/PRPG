// Climbables on cliff faces: ladder, rope, rough rock and smooth rock. Prints measurements; read them against the notes.
const fs=require('fs'), path=require('path');
const html=fs.readFileSync(path.join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level=script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];
const P=JSON.parse(fs.readFileSync(path.join(__dirname,'..','traces','_world.json'),'utf8')).params;
new Function('P', sim+level+`
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false,up:false,down:false,grab:false,swapHand:-1},o);
const at=(x)=>{ let best=null; for(const L of LINES) if(L.solid&&!L.slab&&x>=L.x1&&x<=L.x2&&(!best||yAt(L,x)>yAt(best,x))) best=L; return makePlayer(x,best); };
let pj=false; const S=(p,i)=>{ const j=!!i.jump; step(p,P,I({dir:i.dir||0,run:!!i.run,jumpHeld:j,jumpPressed:j&&!pj,up:!!i.up,down:!!i.down,grab:!!i.grab,swapHand:i.hand===undefined?-1:i.hand}),LINES,WORLD_W,CLIMBS,CLIFFS,ROOFS); pj=j; };
const go=(p,seq)=>{ const seen=new Set(); for(const [n,i] of seq) for(let t=0;t<n;t++){ S(p,t===0?i:Object.assign({},i,{hand:undefined})); seen.add(p.mode+(p.climb?':'+p.climb.kind:'')); } p.seen=[...seen].join(' > '); return p; };
const f=(p)=>'x '+p.x.toFixed(2)+' y '+p.y.toFixed(2)+' '+p.mode+(p.grounded?' grounded':' air')+' stamina '+p.stamina.toFixed(0);
let p;
p=go(at(314),[[40,{dir:-1}],[10,{grab:true}],[100,{grab:true,up:true}]]); console.log('1 rough rock, 3 block up to 6.5   ', f(p), '|', p.seen); go(p,[[80,{grab:true,up:true}],[20,{}]]); console.log('  and on                          ', f(p), '(expect on top at y 6.50)');
pj=false; p=go(at(346),[[40,{dir:-1}],[10,{grab:true}],[260,{grab:true,up:true}],[30,{}]]); console.log('2 rope up the 7-high face         ', f(p), '|', p.seen, '(expect on top at y 7.00)');
go(p,[[5,{dir:1}],[14,{}],[2,{grab:true,down:true}]]); console.log('  grab and down at the edge       ', f(p), '(expect on the rope, feet at 5.10)'); go(p,[[60,{grab:true,down:true,run:true}],[20,{}]]); console.log('  slide down                      ', f(p), '(expect on the ground beside the face, x 344.30)');
pj=false; p=go(at(360),[[40,{dir:1}],[40,{dir:1,up:true}]]); console.log('3 ladder that starts 1.5 up: up   ', f(p), '(expect still on the ground: it is out of reach)');
go(p,[[1,{}],[30,{jump:true,up:true}],[160,{up:true}],[20,{}]]); console.log('  jump and up                     ', f(p), '|', p.seen, '(expect on top at y 6.00)');
pj=false; p=go(at(380.4),[[2,{}],[30,{jump:true,grab:true}],[120,{grab:true,up:true}],[20,{}]]); console.log('4 smooth rock on floating rock    ', f(p), '|', p.seen, '(expect exhaustion and a fall: the jump, the climb and the pull-up together cost more than 100)');
pj=false; p=go(at(380.4),[[1,{hand:1}],[40,{}],[30,{jump:true,grab:true}],[60,{grab:true,up:true}]]); console.log('5 the same holding a sword        ', f(p), '|', p.seen, '(expect no climb, since smooth rock needs two free hands; he catches the corner one-handed instead)');
pj=false; p=go(at(314),[[1,{hand:1}],[40,{dir:-1}],[10,{grab:true}],[220,{grab:true,up:true}],[20,{}]]); console.log('6 rough rock holding a sword      ', f(p), '|', p.seen, '(expect it works one-handed, slower)');
`)(P);
