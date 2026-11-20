const fs=require('fs');
const f='/home/claude/feel-lab/index.html';
let h=fs.readFileSync(f,'utf8');
const sim=fs.readFileSync('/home/claude/scratch/sim.js','utf8').trimEnd();
const a=h.indexOf('/*SIM-START*/'), b=h.indexOf('/*SIM-END*/')+'/*SIM-END*/'.length;
if(a<0||b<a) throw new Error('markers');
h=h.slice(0,a)+sim+h.slice(b);
fs.writeFileSync(f,h);
console.log('spliced', h.length);
