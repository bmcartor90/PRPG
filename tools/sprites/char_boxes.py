# Find every figure on each of a character's sheets and group them into rows. Usage: char_boxes.py ramorand
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, sys, numpy as np, scipy.ndimage as ndi
from PIL import Image
CH=sys.argv[1]
SHEETS=dict(ramorand=dict(walk_run='7997c641', action='9bc3dcb6', ledge='f8aa2c67', rope_wall='b64f54a6', combat='5e5e0121'))[CH]
out={}
for tag,n in SHEETS.items():
    a=np.asarray(Image.open(SRC(n)).convert('RGBA'))
    if (a[...,3]<10).mean()<0.05:
        # opaque background: flood in from the border through pixels close to the corner colour
        bg=a[5,5,:3].astype(int); close=np.abs(a[...,:3].astype(int)-bg).sum(-1)<=18; lab,k=ndi.label(close)
        edge=set(lab[0])|set(lab[-1])|set(lab[:,0])|set(lab[:,-1]); edge.discard(0); fg=~np.isin(lab,list(edge))
        np.save(f'{WORK}/alpha_{CH}_{tag}.npy',(fg*255).astype(np.uint8))
    else: fg=a[...,3]>100
    fg=fg.copy(); fg[780:,1270:]=False
    l,k=ndi.label(ndi.binary_dilation(fg,iterations=2)); bx=[]
    for i,sl in enumerate(ndi.find_objects(l),1):
        m=(l[sl]==i)&fg[sl]
        if m.sum()<5000: continue
        ys,xs=np.nonzero(m); bx.append(dict(x0=int(sl[1].start+xs.min()),y0=int(sl[0].start+ys.min()),x1=int(sl[1].start+xs.max()+1),y1=int(sl[0].start+ys.max()+1),cx=float(sl[1].start+xs.mean()),area=int(m.sum())))
    rows=[sorted([b for b in bx if (b['y0']+b['y1'])/2<432],key=lambda b:b['x0']), sorted([b for b in bx if (b['y0']+b['y1'])/2>=432],key=lambda b:b['x0'])]
    out[tag]=rows; print(tag,[len(r) for r in rows],[[(b['x1']-b['x0'],b['y1']-b['y0']) for b in r] for r in rows])
json.dump(out,open(f'{WORK}/boxes_{CH}.json','w'))
