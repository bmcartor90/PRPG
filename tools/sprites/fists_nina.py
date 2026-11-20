# Nina: which patch is which fist on each frame (labelled by eye), the item poses for her draw frames, and her grip points for ledges and ropes.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import json
# [back x, back y, front x, front y] per frame, in the frame's own pixels; N = hidden. First labelled by blob number from blobs.py contact
# sheets; stored as coordinates so a change to the blob finder can't silently relabel them. To relabel, run blobs.py and read the sheet.
N=None
F=dict(
  walk=[(69,191,127,185),(73,196,153,182),(71,190,163,179),(79,192,145,185),(74,187,167,178),(72,191,132,184)],
  run=[(65,123,213,119),(76,120,230,99),(81,113,213,105),(100,118,224,110),(69,118,218,102),(75,123,221,119)],
  jump=[(49,119,N,N),(43,116,N,N),(28,108,171,106),(36,103,184,116),(28,114,149,116),(26,57,189,109),(29,110,N,N)],
  slide=[(30,117,206,84),(22,114,N,N),(32,140,185,71)],
  crouch=[(37,126,160,156),(47,144,N,N)],
  drawSword=[(125,150,134,178),(72,144,158,182),(196,103,N,N),(60,189,155,190)],
  drawShield=[(57,53,166,190),(143,138,158,197)],
  nearShield=[(57,53,166,190),(143,138,158,197)],
  walkDrawSword=[(134,157,167,185),(90,141,164,181),(201,104,147,187),(58,187,162,184)],
  walkDrawShield=[(45,48,161,185),(116,157,149,183)],
  runDrawSword=[(125,128,226,166),(120,125,194,151),(265,97,N,N),(99,149,214,146)],
  runDrawShield=[(109,46,223,165),(162,144,206,135)])
fists={k:[list(q) for q in v] for k,v in F.items()}
POSE=dict(drawSword=[None,[72,144,37,0],[196,103,-19,0],[60,189,38,0]], drawShield=[[70,135,0,1],None], nearShield=[[70,135,0,1],None],
          walkDrawSword=[None,[90,141,37,0],[201,104,-19,0],[58,187,38,0]], runDrawSword=[None,[120,125,37,0],[265,97,-19,0],[99,149,152,0]],
          walkDrawShield=[[65,135,0,1],None], runDrawShield=[[125,125,0,1],None])
# Where her gripping hand is, in each frame's own pixels: the point pinned to the ledge corner or the rope.
HANDS=dict(hang=[(73,12),(63,13)], pull=[(70,12),(97,110),(165,146),None], hang1=[(57,13)],
           rope=[(120,78),(108,150),(104,48),(99,147)], ropeHang=[(101,94)], rope1=[(156,36)], ropeSlide=[(125,151)])
m=json.load(open(WORK+'/meta_nina.json'))
for nm,hs in HANDS.items():
    for i,h in enumerate(hs):
        if h: m['frames'][nm][i]=m['frames'][nm][i][:6]+[h[0],h[1]]
json.dump(m,open(WORK+'/meta_nina.json','w'),separators=(',',':')); json.dump(dict(fists=fists,pose=POSE),open(WORK+'/fists_nina.json','w')); print('ok',len(fists))
