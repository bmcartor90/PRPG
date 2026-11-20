const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
fs.writeFileSync(require('path').join(require('os').tmpdir(),'feel-lab-all.js'),script);
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level=script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];
eval(sim+level+`
const base={jumpHeight:4,apexTime:0.40,fallMult:1.6,cutMult:3,terminal:20,walkSpeed:5.5,runSpeed:10,accelTime:0.45,stopTime:0.30,turnTime:0.35,airControl:0.65,coyoteMs:80,bufferMs:100,hardCut:false,
  walkLimit:50,upCost:20,downBoost:15,slideFriction:0.75,slideSteer:0.3,slideJump:1,footWidth:0.6,
  staminaMax:100,jumpCost:30,runDrain:2,walkRegen:12,crouchRegen:60,exhaustTime:1.2,charHeight:1.9,armReach:0.1,lungeLean:0.8,lungeCost:15,proneTime:0.6,pullTime:1,pullCost:40,climbSpeed:2,weight:1,climbEffort:10,hangCost:3,multLadder:0.5,multRope:1.5,multRoughRock:1,multSmoothRock:8,oneHand:0.65,drawTime:0.35,climbSprint:1.75,climbSprintCost:3};
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false,up:false,down:false,grab:false,swapHand:-1},o);
const S=(p,P,i)=>step(p,P,I(i),LINES,WORLD_W,CLIMBS);
const ground=(x)=>LINES.find(L=>x>=L.x1&&x<=L.x2&&L.sign===0&&Math.abs(L.y1)<1e-9);
console.log('world',WORLD_W,'climbs',CLIMBS.map(c=>c.kind+'@'+c.x).join(' '));
// regression: flat full jump, slopes run
{ const p=makePlayer(3,START); S(p,base,{jumpHeld:true,jumpPressed:true}); let n=0; while(!p.grounded&&n++<999) S(p,base,{jumpHeld:true}); console.log('flat full jump',p.last.height.toFixed(2),p.last.ticks,'mode',p.mode); }
// ledge grab with the button: stand just left of a ledge, jump straight up holding grab
function grabTest(P,ledgeX,label,hold=true){ const p=makePlayer(ledgeX-0.4,ground(ledgeX-0.4)); S(p,P,{jumpHeld:true,jumpPressed:true,grab:hold}); let n=0; while(p.mode==='normal'&&!p.grounded&&n++<300) S(p,P,{jumpHeld:true,grab:hold});
  if(p.mode!=='hang'){ console.log(label,'no grab; back on the ground, stamina',p.stamina.toFixed(0)); return; }
  const y0=p.y, st0=p.stamina; S(p,P,{jumpPressed:true,grab:true}); n=1; while(p.mode==='pull'&&n++<600) S(p,P,{grab:true});
  console.log(label,'caught with feet at',y0.toFixed(2),'stamina',st0.toFixed(0),'| pull-up took',(n/60).toFixed(2),'s -> on the ledge',p.grounded,'y',p.y.toFixed(2),'stamina',p.stamina.toFixed(0),'exhausted',p.exhaust>0); }
grabTest(base,234,'ledge 5.0:'); grabTest(base,242,'ledge 5.75:'); grabTest(base,250,'ledge 6.5:');
grabTest(base,234,'no button:',false); grabTest({...base,weight:2},234,'weight x2:');
{ const p=makePlayer(233.6,ground(233.6)); S(p,base,{jumpHeld:true,jumpPressed:true,grab:true}); let n=0; while(p.mode==='normal'&&n++<300) S(p,base,{jumpHeld:true,grab:true}); S(p,base,{}); console.log('release grab while hanging -> mode',p.mode,'falling',!p.grounded); }
// ladder / rope / wall: climb from the ground to the top
for (const C of CLIMBS.filter(c => !c.face && c.y2 === 8)){ const p=makePlayer(C.x,ground(C.x)); let n=0; while(n++<3000 && !(p.grounded&&p.y>7.9)){ S(p,base,{up:true,grab:true}); if(p.exhaust>0) break; }
  console.log(C.kind,'-> reached top',p.grounded&&p.y>7.9,'| y',p.y.toFixed(2),'stamina',p.stamina.toFixed(1),'exhausted',p.exhaust>0,'ticks',n); }
// climb down a ladder from the top platform
{ const C=CLIMBS[0]; const top=LINES.find(L=>Math.abs(L.y1-8)<1e-9&&C.x>=L.x1&&C.x<=L.x2); const p=makePlayer(C.x,top); let n=0; while(n++<800&&!(p.grounded&&p.y<0.1)) S(p,base,{down:true}); console.log('ladder down: on ground',p.grounded&&p.y<0.1,'ticks',n); }
// hop off a ladder sideways
{ const C=CLIMBS[0]; const p=makePlayer(C.x,ground(C.x)); for(let i=0;i<60;i++) S(p,base,{up:true}); const y=p.y; S(p,base,{dir:1,jumpPressed:true}); console.log('hop off at y',y.toFixed(2),'-> mode',p.mode,'vx',p.vx.toFixed(1),'vy',p.vy.toFixed(1)); let n=0; while(!p.grounded&&n++<300) S(p,base,{}); console.log('  landed y',p.y.toFixed(2)); }
// stamina on the ground
{ const p=makePlayer(224,ground(224)); for(let i=0;i<300;i++) S(p,base,{dir:1,run:true}); console.log('5 s of running: stamina',p.stamina.toFixed(1));
  for(let i=0;i<120;i++) S(p,base,{dir:-1}); console.log('2 s of walking: stamina',p.stamina.toFixed(1));
  const q=makePlayer(240,ground(240)); let jumps=0; for(let j=0;j<6&&q.exhaust<=0;j++){ S(q,base,{jumpPressed:true,jumpHeld:true}); jumps++; let n=0; while(!q.grounded&&n++<200) S(q,base,{jumpHeld:true}); }
  console.log('back-to-back jumps before going down:',jumps,'| exhausted',q.exhaust>0,'stamina',q.stamina.toFixed(0)); let m=0; while(q.exhaust>0){S(q,base,{});m++;}
  for(let i=0;i<60;i++) S(q,base,{down:true}); console.log('down',(m/60).toFixed(2),'s, then 1 s crouched: stamina',q.stamina.toFixed(0),'crouch',q.crouch); }
// crest of the 63-degree hill (slope 211..214 rising to the plateau at y=6): charge it, then hold grab
{ const p=makePlayer(200,ground(207)); p.x=207; let caught=null, maxY=0; for(let t=0;t<400&&!caught;t++){ S(p,base,{dir:1,run:true,grab:true}); maxY=Math.max(maxY,p.y); if(p.mode==='hang') caught={x:+p.x.toFixed(2),y:+p.y.toFixed(2),st:+p.stamina.toFixed(0)}; }
  console.log('crest: charged the steep hill holding grab ->',caught?('caught at '+JSON.stringify(caught)):'no catch, got as high as '+maxY.toFixed(2));
  if(caught){ S(p,base,{grab:true,jumpPressed:true}); let n=0; while(p.mode==='pull'&&n++<200) S(p,base,{grab:true}); console.log('  pulled up: grounded',p.grounded,'y',p.y.toFixed(2),'x',p.x.toFixed(2),'stamina',p.stamina.toFixed(0)); }
  const q=makePlayer(200,ground(207)); q.x=207; let c2=false; for(let t=0;t<400&&!c2;t++){ S(q,{...base,walkLimit:65},{dir:1,run:true,grab:true}); if(q.mode==='hang') c2=true; } console.log('  with walkable limit 65 (slope is a path): caught',c2,'| x',q.x.toFixed(1),'y',q.y.toFixed(2)); }
// lunge: a platform edge ahead, too far for a plain grab. Jump straight up beside it at various distances.
{ for (const P of [base,{...base,charHeight:2.4}]){ let plain=0, lunge=0; for(let gap=0.3;gap<=2.6;gap+=0.05){ const G=mkLine(0,0,20,0), T=mkLine(10,3,14,3);
    for (const useLunge of [false,true]){ const p=makePlayer(10-gap,G); step(p,P,I({jumpHeld:true,jumpPressed:true,grab:true,dir:1}),[G,T],60,[]); let n=0, did=false;
      while(p.mode==='normal'&&!p.grounded&&n++<300){ const press=useLunge&&!did&&n===12; if(press)did=true; step(p,{...P,airControl:0},I({jumpHeld:true,grab:true,dir:1,jumpPressed:press}),[G,T],60,[]); }
      if(p.mode==='hang'){ if(useLunge) lunge=gap; else plain=gap; } } }
    console.log('height',P.charHeight,': farthest edge caught from a standing jump -> plain grab',plain.toFixed(2),'| with a lunge',lunge.toFixed(2)); } }
// a lunge that misses: prone on landing
{ const G=mkLine(0,0,40,0); const p=makePlayer(5,G); step(p,base,I({jumpHeld:true,jumpPressed:true,dir:1,run:true}),[G],60,[]); for(let i=0;i<10;i++) step(p,base,I({jumpHeld:true,dir:1,grab:true}),[G],60,[]);
  const st=p.stamina; step(p,base,I({jumpHeld:true,dir:1,grab:true,jumpPressed:true}),[G],60,[]); console.log('lunge: stamina',st.toFixed(0),'->',p.stamina.toFixed(0),'lunging',p.lunge); let n=0; while(!p.grounded&&n++<200) step(p,base,I({dir:-1,grab:true}),[G],60,[]);
  let m=0; const x0=p.x; while(p.prone>0){ step(p,base,I({dir:1,run:true}),[G],60,[]); m++; } console.log('  missed: face down for',(m/60).toFixed(2),'s, slid',(p.x-x0).toFixed(2),'tiles; then free',p.prone<=0); }
// crest of the 63-degree hill with a lunge from the slide
{ const g=ground(207); const p=makePlayer(207,g); let caught=null, lunged=false; for(let t=0;t<400&&!caught;t++){ const press=!lunged&&p.grounded&&p.line.deg>60&&p.vx<0.35&&p.y>3.5; if(press)lunged=true; S(p,base,{dir:1,run:true,grab:true,jumpPressed:press}); if(p.mode==='hang') caught={x:+p.x.toFixed(2),y:+p.y.toFixed(2),st:+p.stamina.toFixed(0)}; if(lunged&&p.prone>0)break; }
  console.log('crest with a lunge at the top of the charge ->',caught?('caught '+JSON.stringify(caught)):'missed, prone '+p.prone.toFixed(2));
  if(caught){ S(p,base,{grab:true,up:true}); let n=0; while(p.mode==='pull'&&n++<200) S(p,base,{grab:true}); console.log('  pulled up onto the plateau:',p.grounded,'y',p.y.toFixed(2),'stamina',p.stamina.toFixed(0)); } }
// ---- hands ----
function climbTest(kind, held, opts={}){ const C=CLIMBS.find(c=>c.kind===kind); const p=makePlayer(C.x,ground(C.x)); p.held=held.slice(); let n=0;
  while(n++<4000 && !(p.grounded&&p.y>7.9)){ S(p,base,{up:true,grab:true,run:!!opts.run}); if(p.exhaust>0||(n>5&&p.mode==='normal'&&p.grounded&&p.y<0.1)) break; }
  const top=p.grounded&&p.y>7.9; return (top?('top in '+(n/60).toFixed(1)+' s, stamina left '+p.stamina.toFixed(0)):(p.exhaust>0?('fell exhausted at '+p.py.toFixed(1)+' tiles'):'could not hold on')); }
for (const kind of ['Ladder','Rope','SmoothRock']) console.log(kind.padEnd(10),'| both free:',climbTest(kind,[false,false]),'| one free:',climbTest(kind,[false,true]),'| none free:',climbTest(kind,[true,true]));
console.log('sprint-climb, both free | ladder:',climbTest('Ladder',[false,false],{run:true}),'| rope:',climbTest('Rope',[false,false],{run:true}));
// pull-up with one hand
{ for (const held of [[false,false],[false,true],[true,true]]){ const p=makePlayer(233.6,ground(233.6)); p.held=held.slice(); S(p,base,{jumpHeld:true,jumpPressed:true,grab:true}); let n=0; while(p.mode==='normal'&&!p.grounded&&n++<300) S(p,base,{jumpHeld:true,grab:true});
    if(p.mode!=='hang'){ console.log('ledge, free hands',2-held.filter(Boolean).length,': cannot catch it'); continue; } S(p,base,{grab:true,jumpPressed:true}); n=1; while(p.mode==='pull'&&n++<600) S(p,base,{grab:true});
    console.log('ledge, free hands',2-held.filter(Boolean).length,': pull-up took',(n/60).toFixed(2),'s, on the ledge',p.grounded,'stamina',p.stamina.toFixed(0),'exhausted',p.exhaust>0); } }
// draw mid-air: jump, sheathe-toggle the right hand while airborne, land
{ const p=makePlayer(240,ground(240)); S(p,base,{jumpHeld:true,jumpPressed:true}); S(p,base,{jumpHeld:true,swapHand:1}); const during=freeHands(p); let n=0; while(p.swap&&n++<100) S(p,base,{jumpHeld:true}); console.log('draw in mid-air: free hands during',during,'| took',((n+1)/60).toFixed(2),'s | right hand now holds gear',p.held[1],'| still airborne',!p.grounded); }
// start a draw while on a wall (needs both hands) -> you drop
{ const C=CLIMBS[2]; const p=makePlayer(C.x,ground(C.x)); for(let i=0;i<30;i++) S(p,base,{up:true,grab:true}); const y=p.y; S(p,base,{up:true,grab:true,swapHand:0}); S(p,base,{up:true,grab:true}); console.log('drawing while on the wall at',y.toFixed(2),'-> mode',p.mode); }
// jump off a rope: real jump away, then slide down a rope
{ const C=CLIMBS[1]; const p=makePlayer(C.x,ground(C.x)); for(let i=0;i<120;i++) S(p,base,{up:true,grab:true}); const y=p.y, st=p.stamina; S(p,base,{dir:-1,jumpPressed:true,jumpHeld:true,grab:false}); let peak=p.y,n=0; while(!p.grounded&&n++<300){ S(p,base,{dir:-1,jumpHeld:true}); peak=Math.max(peak,p.y);} console.log('jump off the rope at',y.toFixed(2),': rose to',peak.toFixed(2),'landed',(C.x-p.x).toFixed(1),'tiles away, cost',(st-p.stamina-0).toFixed(0)+'ish stamina; last jump height',p.last&&p.last.height.toFixed(2));
  const q=makePlayer(C.x,ground(C.x)); for(let i=0;i<200;i++) S(q,base,{up:true,grab:true}); const y2=q.y, s2=q.stamina; let m=0; while(!q.grounded&&m++<600) S(q,base,{down:true,run:true,grab:true}); console.log('slide down the rope from',y2.toFixed(2),': took',(m/60).toFixed(2),'s, stamina cost',(s2-q.stamina).toFixed(1)); }
// ---- round 3: rope from the top, jumping off, resting on a ladder ----
{ const C=CLIMBS[1]; const top=LINES.find(L=>Math.abs(L.y1-8)<1e-9&&C.x>=L.x1&&C.x<=L.x2); const p=makePlayer(C.x+0.3,top); S(p,base,{down:true}); console.log('rope top, down only -> mode',p.mode,'crouch',p.crouch);
  S(p,base,{down:true,grab:true}); console.log('rope top, grab + down -> mode',p.mode); let n=0; while(n++<800&&!(p.grounded&&p.y<0.1)) S(p,base,{down:true,grab:true}); console.log('  climbed down to the ground:',p.grounded&&p.y<0.1,'in',(n/60).toFixed(1),'s'); }
for (const C of [CLIMBS[0],CLIMBS[1]]){ const p=makePlayer(C.x,ground(C.x)); for(let i=0;i<120;i++) S(p,base,{up:true,grab:true}); const y=p.y, st=p.stamina;
  S(p,base,{up:true,grab:true,jumpPressed:true,jumpHeld:true}); let peak=p.y,n=0,re=null; while(n++<200&&!p.grounded){ S(p,base,{up:true,grab:true,jumpHeld:true}); peak=Math.max(peak,p.y); if(p.mode==='climb'&&re===null) re=p.y; }
  console.log(C.kind,': jump with no direction at',y.toFixed(2),'-> rose to',peak.toFixed(2),'| caught it again at',re===null?'never':re.toFixed(2),'| cost',(st-p.stamina).toFixed(0)+'-ish');
  const q=makePlayer(C.x,ground(C.x)); for(let i=0;i<120;i++) S(q,base,{up:true,grab:true}); S(q,base,{down:true,grab:true,jumpPressed:true}); console.log('  down + jump -> mode',q.mode,'vy',q.vy.toFixed(1),'stamina kept',q.stamina.toFixed(0)); }
{ const C=CLIMBS[0]; const p=makePlayer(C.x,ground(C.x)); p.stamina=40; for(let i=0;i<60;i++) S(p,base,{up:true}); const a=p.stamina; for(let i=0;i<120;i++) S(p,base,{}); console.log('ladder rest, both hands free: stamina',a.toFixed(0),'->',p.stamina.toFixed(0),'in 2 s');
  const q=makePlayer(C.x,ground(C.x)); q.held=[true,false]; q.stamina=40; for(let i=0;i<60;i++) S(q,base,{up:true}); const b=q.stamina; for(let i=0;i<120;i++) S(q,base,{}); console.log('ladder, one hand full: stamina',b.toFixed(0),'->',q.stamina.toFixed(0),'in 2 s'); }
// save by catching the far edge of a gap you just missed: 8-gap at 87..95, jump early
{ const p=makePlayer(80,ground(80)); let jumped=false,n=0; while(n++<600){ const press=!jumped&&p.x>=86.6; S(p,base,{dir:1,run:true,jumpHeld:true,jumpPressed:press,grab:true}); if(press)jumped=true; if(jumped&&(p.grounded||p.mode!=='normal'))break; } console.log('8-gap, early jump: mode',p.mode,'x',p.x.toFixed(2),'y',p.y.toFixed(2)); }
`);
