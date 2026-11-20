/*SIM-START*/
// Everything in the simulation is measured in tiles and seconds. One tick is 1/60 s.
// On the ground, p.vx is speed along the surface. In the air, it is horizontal speed.
const DT = 1/60;
const RADIUS = 0.45;              // drawing only: the size of the stand-in circle
// Numbers that are fixed in the lab (no slider) but are still tuning, not code. A game build should load these from data.
// BODY numbers belong to a character, like the slider numbers do. WORLD numbers belong to the game.
const BODY = {
  catchOut:0.8,          // tiles outside a ledge end that still catch it
  catchUnder:0.6,        // tiles under a ledge (inside its end) that still catch it
  catchMinDrop:0.3,      // the ledge must be at least this far above his feet to be caught
  hangOut:0.3,           // where he hangs: this far outside the ledge end
  hangIn:0.4,            // where a pull-up puts him: this far in from the end
  hangEase:0.35,         // share of the remaining distance he closes each tick while settling into the hang
  climbReach:0.6,        // tiles either side of a climbable that still catch it, and of a platform at its top or bottom
  climbJumpSpeed:0.8,    // sprint-jumping off a climbable leaves at this share of run speed
  slideDownSpeed:3,      // sliding down a ladder or rope is this many times climb speed
  dropGrabLock:0.3,      // seconds after letting go before he can grab again
  jumpGrabLock:0.45,     // seconds after jumping off a climbable before he can grab again
  windedUntil:0.25,      // after exhaustion he cannot sprint until this share of the stamina bar is back
  sprintShare:0.25,      // he counts as sprinting above walk speed plus this share of the gap up to run speed
  duckStopShare:0.5,     // a dodge slide ends early below this share of walk speed
  runDrainAbove:0.1,     // running drains stamina only above walk speed plus this many tiles a second
  crouchBelow:0.2,       // he can crouch only when moving slower than this, in tiles a second
  minUphillShare:0.1,    // however steep the climb, top speed never drops below this share of normal
  pullSlideFrom:0.6      // a pull-up lifts him straight up until this share of it is done, then also carries him in over the ledge
};
const WORLD = {
  snap:0.3,              // how far apart two line ends can be and still count as joined
  join:0.05,             // how close two line ends must be to count as the same point
  stepOff:0.15,          // how close to a platform's height the top of a climbable must be to step onto it
  edgeMargin:0.45,       // he cannot go closer than this to either end of the map
  respawnBelow:-7,       // fall below this and he is put back on the last safe ground (a lab convenience)
  safeInset:0.6,         // the safe spot is kept this far in from a platform's ends
  cliffProbe:0.01,       // how far past the end of a ground line to look for more ground, when working out cliffs
  cliffLevel:0.05,       // ground beyond an end counts as carrying on if it is no more than this far below it
  cliffTop:0.02          // his feet count as over a cliff, not against it, from this far below its top
};
const SNAP = WORLD.snap;
const SIN45 = Math.SQRT1_2;

function mkLine(x1, y1, x2, y2){
  const th = Math.atan2(Math.abs(y2-y1), x2-x1);
  return {x1, y1, x2, y2, th, cos:Math.cos(th), sin:Math.sin(th), sign:Math.sign(y2-y1), deg:th*180/Math.PI};
}
function yAt(L, x){ return L.y1 + (L.y2-L.y1)*(x-L.x1)/(L.x2-L.x1); }
function clampX(L, x){ return Math.min(Math.max(x, L.x1), L.x2); }
function isSteep(L, P){ return L.deg > P.walkLimit + 1e-6; }

