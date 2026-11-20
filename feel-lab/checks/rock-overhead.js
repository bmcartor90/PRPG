// Floating rock: undersides, faces with a bottom, overhangs, and grabbing a top corner. Prints measurements; read them against the notes.
const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'..','index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
const sim=script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level=script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];
const P=JSON.parse(fs.readFileSync(require('path').join(__dirname,'..','traces','_world.json'),'utf8')).params;
new Function('P', sim+level+`
const I=(o)=>Object.assign({dir:0,run:false,jumpHeld:false,jumpPressed:false,up:false,down:false,grab:false,swapHand:-1},o);
const at=(x)=>{ let best=null; for(const L of LINES) if(L.solid&&!L.slab&&x>=L.x1&&x<=L.x2&&(!best||yAt(L,x)>yAt(best,x))) best=L; return makePlayer(x,best); };
let pj=false; const S=(p,i)=>{ const j=!!i.jump; step(p,P,I({dir:i.dir||0,run:!!i.run,jumpHeld:j,jumpPressed:j&&!pj,up:!!i.up,down:!!i.down,grab:!!i.grab}),LINES,WORLD_W,CLIMBS,CLIFFS,ROOFS); pj=j; };
const go=(p,seq)=>{ let peak=p.y; for(const [n,i] of seq) for(let t=0;t<n;t++){ S(p,i); if(p.y>peak) peak=p.y; } p.peak=peak; return p; };
const f=(p)=>'x '+p.x.toFixed(2)+' y '+p.y.toFixed(2)+' '+p.mode+(p.grounded?' grounded':' air');
console.log('faces', SLAB_FACES.length, 'undersides', ROOFS.length);
let p;
p=go(at(353),[[2,{}],[50,{jump:true}],[40,{}]]); console.log('1 jump under the underside at 3     peak feet height', p.peak.toFixed(2), '(expect 1.10 = 3 - 1.9)', f(p));
p=go(at(347),[[2,{}],[50,{jump:true}],[40,{}]]); console.log('2 the same jump in the open         peak feet height', p.peak.toFixed(2), '(expect 4.00)');
p=go(at(346),[[120,{dir:1}]]); console.log('3 walk under the rock at 3          ', f(p), '(expect to pass: x well past 350)');
pj=false; p=go(at(347),[[2,{}],[26,{dir:1,jump:true}],[60,{dir:1}]]); console.log('4 jump into its left face           ', f(p), '(expect stopped at x 349.70 while his feet were between 1.1 and 4.5, then on under it)');
pj=false; p=go(at(348.6),[[2,{}],[40,{dir:1,jump:true,grab:true}]]); console.log('5 jump beside it holding grab       ', f(p), '(expect hang at the corner 350, 4.5)');
go(p,[[80,{grab:true,up:true}],[20,{}]]); console.log('  then pull up                      ', f(p), '(expect y 4.50 grounded on top)');
p=go(at(372),[[200,{dir:-1}]]); console.log('6 walk left under the overhang      ', f(p), '(expect stopped near x 363.37, where it drops to his height)');
pj=false; p=go(at(368),[[2,{}],[50,{jump:true}],[40,{}]]); console.log('7 jump under the overhang at 368    peak feet height', p.peak.toFixed(2), '(underside there is about 3.64 at the low side of his body, so expect about 1.74)');
p=go(at(358),[[120,{dir:1}]]); console.log('8 walk right into the overhang face ', f(p), '(expect x 361.70: the face runs from 1.5 up to 6)');
p=go(at(372),[[160,{dir:1}]]); console.log('9 walk under the rock at 2.5        ', f(p), '(expect to pass: x past 380)');
pj=false; p=go(at(355),[[30,{dir:1,run:true}],[60,{dir:1,run:true,down:true}]]); console.log('10 dodge slide under the rock at 3   ', f(p), '(fine: it is higher than he stands)');
`)(P);
