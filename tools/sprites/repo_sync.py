# Write each character's packed frames and the items into assets/ in their documented layout. (The lab images are already built in place in feel-lab/.)
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json, shutil, os
R=ROOT; S=WORK+'/'
NOTE="Each frame: x, y, w, h locate it in the image. anchorX, anchorY is the point under the feet, relative to the frame's top-left. handX, handY (when present) is where the gripping hand is, used to pin the character to a ledge or rope instead. backFist and frontFist (when present) are [x, y] of the two fists, for pinning held items: back is the fist on the side they face away from, front the one on the side they face, null when hidden behind the body. Facing right the back fist is the right hand; mirrored to face left, the right hand is the front fist. itemPose (draw frames only) is [x, y, angleDegrees, behindThem] for the item on its way out, null when it is not visible yet."
for ch,metaf,fistf,img,height in [('lucca','meta.json','fists.json','sprites.png',1.9),('nina','meta_nina.json','fists_nina.json','nina.png',1.7),('ramorand','meta_ramorand.json','fists_ramorand.json','ramorand.png',1.9)]:
    m=json.load(open(S+metaf)); fx=json.load(open(S+fistf)); d0=R+f'/assets/sprites/{ch}/atlas'; os.makedirs(d0,exist_ok=True)
    out={"image":f"{ch}_atlas.png","standingHeightPx":m["standH"],"heightInTiles":height,"note":NOTE,"animations":{}}
    for name,frames in m["frames"].items():
        out["animations"][name]=[]
        for i,f in enumerate(frames):
            d=dict(x=f[0],y=f[1],w=f[2],h=f[3],anchorX=f[4],anchorY=f[5])
            if len(f)>6: d.update(handX=f[6],handY=f[7])
            if name in fx['fists']:
                q=fx['fists'][name][i]; d['backFist']=None if q[0] is None else q[:2]; d['frontFist']=None if q[2] is None else q[2:]
            if name in fx['pose']: d['itemPose']=fx['pose'][name][i]
            out["animations"][name].append(d)
    json.dump(out,open(d0+f'/{ch}_atlas.json','w'),indent=1); shutil.copy(LAB+'/'+img,d0+f'/{ch}_atlas.png')
it=json.load(open(S+'items.json')); who=lambda k: 'Ramorand' if k.startswith('r_') else 'Lucca and Nina'
json.dump({"image":"items_atlas.png","scaledForPixelsPerTile":round(307/1.9,1),
 "note":"Items are separate pictures laid over a character's fists, so one sword works with every animation. All are scaled to the same pixels per tile as Lucca's atlas; draw them the same size in the world whoever holds them. x, y, w, h locate each in the image. gripX, gripY is the point the hand holds, relative to the item's top-left; rotate the item about it. Names starting r_ are Ramorand's own gear.",
 "items":{k:dict(x=v[0],y=v[1],w=v[2],h=v[3],gripX=v[4],gripY=v[5],usedBy=who(k)) for k,v in it.items()}},open(R+'/assets/sprites/items/items_atlas.json','w'),indent=1)
shutil.copy(LAB+'/items.png',R+'/assets/sprites/items/items_atlas.png')
print('synced')
