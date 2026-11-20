// Records reference runs of the lab's movement simulation, so a port can be checked against it tick by tick.
// Run: node checks/make-traces.js     Writes one JSON file per scenario into traces/.
const fs = require('fs'), path = require('path');
const html = fs.readFileSync(path.join(__dirname, '..', 'index.html'), 'utf8');
const script = html.split('<script>')[1].split('</script>')[0];
const sim = script.split('/*SIM-START*/')[1].split('/*SIM-END*/')[0];
const level = script.split('// ---------- Level ----------')[1].split('// ---------- Parameters ----------')[0];

// The lab's default numbers: the agreed baseline for the first build.
const PARAMS = {jumpHeight:4, apexTime:0.40, fallMult:1.6, cutMult:3, terminal:20, walkSpeed:5.5, runSpeed:10, accelTime:0.45, stopTime:0.30, turnTime:0.35,
  airControl:0.65, coyoteMs:80, bufferMs:100, hardCut:false, walkLimit:50, upCost:20, downBoost:15, slideFriction:0.75, slideSteer:0.3, slideJump:1, footWidth:0.6, bodyWidth:0.6,
  staminaMax:100, jumpCost:30, runDrain:2, walkRegen:12, crouchRegen:60, exhaustTime:1.2, charHeight:1.9, armReach:0.1, lungeLean:0.8, lungeCost:15, proneTime:0.6,
  pullTime:1, pullCost:40, climbSpeed:2, weight:1, climbEffort:10, hangCost:3, multLadder:0.5, multRope:1.5, multWall:8, oneHand:0.65, drawTime:0.35,
  climbSprint:1.75, climbSprintCost:3};

// Each scenario: where he starts (x, standing on the solid ground there) and a list of [ticks, held inputs].
// dir: -1 left, 1 right. hand: 0 or 1 presses that hand's draw/sheathe key on the first tick of the segment.
const R = {dir:1}, RUN = {dir:1, run:true}, L = {dir:-1};
const SCENARIOS = [
  {name:'walk-run-stop-turn', about:'Walk, break into a run, let go, then turn around.', x:20,
   steps:[[60,R],[90,RUN],[60,{}],[40,L],[30,{}]]},
  {name:'jumps-full-and-tapped', about:'A full held jump, then a one-tick tap, then a jump released halfway up.', x:20,
   steps:[[10,{}],[45,{jump:true}],[70,{}],[1,{jump:true}],[70,{}],[12,{jump:true}],[80,{}]]},
  {name:'running-gap-jumps', about:'Run across the row of gaps, jumping near each edge.', x:30,
   steps:[[52,RUN],[30,{...RUN,jump:true}],[42,RUN],[30,{...RUN,jump:true}],[40,RUN],[30,{...RUN,jump:true}],[60,RUN],[60,{}]]},
  {name:'height-ledges', about:'Jump up through the height ledges and walk off them.', x:10,
   steps:[[45,{jump:true}],[30,R],[45,{jump:true,dir:1}],[60,R],[45,{jump:true}],[90,R],[60,{}]]},
  {name:'gentle-slopes', about:'Run over the 27 degree hill and the 45 degree hill, with one jump on the way up.', x:160,
   steps:[[120,RUN],[30,{...RUN,jump:true}],[400,RUN],[60,{}]]},
  {name:'steep-slope-slide', about:'Run at the 63 degree hill, slide back, steer, and jump off the slide.', x:207,
   steps:[[150,RUN],[60,{}],[90,R],[20,{jump:true}],[120,{}]]},
  {name:'ledge-grab-and-pull-up', about:'Jump under the 5.0 ledge holding grab, hang, then pull up.', x:233.5,
   steps:[[10,{grab:true}],[60,{grab:true,jump:true}],[60,{grab:true}],[100,{grab:true,up:true}],[60,{}]]},
  {name:'ledge-hang-and-drop', about:'Catch the ledge, hang until let go.', x:233.5,
   steps:[[60,{grab:true,jump:true}],[120,{grab:true}],[80,{}]]},
  {name:'ladder-climb-rest-jump-off', about:'Climb the ladder, rest on it, climb down a little, then jump away.', x:262,
   steps:[[150,{grab:true,up:true}],[90,{}],[40,{down:true}],[20,{jump:true,dir:-1}],[100,{}]]},
  {name:'rope-with-sword-drawn', about:'Draw the right-hand item, then climb the rope one-handed and let go.', x:272,
   steps:[[1,{hand:1}],[40,{}],[200,{grab:true,up:true}],[60,{grab:true}],[90,{}]]},
  {name:'wall-climb-to-exhaustion', about:'Climb the brick wall until stamina runs out and he falls.', x:282,
   steps:[[400,{grab:true,up:true}],[200,{}]]},
  {name:'jump-until-exhausted-then-crouch', about:'Jump repeatedly until exhausted, then crouch to recover.', x:20,
   steps:[[30,{jump:true}],[25,{}],[30,{jump:true}],[25,{}],[30,{jump:true}],[25,{}],[30,{jump:true}],[25,{}],[30,{jump:true}],[25,{}],[30,{jump:true}],[120,{}],[150,{down:true}],[40,{}]]},
  {name:'lunge-hit', about:'Running jump with a lunge that catches the 5.0 ledge, hangs, then drops.', x:225,
   steps:[[40,RUN],[18,{...RUN,jump:true}],[2,RUN],[60,{dir:1,grab:true,jump:true}],[120,{grab:true}],[80,{}]]},
  {name:'lunge-miss', about:'The same lunge from too close, which misses and lands him face down.', x:228,
   steps:[[40,RUN],[18,{...RUN,jump:true}],[2,RUN],[60,{dir:1,grab:true,jump:true}],[120,{grab:true}],[80,{}]]},
  {name:'cliff-face-blocks', about:'Run into the 2-high cliff face, then jump at the 6.5 face without grabbing.', x:295,
   steps:[[120,RUN],[30,{}],[1,{}],[40,{...RUN,jump:true}],[60,R],[1,{}],[50,{dir:1,jump:true}],[60,R]]},
  {name:'cliff-grab-and-pull-up', about:'From the 2 block, jump at the 6.5 cliff holding grab, hang on its edge, pull up, walk on and drop to the 3 block.', x:304.5,
   steps:[[2,{}],[70,{dir:1,jump:true,grab:true}],[80,{grab:true,up:true}],[20,{}],[75,R],[40,{}]]},
  {name:'pit-and-respawn', about:'Walk off the 3 block into the walled pit, push against both faces while falling, and respawn.', x:316,
   steps:[[45,R],[12,R],[12,L],[120,{}]]},
  {name:'scaffold-and-cliff-ladder', about:'Jump up through a scaffold, walk off it to the 7-high cliff, and take the ladder to the top.', x:325,
   steps:[[2,{}],[45,{jump:true}],[20,{}],[120,R],[250,{up:true}],[60,R]]},
  {name:'gap-with-walls', about:'Hop short into the 4-wide gap without grabbing and hit the far face; then the same jump holding grab.', x:60.5,
   steps:[[2,{}],[26,{dir:1,jump:true}],[150,R],[2,{}],[26,{dir:1,jump:true,grab:true}],[60,{dir:1,grab:true}],[80,{grab:true,up:true}],[30,{}]]},
  {name:'draw-and-sheathe', about:'Draw both hands, sheathe the left, while walking.', x:20,
   steps:[[1,{hand:1}],[30,{}],[1,{hand:0,dir:1}],[40,R],[1,{hand:0}],[40,{}]]},
];

