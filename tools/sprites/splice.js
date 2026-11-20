// NOT an art tool: splices the movement simulation into the lab. The simulation belongs to the design conversation; do not run this from art work.
const fs=require('fs');
const f=require('path').join(__dirname,'../../feel-lab/index.html');
let h=fs.readFileSync(f,'utf8');
const sim=fs.readFileSync(require('path').join(__dirname,'work/sim.js'),'utf8').trimEnd();
const a=h.indexOf('/*SIM-START*/'), b=h.indexOf('/*SIM-END*/')+'/*SIM-END*/'.length;
if(a<0||b<a) throw new Error('markers');
h=h.slice(0,a)+sim+h.slice(b);
fs.writeFileSync(f,h);
console.log('spliced', h.length);
