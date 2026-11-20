# Find the skin-coloured patches (face and fists) in each frame and draw numbered contact sheets so the fists can be labelled.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, sys, numpy as np, scipy.ndimage as ndi
from PIL import Image, ImageDraw
CH=sys.argv[3] if len(sys.argv)>3 else ''
M=json.load(open(WORK+'/meta_'+CH+'.json' if CH else WORK+'/meta.json'))['frames']; A=Image.open(LAB+'/'+(CH or 'sprites')+'.png').convert('RGBA')
names=sys.argv[2].split(','); out={}
tiles=[]
for nm in names:
    for i,f in enumerate(M[nm]):
        c=A.crop((f[0],f[1],f[0]+f[2],f[1]+f[3])); a=np.asarray(c).astype(int); r,g,b,al=a[...,0],a[...,1],a[...,2],a[...,3]
        skin=(al>200)&(r>165)&(g>110)&(b>70)&(r-b>45)&(r>g)&(g>b)&(r<250)
        if CH=='ramorand': skin=(al>200)&(r>95)&(r<215)&(g>55)&(g<150)&(b>25)&(b<110)&(r-b>48)&(r>g)&(g>b)      # dark brown skin against grey armour and navy cloth
        L,n=ndi.label(ndi.binary_dilation(skin,iterations=1)); bl=[]
        for k,sl in enumerate(ndi.find_objects(L),1):
            m=(L[sl]==k)&skin[sl]
            if m.sum()<25: continue
            ys,xs=np.nonzero(m); bl.append(dict(x=round(float(sl[1].start+xs.mean()),1),y=round(float(sl[0].start+ys.mean()),1),n=int(m.sum()),top=int(sl[0].start+ys.min()),bot=int(sl[0].start+ys.max())))
        bl.sort(key=lambda d:d['y']); out[f'{nm}{i}']=bl
        bg=Image.new('RGBA',c.size,(70,70,90,255)); bg.alpha_composite(c); bg=bg.resize((c.size[0]*2,c.size[1]*2),Image.NEAREST); d=ImageDraw.Draw(bg)
        for k,q in enumerate(bl):
            d.ellipse((q['x']*2-5,q['y']*2-5,q['x']*2+5,q['y']*2+5),outline=(255,0,255,255),width=2); d.text((q['x']*2+8,q['y']*2-8),str(k),fill=(255,255,0,255))
        d.text((4,4),f'{nm}{i}',fill=(255,255,255,255)); tiles.append(bg)
W=sum(t.size[0]+6 for t in tiles); H=max(t.size[1] for t in tiles); sh=Image.new('RGB',(W,H),(60,0,0)); x=0
for t in tiles: sh.paste(t.convert('RGB'),(x,0)); x+=t.size[0]+6
sh.save(f'{WORK}/blobs_{CH}{sys.argv[1]}.png'); json.dump(out,open(f'{WORK}/blobs_{CH}{sys.argv[1]}.json','w')); print(sh.size, {k:len(v) for k,v in out.items()})
