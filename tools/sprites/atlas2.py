# Build a character's packed frames from sheets with transparent backgrounds. Usage: atlas2.py nina
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, sys, numpy as np, scipy.ndimage as ndi
from PIL import Image
CH=sys.argv[1]
CFG=dict(nina=dict(
  sheets=dict(walk_run='65975ccc', combat='72dd2f7a', mdraw='0c517fb8', rope_wall='65ec7e6d', ledge='b04cdec0', action='4d7d789d'),
  rel=dict(combat=1.01, mdraw=1.02, ledge=1.05),          # each sheet drew her a slightly different size; this evens them out
  robe_fix=dict(ledge=(215/234, 206/213, 203/195)),      # measured: this sheet's robe is (234,213,195), the others' about (215,206,203)
  stand=('walk_run',0,0),
  anims=dict(walk=('walk_run',0,range(6)), run=('walk_run',1,range(6)), jump=('action',0,range(7)), slide=('action',1,[2,3,4]), crouch=('action',1,[0,1]),
    hang=('ledge',0,[0,1]), pull=('ledge',0,[2,3,4,5]), hang1=('ledge',1,[0]), lunge=('ledge',1,[2,3]), prone=('ledge',1,[4]), getup=('ledge',1,[5,6]),
    rope=('rope_wall',0,[0,1,2,3]), ropeHang=('rope_wall',0,[4]), rope1=('rope_wall',0,[5]), ropeSlide=('rope_wall',0,[6]),
    drawSword=('combat',0,[0,1,2,3]), drawShield=('combat',0,[4,5]), drawStaff=('combat',1,[0,1,2,3]), guard=('combat',1,[4,5]), nearShield=('combat',0,[4,5]),
    walkDrawSword=('mdraw',0,[0,1,2,3]), walkDrawShield=('mdraw',0,[4,5]), runDrawSword=('mdraw',1,[0,1,2,3]), runDrawShield=('mdraw',1,[4,5]),
    ladder=('rope_wall',1,[0,1,2]), ladderRest=('rope_wall',1,[3]), wall=('rope_wall',1,[4,5]), wallHang=('rope_wall',1,[6]))),
 ramorand=dict(
  sheets=dict(walk_run='7997c641', action='9bc3dcb6', ledge='f8aa2c67', rope_wall='b64f54a6', combat='5e5e0121'),
  alpha=dict(ledge=WORK+'/alpha_ramorand_ledge.npy'),   # this sheet came on a grey background
  rel=dict(ledge=1.12, combat=1.013), halo=205,
  stand=('walk_run',0,0),
  # His action sheet has 5 jump frames, 1 crouch and 1 slide where the others have 7, 2 and 3, so frames are reused to fill the same slots.
  anims=dict(walk=('walk_run',0,range(6)), run=('walk_run',1,range(6)), jump=('action',0,[0,1,1,2,3,3,4]), slide=('action',1,[1,1,1]), crouch=('action',1,[0,0]),
    hang=('ledge',0,[0,1]), pull=('ledge',0,[2,3,4,5]), hang1=('ledge',1,[0]), lunge=('ledge',1,[2,3]), prone=('ledge',1,[4]), getup=('ledge',1,[5,6]),
    rope=('rope_wall',0,[0,1,2,3]), ropeHang=('rope_wall',0,[4]), rope1=('rope_wall',0,[5]), ropeSlide=('rope_wall',0,[6]),
    drawSword=('combat',0,[0,1,2,3]), drawShield=('combat',0,[4,5]), drawStaff=('combat',1,[0,1,2,3]), guard=('combat',1,[4,5]), nearShield=('combat',0,[4,5]),
    ladder=('rope_wall',1,[0,1,2]), ladderRest=('rope_wall',1,[3]), wall=('rope_wall',1,[4,5]), wallHang=('rope_wall',1,[6]))))[CH]