// ---- Solid ground ----
// A line marked solid is the top of filled ground: rock runs from it down to the bottom of the map.
// Lines not marked solid are scaffolds: you jump up through them and land on top, and there is nothing underneath.
// Where solid ground ends or steps down there is a cliff face, which blocks sideways movement for anyone whose feet are below its top.
// (Called cliffs here because "Wall" is already the name of a climbable surface.)
function buildCliffs(lines){
  const cliffs = [];
  for (const M of lines){
    if (!M.solid) continue;
    for (const side of [-1, 1]){
      const ex = side < 0 ? M.x1 : M.x2, ey = side < 0 ? M.y1 : M.y2, bx = ex + side*WORLD.cliffProbe;   // a point just beyond this end
      let below = -Infinity, covered = false;
      for (const N of lines){
        if (N === M || !N.solid || bx < N.x1 || bx > N.x2) continue;
        const ny = yAt(N, bx);
        if (ny >= ey - WORLD.cliffLevel){ covered = true; break; }      // ground carries on, or steps up: no cliff belongs to this end
        if (ny > below) below = ny;                         // lower ground beyond: the cliff runs down to it
      }
      if (!covered) cliffs.push({x:ex, top:ey, bottom:below, face:side});   // face: which side is open air
    }
  }
  return cliffs;
}
// Stop him at a cliff face. x0 and y0 are where he was before this tick's sideways move.
function hitCliffs(p, P, cliffs, x0, y0){
  if (!cliffs) return;
  const hb = (P.bodyWidth || 0)/2;
  for (const W of cliffs){
    if (y0 >= W.top - WORLD.cliffTop) continue;                       // feet at or above the top: he is over it, not against it
    if (W.face < 0){ if (x0 <= W.x && p.x + hb > W.x){ p.x = W.x - hb; if (p.vx > 0) p.vx = 0; } }
    else           { if (x0 >= W.x && p.x - hb < W.x){ p.x = W.x + hb; if (p.vx < 0) p.vx = 0; } }
  }
}

function derive(P){
  const g = 2*P.jumpHeight/(P.apexTime*P.apexTime);   // gravity while rising with jump held
  const v0 = 2*P.jumpHeight/P.apexTime;                // takeoff speed
  const gFall = g*P.fallMult;
  const tT = P.terminal/gFall, dT = P.terminal*P.terminal/(2*gFall);
  const fallTime = dT < P.jumpHeight ? tT + (P.jumpHeight-dT)/P.terminal : Math.sqrt(2*P.jumpHeight/gFall);
  const air = P.apexTime + fallTime;
  return {g, v0, gFall, air, reachWalk:P.walkSpeed*air, reachRun:P.runSpeed*air};
}

function toward(v, target, stepSize){
  return v < target ? Math.min(v+stepSize, target) : Math.max(v-stepSize, target);
}

// Accelerate, coast or skid toward the speed the input asks for.
function drive(v, dir, top, P, ctrl){
  const accel = P.runSpeed/P.accelTime*ctrl;
  const brake = P.runSpeed/P.stopTime*ctrl;
  const skid  = P.runSpeed/P.turnTime*ctrl;
  if (dir === 0) return toward(v, 0, brake*DT);
  const target = dir*top;
  if (v !== 0 && Math.sign(v) !== dir) return toward(v, target, skid*DT);
  if (Math.abs(v) > top) return toward(v, target, brake*DT);
  return toward(v, target, accel*DT);
}

function makePlayer(x, line){
  const y = yAt(line, x);
  return {x, y, px:x, py:y, vx:0, vy:0, grounded:true, line,
          coyote:0, buffer:0, jumping:false, cutting:false, jump:null, last:null, trails:[],
          lift:0, plift:0, safeX:x, safeLine:line,
          stamina:null, exhaust:0, winded:false, spent:false, crouch:false, mode:'normal', hang:null, climb:null, grabLock:0, lunge:0, prone:0,
          held:[false, false], swap:null,
          duck:0, downWas:false};               // dodge slide time left, and whether down was held last tick      // what is in the left and right hand, and a draw or sheathe in progress
}

function respawn(p, x, line){
  p.x = p.px = x; p.y = p.py = yAt(line, x); p.vx = 0; p.vy = 0;
  p.grounded = true; p.line = line; p.jumping = false; p.cutting = false; p.jump = null; p.coyote = 0; p.buffer = 0;
  p.stamina = null; p.exhaust = 0; p.winded = false; p.spent = false; p.crouch = false; p.mode = 'normal'; p.hang = null; p.climb = null; p.grabLock = 0; p.lunge = 0; p.prone = 0; p.swap = null; p.duck = 0; p.downWas = false;
}

