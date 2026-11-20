const fs=require('fs');
const html=fs.readFileSync(require('path').join(__dirname,'../../feel-lab/index.html'),'utf8');
const script=html.split('<script>')[1].split('</script>')[0];
fs.writeFileSync(require('path').join(__dirname,'work/all.js'),script);
const ids=[...html.matchAll(/id="([^"]+)"/g)].map(m=>m[1]);
const handlers={}; const els={};
function mkEl(id){
  const el={id,style:{},children:[],listeners:{},value:'',textContent:'',hidden:false,checked:true,clientWidth:1000,clientHeight:400,width:0,height:0,tagName:'DIV',
    addEventListener(t,f){(this.listeners[t]=this.listeners[t]||[]).push(f)},
    appendChild(c){this.children.push(c); if(c.id) els[c.id]=c; return c}, append(...c){c.forEach(x=>this.appendChild(x))},
    blur(){},focus(){},select(){},
    getContext(){return new Proxy({}, {get:(t,k)=>k in t?t[k]:(()=>{}), set:(t,k,v)=>{t[k]=v;return true}})}};
  return new Proxy(el,{set(t,k,v){t[k]=v; if(k==='id') els[v]=p; return true}}); var p;
}
const document={activeElement:null, addEventListener(){},
  getElementById(id){ if(!els[id]){ if(!ids.includes(id)) throw new Error('missing element #'+id); els[id]=mkEl(id);} return els[id]; },
  createElement(tag){ const e=mkEl(''); e.tagName=tag.toUpperCase(); const px=new Proxy(e,{set(t,k,v){t[k]=v; if(k==='id') els[v]=px; return true}}); return px; },
  createTextNode(t){return {text:t}}};
const window={devicePixelRatio:1,addEventListener(t,f){(handlers[t]=handlers[t]||[]).push(f)}};
let raf=null; const requestAnimationFrame=f=>{raf=f};
const performance={now:()=>0}; const navigator={}; const localStorage={getItem(){return JSON.stringify({jumpHeight:4,stopTime:0.5,hardCut:false})},setItem(){}};
class Image{ set src(v){ this._src=v; if(this.onload) this.onload(); } }
const ResizeObserver=undefined; window.ResizeObserver=undefined;
eval(script+`
;let now=0; function run(n){ for(let i=0;i<n;i++){ now+=1000/60; raf(now);} }
function key(type,code){ (handlers[type]||[]).forEach(f=>f({code,preventDefault(){},repeat:false})); }
run(5);
console.log('loaded; P.stopTime',P.stopTime,'walkLimit',P.walkLimit,'slideJump',P.slideJump,'| values:',els['values'].value.slice(-120));
key('keydown','Digit4'); run(2); console.log('warp 4 -> x',player.x,'section',currentSection);
key('keydown','ArrowRight'); key('keydown','ShiftLeft'); run(8); console.log('keys readout:',els['r-keys'].textContent); run(592); key('keyup','ArrowRight');
console.log('after 10s right: x',player.x.toFixed(1),'| now:',els['r-now'].textContent);
key('keydown','KeyT'); run(1); console.log('R -> x',player.x);
els['go-1'].listeners.click[0]({currentTarget:els['go-1']}); run(1); console.log('button gaps -> x',player.x);
key('keydown','Space'); run(3); key('keyup','Space'); run(60); console.log('last jump:',els['r-last'].textContent);
els['p-slideJump'].value='0'; els['p-slideJump'].listeners.change[0](); console.log('slideJump now',P.slideJump);
els['p-walkLimit'].value='65'; els['p-walkLimit'].listeners.input[0](); console.log('walkLimit out:',els['o-walkLimit'].textContent);

console.log('binds default:', ['left','right','jump','run'].map(a=>els['bind-'+a].textContent).join(' | '));
els['bind-jump'].listeners.click[0]({currentTarget:els['bind-jump']}); key('keydown','KeyT'); console.log('reserved note:',els['bind-note'].textContent,'| still capturing',capturing);
key('keydown','KeyW'); key('keyup','KeyW');
els['bind-run'].listeners.click[0]({currentTarget:els['bind-run']}); key('keydown','Space'); key('keyup','Space');
console.log('binds now:', ['left','right','jump','run'].map(a=>els['bind-'+a].textContent).join(' | '), '| note:', els['bind-note'].textContent);
warp(0); key('keydown','KeyD'); key('keydown','Space'); run(60); console.log('D+Space 1s: vx',player.vx.toFixed(2),'(run speed',P.runSpeed+')');
key('keydown','KeyW'); run(2); console.log('W -> airborne',!player.grounded); key('keyup','KeyW'); key('keyup','KeyD'); key('keyup','Space');
key('keydown','ArrowUp'); run(40); console.log('old jump key ArrowUp ignored: grounded after 40 ticks', player.grounded, 'hint:', els['hint'].textContent);
key('keyup','ArrowUp'); els['p-coyoteMs'].value='150'; els['p-coyoteMs'].listeners.input[0](); els['reset-all'].listeners.click[0]({currentTarget:els['reset-all']}); run(1);
console.log('after reset: coyote',P.coyoteMs,'cut',P.cutMult,'stop',P.stopTime,'walkLimit',P.walkLimit,'slideJump',P.slideJump,'x',player.x,'| binds:',els['bind-jump'].textContent,'|',els['bind-run'].textContent);
/* lock test */ for (const h of [4,5,6,6.5,7,8]){ els['p-jumpHeight'].value=String(h); els['p-jumpHeight'].listeners.input[0](); const d=derive(P); console.log('height',h,'apex',P.apexTime.toFixed(3),'gravity',d.g.toFixed(1),'airtime',d.air.toFixed(3),'reach run',(d.reachRun+P.footWidth).toFixed(2)); }
els['p-jumpHeight'].value='4'; els['p-jumpHeight'].listeners.input[0](); console.log('back to 4: apex',P.apexTime.toFixed(3));
console.log('sprite ready',spriteReady,'frames',Object.entries(SPRITE.frames).map(([n,f])=>n+':'+f.length).join(' '));
warp(0); const seen=new Set(); const nm=f=>{for(const [n,a] of Object.entries(SPRITE.frames)){const i=a.indexOf(f); if(i>=0) return n+i;} return '?'};
seen.add(nm(pickFrame(player))); key('keydown','ArrowRight'); for(let i=0;i<50;i++){run(1);seen.add(nm(pickFrame(player)));} key('keydown','ShiftLeft'); for(let i=0;i<90;i++){run(1);seen.add(nm(pickFrame(player)));}
key('keydown','Space'); for(let i=0;i<50;i++){run(1);seen.add(nm(pickFrame(player)));} key('keyup','Space'); key('keyup','ArrowRight'); key('keyup','ShiftLeft'); for(let i=0;i<60;i++){run(1);seen.add(nm(pickFrame(player)));}
key('keydown','ArrowLeft'); run(3); console.log('face after left',anim.face); key('keyup','ArrowLeft');
warp(3); player.x=219; player.line=LINES.find(L=>L.deg>60&&L.sign<0); player.y=yAt(player.line,219); for(let i=0;i<20;i++){run(1);seen.add(nm(pickFrame(player)));}
console.log('frames used:',[...seen].sort().join(' '));
`);
