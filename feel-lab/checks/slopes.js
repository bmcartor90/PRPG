const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
fs.writeFileSync(require('path').join(require('os').tmpdir(),'feel-lab-all.js'),script);
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level=script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];
eval(sim+level+`
const base={jumpHeight:4,apexTime:0.40,fallMult:1.6,cutMult:5.5,terminal:20,walkSpeed:5.5,runSpeed:10,accelTime:0.45,stopTime:0.50,turnTime:0.35,airControl:0.65,coyoteMs:84,bufferMs:100,hardCut:false,
  walkLimit:50,upCost:20,downBoost:15,slideFriction:0.75,slideSteer:0.3,slideJump:1};
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false},o);
const at=(x)=>LINES.find(L=>x>=L.x1&&x<=L.x2&&Math.abs(L.y1)<1e-9&&L.sign===0);
console.log('lines', LINES.length, 'slopes', LINES.filter(L=>L.sign).map(L=>L.x1+'-'+L.x2+':'+L.deg.toFixed(1)).join(' '));
// flat regression
{ const p=makePlayer(3,START); step(p,base,I({jumpHeld:true,jumpPressed:true}),LINES,WORLD_W); let n=0; while(!p.grounded&&n++<999) step(p,base,I({jumpHeld:true}),LINES,WORLD_W); console.log('flat full jump', p.last.height.toFixed(2), p.last.ticks);
  const q=makePlayer(3,START); step(q,base,I({jumpPressed:true}),LINES,WORLD_W); n=0; while(!q.grounded&&n++<999) step(q,base,I(),LINES,WORLD_W); console.log('flat tap x5.5', q.last.height.toFixed(2)); }
// run right across all hills
function runAcross(P,label){
  const p=makePlayer(160,at(160)); let air=0, maxX=0, log=[], seen={};
  for(let t=0;t<1500;t++){ step(p,P,I({dir:1,run:true}),LINES,WORLD_W); if(!p.grounded) air++; maxX=Math.max(maxX,p.x);
    if(p.grounded&&p.line.sign!==0){const k=p.line.x1+(p.line.sign>0?'up':'dn'); seen[k]=Math.max(seen[k]||0,Math.abs(p.vx));}
  }
  console.log(label,'maxX',maxX.toFixed(1),'airTicks',air,'end x',p.x.toFixed(1), JSON.stringify(Object.fromEntries(Object.entries(seen).map(([k,v])=>[k,+v.toFixed(2)]))));
}
runAcross(base,'limit50');
runAcross({...base,walkLimit:65},'limit65');
// stand still on 45 slope
{ const L=LINES.find(L=>Math.abs(L.deg-45)<1&&L.sign>0); const p=makePlayer(L.x1+2,L); for(let t=0;t<120;t++) step(p,base,I(),LINES,WORLD_W); console.log('stand on 45: x',p.x.toFixed(3),'y',p.y.toFixed(3),'grounded',p.grounded); }
// jump while running uphill on 45: lands on slope/plateau, no fall-through
{ const L=LINES.find(L=>Math.abs(L.deg-45)<1&&L.sign>0); const p=makePlayer(L.x1-3,at(L.x1-3)); let jumped=false, minRel=9;
  for(let t=0;t<200;t++){ const press=!jumped&&p.grounded&&p.line===L&&p.x>L.x1+1; step(p,base,I({dir:1,run:true,jumpHeld:true,jumpPressed:press}),LINES,WORLD_W); if(press)jumped=true; if(jumped&&p.grounded){console.log('uphill jump landed x',p.x.toFixed(2),'y',p.y.toFixed(2),'deg',p.line.deg.toFixed(0),p.last);break;} } }
// slide: stand on steep downslope, measure; then kick jump
{ const L=LINES.find(L=>L.deg>60&&L.sign<0); const p=makePlayer(L.x1+0.2,L); for(let t=0;t<10;t++) step(p,base,I(),LINES,WORLD_W); console.log('slide 10 ticks: vx',p.vx.toFixed(2),'x',p.x.toFixed(2),'line deg',p.line.deg.toFixed(0));
  step(p,base,I({jumpPressed:true,jumpHeld:true}),LINES,WORLD_W); console.log('kick: vx',p.vx.toFixed(2),'vy',p.vy.toFixed(2),'grounded',p.grounded);
  const q=makePlayer(L.x1+0.2,L); for(let t=0;t<10;t++) step(q,{...base,slideJump:0},I(),LINES,WORLD_W); step(q,{...base,slideJump:0},I({jumpPressed:true,jumpHeld:true}),LINES,WORLD_W); console.log('no-jump mode still grounded',q.grounded);
  let n=0; while(n++<400) step(p,base,I(),LINES,WORLD_W); console.log('after slide settle x',p.x.toFixed(2),'y',p.y.toFixed(2),'vx',p.vx.toFixed(2),'grounded',p.grounded); }
// run LEFT down the 27 hill from plateau: stays grounded
{ const L=LINES.find(L=>Math.abs(L.deg-26.57)<1&&L.sign>0); const top=LINES.find(M=>M.sign===0&&Math.abs(M.x1-L.x2)<1e-9); const p=makePlayer(top.x2-0.5,top); let air=0; for(let t=0;t<200;t++){step(p,base,I({dir:-1,run:true}),LINES,WORLD_W); if(!p.grounded)air++;} console.log('run left down 27: air',air,'x',p.x.toFixed(1)); }
// ramp launch: custom lines
{ const A=mkLine(0,0,10,0), R=mkLine(10,0,13,3), G=mkLine(13,0,40,0)... 
`.replace("const A=mkLine(0,0,10,0), R=mkLine(10,0,13,3), G=mkLine(13,0,40,0)... ","const A=mkLine(0,0,10,0), R=mkLine(10,0,13,3), G=mkLine(14,0,40,0), LL=[A,R,G]; const p=makePlayer(2,A); let left=null; for(let t=0;t<300;t++){ const g0=p.grounded; step(p,base,I({dir:1,run:true}),LL,60); if(g0&&!p.grounded&&!left) left={x:+p.x.toFixed(2),vx:+p.vx.toFixed(2),vy:+p.vy.toFixed(2)}; if(left&&p.grounded){console.log('ramp launch',left,'landed x',p.x.toFixed(2),'on G',p.line===G);break;} } }"));
