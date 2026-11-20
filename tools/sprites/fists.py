# Which skin-coloured patch is which fist, labelled by eye from the numbered contact sheets (blobs.py).
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
# back = the fist on the side he is facing away from, front = the fist on the side he is facing. None = hidden behind his body.
# Facing right, his right hand is the back fist; mirrored to face left, his right hand is the front fist.
import json, re
# [back x, back y, front x, front y] per frame, in the frame's own pixels; N = hidden. First labelled by blob number from blobs.py contact
# sheets; stored as coordinates so a change to the blob finder can't silently relabel them. To relabel, run blobs.py and read the sheet.
N=None
F=dict(
  walk=[(77,179,127,172),(61,178,147,173),(61,179,156,173),(79,176,142,177),(N,N,164,172),(73,176,127,175)],
  run=[(58,132,198,139),(64,125,220,106),(77,115,208,111),(98,117,221,121),(64,116,209,105),(80,132,211,126)],
  jump=[(40,119,N,N),(44,98,N,N),(24,117,157,103),(36,112,170,131),(32,127,N,N),(36,54,179,106),(34,116,N,N)],
  slide=[(37,119,200,80),(27,117,178,90),(39,143,180,81)],
  crouch=[(37,127,161,161),(46,143,N,N)],
  drawSword=[(107,134,114,164),(60,129,125,162),(153,102,N,N),(48,176,148,177)],
  drawShield=[(41,49,145,178),(39,177,140,177)],
  drawStaff=[(51,34,N,N),(75,31,122,96),(69,148,148,116),(73,136,160,96)],
  guard=[(48,159,175,159),(64,164,152,120)],
  walkDrawSword=[(125,149,153,177),(84,132,155,169),(180,99,138,174),(52,176,152,173)],
  walkDrawShield=[(45,46,152,175),(110,149,141,171)],
  nearShield=[(54,41,145,179),(92,144,139,179)],
  runDrawSword=[(112,121,208,157),(110,117,178,144),(244,93,N,N),(88,141,200,139)],
  runDrawShield=[(99,41,208,155),(151,136,191,128)])
# Hanging, climbing, lunge and get-up frames, read by eye from grid.py contact sheets. Side views follow the same rule as above
# (back = the fist on the side away from the way they face = right hand, unmirrored). From-behind frames (ladder, wall) have
# no facing, so there back is simply the right hand, which is the hand on the right of the unmirrored picture.
F.update(dict(
  hang=[(44,31,97,29),(46,33,103,31)], hang1=[(66,35,132,224)], pull=[(42,27,107,24),(48,104,140,104),(59,123,161,124),(126,196,N,N)],
  lunge=[(248,32,273,51),(334,50,347,67)], prone=[(272,82,298,73)], getup=[(122,160,188,155),(54,141,N,N)],
  rope=[(106,75,104,40),(97,140,100,30),(105,52,84,34),(95,142,92,36)], ropeHang=[(89,93,82,43)], rope1=[(28,180,130,24)], ropeSlide=[(117,153,124,136)],
  ladder=[(118,30,24,109),(132,51,29,56),(110,32,23,117)], ladderRest=[(126,63,30,63)], wall=[(154,28,28,107),(122,23,22,108)], wallHang=[(197,21,46,27)]))
fists={k:[list(q) for q in v] for k,v in F.items()}
POSE=dict(drawSword=[None,[60,129,37,0],[153,102,-19,0],[48,176,38,0]], drawShield=[[55,128,0,1],None],
          walkDrawSword=[None,[84,132,37,0],[180,99,-19,0],[52,176,38,0]], runDrawSword=[None,[110,117,37,0],[244,93,-19,0],[88,141,152,0]],
          walkDrawShield=[[62,128,0,1],None], nearShield=[[62,128,0,1],None], runDrawShield=[[118,122,0,1],None])
js='const FISTS = '+json.dumps(fists,separators=(',',':'))+';\nconst DRAW_POSE = '+json.dumps(POSE,separators=(',',':'))+';'
json.dump(dict(fists=fists,pose=POSE),open(WORK+'/fists.json','w')); open(WORK+'/fists.js','w').write(js); print(js[:400])
