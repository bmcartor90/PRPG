# Nina: which patch is which fist on each frame (labelled by eye), the item poses for her draw frames, and her grip points for ledges and ropes.
import json
bl={}
for t in 'abcd': bl.update(json.load(open(f'sprites/blobs_nina{t}.json')))
PICK=dict(walk=[(3,2),(5,3),(2,1),(3,2),(3,2),(3,2)], run=[(2,1),(3,2),(2,1),(2,1),(2,1),(2,1)],
          jump=[(1,None),(1,None),(2,1),(1,2),(1,2),(1,2),(1,None)], slide=[(2,1),(1,None),(2,1)], crouch=[(1,2),(1,None)],
          drawSword=[(3,4),(2,3),(2,None),(3,4)], drawShield=[(0,3),(3,4)], nearShield=[(0,3),(3,4)],
          walkDrawSword=[(2,3),(1,2),(2,3),(3,2)], walkDrawShield=[(0,3),(2,3)], runDrawSword=[(1,2),(1,2),(1,None),(2,1)], runDrawShield=[(0,2),(3,1)])
fists={}
for nm,rows in PICK.items():
    src='drawShield' if nm=='nearShield' else nm; fists[nm]=[]
    for i,(b,f) in enumerate(rows):
        B=bl[f'{src}{i}']; g=lambda k: [None,None] if k is None else [round(B[k]['x']),round(B[k]['y'])]
        fists[nm].append(g(b)+g(f))
POSE=dict(drawSword=[None,[72,144,37,0],[196,103,-19,0],[60,189,38,0]], drawShield=[[70,135,0,1],None], nearShield=[[70,135,0,1],None],
          walkDrawSword=[None,[90,141,37,0],[201,104,-19,0],[58,187,38,0]], runDrawSword=[None,[120,125,37,0],[265,97,-19,0],[99,149,152,0]],
          walkDrawShield=[[65,135,0,1],None], runDrawShield=[[125,125,0,1],None])
# Where her gripping hand is, in each frame's own pixels: the point pinned to the ledge corner or the rope.
HANDS=dict(hang=[(73,12),(63,13)], pull=[(70,12),(97,110),(165,146),None], hang1=[(57,13)],
           rope=[(120,78),(108,150),(104,48),(99,147)], ropeHang=[(101,94)], rope1=[(156,36)], ropeSlide=[(125,151)])
m=json.load(open('sprites/meta_nina.json'))
for nm,hs in HANDS.items():
    for i,h in enumerate(hs):
        if h: m['frames'][nm][i]=m['frames'][nm][i][:6]+[h[0],h[1]]
json.dump(m,open('sprites/meta_nina.json','w'),separators=(',',':')); json.dump(dict(fists=fists,pose=POSE),open('sprites/fists_nina.json','w')); print('ok',len(fists))
