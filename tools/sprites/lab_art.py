# Environment art for the lab: the far background, a seamless rock texture for solid ground, and the scaffold pieces.
# Writes feel-lab/cavern_far.jpg, feel-lab/rock.png, feel-lab/scaffold.png and work/scaffold_lab.json (pasted into the lab by lab_data.py).
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, numpy as np
from PIL import Image
A=os.path.join(ROOT,'assets')
# Far layer: the violet repeating matte, already seamless left to right.
Image.open(f'{A}/backgrounds/crystal-cavern/far_matte_violet_repeating.png').convert('RGB').save(f'{LAB}/cavern_far.jpg',quality=86)
# Rock: the plain block from the solid wall set, outline trimmed, then made to repeat both ways by cross-fading each edge band
# into the opposite side (the seams Meshy left are where the light top edge and dark outline were).
W=np.asarray(Image.open(SRC('ab22f874')).convert('RGB')).astype(float)
x,y,w,h=54,57,438,376; ins=12; r=W[y+ins:y+h-ins, x+ins:x+w-ins]
def wrap(a,axis,band):
    a=np.moveaxis(a,axis,0).copy(); n=a.shape[0]; t=np.linspace(0,1,band)[:,None,None]
    a[:band]=a[:band]*t+a[n-band:]*(1-t); a=a[:n-band]; return np.moveaxis(a,0,axis)
r=wrap(wrap(r,1,40),0,40)*0.72   # darker than the art itself, so the characters stand out against it
Image.fromarray(r.clip(0,255).astype(np.uint8)).save(f'{LAB}/rock.png',optimize=True)
# Ground edges from the edge set, toned down to sit with the rock fill (the edge set came out brighter and more saturated than the wall set).
E=np.asarray(Image.open(SRC('c88628f8')).convert('RGBA')).astype(float)
def edge(x0,y0,x1,y1):
    c=E[y0:y1,x0:x1].copy(); g=c[...,:3].mean(-1,keepdims=True); c[...,:3]=(c[...,:3]*0.8+g*0.2)*0.78; return c   # a little less saturated, darker
TOP_ROWS=120
t=edge(48+3,181,48+362-3,181+TOP_ROWS); t=wrap(t,1,30)                      # top surface strip, repeats left to right
t[-45:,:,3]*=np.linspace(1,0,45)[:,None]                                     # fades into the rock fill below
Image.fromarray(t.clip(0,255).astype(np.uint8),'RGBA').save(f'{LAB}/edge_top.png',optimize=True)
f=edge(49,472+4,49+70,472+313-4); f=wrap(f,0,40)                             # cliff face strip, open air on its left, repeats top to bottom
f[:,-30:,3]*=np.linspace(1,0,30)[None,:]
Image.fromarray(f.clip(0,255).astype(np.uint8),'RGBA').save(f'{LAB}/edge_face.png',optimize=True)
# Climbable back walls: the very rough and slightly rough panels, outline trimmed, joins blended so they repeat both ways.
B=np.asarray(Image.open(SRC('c4b0841c')).convert('RGB')).astype(float)
for name,(x,y,w,h) in (('wall_very_rough',(92,30,273,381)),('wall_slightly_rough',(93,444,268,386))):
    p=wrap(wrap(B[y+8:y+h-8,x+8:x+w-8],1,24),0,24)*0.85
    Image.fromarray(p.clip(0,255).astype(np.uint8)).save(f'{LAB}/{name}.png',optimize=True)
# Scaffold: pack the pieces the lab uses into one image.
S=Image.open(f'{A}/tiles/scaffold/source/Meshy_AI_miner_scaffold_tileset.png').convert('RGBA'); P=json.load(open(f'{A}/tiles/scaffold/scaffold_pieces.json'))
names=['deckLeft','deckMid','deckRight','postMid','ladder','rope','ropeTop']; out={}; ims=[]; X=2
for n in names:
    q=P['pieces'][n]; im=S.crop((q['x'],q['y'],q['x']+q['w'],q['y']+q['h'])); ims.append((n,im,X)); out[n]=[X,2,q['w'],q['h'],q['y']]; X+=q['w']+2
sheet=Image.new('RGBA',(X,max(i.size[1] for _,i,_ in ims)+4),(0,0,0,0))
for n,im,px in ims: sheet.paste(im,(px,2))
sheet.save(f'{LAB}/scaffold.png',optimize=True)
json.dump(dict(pxPerTile=P['pixelsPerTile'],surfaceY=P['deckSurfaceY'],pieces=out),open(f'{WORK}/scaffold_lab.json','w'),separators=(',',':'))
print('rock',r.shape[1],'x',r.shape[0],'scaffold',sheet.size)
