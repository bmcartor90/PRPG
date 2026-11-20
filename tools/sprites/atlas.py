import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
B=json.load(open('sprites/boxes.json'))
B['ledge']=json.load(open('sprites/boxes_ledge.json'))
B['climb']=json.load(open('sprites/boxes_climb.json'))
B['empty']=json.load(open('sprites/boxes_empty.json'))
B['mdraw']=json.load(open('sprites/boxes_mdraw.json'))
B['opp']=json.load(open('sprites/boxes_opp.json'))
src=dict(action='/mnt/user-data/uploads/9aa0aed0-image.png',move='/mnt/user-data/uploads/67ddc9c4-image.png')
arr={k:np.asarray(Image.open(v).convert('RGB')) for k,v in src.items()}
# The ledge-and-lunge sheet arrived with a transparent background, so its own alpha is the cut-out.
ALPHAS={}
for _k,_p in dict(ledge='/mnt/user-data/uploads/ebd959ba-image.png', climb='/mnt/user-data/uploads/31b2f36f-image.png').items():
    _im=Image.open(_p).convert('RGBA'); arr[_k]=np.asarray(_im)[...,:3]; ALPHAS[_k]=np.asarray(_im)[...,3]
# The empty-handed draw sheet has a black background; its cut-out mask comes from empty_boxes.py.
_im=Image.open('/mnt/user-data/uploads/51f22a66-image.png').convert('RGB'); arr['empty']=np.asarray(_im); ALPHAS['empty']=np.load('sprites/alpha_empty.npy')
for _k,_p in dict(mdraw='/mnt/user-data/uploads/442ad3cf-image.png', opp='/mnt/user-data/uploads/1dce5bb6-image.png').items():
    _im=Image.open(_p).convert('RGBA'); arr[_k]=np.asarray(_im)[...,:3]; ALPHAS[_k]=np.asarray(_im)[...,3]
REL={'ledge':1.05,'empty':0.97,'mdraw':0.97,'opp':0.955}      # this sheet drew him about 5% smaller than the first two
# Where his hands are in the hanging and pull-up frames (sheet pixels), so the sprite can be pinned to the ledge by the hands.
HANDS={('climb',0,0):(165,191),('climb',0,1):(380,192),('climb',0,2):(593,164),('climb',0,3):(788,208),('climb',0,4):(1003,219),('climb',0,5):(1232,210),('climb',0,6):(1447,252),
       ('ledge',0,0):(126,131),('ledge',0,1):(330,131),('ledge',0,2):(543,131),('ledge',0,3):(766,207),('ledge',0,4):(1029,240),('ledge',1,0):(88,481)}
S=1.0; PAD=2
anims=dict(walk=('move',0,range(6)), run=('move',1,range(6)), jump=('action',0,range(7)), slide=('action',1,range(2,5)), crouch=('action',1,range(0,2)),
           hang=('ledge',0,[0,1]), pull=('ledge',0,[2,3,4,5]), hang1=('ledge',1,[0]), lunge=('ledge',1,[2,3]), prone=('ledge',1,[4]), getup=('ledge',1,[5,6]),
           rope=('climb',0,[0,1,2,3]), ropeHang=('climb',0,[4]), rope1=('climb',0,[5]), ropeSlide=('climb',0,[6]),
           drawSword=('empty',0,[0,1,2,3]), drawShield=('empty',0,[4,5]), drawStaff=('empty',1,[0,1,2,3]), guard=('empty',1,[4,5]),
           walkDrawSword=('mdraw',0,[0,1,2,3]), walkDrawShield=('mdraw',0,[4,5]), runDrawSword=('mdraw',1,[0,1,2,3]), runDrawShield=('mdraw',1,[4,5]), nearShield=('opp',1,[4,5]),
           ladder=('climb',1,[0,1,2]), ladderRest=('climb',1,[3]), wall=('climb',1,[4,5]), wallHang=('climb',1,[6]))
