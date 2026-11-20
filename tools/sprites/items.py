import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
STAND=307.0   # Lucca's standing height in pixels: every item is scaled to his pixels per tile, whoever holds it
# Per sheet: name, box index, target length of the long side as a share of his height, grip point as a share of the box (x, y).
# Optional fifth field: a nudge (x0, y0, x1, y1) to the frame's box, or 'frozen' to paste work/frozen_<name>.png as is.
# The lightning sword was made with a semi-transparent treatment whose recipe was lost, so its pixels are kept rather than rebuilt.
# Ramorand's numbers were recovered from the atlas as first built (it was cut inline then), so they reproduce it exactly.
SETS=[('b4e1e56c','boxes_items.json',[('sword',0,0.60,(0.15,0.50)),('sheath',1,0.52,(0.5,0.5)),('shield',2,0.42,(0.5,0.5)),('shieldBack',3,0.42,(0.5,0.5)),('shieldEdge',4,0.42,(0.5,0.5)),('staff',5,1.10,(0.5,0.5))]),
      ('61b27d1a','boxes_items_ramorand.json',[('r_sword',0,0.621,(0.1497,0.497),(-1,0,0,0)),('r_sheath',1,0.52,(0.5,0.501)),('r_swordLightning',2,0.609,(0.153,0.506),'frozen'),
        ('r_shield',3,0.56,(0.499,0.45),(0,-1,0,0)),('r_shieldBack',4,0.56,(0.5,0.45)),('r_shieldEdge',5,0.56,(0.506,0.45),(-1,0,0,0))])]
PAD=3; rows=[]; meta={}
for sid,boxf,spec in SETS:
  a=np.asarray(Image.open(SRC(sid)).convert('RGBA')); B=json.load(open(WORK+'/'+boxf)); ims=[]
  for name,i,share,grip,*opt in spec:
    f=dict(B[i]); m=8; nudge=opt[0] if opt else (0,0,0,0)
    if nudge=='frozen': nudge=(0,0,0,0)
    for kk,dd in zip(('x0','y0','x1','y1'),nudge): f[kk]+=dd
    c=a[f['y0']-m:f['y1']+m, f['x0']-m:f['x1']+m].copy()
    al=c[...,3]>100; c[...,3]=np.where(al,255,0)
    im=Image.fromarray(c,'RGBA'); g0=B[i]; k=share*STAND/max(g0['x1']-g0['x0'],g0['y1']-g0['y0'])   # scale and grip from the un-nudged box
    im2=im.convert('RGBa').resize((max(1,round(im.size[0]*k)),max(1,round(im.size[1]*k))),Image.LANCZOS).convert('RGBA')
    al2=np.asarray(im2)[...,3]>40; ring=ndi.binary_dilation(al2,iterations=3); soft=ndi.gaussian_filter(ring.astype(float),0.9)
    halo=np.zeros(al2.shape+(4,),np.uint8); halo[...,:3]=205; halo[...,3]=(np.clip(soft,0,1)*255).astype(np.uint8)
    out=Image.fromarray(halo,'RGBA'); out.alpha_composite(im2)
    if opt and opt[0]=='frozen': out=Image.open(f'{WORK}/frozen_{name}.png').convert('RGBA')
    gx=(m+(g0['x1']-g0['x0'])*grip[0])*k; gy=(m+(g0['y1']-g0['y0'])*grip[1])*k
    ims.append((name,out,gx,gy))
  rows.append(ims)
Wd=max(sum(o.size[0]+PAD for _,o,_,_ in r)+PAD for r in rows); y=PAD; place=[]
for r in rows:
    x=PAD
    for name,o,gx,gy in r: place.append((o,x,y)); meta[name]=[x,y,o.size[0],o.size[1],round(gx,1),round(gy,1)]; x+=o.size[0]+PAD
    y+=max(o.size[1] for _,o,_,_ in r)+2*PAD
atlas=Image.new('RGBA',(Wd,y-PAD),(0,0,0,0))
for o,x,yy in place: atlas.paste(o,(x,yy))
atlas.save(LAB+'/items.png',optimize=True)
json.dump(meta,open(WORK+'/items.json','w'),separators=(',',':')); print(atlas.size, json.dumps(meta,separators=(',',':')))
prev=Image.new('RGBA',atlas.size,(0,0,0,255)); prev.alpha_composite(atlas); prev.convert('RGB').save(WORK+'/items_prev.png')
