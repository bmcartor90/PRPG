# Ramorand: fist positions per frame, read by eye from the contact sheets (his near fist is often gauntlet-grey, so the colour test misses it),
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
# item poses for his draw frames, and grip points for ledges and ropes. All in each frame's own pixels.
import json
N=None
F=dict(walk=[(112,179,179,175),(113,173,179,177),(97,173,207,171),(91,173,179,173),(195,160,N,N),(117,179,177,177)],
       run=[(123,134,230,139),(121,123,245,91),(116,123,239,98),(132,133,246,109),(117,123,238,98),(108,134,236,116)],
       jump=[(105,135,231,128),(99,111,259,78),(99,111,259,78),(86,111,240,106),(106,121,251,110),(106,121,251,110),(160,148,N,N)],
       slide=[(114,137,307,108)]*3, crouch=[(104,132,250,179)]*2,
       drawSword=[(131,148,197,178),(101,118,191,173),(245,87,191,170),(90,184,209,176)], drawShield=[(108,39,218,177),(98,181,216,87)], nearShield=[(108,39,218,177),(98,181,216,87)])
fists={k:[list(q) for q in v] for k,v in F.items()}
POSE=dict(drawSword=[None,[101,118,37,0],[245,87,-19,0],[90,184,38,0]], drawShield=[[98,130,0,1],None], nearShield=[[98,130,0,1],None])
HANDS=dict(hang=[(100,15),(86,15)], pull=[(81,15),(101,126),(180,134),None], hang1=[(76,13)],
           rope=[(147,45),(137,144),(145,46),(131,136)], ropeHang=[(139,67)], rope1=[(169,28)], ropeSlide=[(151,130)])
m=json.load(open(WORK+'/meta_ramorand.json'))
for nm,hs in HANDS.items():
    for i,h in enumerate(hs):
        if h: m['frames'][nm][i]=m['frames'][nm][i][:6]+[h[0],h[1]]
json.dump(m,open(WORK+'/meta_ramorand.json','w'),separators=(',',':')); json.dump(dict(fists=fists,pose=POSE),open(WORK+'/fists_ramorand.json','w')); print('ok')
