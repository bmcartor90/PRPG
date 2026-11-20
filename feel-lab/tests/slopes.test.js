const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
eval(sim+`
const base={jumpHeight:4,apexTime:0.40,fallMult:1.6,cutMult:3,terminal:20,walkSpeed:5.5,runSpeed:10,accelTime:0.45,stopTime:0.30,turnTime:0.35,airControl:0.65,coyoteMs:80,bufferMs:100,hardCut:false,
  walkLimit:50,upCost:20,downBoost:15,slideFriction:0.75,slideSteer:0.3,slideJump:1,footWidth:0.6};
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false},o);
// 1) walk off a ledge: when do you leave the ground?
for (const fw of [0,0.6]){ const A=mkLine(0,0,10,0); const p=makePlayer(8,A); const P={...base,footWidth:fw}; let leftAt=null;
  for(let t=0;t<200&&leftAt===null;t++){ step(p,P,I({dir:1}),[A],60); if(!p.grounded) leftAt=p.x; } console.log('footWidth',fw,'left the ledge with centre at x =',leftAt.toFixed(2),'(ledge ends at 10)'); }
// 2) jump a gap: find the widest gap cleared when jumping at full run from the very edge, for point feet and wide feet
function widest(fw){ const P={...base,footWidth:fw}; let best=0;
  for(let gap=6;gap<=9.5;gap+=0.05){ const A=mkLine(0,0,20,0), Bn=mkLine(20+gap,0,60,0); const p=makePlayer(5,A); let jumped=false, ok=false;
    for(let t=0;t<400;t++){ const press=!jumped&&p.x>=20+fw/2-0.17; step(p,P,I({dir:1,run:true,jumpHeld:true,jumpPressed:press}),[A,Bn],80); if(press&&!p.grounded) jumped=true; if(jumped&&p.grounded){ ok=(p.line===Bn); break;} if(p.y<-3) break; }
    if(ok) best=gap; }
  return best.toFixed(2); }
console.log('widest gap cleared at a run: point feet',widest(0),'| foot width 0.6',widest(0.6));
// 3) a gap narrower than the footing is walked straight across
{ const A=mkLine(0,0,10,0), Bn=mkLine(10.4,0,20,0); const p=makePlayer(8,A); let air=0; for(let t=0;t<120;t++){ step(p,base,I({dir:1}),[A,Bn],60); if(!p.grounded) air++; } console.log('0.4 gap with 0.6 feet: airborne ticks',air,'x',p.x.toFixed(1),'on B',p.line===Bn); }
// 4) stand in the overhang, then jump: full height
{ const A=mkLine(0,0,10,0); const p=makePlayer(10.2,A); p.x=10.2; for(let t=0;t<30;t++) step(p,base,I(),[A],60); const g=p.grounded; step(p,base,I({jumpPressed:true,jumpHeld:true}),[A],60); let n=0; while(!p.grounded&&n++<200&&p.y>-3) step(p,base,I({jumpHeld:true}),[A],60); console.log('standing 0.2 past the edge: grounded',g,'| jump from there height',p.last&&p.last.height.toFixed(2)); }
`);
