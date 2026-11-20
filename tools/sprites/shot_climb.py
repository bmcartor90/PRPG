# Capture a character hanging and climbing with each item in hand, to check fist placement. Usage: shot_climb.py <character> [out.png]
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__))); from paths import ROOT, WORK, LAB, SRC
import asyncio
from playwright.async_api import async_playwright
from PIL import Image, ImageDraw
import numpy as np
CH=sys.argv[1]; OUT=sys.argv[2] if len(sys.argv)>2 else f'{WORK}/climb_{CH}.png'
async def main():
    async with async_playwright() as p:
        b=await p.chromium.launch(executable_path='/usr/bin/google-chrome',args=['--no-sandbox'])
        pg=await b.new_page(viewport={'width':1280,'height':900})
        errs=[]; pg.on('pageerror',lambda e:errs.append(str(e))); pg.on('console',lambda m: errs.append(m.text) if m.type=='error' else None)
        await pg.goto('file://'+LAB+'/index.html'); await pg.wait_for_timeout(900)
        c=pg.locator('#game'); await pg.select_option('#character',CH); await pg.select_option('#view-preset','1080'); await c.click()
        K=pg.keyboard; shots=[]
        place=lambda x: pg.evaluate(f'(()=>{{let L=null; for(const M of LINES) if(M.solid&&{x}>=M.x1&&{x}<=M.x2&&(!L||yAt(M,{x})>yAt(L,{x}))) L=M; respawn(player,{x},L); cam.x=cam.px={x}-6; player.stamina=P.staminaMax; }})()')
        async def snap(n): await c.screenshot(path=f'{WORK}/c_{n}.png'); shots.append(n)
        async def arm(key): await K.press(key); await pg.wait_for_timeout(900)
        for item,key in (('shield','q'),('sword','w')):
            await place(230); await arm(key)
            await place(233.5); await K.down('f'); await K.down('Space'); await pg.wait_for_timeout(1000); await K.up('Space'); await snap(f'hang-{item}')
            await K.down('ArrowUp'); await pg.wait_for_timeout(450); await snap(f'pull-{item}'); await pg.wait_for_timeout(900); await K.up('ArrowUp'); await K.up('f')
            await place(272); await K.down('f'); await K.down('ArrowUp'); await pg.wait_for_timeout(900); await snap(f'rope-{item}'); await K.up('ArrowUp'); await pg.wait_for_timeout(250); await snap(f'ropeStill-{item}'); await K.up('f'); await pg.wait_for_timeout(700)
            await place(262); await K.down('ArrowUp'); await pg.wait_for_timeout(700); await snap(f'ladder-{item}'); await pg.wait_for_timeout(230); await snap(f'ladder2-{item}'); await K.up('ArrowUp'); await pg.wait_for_timeout(300)
            await place(304.5); await K.down('f'); await K.down('ArrowRight'); await K.down('Space'); await pg.wait_for_timeout(1100); await snap(f'wall-{item}'); await K.up('Space'); await K.up('ArrowRight'); await K.up('f')
            await place(230); await arm(key)   # put it away
        print('errors',errs[:5]); await b.close()
        tiles=[]
        for n in shots:   # crop round the character: the only strongly coloured thing on screen
            im=Image.open(f'{WORK}/c_{n}.png').convert('RGB'); a=np.asarray(im).astype(int); sat=a.max(-1)-a.min(-1)
            ys,xs=np.nonzero(sat>40); cx,cy=int(np.median(xs)),int(np.median(ys)); t=im.crop((cx-110,cy-130,cx+110,cy+130)).resize((330,390),Image.LANCZOS)
            ImageDraw.Draw(t).text((4,4),n,fill=(255,255,0)); tiles.append(t)
        tw,th=tiles[0].size; cols=7; rows=(len(tiles)+cols-1)//cols
        out=Image.new('RGB',(cols*(tw+4),rows*(th+4)),(90,0,0))
        for i,t in enumerate(tiles): out.paste(t,((i%cols)*(tw+4),(i//cols)*(th+4)))
        out.save(OUT); print(out.size,shots)
asyncio.run(main())
