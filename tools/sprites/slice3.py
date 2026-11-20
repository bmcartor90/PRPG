import json, numpy as np, scipy.ndimage as ndi
from PIL import Image, ImageDraw
import sys; path=sys.argv[1]; TAG=sys.argv[2]
raw=Image.open(path).convert('RGBA'); al=np.asarray(raw)[...,3]; im=Image.new('RGB',raw.size,(120,120,120)); im.paste(raw,mask=raw.split()[3]); a=np.asarray(im).astype(int); print('size',im.size)
bg=np.median(a[:30,:].reshape(-1,3),axis=0); print('background',bg, 'std', a[:30,:].reshape(-1,3).std(0).round(2))
fg=al>100
fg[780:,1270:]=False
big=ndi.binary_dilation(fg,iterations=3); lab,n=ndi.label(big)
out=[]
for i,sl in enumerate(ndi.find_objects(lab),1):
    m=(lab[sl]==i)&fg[sl]
    if m.sum()<1500: continue
    ys,xs=np.nonzero(m)
    f=dict(x0=int(sl[1].start+xs.min()),y0=int(sl[0].start+ys.min()),x1=int(sl[1].start+xs.max()+1),y1=int(sl[0].start+ys.max()+1),cx=float(sl[1].start+xs.mean()),area=int(m.sum()))
    # skin blobs: face and hands
    c=a[f['y0']:f['y1'],f['x0']:f['x1']]; r,g,b=c[...,0],c[...,1],c[...,2]
    skin=(r>200)&(g>150)&(g<235)&(b>100)&(b<200)&(r-b>40)
    L,k=ndi.label(skin); blobs=[]
    for j,s2 in enumerate(ndi.find_objects(L),1):
        mm=L[s2]==j
        if mm.sum()<25: continue
        yy,xx=np.nonzero(mm); blobs.append(dict(x=int(f['x0']+s2[1].start+xx.mean()),y=int(f['y0']+s2[0].start+yy.mean()),top=int(f['y0']+s2[0].start),w=int(xx.max()-xx.min()+1),h=int(yy.max()-yy.min()+1),n=int(mm.sum())))
    f['skin']=blobs; out.append(f)
out.sort(key=lambda f:(f['y0']+f['y1'])/2)
rows=[[],[]]
for f in out: rows[0 if (f['y0']+f['y1'])/2<432 else 1].append(f)
for r in rows: r.sort(key=lambda f:f['x0'])
dbg=im.copy(); d=ImageDraw.Draw(dbg)
for ri,r in enumerate(rows):
    print('row',ri,'frames',len(r))
    for fi,f in enumerate(r):
        d.rectangle([f['x0'],f['y0'],f['x1'],f['y1']],outline=(255,0,0)); d.text((f['x0']+2,f['y0']-12),f'{ri}.{fi}',fill=(255,0,0))
        for b in f['skin']: d.ellipse([b['x']-4,b['y']-4,b['x']+4,b['y']+4],outline=(0,180,0),width=2)
        print(f"  {ri}.{fi} box {f['x1']-f['x0']}x{f['y1']-f['y0']} at ({f['x0']},{f['y0']}) skin:", [(b['x'],b['y'],b['w'],b['h'],b['n']) for b in f['skin']])
# gaps between neighbours in a row (bounding boxes) and true pixel gaps
for ri,r in enumerate(rows):
    gaps=[]
    for i in range(len(r)-1):
        A,B=r[i],r[i+1]
        ma=np.zeros_like(fg); ma[A['y0']:A['y1'],A['x0']:A['x1']]=fg[A['y0']:A['y1'],A['x0']:A['x1']]
        dist=ndi.distance_transform_edt(~ma)
        mb=fg[B['y0']:B['y1'],B['x0']:B['x1']]
        gaps.append(int(dist[B['y0']:B['y1'],B['x0']:B['x1']][mb].min()))
    print('row',ri,'nearest pixel gap between neighbours:',gaps)
dbg.save(f'sprites/dbg_{TAG}.png'); json.dump(rows,open(f'sprites/boxes_{TAG}.json','w'))
