# Contact sheet with a coordinate grid, for reading fist and grip positions by eye (frame pixels; lines every 25, labels every 50).
# Skin patches found by colour are marked with their centre. Usage: grid.py <character|lucca> anim,anim,... out.png [scale]
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, numpy as np, scipy.ndimage as ndi
from PIL import Image, ImageDraw
CH=sys.argv[1]; names=sys.argv[2].split(','); OUT=sys.argv[3]; K=float(sys.argv[4]) if len(sys.argv)>4 else 1.5
meta=json.load(open(WORK+('/meta.json' if CH=='lucca' else f'/meta_{CH}.json')))['frames']
A=Image.open(LAB+'/'+('sprites' if CH=='lucca' else CH)+'.png').convert('RGBA')
tiles=[]
for nm in names:
    for i,f in enumerate(meta[nm]):
        c=A.crop((f[0],f[1],f[0]+f[2],f[1]+f[3])); a=np.asarray(c).astype(int); r,g,b,al=[a[...,j] for j in range(4)]
        skin=(al>200)&(r>165)&(g>110)&(b>70)&(r-b>45)&(r>g)&(g>b)&(r<250)
        if CH=='ramorand': skin=(al>200)&(r>95)&(r<215)&(g>55)&(g<150)&(b>25)&(b<110)&(r-b>48)&(r>g)&(g>b)
        bg=Image.new('RGBA',c.size,(70,70,90,255)); bg.alpha_composite(c); bg=bg.resize((round(c.size[0]*K),round(c.size[1]*K)),Image.LANCZOS); d=ImageDraw.Draw(bg)
        for x in range(0,c.size[0],25): d.line([(x*K,0),(x*K,bg.size[1])],fill=(0,255,255,90) if x%50 else (0,255,255,170))
        for y in range(0,c.size[1],25): d.line([(0,y*K),(bg.size[0],y*K)],fill=(0,255,255,90) if y%50 else (0,255,255,170))
        for x in range(0,c.size[0],50): d.text((x*K+2,2),str(x),fill=(255,255,0,255))
        for y in range(50,c.size[1],50): d.text((2,y*K+2),str(y),fill=(255,255,0,255))
        L,n=ndi.label(ndi.binary_dilation(skin,iterations=1))
        for k,sl in enumerate(ndi.find_objects(L),1):
            m=(L[sl]==k)&skin[sl]
            if m.sum()<25: continue
            ys,xs=np.nonzero(m); x,y=sl[1].start+xs.mean(),sl[0].start+ys.mean()
            d.ellipse((x*K-4,y*K-4,x*K+4,y*K+4),outline=(255,0,255,255),width=2); d.text((x*K+6,y*K-6),f'{x:.0f},{y:.0f}',fill=(255,120,255,255))
        if len(f)>6: d.ellipse((f[6]*K-5,f[7]*K-5,f[6]*K+5,f[7]*K+5),outline=(0,255,0,255),width=2)
        d.text((4,bg.size[1]-14),f'{nm}{i}  {f[2]}x{f[3]}',fill=(255,255,255,255)); tiles.append(bg)
cols=int(sys.argv[5]) if len(sys.argv)>5 else 6; rows=[tiles[i:i+cols] for i in range(0,len(tiles),cols)]
W=max(sum(t.size[0]+6 for t in r) for r in rows); H=sum(max(t.size[1] for t in r)+6 for r in rows)
sh=Image.new('RGB',(W,H),(30,0,0)); y=0
for r in rows:
    x=0
    for t in r: sh.paste(t.convert('RGB'),(x,y)); x+=t.size[0]+6
    y+=max(t.size[1] for t in r)+6
sh.save(OUT); print(sh.size)
