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
- [x] **Stage/wave structure with a win condition + boss wave.** `WaveSpawner` restructured into 15 discrete timed waves (difficulty scales by wave number, not raw time) with intermission gaps; every 5th wave is a boss wave (single tough enemy, no other spawns). Clearing wave 15 triggers a Victory screen with a coin bonus. Confirmed working on-device.
- [x] **Multiple unlockable shooter types.** `ShooterManager.cs` defines 3 types (Standard/free, Rapid/150 coins, Heavy/300 coins) with distinct damage/fire-rate/speed/color/scale, selected from a "SHOOTERS" panel on the main menu, applied at run start. Confirmed working on-device, including persistence.
- [x] **Chest/reward system on milestones.** Every 3rd wave completed awards a bonus (scaling with wave number) added to run score, shown via a "CHEST! +N COINS" screen banner with a distinct sound. Confirmed working on-device.

## P2 — Depth & juice

- [x] **More buff types.** Added piercing bullets (permanent, bullets pass through and hit multiple enemies), wall heal (instant repair), and a temporary (8s) wall shield/invulnerability that also blocks ranged-enemy damage, not just melee. Skipped "shooter move speed" — doesn't map to the drag-to-position control scheme (no speed/acceleration concept to buff). Confirmed working on-device.
- [x] **Particle VFX.** Muzzle flash on fire, burst on enemy death, sparkle on pickup collection — built procedurally via `ParticleFX.cs` (Unity's `ParticleSystem` API, no texture assets needed). Confirmed working on-device.
- [x] **Haptic feedback.** Vibration pulse on wall hits and enemy kills via `Handheld.Vibrate()`. Confirmed working on-device. (Also fixed a real bug found in this pass: `ParticleFX`'s `Shader.Find` returned null on-device since the shader was only referenced from code, never a real asset — silently stripped from the build — which threw mid-`Weapon.Fire()` and blocked shooting entirely. Switched to the already-asset-referenced URP/Lit shader and reordered all VFX calls to happen after critical gameplay logic everywhere.)
- [x] **Merge mechanic for shooters.** Full Rush-Royale-style squad system: up to 5 shooter units on fixed slots, draggable, spawned via new "Shooter Crate" pickups, drag one onto a matching type/tier unit to merge into a stronger tier (damage scales x2/x4, visuals scale up, max tier 3). Fire Rate/Damage/Piercing buffs now apply squad-wide via `SquadManager` instead of a single shooter. Confirmed working on-device. (Also fixed a real bug found in this pass: `ParticleFX.SpawnBurst` set `main.duration` on a `ParticleSystem` that had already auto-started via `playOnAwake`, throwing a "system still playing" warning on every shot — now stopped/cleared immediately after `AddComponent` before configuring.)

## P3 — Live-ops / long-tail (later)

- [x] **Daily rewards.** `DailyRewardManager` tracks a 7-day claim streak via PlayerPrefs (coins: 20/30/40/60/80/100/200, resets to Day 1 if a day is missed). New "DAILY REWARD" panel on the main menu shows all 7 days with the claimable one highlighted, claim button disables once claimed for the day. Confirmed working on-device.
- [ ] **Leaderboards.**
- [ ] **Achievements.**
- [ ] **Monetization hooks** (ads/IAP) — only if/when you decide to publish commercially.

---

## Already implemented (context, not actionable)

Core loop, drag-to-move shooter, straight-fire weapon, enemy wave spawner with difficulty ramp, defense wall with HP/flash/shake, HUD (score, wall HP bar, game over, restart), 3 stacking permanent buffs (fire rate, damage, multi-shot) with floating-text feedback, colored flat-shaded materials, ground plane, object pooling throughout.
