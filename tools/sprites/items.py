import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
src=Image.open('/mnt/user-data/uploads/b4e1e56c-image.png').convert('RGBA'); a=np.asarray(src)
B=json.load(open('sprites/boxes_items.json'))
STAND=307.0
# name, box index, target length of the long side as a share of his height, grip point as a share of the box (x, y)
SPEC=[('sword',0,0.60,(0.15,0.50)),('sheath',1,0.52,(0.5,0.5)),('shield',2,0.42,(0.5,0.5)),('shieldBack',3,0.42,(0.5,0.5)),('shieldEdge',4,0.42,(0.5,0.5)),('staff',5,1.10,(0.5,0.5))]
PAD=3; ims=[]; meta={}
for name,i,share,grip in SPEC:
    f=B[i]; m=8
    c=a[f['y0']-m:f['y1']+m, f['x0']-m:f['x1']+m].copy()
    al=c[...,3]>100; c[...,3]=np.where(al,255,0)
    im=Image.fromarray(c,'RGBA'); k=share*STAND/max(f['x1']-f['x0'],f['y1']-f['y0'])
    im2=im.convert('RGBa').resize((max(1,round(im.size[0]*k)),max(1,round(im.size[1]*k))),Image.LANCZOS).convert('RGBA')
    al2=np.asarray(im2)[...,3]>40; ring=ndi.binary_dilation(al2,iterations=3); soft=ndi.gaussian_filter(ring.astype(float),0.9)
    halo=np.zeros(al2.shape+(4,),np.uint8); halo[...,:3]=205; halo[...,3]=(np.clip(soft,0,1)*255).astype(np.uint8)
    out=Image.fromarray(halo,'RGBA'); out.alpha_composite(im2)
    gx=(m+(f['x1']-f['x0'])*grip[0])*k; gy=(m+(f['y1']-f['y0'])*grip[1])*k
    ims.append((name,out,gx,gy))
W=sum(o.size[0]+PAD for _,o,_,_ in ims)+PAD; H=max(o.size[1] for _,o,_,_ in ims)+2*PAD
atlas=Image.new('RGBA',(W,H),(0,0,0,0)); x=PAD
for name,o,gx,gy in ims:
    atlas.paste(o,(x,PAD)); meta[name]=[x,PAD,o.size[0],o.size[1],round(gx,1),round(gy,1)]; x+=o.size[0]+PAD
atlas.save('/home/claude/feel-lab/items.png',optimize=True)
json.dump(meta,open('sprites/items.json','w'),separators=(',',':')); print(atlas.size, json.dumps(meta,separators=(',',':')))
prev=Image.new('RGBA',atlas.size,(0,0,0,255)); prev.alpha_composite(atlas); prev.convert('RGB').save('sprites/items_prev.png')