// ---- Hands ----
// Each hand is either holding gear or free. A hand in the middle of a draw or sheathe is neither.
function freeHands(p){
  let n = 0;
  for (let i = 0; i < 2; i++) if (!p.held[i] && !(p.swap && p.swap.hand === i)) n++;
  return n;
}
// How many free hands each surface needs to hold onto at all.
function handsNeeded(C){ return C.kind === 'Wall' ? 2 : C.kind === 'Rope' ? 1 : 0; }

// ---- Stamina, ledge grabs and climbing ----
// Modes: 'normal' (everything above), 'hang' (holding a ledge end), 'pull' (pulling up onto it), 'climb' (on a ladder, rope or wall).
// Hanging and climbing are anchored but not grounded.
const HANG_OUT = BODY.hangOut, HANG_IN = BODY.hangIn, GRAB_X = BODY.catchOut, GRAB_IN = BODY.catchUnder;

function letGo(p){
  p.mode = 'normal'; p.hang = null; p.climb = null; p.grounded = false; p.line = null;
  p.vx = 0; p.vy = 0; p.coyote = 0; p.jumping = false; p.cutting = false; p.grabLock = BODY.dropGrabLock;
}
// The bar hit zero: you go down for a moment, and you drop off anything you were holding.
function tire(p, P){
  p.stamina = 0; p.exhaust = P.exhaustTime; p.winded = true;
  if (p.mode !== 'normal') letGo(p);
}
// A ledge is the end of a line with nothing to stand on beyond it: either nothing joins there,
// or the only thing joining is a slope too steep to stand on that drops away from it (a crest).
function endIsOpen(lines, M, side, P){
  const ex = side < 0 ? M.x1 : M.x2, ey = side < 0 ? M.y1 : M.y2;
  for (const L of lines){
    if (L === M) continue;
    const at1 = Math.abs(L.x1-ex) < WORLD.join && Math.abs(L.y1-ey) < WORLD.join;
    const at2 = Math.abs(L.x2-ex) < WORLD.join && Math.abs(L.y2-ey) < WORLD.join;
    if (!at1 && !at2) continue;
    if (isSteep(L, P) && (at1 ? L.y2 : L.y1) < ey - WORLD.join) continue;
    return false;
  }
  return true;
}
// Hands reach your own height plus a little: a taller character catches higher ledges.
// How tall he is right now. Nothing in the lab checks this yet; hazards and enemies will.
function heightNow(p, P){ return p.duck > 0 ? P.duckHeight : P.charHeight; }
function reachOf(P){ return P.charHeight + P.armReach; }
// The ledge end, if any, that your hands can reach from feet height y.
// Normally hands go up: a ledge end within reach above you and close to you sideways.
// In a lunge the body can lean forward, so anything within reach inside that lean angle counts too.
function findLedge(p, P, lines, y){
  if (freeHands(p) < 1 || p.grabLock > 0 || p.stamina <= 0) return null;
  const R = reachOf(P);
  for (const M of lines){
    if (isSteep(M, P)) continue;
    for (const side of [-1, 1]){
      const ex = side < 0 ? M.x1 : M.x2, ey = side < 0 ? M.y1 : M.y2;
      const dy = ey - y;
      if (dy < BODY.catchMinDrop || dy > R) continue;
      const out = (p.x - ex)*side;              // how far outside the end you are
      let ok = out >= -GRAB_IN && out <= GRAB_X;
      if (!ok && p.lunge && side === -p.lunge){  // a lunge only adds the edge facing you
        const fwd = (ex - p.x)*p.lunge, dist = Math.hypot(fwd, dy);
        ok = fwd >= 0 && dist <= R && fwd/dist <= P.lungeLean;
      }
      if (ok && endIsOpen(lines, M, side, P)) return {line:M, side, ex, ey};
    }
  }
  return null;
}
function startHang(p, g, y){
  p.mode = 'hang'; p.hang = {line:g.line, side:g.side, prog:0, y0:y}; p.lunge = 0;
  p.y = y; p.vx = 0; p.vy = 0; p.grounded = false; p.line = null;      // x eases to the ledge over the next few ticks
  p.jumping = false; p.cutting = false; p.jump = null; p.coyote = 0;
}
function holdStep(p, P, inp, lines){
  const effort = P.climbEffort*P.weight;        // stamina per second of climbing at difficulty x1
  if (p.mode === 'hang' || p.mode === 'pull'){
    const L = p.hang.line, side = p.hang.side, ex = side < 0 ? L.x1 : L.x2, ey = side < 0 ? L.y1 : L.y2;
    const f = freeHands(p);
    if (!inp.grab || inp.down || f < 1){ letGo(p); return; }   // grabbing is a held button: release it and you drop
    if (p.mode === 'hang'){
      p.x += (ex + side*HANG_OUT - p.x)*BODY.hangEase;
      p.stamina -= P.hangCost*P.weight*DT;
      if (inp.jumpPressed || inp.up) p.mode = 'pull';
    }
    if (p.mode === 'pull'){
      // A pull-up takes a fixed time and drains stamina every tick of it. Run dry partway and you fail.
      p.hang.prog += DT/P.pullTime*(f === 1 ? P.oneHand : 1);   // one-handed is slower, so it costs more in total
      p.stamina -= P.pullCost*P.weight*DT;
      const t = Math.min(1, p.hang.prog);
      p.y = p.hang.y0 + (ey - p.hang.y0)*t;
      p.x = ex + side*HANG_OUT - side*(HANG_OUT+HANG_IN)*Math.max(0, (t-BODY.pullSlideFrom)/(1-BODY.pullSlideFrom));
      if (t >= 1){
        p.mode = 'normal'; p.hang = null; p.grounded = true; p.line = L;
        p.x = ex - side*HANG_IN; p.y = yAt(L, p.x); p.vx = 0; p.vy = 0;
        return;
      }
    }
    if (p.stamina <= 0) tire(p, P);
    return;
  }
  // On a climbable.
  const C = p.climb, mv = (inp.up ? 1 : 0) - (inp.down ? 1 : 0), f = freeHands(p), ladder = C.kind === 'Ladder';
  if (f < handsNeeded(C)){ letGo(p); return; }                 // not enough free hands to hold on
  if (inp.jumpPressed){
    // Jump always jumps, at full height and full jump cost: away from the climbable if a direction is held,
    // straight up if not. Down plus jump just lets go.
    const d = derive(P);
    letGo(p);
    if (!inp.down){
      p.vx = inp.dir*(inp.run ? P.runSpeed*BODY.climbJumpSpeed : P.walkSpeed); p.vy = d.v0; p.jumping = true; p.grabLock = BODY.jumpGrabLock;
      p.stamina -= P.jumpCost;
      if (p.stamina <= 0){ p.stamina = 0; p.spent = true; }
      p.jump = {x0:p.x, y0:p.y, peak:p.y, ticks:0, trail:[{x:p.x, y:p.y}]};
      p.trails.push(p.jump.trail); if (p.trails.length > 3) p.trails.shift();
    }
    return;
  }
  if (!ladder && !inp.grab){ letGo(p); return; }               // ropes and walls need the grab held; a ladder holds you
  // Speed: a ladder is mostly legs, a rope is mostly arms.
  let speed = P.climbSpeed;
  if (ladder) speed *= f === 2 ? 1 : f === 1 ? (1 + P.oneHand)/2 : P.oneHand;
  else if (f === 1) speed *= P.oneHand;
  const mult = P['mult'+C.kind];                // difficulty is a stamina cost multiplier
  let cost = (mv !== 0 ? effort : P.hangCost*P.weight)*mult;
  const canSprint = inp.run && !p.winded && (ladder ? f >= 1 : f === 2);
  if (canSprint && mv > 0){ speed *= P.climbSprint; cost *= P.climbSprintCost; }          // sprinting up: faster, worse value per tile
  if (inp.run && mv < 0 && C.kind !== 'Wall'){ speed = P.climbSpeed*BODY.slideDownSpeed; cost = P.hangCost*P.weight*mult; }   // sliding down: fast and cheap
  p.y = Math.min(C.y2, Math.max(C.y1, p.y + mv*speed*DT));
  if (ladder && mv === 0 && f === 2){
    p.stamina = Math.min(P.staminaMax, p.stamina + P.walkRegen*DT);   // both hands free on a ladder: hook in and rest
  } else p.stamina -= cost*DT;
  if ((mv > 0 && p.y >= C.y2) || (mv < 0 && p.y <= C.y1)){
    // At an end: step onto a platform if one is there.
    for (const L of lines){
      if (isSteep(L, P) || C.x < L.x1 - BODY.climbReach || C.x > L.x2 + BODY.climbReach) continue;
      const nx = clampX(L, C.x);
      if (Math.abs(yAt(L, nx) - p.y) < WORLD.stepOff){
        p.mode = 'normal'; p.climb = null; p.grounded = true; p.line = L; p.x = nx; p.y = yAt(L, nx); p.vx = 0; p.vy = 0;
        return;
      }
    }
  }
  if (p.stamina <= 0) tire(p, P);
}
function tail(p, P){
  // Drawing only: lift the circle so it sits on a slope instead of sinking into it.
  const liftTarget = p.grounded ? RADIUS/p.line.cos - RADIUS : 0;
  p.lift += (liftTarget - p.lift)*0.35;
  if (p.winded && p.stamina >= P.staminaMax*BODY.windedUntil) p.winded = false;
  if (p.grounded && !isSteep(p.line, P)){
    const L = p.line;
    p.safeX = (L.x2-L.x1 > 2*WORLD.safeInset) ? Math.min(Math.max(p.x, L.x1+WORLD.safeInset), L.x2-WORLD.safeInset) : (L.x1+L.x2)/2;
    p.safeLine = L;
  }
  if (p.y < WORLD.respawnBelow) respawn(p, p.safeX, p.safeLine);
}

