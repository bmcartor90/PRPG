# Rune portal as layers: a still arch (dormant and active), the doorway's mask, and one round vortex the game spins behind the arch.
# Writes assets/props/doors/portal/ and a preview GIF in work/.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
OUT=os.path.join(ROOT,'assets','props','doors','portal'); os.makedirs(OUT,exist_ok=True)
S=np.asarray(Image.open(SRC('2ae07e3a')).convert('RGB')).astype(int)
DORM=(27,32,344,377); ACT=(1166,32,344,377)          # frames 1 (dormant) and 4 (active) on the states sheet
def cut(box,pad=6):
    x,y,w,h=box; c=S[y-pad:y+h+pad,x-pad:x+w+pad]; bg=S[5,5]
    close=np.abs(c-bg).sum(-1)<=24; L,n=ndi.label(close); edge=set(L[0])|set(L[-1])|set(L[:,0])|set(L[:,-1]); edge.discard(0)
    return c, ~np.isin(L,list(edge))
dc,dfg=cut(DORM); ac,afg=cut(ACT)
# Line the active frame up with the dormant one on the stonework.
def score(dx,dy):
    b=np.roll(np.roll(ac,dy,0),dx,1); m=dfg&np.roll(np.roll(afg,dy,0),dx,1); return np.abs(b-dc)[m].mean()
dx,dy=min(((dx,dy) for dx in range(-12,13) for dy in range(-6,7)),key=lambda t:score(*t)); ac=np.roll(np.roll(ac,dy,0),dx,1); afg=np.roll(np.roll(afg,dy,0),dx,1)
# The doorway: the flat dark fill of the dormant frame, holes filled.
h,w=dc.shape[:2]; cc=dc[h//2,w//2]; d=np.abs(dc-cc).sum(-1)<=20; L,_=ndi.label(d); hole=ndi.binary_fill_holes(L==L[h//2,w//2])
# On the active frame the vortex reaches a pixel or two past that line; take bluish pixels next to it as well.
blue=(ac[...,2]-ac[...,0]>40); hole_a=hole|(ndi.binary_dilation(hole,iterations=3)&blue)
def save(c,fg,hl,name):
    al=(fg&~hl).astype(np.uint8)*255; Image.fromarray(np.dstack([c,al]).astype(np.uint8),'RGBA').save(f'{OUT}/{name}')
save(dc,dfg,hole,'arch_dormant.png'); save(ac,afg,hole_a,'arch_active.png')
mask=hole|hole_a; Image.fromarray((mask*255).astype(np.uint8)).save(f'{OUT}/doorway_mask.png')
ys,xs=np.nonzero(mask); cx,cy=float(xs.mean()),float(ys.mean()); R=float(np.sqrt((xs-cx)**2+(ys-cy)**2).max())+4
# The vortex: trim the light rim, scale so its circle covers the doorway at any angle.
V=Image.open(SRC('60f35940')).convert('RGBA'); vs=V.size[0]; va=np.asarray(V).copy(); yy,xx=np.mgrid[:vs,:vs]; r0=vs/2-14
va[...,3]=np.where((xx-vs/2+.5)**2+(yy-vs/2+.5)**2<=r0**2,255,0); D=int(np.ceil(2*R)); V=Image.fromarray(va,'RGBA').crop((int(vs/2-r0),)*2+(int(vs/2+r0),)*2).resize((D,D),Image.LANCZOS)
V.save(f'{OUT}/vortex.png')
open_px=int(ys.max()-ys.min()+1); json.dump(dict(
  note="Layers, back to front: vortex (spun about its centre, which sits at vortexCentre in the arch's pixels, and clipped to doorway_mask), then the arch. Dormant: arch_dormant, no vortex. Active: arch_active and the vortex turning steadily. Waking, opening and closing: fade or grow the vortex. Scale everything by pxPerTileScale so the doorway is 2.4 tiles tall.",
  archSize=[w,h], vortexCentre=[round(cx,1),round(cy,1)], vortexDiameter=D, doorwayHeightPx=open_px, doorwayWidthPx=int(xs.max()-xs.min()+1),
  pxPerTileScale=round(160*2.4/open_px,3), footY=int(np.nonzero(dfg.any(1))[0].max())),open(f'{OUT}/portal.json','w'),indent=1)
# Preview: dormant, waking (runes and vortex fade in), then two full turns.
bgc=(26,20,40); frames=[]
A0=Image.open(f'{OUT}/arch_dormant.png'); A1=Image.open(f'{OUT}/arch_active.png'); M=Image.open(f'{OUT}/doorway_mask.png')
def frame(t_on,ang):
    f=Image.new('RGBA',(w,h),bgc+(255,)); v=V.rotate(-ang,resample=Image.BICUBIC); vl=Image.new('RGBA',(w,h),(0,0,0,0)); vl.paste(v,(int(round(cx-D/2)),int(round(cy-D/2))))
    vl.putalpha(Image.fromarray((np.asarray(vl)[...,3]*(np.asarray(M)/255)*t_on).astype(np.uint8))); f.alpha_composite(vl)
    f.alpha_composite(A1 if t_on>=0.5 else A0); return f.convert('RGB')   # the two arches are drawn slightly differently, so swap them rather than cross-fade
for i in range(10): frames.append(frame(0,0))
for i in range(16): frames.append(frame((i+1)/16,i*3))
for i in range(60): frames.append(frame(1,48+i*12))
frames[0].save(f'{WORK}/portal_preview.gif',save_all=True,append_images=frames[1:],duration=50,loop=0)
print('aligned',dx,dy,'doorway',open_px,'tall; vortex',D,'centre',round(cx),round(cy))
