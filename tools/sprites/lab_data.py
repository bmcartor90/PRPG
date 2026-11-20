# Rewrite the lab's four sprite-data blocks (SPRITES frames, FISTS, DRAW_POSE, ITEMS) in feel-lab/index.html from the files in work/.
# Touches only those lines. Run after any atlas or label change; build.sh runs it.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, re
J=lambda o: json.dumps(o,separators=(',',':'))
CH=[('lucca','meta.json','fists.json'),('nina','meta_nina.json','fists_nina.json'),('ramorand','meta_ramorand.json','fists_ramorand.json')]
f=LAB+'/index.html'; lines=open(f,encoding='utf-8').read().split('\n')
def at(prefix): return next(i for i,l in enumerate(lines) if l.startswith(prefix))
s=at('const SPRITES = {')
for k,(c,mf,ff) in enumerate(CH):
    i=s+1+k; m=json.load(open(f'{WORK}/{mf}')); tail=lines[i][lines[i].rindex('}}, {'):]   # keep the per-character options after the frames
    lines[i]=f'  {c}:Object.assign('+J(m)[:-2]+tail
fi=at('const FISTS = {'); dp=at('const DRAW_POSE = {')
for k,(c,mf,ff) in enumerate(CH):
    x=json.load(open(f'{WORK}/{ff}')); end='}};' if k==2 else '},'
    lines[fi+k]=('const FISTS = {' if k==0 else '  ')+f'{c}:'+J(x['fists'])[:-1]+end
    lines[dp+k]=('const DRAW_POSE = {' if k==0 else '  ')+f'{c}:'+J(x['pose'])[:-1]+end
it=at('const ITEMS = {'); lines[it]='const ITEMS = '+J(json.load(open(WORK+'/items.json')))+';'
open(f,'w',encoding='utf-8').write('\n'.join(lines)); print('lab data written')
