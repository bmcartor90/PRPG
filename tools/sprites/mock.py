# Mock-up of Lucca on the crystal platforms over a background, at 1080p game scale. Usage: mock.py <background> <output>
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import sys, json, numpy as np
from PIL import Image
T=Image.open(SRC('6dd62cb4')).convert('RGBA'); ta=np.asarray(T).copy(); ta[...,3]=np.load(WORK+'/alpha_tiles.npy'); T=Image.fromarray(ta,'RGBA')
B=json.load(open(WORK+'/boxes_tiles.json')); M=json.load(open(WORK+'/meta.json')); A=Image.open(LAB+'/sprites.png').convert('RGBA')
BG=Image.open(sys.argv[1]).convert('RGBA').resize((1920,1080),Image.LANCZOS); TILE=80; ks=0.8
def piece(i):
    im=T.crop(tuple(B[i])); return im.resize((round(im.size[0]*ks),round(im.size[1]*ks)),Image.LANCZOS)
def lucca(name,i,flip=False):
    f=M['frames'][name][i]; k=1.9*TILE/M['standH']; im=A.crop((f[0],f[1],f[0]+f[2],f[1]+f[3])).resize((round(f[2]*k),round(f[3]*k)),Image.LANCZOS)
    ax,ay=f[4]*k,f[5]*k
    if flip: im=im.transpose(Image.FLIP_LEFT_RIGHT); ax=im.size[0]-ax
    return im,ax,ay
out=BG.copy(); put=lambda im,x,y: out.alpha_composite(im,(int(x),int(y)))
g=piece(8); x=-20
while x<1920: put(g,x,900); x+=g.size[0]-28
for i,x,y in [(2,620,640),(4,1130,455),(0,430,520),(14,1500,330)]: put(piece(i),x,y)
def stand(name,i,x,top,flip=False):
    im,ax,ay=lucca(name,i,flip); put(im,x-ax,top+12-ay)
stand('walk',0,260,900); stand('run',2,760,640); stand('jump',3,1010,520); stand('walk',2,1250,455+(piece(4).size[1]-piece(1).size[1]),True); stand('guard',0,1640,330)
out.convert('RGB').save(sys.argv[2])
