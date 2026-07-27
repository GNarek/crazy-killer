# Crazy Killer — Feature Roadmap

Research-backed feature list for this vertical auto-shooter, based on genre conventions from Squad Busters, Mob Control, and Rush Royale, plus general endless-runner/auto-shooter design patterns.

## How this file is used

- Items are grouped by priority tier (P0 = do next, P3 = long-tail).
- Within each tier, work top to bottom.
- Workflow: tell Claude "check ROADMAP.md and implement the next item" — it implements one unchecked item, marks it `[x]`, and stops for you to test in Unity before continuing.
- Do not implement multiple items in one pass unless explicitly asked.

---

## P0 — Foundation (do these next)

- [ ] **Build and test on a real Android device/emulator.** Everything so far has only run in the Unity Editor. Need to verify touch dragging, screen scaling (portrait, various aspect ratios), and performance actually work outside the Editor before adding more content.
- [ ] **Sound effects + music.** Currently silent. Needs: shoot SFX, enemy hit/death SFX, pickup SFX, wall-hit SFX, game-over stinger, looping background music. (Endless-runner/shooter research: audio feedback is one of the most-cited "juice" gaps in prototype-stage games.)
- [ ] **Main menu, pause, and settings shell.** Right now the game boots straight into gameplay with no way to pause, restart deliberately, or mute audio. Every reference game (Squad Busters, Mob Control, Rush Royale) wraps the core loop in a menu shell.
- [ ] **Enemy variety.** Currently one enemy type. Add 2–3 more (e.g., fast/low-HP, slow/tanky, ranged-attacker) using the existing `EnemyDefinition` ScriptableObject system — architecture already supports this without code changes. Scrolling-shooter design research specifically calls out enemy-ability variety (not just stat scaling) as the standard way to raise difficulty meaningfully.

## P1 — Core progression (defines the genre)

- [ ] **Meta-currency + permanent upgrades between runs.** Earn coins per run (from kills/score), spend them on persistent upgrades (base damage, fire rate, wall HP) that carry over after Game Over. This is the backbone progression loop in all three reference games.
- [ ] **Stage/wave structure with a win condition + boss wave.** Currently the game is endless-only. Add "survive N waves" structure with a boss enemy every N waves (Mob Control's "Boss Levels" pattern) — gives runs a shape instead of just difficulty ramping forever.
- [ ] **Multiple unlockable shooter types.** You already asked for the architecture to support this (`ShooterDefinition` exists but is unused). Add 2–3 shooter variants with different stats/visuals, unlockable with meta-currency — mirrors Squad Busters' "squad" identity.
- [ ] **Chest/reward system on milestones.** Reward chest every N waves or on run-end score thresholds, granting coins/buffs — matches the chest-opening loop core to Rush Royale and Squad Busters' retention design.

## P2 — Depth & juice

- [ ] **More buff types.** Piercing bullets (hit multiple enemies in a line), wall heal, temporary shield/invulnerability, shooter move speed. Slots into the existing `BuffDefinition`/`IBuffEffect` system with no architecture changes.
- [ ] **Particle VFX.** Muzzle flash on fire, burst on enemy death, sparkle on pickup collection — currently only have scale-pop + hit-flash, no real particles.
- [ ] **Haptic feedback.** Android vibration pulse on wall hits and enemy kills — cheap, high-impact mobile-specific juice.
- [ ] **Merge mechanic for shooters.** Collecting a duplicate shooter-type pickup merges into a stronger version instead of being wasted (Rush Royale's signature mechanic) — bigger design decision, revisit after P0/P1 are solid.

## P3 — Live-ops / long-tail (later)

- [ ] **Daily rewards.**
- [ ] **Leaderboards.**
- [ ] **Achievements.**
- [ ] **Monetization hooks** (ads/IAP) — only if/when you decide to publish commercially.

---

## Already implemented (context, not actionable)

Core loop, drag-to-move shooter, straight-fire weapon, enemy wave spawner with difficulty ramp, defense wall with HP/flash/shake, HUD (score, wall HP bar, game over, restart), 3 stacking permanent buffs (fire rate, damage, multi-shot) with floating-text feedback, colored flat-shaded materials, ground plane, object pooling throughout.