def cut(sheet,f,key=None):
    a=arr[sheet].astype(int); bg=np.median(a[:40,:].reshape(-1,3),axis=0)
    m=10
    y0,y1,x0,x1=max(0,f['y0']-m),min(a.shape[0],f['y1']+m),max(0,f['x0']-m),min(a.shape[1],f['x1']+m)
    c=a[y0:y1,x0:x1]; fg=np.abs(c-bg).sum(2)>30
    if sheet in ALPHAS: fg=ALPHAS[sheet][y0:y1,x0:x1]>100
    # keep the main figure only, close pin-holes, shave one pixel of grey fringe
    L,n=ndi.label(ndi.binary_dilation(fg,iterations=2 if sheet in ALPHAS else 4)); sizes=ndi.sum(fg,L,range(1,n+1)); keep=(L==int(np.argmax(sizes))+1)&fg
    holes=~keep; HL,hn=ndi.label(holes); hs=ndi.sum(holes,HL,range(1,hn+1))
    for i,s in enumerate(hs,1):
        if s<40: keep[HL==i]=True
    keep=ndi.binary_erosion(keep,iterations=1)
    rgba=np.dstack([arr[sheet][y0:y1,x0:x1],(keep*255).astype(np.uint8)])
    im=Image.fromarray(rgba,'RGBA')
    k=S*REL.get(sheet,1.0)
    w,h=im.size; im2=im.convert('RGBa').resize((max(1,round(w*k)),max(1,round(h*k))),Image.LANCZOS).convert('RGBA')
    ax=(f['cx']-x0)*k; ay=(f['y1']-y0)*k
    hand=HANDS.get(key); hx,hy=((hand[0]-x0)*k,(hand[1]-y0)*k) if hand else (None,None)       # anchor: horizontal centre of mass, bottom of the feet
    # light outline so the dark boots and hair still read against the lab's black background
    al=np.asarray(im2)[...,3]>40
    ring=ndi.binary_dilation(al,iterations=5)
    soft=ndi.gaussian_filter(ring.astype(float),1.1)
    halo=np.zeros(al.shape+(4,),np.uint8); halo[...,:3]=205; halo[...,3]=(np.clip(soft,0,1)*255).astype(np.uint8)
    out=Image.fromarray(halo,'RGBA'); out.alpha_composite(im2)
    return out,ax,ay,hx,hy
rows=[]; meta={}
for name,(sheet,ri,idx) in anims.items():
    rows.append((name,[cut(sheet,B[sheet][ri][i],(sheet,ri,i)) for i in idx]))
# Pack every frame left to right across a fixed width, starting a new shelf when one fills up.
MAXW=2040; x=PAD; y=PAD; shelf=0; placed=[]
for name,fr in rows:
    meta[name]=[]
    for im,ax,ay,hx,hy in fr:
        if x+im.size[0]+PAD>MAXW: x=PAD; y+=shelf+PAD; shelf=0
        placed.append((im,x,y)); shelf=max(shelf,im.size[1])
        meta[name].append([x,y,im.size[0],im.size[1],round(ax,1),round(ay,1)]+([round(hx,1),round(hy,1)] if hx is not None else [])); x+=im.size[0]+PAD
atlas=Image.new('RGBA',(MAXW,y+shelf+PAD),(0,0,0,0))
for im,px,py in placed: atlas.paste(im,(px,py))
atlas=atlas.quantize(colors=128,method=Image.FASTOCTREE,dither=Image.NONE).convert('RGBA') if False else atlas
q=atlas.quantize(colors=160,method=Image.FASTOCTREE,dither=Image.NONE)
q.save('/home/claude/feel-lab/sprites.png',optimize=True)
atlas=q.convert('RGBA')
stand=B['move'][0][0]; meta_out=dict(standH=round((stand['y1']-stand['y0'])*S,1),frames=meta)
json.dump(meta_out,open('sprites/meta.json','w'),separators=(',',':'))
print(atlas.size, json.dumps(meta_out,separators=(',',':')))
# preview on black to judge the cut-out edges
prev=Image.new('RGBA',atlas.size,(0,0,0,255)); prev.alpha_composite(atlas); prev.convert('RGB').save('sprites/preview.png')
