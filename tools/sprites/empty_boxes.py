# The empty-handed sheet has an opaque black background. Find the background by flooding in from the border
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
# through near-black pixels, so his dark outline, hair and boots (which are enclosed by lighter pixels or are not pure black) survive.
import json, numpy as np, scipy.ndimage as ndi
from PIL import Image
a=np.asarray(Image.open(SRC('51f22a66')).convert('RGB')).astype(int)
dark=a.sum(-1)<=24
lab,n=ndi.label(dark); edge=set(lab[0])|set(lab[-1])|set(lab[:,0])|set(lab[:,-1]); edge.discard(0)
fg=~np.isin(lab,list(edge)); fg[760:,1280:]=False       # drop the "AI-generated" badge
np.save(WORK+'/alpha_empty.npy',(fg*255).astype(np.uint8))
l2,n2=ndi.label(ndi.binary_dilation(fg,iterations=2)); out=[]
for i,sl in enumerate(ndi.find_objects(l2),1):
    m=(l2[sl]==i)&fg[sl]
    if m.sum()<5000: continue
    ys,xs=np.nonzero(m)
    out.append(dict(x0=int(sl[1].start+xs.min()),y0=int(sl[0].start+ys.min()),x1=int(sl[1].start+xs.max()+1),y1=int(sl[0].start+ys.max()+1),cx=float(sl[1].start+xs.mean()),area=int(m.sum())))
rows=[sorted([f for f in out if f['y0']<400],key=lambda f:f['x0']), sorted([f for f in out if f['y0']>=400],key=lambda f:f['x0'])]
json.dump(rows,open(WORK+'/boxes_empty.json','w')); print([len(r) for r in rows], [(f['x1']-f['x0'],f['y1']-f['y0']) for r in rows for f in r])
