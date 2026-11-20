const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level=script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];
eval(sim+level+`
const P={jumpHeight:4,apexTime:0.40,fallMult:1.6,cutMult:3,terminal:20,walkSpeed:5.5,runSpeed:10,accelTime:0.45,stopTime:0.30,turnTime:0.35,airControl:0.65,coyoteMs:80,bufferMs:100,hardCut:false,
  walkLimit:50,upCost:20,downBoost:15,slideFriction:0.75,slideSteer:0.3,slideJump:1,footWidth:0.6,bodyWidth:0.6,
  staminaMax:100,jumpCost:30,runDrain:2,walkRegen:12,crouchRegen:60,exhaustTime:1.2,charHeight:1.9,armReach:0.1,lungeLean:0.8,lungeCost:15,proneTime:0.6,pullTime:1,pullCost:40,climbSpeed:2,weight:1,climbEffort:10,hangCost:3,multLadder:0.5,multRope:1.5,multRoughRock:1,multSmoothRock:8,oneHand:0.65,drawTime:0.35,climbSprint:1.75,climbSprintCost:3};
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false,up:false,down:false,grab:false,swapHand:-1},o);
const at=(x)=>{ let best=null; for(const L of LINES) if(L.solid&&x>=L.x1&&x<=L.x2&&(!best||yAt(L,x)>yAt(best,x))) best=L; return makePlayer(x,best); };
let pj=false; const S=(p,i)=>{ const j=!!i.jump; step(p,P,I({dir:i.dir||0,run:!!i.run,jumpHeld:j,jumpPressed:j&&!pj,up:!!i.up,down:!!i.down,grab:!!i.grab}),LINES,WORLD_W,CLIMBS,CLIFFS); pj=j; };
const go=(p,seq)=>{ for(const [n,i] of seq) for(let t=0;t<n;t++) S(p,i); return p; };
const f=(p)=>'x '+p.x.toFixed(2)+' y '+p.y.toFixed(2)+' '+p.mode+(p.grounded?' grounded':' air');
console.log('cliffs', CLIFFS.length, CLIFFS.filter(c=>c.x>=299).map(c=>c.x+':'+c.top+'>'+(c.bottom===-Infinity?'pit':c.bottom)+(c.face<0?'L':'R')).join(' '));
let p;
p=go(at(295),[[120,{dir:1,run:true}]]); console.log('1 run into the 2-high cliff      ', f(p), '(expect x 299.70 y 0)');
p=go(at(296),[[18,{dir:1,run:true}],[40,{dir:1,run:true,jump:true}],[40,{}]]); console.log('2 running jump onto the 2 block   ', f(p), '(expect y 2)');
p=go(at(303),[[60,{dir:1}],[1,{}],[50,{dir:1,jump:true}],[40,{dir:1}]]); console.log('3 jump at the 6.5 face, no grab   ', f(p), '(expect x 305.70 y 2)');
p=go(at(304.5),[[2,{}],[70,{dir:1,jump:true,grab:true}]]); console.log('4 same with grab held             ', f(p), '(expect hang)');
go(p,[[80,{grab:true,up:true}],[20,{}]]); console.log('  then pull up                    ', f(p), '(expect y 6.5 grounded)');
go(p,[[150,{dir:1}]]); console.log('5 walk on, drop to the 3 block    ', f(p), '(expect y 3)');
p=go(at(314),[[2,{}],[60,{dir:-1,jump:true}],[60,{dir:-1}]]); console.log('6 from the 3 block, jump back left', f(p), '(expect x 312.30 y 3)');
p=go(at(316),[[60,{dir:1}],[30,{}]]); const inPit=f(p); go(p,[[60,{dir:1}],[60,{dir:-1}]]); console.log('7 walk into the pit               ', inPit, '| after pushing both ways', f(p));
go(p,[[200,{}]]); console.log('  later                           ', f(p), '(expect respawned on the 3 block)');
p=go(at(325),[[2,{}],[45,{jump:true}],[60,{}]]); console.log('8 jump up through the scaffold    ', f(p), '(expect y 3.5)');
p=go(at(328),[[90,{dir:1}]]); const atFace=f(p); go(p,[[260,{up:true}],[30,{}]]); console.log('9 walk to the 7 cliff, take ladder', atFace, '|', f(p), '(expect y 7 grounded)');
pj=false; p=go(at(60.5),[[2,{}],[26,{dir:1,jump:true,grab:true}],[90,{dir:1,grab:true}]]); console.log('10 short hop into the 4-gap, grab ', f(p), '(expect hang, x 64.70)');
pj=false; p=go(at(60.5),[[2,{}],[26,{dir:1,jump:true}],[28,{dir:1}]]); console.log('11 same, no grab, mid-fall        ', f(p), '(expect in the air, x <= 64.70, y < 0)');
pj=false; p=go(at(317),[[40,{dir:1}],[14,{dir:1}]]); const a1=f(p); go(p,[[10,{dir:1}]]); console.log('12 fall into the pit, push right  ', a1, '|', f(p), '(expect x <= 320.70)');
pj=false; p=go(at(322),[[40,{dir:-1}],[14,{dir:-1}]]); const a2=f(p); go(p,[[10,{dir:-1}]]); console.log('13 into the pit from the right    ', a2, '|', f(p), '(expect x >= 318.30)');
`);