function step(p, P, inp, lines, worldW, climbs, cliffs){
  const d = derive(P);
  p.px = p.x; p.py = p.y; p.plift = p.lift;
  const downPressed = !!inp.down && !p.downWas;   // a fresh press of down, not a hold
  p.downWas = !!inp.down;
  if (p.mode !== 'normal' || !p.grounded) p.duck = 0;
  if (p.stamina == null || p.stamina > P.staminaMax) p.stamina = P.staminaMax;
  if (p.grabLock > 0) p.grabLock -= DT;
  // Draw or sheathe: takes a moment, and that hand is useless until it finishes.
  if (p.swap){ p.swap.t -= DT; if (p.swap.t <= 0){ p.held[p.swap.hand] = !p.held[p.swap.hand]; p.swap = null; } }
  else if (inp.swapHand === 0 || inp.swapHand === 1) p.swap = {hand:inp.swapHand, t:P.drawTime, total:P.drawTime};
  if (p.exhaust > 0){
    // Down for a moment: no input counts, and anything held is dropped.
    p.exhaust -= DT;
    inp = {dir:0, run:false, jumpHeld:false, jumpPressed:false, up:false, down:false};
    if (p.mode !== 'normal') letGo(p);
  }
  if (p.prone > 0){
    // Face down after a missed lunge: nothing you press counts until you are up.
    p.prone -= DT;
    inp = {dir:0, run:false, jumpHeld:false, jumpPressed:false, up:false, down:false, grab:false};
  }
  if (p.mode !== 'normal'){ holdStep(p, P, inp, lines); tail(p, P); return; }

  // Getting onto a climbable: hold grab beside it. A ladder also takes plain up. From the platform at the top,
  // press down (ladder) or hold grab and press down (rope, wall) to climb down.
  if (climbs && p.grabLock <= 0 && p.exhaust <= 0 && p.stamina > 0 && (inp.grab || inp.up || (inp.down && p.grounded))){
    for (const C of climbs){
      if (Math.abs(p.x - C.x) > BODY.climbReach) continue;
      const ladder = C.kind === 'Ladder';
      if (freeHands(p) < handsNeeded(C)) continue;
      const within = p.y >= C.y1 - WORLD.stepOff && p.y < C.y2 - WORLD.join;
      const goOn = within && (inp.grab || (ladder && inp.up));
      const goDown = inp.down && p.grounded && Math.abs(p.y - C.y2) < WORLD.stepOff && (ladder || inp.grab);   // from the platform at the top
      if (goOn || goDown){
        p.mode = 'climb'; p.climb = C; p.x = C.x; p.vx = 0; p.vy = 0; p.grounded = false; p.line = null;
        p.jumping = false; p.cutting = false; p.jump = null; p.coyote = 0; p.buffer = 0;
        tail(p, P); return;
      }
    }
  }
  // Lunge: in the air, or sliding on a slope too steep to stand on, hold grab and a direction and press jump.
  // It costs stamina, you lose air control, and if you catch nothing you land face down.
  if ((!p.grounded || isSteep(p.line, P)) && !p.lunge && inp.jumpPressed && inp.grab && inp.dir !== 0 && p.coyote <= 0
      && freeHands(p) >= 1 && p.stamina > P.lungeCost){
    p.lunge = inp.dir; p.stamina -= P.lungeCost;
    inp = Object.assign({}, inp, {jumpPressed:false});     // that press was the lunge, not a jump
  }
  // On the ground with a ledge end within reach: grab catches it without jumping.
  if (p.grounded && inp.grab && p.exhaust <= 0 && p.prone <= 0){
    const g = findLedge(p, P, lines, p.y);
    if (g){ startHang(p, g, p.y); tail(p, P); return; }
  }
  if (p.grounded && p.lunge){ p.lunge = 0; p.prone = P.proneTime; }   // lunged from a slide and caught nothing

  const run = inp.run && !p.winded;             // too winded to sprint until a quarter of the bar is back
  const baseTop = run ? P.runSpeed : P.walkSpeed;
  let ax = p.x, ay = p.y;      // where this tick's airborne motion starts from

  if (p.grounded){
    const L = p.line;
    if (isSteep(L, P)){
      // Too steep to stand on: gravity pulls you downslope, friction resists, input only nudges.
      const pull = d.gFall*L.sin*(1-P.slideFriction);
      const steer = inp.dir*P.slideSteer*P.runSpeed/P.accelTime;
      p.vx += (-L.sign*pull + steer)*DT;
      p.vx = Math.max(-P.terminal, Math.min(P.terminal, p.vx));
      p.duck = 0;
    } else if (p.duck > 0){
      // Dodge slide: low to the ground, no steering, bleeding off speed. It ends on its own, when it runs out of speed, or with a jump.
      p.duck -= DT;
      p.vx *= Math.pow(P.duckKeep, DT/P.duckTime);
      if (p.duck <= 0 || Math.abs(p.vx) < BODY.duckStopShare*P.walkSpeed) p.duck = 0;
    } else if (downPressed && Math.abs(p.vx) > P.walkSpeed + BODY.sprintShare*(P.runSpeed - P.walkSpeed) && p.stamina > P.duckCost && p.exhaust <= 0 && P.duckTime > 0){
      // Press down while sprinting: drop into a dodge slide.
      p.duck = P.duckTime; p.stamina -= P.duckCost;
    } else {
      // Walkable: no gravity along the surface. Slopes only change top speed.
      const f = L.sin/SIN45;
      const heading = inp.dir !== 0 ? inp.dir : Math.sign(p.vx);
      let top = baseTop;
      if (L.sign !== 0 && heading !== 0){
        if (heading*L.sign > 0) top *= Math.max(BODY.minUphillShare, 1 - P.upCost/100*f);
        else top *= 1 + P.downBoost/100*f;
      }
      p.vx = drive(p.vx, inp.dir, top, P, 1);
    }
    p.x += p.vx*L.cos*DT;
    if (p.x < WORLD.edgeMargin){ p.x = WORLD.edgeMargin; p.vx = 0; }
    if (p.x > worldW-WORLD.edgeMargin){ p.x = worldW-WORLD.edgeMargin; p.vx = 0; }
    hitCliffs(p, P, cliffs, p.px, p.py);

    if (p.x < L.x1 || p.x > L.x2){
      // The middle of the body is past the end of this line.
      const hw = isSteep(L, P) ? 0 : (P.footWidth || 0)/2;   // half the footing; none while sliding
      const yEdge = yAt(L, p.x);                              // where the surface would continue
      const xe = clampX(L, p.x);                              // the end you just passed
      const yEnd = yAt(L, xe);                                // and its height
      // Ground snapping: if another line carries on from here, stay glued to it.
      let best = null, bestD = SNAP;
      for (const M of lines){
        if (M === L || p.x < M.x1 || p.x > M.x2) continue;
        // Compare heights at the joint itself, not where you are now: at speed you can be a third of a tile
        // up a steep slope by the first tick past the joint.
        const dy = Math.abs(yAt(M, clampX(M, xe)) - yEnd);
        if (dy <= bestD){ best = M; bestD = dy; }
      }
      if (best){ p.line = best; p.y = yAt(best, p.x); }
      else if (p.x >= L.x1 - hw && p.x <= L.x2 + hw){
        p.y = yEnd;                                           // part of the footing is still on the ledge
      } else {
        // Fully off this line. A line the footing reaches across a small gap still counts as ground.
        let reach = null, reachD = SNAP, reachY = 0;
        for (const M of lines){
          if (M === L || isSteep(M, P) || p.x < M.x1 - hw || p.x > M.x2 + hw) continue;
          const my = yAt(M, clampX(M, p.x)), dy = Math.abs(my - yEnd);
          if (dy <= reachD){ reach = M; reachD = dy; reachY = my; }
        }
        if (reach){ p.line = reach; p.y = reachY; }
        else {
          // Nothing underfoot: leave the ground carrying the slope's direction, and start the coyote clock.
          const sp = p.vx;
          p.vx = sp*L.cos; p.vy = sp*L.sin*L.sign;
          p.y = hw > 0 ? yEnd : yEdge; p.grounded = false; p.line = null; p.duck = 0;
          p.coyote = isSteep(L, P) ? 0 : P.coyoteMs/1000;
        }
      }
    } else {
      p.y = yAt(L, p.x);
    }
    ax = p.x; ay = p.y;
  } else {
    if (!p.lunge) p.vx = drive(p.vx, inp.dir, baseTop, P, P.airControl);   // no steering once you have lunged
    p.x += p.vx*DT;
    if (p.x < WORLD.edgeMargin){ p.x = WORLD.edgeMargin; p.vx = 0; }
    if (p.x > worldW-WORLD.edgeMargin){ p.x = worldW-WORLD.edgeMargin; p.vx = 0; }
    hitCliffs(p, P, cliffs, p.px, p.py);
  }

  // Jump: a press counts now, or for a short while before landing (buffer),
  // and the ground counts for a short while after leaving it (coyote).
  if (inp.jumpPressed) p.buffer = P.bufferMs/1000;
  const wants = inp.jumpPressed || p.buffer > 0;
  const sliding = p.grounded && isSteep(p.line, P);
  const can = p.grounded ? (!sliding || P.slideJump > 0) : p.coyote > 0;
  let justJumped = false;
  if (wants && can){
    justJumped = true;
    p.stamina -= P.jumpCost;
    if (p.stamina <= 0){ p.stamina = 0; p.spent = true; }   // the bar is empty: you go down when you land
    if (p.grounded){
      const L = p.line, sp = p.vx;
      if (sliding && P.slideJump === 1){
        // Kick away: a low hop that pushes you off the slope, so you cannot climb it by hopping.
        p.vy = d.v0*L.cos;
        p.vx = -L.sign*Math.max(Math.abs(sp*L.cos), P.walkSpeed);
      } else {
        // Jumps go straight up at full height, whatever the slope.
        p.vy = d.v0; p.vx = sp*L.cos;
      }
    } else {
      p.vy = d.v0;
    }
    p.grounded = false; p.line = null; p.coyote = 0; p.buffer = 0; p.jumping = true; p.cutting = false; p.duck = 0;
    p.jump = {x0:p.x, y0:p.y, peak:p.y, ticks:0, trail:[{x:p.x, y:p.y}]};
    p.trails.push(p.jump.trail);
    if (p.trails.length > 3) p.trails.shift();
  } else {
    if (!p.grounded) p.coyote -= DT;
    p.buffer -= DT;
  }

  // Vertical: three gravities. Rising with jump held, rising after letting go, and falling.
  if (!p.grounded){
    // The takeoff tick always counts as held, so even the fastest tap gets a minimum hop.
    const held = justJumped || (p.jumping && inp.jumpHeld);
    if (p.jumping && !held){
      p.jumping = false; p.cutting = true;                // letting go is final for this jump
      if (P.hardCut && p.vy > 0) p.vy = 0;                // hard cut: stop rising the instant jump is released
    }
    let g;
    if (p.vy > 0) g = p.cutting ? d.g*P.cutMult : d.g;
    else g = d.gFall;
    const v1 = Math.max(p.vy - g*DT, -P.terminal);
    const y1 = ay + 0.5*(p.vy+v1)*DT;

    // You land on a line from above. Scaffolds can also be jumped up through; solid ground cannot be reached from below at all,
    // because its cliffs keep you out from the sides.
    // You land if any part of your footing comes down on the line, not only the middle of the body.
    let land = null, landY = -Infinity;
    const hwAir = (P.footWidth || 0)/2;
    for (const M of lines){
      const hw = isSteep(M, P) ? 0 : hwAir;
      if (p.x < M.x1 - hw || p.x > M.x2 + hw) continue;
      const my = yAt(M, clampX(M, p.x));
      if (ay >= yAt(M, clampX(M, ax)) - 1e-9 && y1 <= my && my > landY){ land = M; landY = my; }
    }
    // Ledge grab: hold the grab button, and when a ledge end comes within reach of your hands you catch it.
    const grab = (!land && inp.grab) ? findLedge(p, P, lines, y1) : null;
    if (p.jump) p.jump.ticks++;
    if (grab){
      startHang(p, grab, y1);
    } else if (land){
      const hv = p.vx;
      p.y = landY; p.grounded = true; p.line = land; p.jumping = false; p.cutting = false;
      // On a slide, part of your fall turns into speed down the slope. On walkable ground you keep your sideways speed.
      p.vx = isSteep(land, P) ? hv*land.cos + v1*land.sin*land.sign*(1-P.slideFriction) : hv;
      p.vy = 0;
      if (p.lunge){ p.lunge = 0; p.prone = P.proneTime; }      // a lunge that caught nothing ends face down
      if (p.spent){ p.spent = false; tire(p, P); }
      if (p.jump){
        p.jump.trail.push({x:p.x, y:p.y});
        p.last = {height:p.jump.peak-p.jump.y0, ticks:p.jump.ticks, air:p.jump.ticks*DT,
                  dist:Math.abs(p.x-p.jump.x0), drop:p.jump.y0-p.y};
        p.jump = null;
      }
    } else {
      p.y = y1; p.vy = v1;
      if (p.jump){
        if (p.y > p.jump.peak) p.jump.peak = p.y;
        p.jump.trail.push({x:p.x, y:p.y});
      }
    }
  }

  // Stamina on the ground: running drains it slowly, walking or standing recovers it, crouching still recovers it fast.
  p.crouch = false;
  if (p.grounded && p.mode === 'normal' && p.exhaust <= 0 && p.prone <= 0){
    const steep = isSteep(p.line, P);
    const running = run && inp.dir !== 0 && Math.abs(p.vx) > P.walkSpeed + BODY.runDrainAbove && !steep;
    p.crouch = inp.down && inp.dir === 0 && Math.abs(p.vx) < BODY.crouchBelow && !steep;
    if (p.duck > 0){ p.crouch = false; }                     // sliding: neither draining nor recovering
    else if (running){ p.stamina -= P.runDrain*DT; if (p.stamina <= 0) tire(p, P); }
    else p.stamina = Math.min(P.staminaMax, p.stamina + (p.crouch ? P.crouchRegen : P.walkRegen)*DT);
  }
  tail(p, P);
}
/*SIM-END*/