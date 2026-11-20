import asyncio
from playwright.async_api import async_playwright
from PIL import Image
async def main():
    async with async_playwright() as p:
        b=await p.chromium.launch(executable_path='/usr/bin/google-chrome',args=['--no-sandbox'])
        pg=await b.new_page(viewport={'width':1280,'height':900})
        errs=[]; pg.on('pageerror',lambda e:errs.append(str(e))); pg.on('console',lambda m: errs.append(m.text) if m.type=='error' else None)
        await pg.goto('file:///home/claude/feel-lab/index.html'); await pg.wait_for_timeout(900)
        c=pg.locator('#game'); await pg.select_option('#character','nina'); await pg.select_option('#view-preset','1080'); await c.click()
        K=pg.keyboard; shots=[]
        place=lambda x,y=0: pg.evaluate(f'(()=>{{let L=null; for(const M of LINES) if(M.solid&&{x}>=M.x1&&{x}<=M.x2&&(!L||yAt(M,{x})>yAt(L,{x}))) L=M; respawn(player,{x},L); cam.x=cam.px={x}-6; }})()')
        async def snap(n): await c.screenshot(path=f'sprites/n_{n}.png'); shots.append(n)
        await place(224); await pg.wait_for_timeout(300); await snap('stand')
        await K.down('ArrowRight'); await pg.wait_for_timeout(400); await snap('walk'); await K.down('d'); await pg.wait_for_timeout(600); await snap('run')
        await K.down('ArrowDown'); await pg.wait_for_timeout(130); await snap('dodge'); await K.up('ArrowDown'); await pg.wait_for_timeout(500)
        await K.down('Space'); await pg.wait_for_timeout(260); await snap('jump'); await K.up('Space'); await K.up('d'); await K.up('ArrowRight'); await pg.wait_for_timeout(900)
        await K.press('w'); await K.press('q'); await pg.wait_for_timeout(900); await snap('armed'); await K.down('ArrowLeft'); await pg.wait_for_timeout(450); await snap('armedLeft'); await K.up('ArrowLeft'); await K.press('w'); await K.press('q'); await pg.wait_for_timeout(900)
        await place(233.5); await K.down('f'); await K.down('Space'); await pg.wait_for_timeout(1000); await K.up('Space'); await snap('hang'); await K.down('ArrowUp'); await pg.wait_for_timeout(450); await snap('pull'); await pg.wait_for_timeout(900); await K.up('ArrowUp'); await K.up('f')
        await place(272); await K.down('f'); await K.down('ArrowUp'); await pg.wait_for_timeout(900); await snap('rope'); await K.up('ArrowUp'); await K.up('f'); await pg.wait_for_timeout(700)
        await place(262); await K.down('ArrowUp'); await pg.wait_for_timeout(900); await snap('ladder'); await K.up('ArrowUp'); await pg.wait_for_timeout(300)
        await place(304.5); await K.down('f'); await K.down('ArrowRight'); await K.down('Space'); await pg.wait_for_timeout(1100); await snap('cliff'); await K.up('Space'); await K.up('ArrowRight'); await K.up('f')
        print('errors',errs[:5],'height',await pg.evaluate('P.charHeight')); await b.close()
        ims=[Image.open(f'sprites/n_{n}.png').convert('RGB') for n in shots]; w,h=ims[0].size; vw=int(h*16/9); x0=(w-vw)//2
        tiles=[im.crop((x0+vw//5,h//8,x0+vw*4//5,h)).resize((vw*3//10,h*7//16)) for im in ims]; tw,th=tiles[0].size; cols=4; rows=(len(tiles)+cols-1)//cols
        out=Image.new('RGB',(cols*(tw+4),rows*(th+4)),(90,0,0))
        for i,t in enumerate(tiles): out.paste(t,((i%cols)*(tw+4),(i//cols)*(th+4)))
        out.save('sprites/look17.png'); print(out.size,shots)
asyncio.run(main())
