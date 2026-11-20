# Find every figure on each Nina sheet (all have transparent backgrounds) and group them into rows.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
SHEETS=dict(walk_run='65975ccc', combat='72dd2f7a', mdraw='0c517fb8', rope_wall='65ec7e6d', ledge='b04cdec0', action='4d7d789d')
out={}
for tag,n in SHEETS.items():
    a=np.asarray(Image.open(SRC(n)).convert('RGBA')); fg=a[...,3]>100; fg[780:,1270:]=False
    l,k=ndi.label(ndi.binary_dilation(fg,iterations=2)); bx=[]
    for i,sl in enumerate(ndi.find_objects(l),1):
        m=(l[sl]==i)&fg[sl]
        if m.sum()<5000: continue
        ys,xs=np.nonzero(m); bx.append(dict(x0=int(sl[1].start+xs.min()),y0=int(sl[0].start+ys.min()),x1=int(sl[1].start+xs.max()+1),y1=int(sl[0].start+ys.max()+1),cx=float(sl[1].start+xs.mean()),area=int(m.sum())))
    bx.sort(key=lambda b:(b['y0']+b['y1'])/2); rows=[[bx[0]]]
    for b in bx[1:]:
        last=rows[-1][-1]
        if (b['y0']+b['y1'])/2-(last['y0']+last['y1'])/2>110: rows.append([b])
        else: rows[-1].append(b)
    rows=[sorted(r,key=lambda b:b['x0']) for r in rows]; out[tag]=rows
    print(tag,'transparent',round((a[...,3]<10).mean(),2),[len(r) for r in rows],[[(b['x1']-b['x0'],b['y1']-b['y0']) for b in r] for r in rows])
json.dump(out,open(WORK+'/boxes_nina.json','w'))
