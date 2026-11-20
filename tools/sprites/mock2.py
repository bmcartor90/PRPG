# Mock-up at 1080p game scale: far background (repeating), crystal ground, wooden scaffolds, ladder, rope, Lucca.
import sys, json, numpy as np
from PIL import Image
W,H,TILE=1920,1080,80
M=json.load(open('sprites/meta.json')); A=Image.open('/home/claude/feel-lab/sprites.png').convert('RGBA')
T=Image.open('/mnt/user-data/uploads/6dd62cb4-image.png').convert('RGBA'); ta=np.asarray(T).copy(); ta[...,3]=np.load('sprites/alpha_tiles.npy'); T=Image.fromarray(ta,'RGBA'); TB=json.load(open('sprites/boxes_tiles.json'))
S=Image.open('/mnt/user-data/uploads/474ce6bb-image.png').convert('RGBA'); SB=json.load(open('sprites/boxes_scaffold.json')); ks=0.53; DECK_TOP=59
bg=Image.open(sys.argv[1]).convert('RGBA'); k=H/bg.size[1]; bg=bg.resize((round(bg.size[0]*k),H),Image.LANCZOS)
out=Image.new('RGBA',(W,H)); x=-200
while x<W: out.paste(bg,(x,0)); x+=bg.size[0]
put=lambda im,x,y: out.alpha_composite(im,(int(round(x)),int(round(y)))) if x<W and y<H else None
def sp(name,crop=None):
    b=SB[name]; im=S.crop(tuple(b))
    if crop: im=im.crop((0,crop[0],im.size[0],crop[1]))
    return im.resize((max(1,round(im.size[0]*ks)),max(1,round(im.size[1]*ks))),Image.LANCZOS)
def lucca(name,i,flip=False,hand=False):
    f=M['frames'][name][i]; kk=1.9*TILE/M['standH']; im=A.crop((f[0],f[1],f[0]+f[2],f[1]+f[3])).resize((round(f[2]*kk),round(f[3]*kk)),Image.LANCZOS)
    ax,ay=(f[6]*kk,f[7]*kk) if hand and len(f)>6 else (f[4]*kk,f[5]*kk)
    if flip: im=im.transpose(Image.FLIP_LEFT_RIGHT); ax=im.size[0]-ax
    return im,ax,ay
def stand(name,i,x,y,flip=False,hand=False):
    im,ax,ay=lucca(name,i,flip,hand); put(im,x-ax,y-ay)
GROUND=900
g=T.crop(tuple(TB[8])); g=g.resize((round(g.size[0]*0.8),round(g.size[1]*0.8)),Image.LANCZOS); x=-20
while x<W: put(g,x,GROUND); x+=g.size[0]-28
fill=Image.new('RGBA',(W,H-GROUND-g.size[1]+6),(10,20,70,255)); put(fill,0,GROUND+g.size[1]-6)
def column(x,y0,y1):                      # a post from y0 down to y1 with a foot at the bottom
    foot=sp('postFoot'); mid=sp('postMid'); fy=y1-foot.size[1]; y=y0
    while y<fy: 
        h=min(mid.size[1],fy-y); put(mid.crop((0,0,mid.size[0],h)),x-mid.size[0]/2,y); y+=h
    put(foot,x-foot.size[0]/2,fy)
def deck(x,top,mids,posts=True):
    l=sp('deckLeft'); m=sp('deckMid'); r=sp('deckRight'); oy=lambda n:(DECK_TOP-SB[n][1])*ks
    if posts:
        pb=top-oy('deckLeft')+l.size[1]-4; column(x+19*ks,pb,GROUND+10); column(x+l.size[0]+mids*m.size[0]+r.size[0]-19*ks,pb,GROUND+10)
    put(l,x,top-oy('deckLeft')); cx=x+l.size[0]
    for _ in range(mids): put(m,cx,top-oy('deckMid')); cx+=m.size[0]
    put(r,cx,top-oy('deckRight')); return x, cx+r.size[0]
def ladder(x,y0,y1):
    L=sp('ladder'); y=y1-L.size[1]
    while y+L.size[1]>y0:
        c=L if y>=y0 else L.crop((0,y0-y,L.size[0],L.size[1])); put(c,x-L.size[0]/2,max(y,y0)); y-=L.size[1]-6
def rope(x,y0,y1):
    top=sp('ropeTop',(0,150)); seg=sp('rope',(70,220)); end=sp('rope',(220,306)); put(top,x-top.size[0]/2,y0); y=y0+top.size[1]-1
    while y<y1-end.size[1]: h=min(seg.size[1],y1-end.size[1]-y); put(seg.crop((0,0,seg.size[0],h)),x-seg.size[0]/2,y); y+=h
    put(end,x-end.size[0]/2,y)
A_TOP,B_TOP=620,330
ladder(700,A_TOP-10,GROUND+4)
a0,a1=deck(420,A_TOP,1)
b0,b1=deck(1080,B_TOP,1)
rope(1560,B_TOP+28,B_TOP+400)
stand('walk',0,200,GROUND+12); stand('ladder',0,700,GROUND-150); stand('run',2,560,A_TOP+2); stand('jump',3,930,470); stand('guard',0,1300,B_TOP+2,True); stand('rope',1,1560,B_TOP+150,hand=True)
out.convert('RGB').save(sys.argv[2])
