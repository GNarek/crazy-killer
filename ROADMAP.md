# Crazy Killer — Feature Roadmap

Research-backed feature list for this vertical auto-shooter, based on genre conventions from Squad Busters, Mob Control, and Rush Royale, plus general endless-runner/auto-shooter design patterns.

## How this file is used

- Items are grouped by priority tier (P0 = do next, P3 = long-tail).
- Within each tier, work top to bottom.
- Workflow: tell Claude "check ROADMAP.md and implement the next item" — it implements one unchecked item, marks it `[x]`, and stops for you to test in Unity before continuing.
- Do not implement multiple items in one pass unless explicitly asked.

---

## P0 — Foundation (do these next)

- [x] **Build and test on a real Android device/emulator.** Built and installed on a physical Android phone. Fixed along the way: Android package/SDK config, a pooling bug where `Health`/`EnemyController`/`DeathPop` all implement `IPoolable` but only one was getting reset on respawn (caused frozen "ghost" enemies that bullets passed through), a spawn-rate/concurrent-enemy-cap balance issue causing pile-ups, and camera framing (ground/FOV/tilt) so the road fills the screen with the wall near the bottom and enemies entering visibly from the top edge.
- [x] **Sound effects + music.** Added procedurally-generated (no external audio files) shoot/hit/enemy-death/pickup/wall-hit/game-over SFX plus a quiet looping ambient pad, via `RetroAudioSynth` + `AudioManager`. Confirmed working on-device.
- [x] **Main menu, pause, and settings shell.** New `MainMenu` scene (title, Play, persistent mute toggle) boots first, gameplay scene now has a pause button/panel (Resume/Restart/Main Menu/Mute) and a Main Menu button on the game-over screen. Confirmed working on-device.
- [x] **Enemy variety.** Added 3 new types on top of the original: fast/fragile Runner, slow/tanky Tank, and a Ranged attacker that stops before the wall and fires projectiles back (`RangedAttacker.cs`) instead of just walking in — genuine behavior variety, not just stat scaling. Confirmed working on-device.

## P1 — Core progression (defines the genre)

- [x] **Meta-currency + permanent upgrades between runs.** Coins earned 1:1 with score, banked via PlayerPrefs on Game Over. Main-menu "UPGRADES" panel spends them on permanent Damage/Fire Rate/Wall HP levels (escalating cost curve), applied automatically at the start of each run. Confirmed working on-device, including persistence across app restarts.
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