const run = new Function('PARAMS', 'SCENARIOS', sim + level + `
  const out = [];
  for (const sc of SCENARIOS){
    let ground = null;
    for (const M of LINES) if (M.solid && sc.x >= M.x1 && sc.x <= M.x2 && (!ground || yAt(M, sc.x) > yAt(ground, sc.x))) ground = M;
    const p = makePlayer(sc.x, ground), states = [], modes = new Set(); let prevJump = false;
    const r = (v) => Math.round(v*1e6)/1e6;
    for (const [ticks, h] of sc.steps) for (let t = 0; t < ticks; t++){
      const jump = !!h.jump;
      step(p, PARAMS, {dir:h.dir || 0, run:!!h.run, jumpHeld:jump, jumpPressed:jump && !prevJump, up:!!h.up, down:!!h.down, grab:!!h.grab,
                       swapHand:(t === 0 && h.hand !== undefined) ? h.hand : -1}, LINES, WORLD_W, CLIMBS, CLIFFS);
      prevJump = jump; modes.add(p.mode + (p.grounded ? '/ground' : '/air') + (p.prone > 0 ? '/prone' : '') + (p.exhaust > 0 ? '/exhausted' : ''));
      states.push([r(p.x), r(p.y), r(p.vx), r(p.vy), r(p.stamina), p.mode, p.grounded ? 1 : 0, p.held[0] ? 1 : 0, p.held[1] ? 1 : 0]);
    }
    out.push({name:sc.name, about:sc.about, modes:[...modes], states, startY:yAt(ground, sc.x)});
  }
  const seg = (M) => [M.x1, M.y1, M.x2, M.y2];
  return {out, world:{width:WORLD_W, ground:LINES.filter(M => M.solid).map(seg), scaffolds:LINES.filter(M => !M.solid).map(seg),
    climbables:CLIMBS.map(c => ({kind:c.kind, x:c.x, y1:c.y1, y2:c.y2}))},
    cliffs:CLIFFS.map(c => ({x:c.x, top:c.top, bottom:c.bottom === -Infinity ? null : c.bottom, openSide:c.face < 0 ? 'left' : 'right'}))};
`);
const res = run(PARAMS, SCENARIOS);
const dir = path.join(__dirname, '..', 'traces');
fs.mkdirSync(dir, {recursive:true});
fs.writeFileSync(path.join(dir, '_world.json'), JSON.stringify({ticksPerSecond:60, units:'tiles; y is up', params:PARAMS, world:res.world,
  cliffsNote:'Derived from ground, not authored: listed so a port can check its own derivation. bottom null means the face runs down out of the map.', cliffs:res.cliffs}, null, 1));
for (let i = 0; i < SCENARIOS.length; i++){
  const sc = SCENARIOS[i], o = res.out[i];
  fs.writeFileSync(path.join(dir, sc.name + '.json'), JSON.stringify({name:sc.name, about:sc.about, startX:sc.x, startY:o.startY,
    inputs:sc.steps.map(([ticks, h]) => ({ticks, ...h})),
    stateColumns:['x', 'y', 'vx', 'vy', 'stamina', 'mode', 'grounded', 'leftHandHolding', 'rightHandHolding'], states:o.states}));
  const last = o.states[o.states.length - 1];
  console.log(sc.name.padEnd(34), String(o.states.length).padStart(4), 'ticks  end x', last[0].toFixed(2), 'y', last[1].toFixed(2), ' ', o.modes.join(' '));
}
