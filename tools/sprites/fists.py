# Which skin-coloured patch is which fist, labelled by eye from the numbered contact sheets (blobs.py).
# back = the fist on the side he is facing away from, front = the fist on the side he is facing. None = hidden behind his body.
# Facing right, his right hand is the back fist; mirrored to face left, his right hand is the front fist.
import json, re
bl={}
for t in ['draw','guard','walk','air','mwalk','mrun']: bl.update(json.load(open(f'sprites/blobs_{t}.json')))
PICK=dict(walk=[(3,2),(3,2),(4,3),(2,3),(None,3),(4,3)], run=[(2,3),(3,2),(3,2),(3,4),(3,2),(3,2)],
          jump=[(2,None),(2,None),(3,2),(4,5),(3,None),(1,3),(2,None)], slide=[(4,3),(3,2),(3,2)], crouch=[(3,4),(2,None)],
          drawSword=[(2,3),(1,2),(1,None),(1,2)], drawShield=[(0,3),(2,1)], drawStaff=[(0,None),(0,2),(2,1),(2,1)], guard=[(1,2),(2,1)],
          walkDrawSword=[(2,3),(1,2),(2,3),(3,2)], walkDrawShield=[(1,3),(2,3)], nearShield=[(0,2),(2,3)],
          runDrawSword=[(1,2),(1,2),(2,None),(2,1)], runDrawShield=[(0,2),(2,1)])
out={}
for nm,rows in PICK.items():
    out[nm]=[]
    for i,(b,f) in enumerate(rows):
        B=bl[f'{nm}{i}']; g=lambda k: [None,None] if k is None else [round(B[k]['x']),round(B[k]['y'])]
        out[nm].append(g(b)+g(f))
# While an item is on its way in or out, where it is and how it is turned: [x, y, angle in degrees, drawn behind him?]. None = not visible yet.
POSE=dict(drawSword=[None,[60,129,37,0],[153,102,-19,0],[48,176,38,0]], drawShield=[[55,128,0,1],None],
          walkDrawSword=[None,[84,132,37,0],[180,99,-19,0],[52,176,38,0]], runDrawSword=[None,[110,117,37,0],[244,93,-19,0],[88,141,152,0]],
          walkDrawShield=[[62,128,0,1],None], nearShield=[[62,128,0,1],None], runDrawShield=[[118,122,0,1],None])
js='const FISTS = '+json.dumps(out,separators=(',',':'))+';\nconst DRAW_POSE = '+json.dumps(POSE,separators=(',',':'))+';'
json.dump(dict(fists=out,pose=POSE),open('sprites/fists.json','w')); open('sprites/fists.js','w').write(js); print(js[:400])