B=json.load(open(f'{WORK}/boxes_{CH}.json')); arr={}; alpha={}
def robe_fix(rgb, gain):
    # One sheet drew her robe cream instead of grey-white. Pull light, low-colour pixels (the robe) toward the other sheets' robe colour,
    # fading the change out toward skin and hair (strongly warm) and toward dark cloth, which already match.
    a=rgb.astype(float); r,g,b=a[...,0],a[...,1],a[...,2]
    w=np.clip((62-(r-b))/18,0,1)*np.clip((a.max(-1)-120)/40,0,1)
    out=a*(1+(np.array(gain)-1)*w[...,None]); return out.clip(0,255).astype(np.uint8)
for k,n in CFG['sheets'].items():
    im=np.asarray(Image.open(SRC(n)).convert('RGBA')); arr[k]=im[...,:3]; alpha[k]=im[...,3]
    if k in CFG.get('alpha',{}): alpha[k]=np.load(CFG['alpha'][k])
    if k in CFG.get('robe_fix',{}): arr[k]=robe_fix(arr[k],CFG['robe_fix'][k])
PAD=2
def cut(sheet,f):
    m=10; a=arr[sheet]
    y0,y1,x0,x1=max(0,f['y0']-m),min(a.shape[0],f['y1']+m),max(0,f['x0']-m),min(a.shape[1],f['x1']+m)
    fg=alpha[sheet][y0:y1,x0:x1]>100
    L,n=ndi.label(ndi.binary_dilation(fg,iterations=2)); sizes=ndi.sum(fg,L,range(1,n+1)); keep=(L==int(np.argmax(sizes))+1)&fg
    holes=~keep; HL,hn=ndi.label(holes); hs=ndi.sum(holes,HL,range(1,hn+1))
    for i,s in enumerate(hs,1):
        if s<40: keep[HL==i]=True
    keep=ndi.binary_erosion(keep,iterations=1)
    im=Image.fromarray(np.dstack([a[y0:y1,x0:x1],(keep*255).astype(np.uint8)]),'RGBA'); k=CFG['rel'].get(sheet,1.0)
    w,h=im.size; im2=im.convert('RGBa').resize((max(1,round(w*k)),max(1,round(h*k))),Image.LANCZOS).convert('RGBA')
    al=np.asarray(im2)[...,3]>40; ring=ndi.binary_dilation(al,iterations=5); soft=ndi.gaussian_filter(ring.astype(float),1.1)
    halo=np.zeros(al.shape+(4,),np.uint8); halo[...,:3]=CFG.get('halo',40); halo[...,3]=(np.clip(soft,0,1)*255).astype(np.uint8)      # she is pale, so her outline halo is dark
    out=Image.fromarray(halo,'RGBA'); out.alpha_composite(im2)
    return out,(f['cx']-x0)*k,(f['y1']-y0)*k
seen={}; rows=[]
for name,(sheet,ri,idx) in CFG['anims'].items():
    fr=[]
    for i in idx:
        key=(sheet,ri,i)
        if key not in seen: seen[key]=cut(sheet,B[sheet][ri][i])
        fr.append(key)
    rows.append((name,fr))
MAXW=2040; x=PAD; y=PAD; shelf=0; where={}; placed=[]
for key,(im,ax,ay) in seen.items():
    if x+im.size[0]+PAD>MAXW: x=PAD; y+=shelf+PAD; shelf=0
    where[key]=[x,y,im.size[0],im.size[1],round(ax,1),round(ay,1)]; placed.append((im,x,y)); shelf=max(shelf,im.size[1]); x+=im.size[0]+PAD
atlas=Image.new('RGBA',(MAXW,y+shelf+PAD),(0,0,0,0))
for im,px,py in placed: atlas.paste(im,(px,py))
atlas.quantize(colors=160,method=Image.FASTOCTREE,dither=Image.NONE).save(f'{LAB}/{CH}.png',optimize=True)
s=CFG['stand']; f=B[s[0]][s[1]][s[2]]; meta=dict(standH=round((f['y1']-f['y0'])*CFG['rel'].get(s[0],1.0),1),frames={name:[list(where[k]) for k in fr] for name,fr in rows})
json.dump(meta,open(f'{WORK}/meta_{CH}.json','w'),separators=(',',':')); print(atlas.size,'standH',meta['standH'],'frames',sum(len(v) for v in meta['frames'].values()),'unique',len(seen))
