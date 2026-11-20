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
