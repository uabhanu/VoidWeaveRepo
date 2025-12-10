# 🌌 VOID WEAVE

**Tactical Bullet-Hell Tower Defence**

Platform: PC (Unity 6) | Type: Single-Player Action Strategy | Status: 🔧 Prototype Phase 1

> *"Weave Your Defence. Navigate the Chaos."*

---

## 📖 Table of Contents

- [Overview](#overview)
- [Core Concept](#core-concept)
- [Game Loop](#game-loop)
- [Player Mechanics](#player-mechanics)
- [Turret System](#turret-system)
- [Enemy Types](#enemy-types)
- [Progression](#progression)
- [Technical Stack](#technical-stack)
- [Roadmap](#roadmap)

---

## Overview

**Void Weave** is a tactical bullet-hell tower defence game where players strategically deploy autonomous turret nodes while dodging geometric enemy patterns in a minimalist sci-fi arena.

**Key Features:**
- Hybrid Gameplay: Strategic turret placement meets skill-based bullet-hell dodging
- Wave-Based Survival: Defend against increasingly complex enemy formations
- Tactical Decisions: Limited resources force meaningful deployment choices
- High-Performance ECS: Built on Unity's Entity Component System
- Geometric Aesthetic: Clean, minimalist visual design

---

## Core Concept

Players face waves of geometric enemies in a contained arena. Success requires mastering three pillars:

**DODGE** → Navigate bullet-hell patterns with precise movement and dashing

**DEPLOY** → Place turret nodes in tactical positions during preparation

**ADAPT** → React to enemy compositions and adjust strategies

---

## Game Loop

### Three-Phase Cycle

**1. PREPARATION PHASE (30 seconds)**
- Navigate the arena freely
- Deploy or relocate turret nodes
- Review enemy intel for next wave
- Spend resources on upgrades

**2. COMBAT WAVE (60+ seconds)**
- Geometric enemies spawn in patterns
- Player dodges bullet-hell attacks
- Deployed turrets engage automatically
- Collect resources from defeated enemies

**3. UPGRADE PHASE**
- Review wave performance
- Spend collected resources
- Unlock new turret types
- Upgrade existing nodes
- Difficulty scales up

Then loop back to Preparation.

---

## Player Mechanics

### Movement
- **WASD** - 8-directional movement (5 units/sec)
- **Responsive** - Instant direction changes
- **No Acceleration** - Direct control for precision

### Dash Ability
- **Keybind:** Spacebar / Shift
- **Distance:** 3 units
- **Duration:** 0.2 seconds
- **Cooldown:** 1 second
- **Special:** Invincibility frames during dash

### Deployment
- **Keybind:** E / Left Click
- **Range:** 2 units from player
- **Max Nodes:** 5 active (base)
- **Cost:** Resources from enemies
- **Restrictions:** Cannot place during combat or overlapping nodes

---

## Turret System

### Three Core Types

**🔺 STRIKER - Tank Killer**
- Damage: High (20)
- Fire Rate: Slow (0.5/sec)
- Range: 5 units
- Special: Single-target precision
- Cost: 100 resources
- Best Against: Square enemies

**🔷 SCATTER - Crowd Control**
- Damage: Low (5 per projectile)
- Fire Rate: Fast (3/sec)
- Range: 4 units
- Special: 5-projectile spread (30° cone)
- Cost: 100 resources
- Best Against: Triangle swarms

**▬ BEAM - Formation Breaker**
- Damage: Medium (12)
- Fire Rate: Continuous
- Range: 6 units
- Special: Pierces 3 enemies
- Cost: 150 resources
- Best Against: Line formations

### Upgrade Path

Level 1 (Base) → Level 2 (+50%) → Level 3 (+100%)

---

## Enemy Types

### 🔻 Triangle - The Swarm
- **Health:** 50 HP
- **Speed:** 4 units/sec (Fast)
- **Damage:** Collision only
- **Reward:** 5 resources
- **Behaviour:** Erratic movement, spawns in groups of 10-20
- **Counter:** Scatter turrets + dash through gaps

### 🟥 Square - The Tank
- **Health:** 200 HP
- **Speed:** 1.5 units/sec (Slow)
- **Damage:** 10 per projectile (shoots back!)
- **Fire Rate:** 0.5/sec
- **Reward:** 25 resources
- **Behaviour:** Slow movement, shoots projectiles, high health
- **Counter:** Striker turrets + kiting

### ▬ Line - The Formation
- **Health:** 100 HP
- **Speed:** 2.5 units/sec (Medium)
- **Damage:** Collision only
- **Reward:** 15 resources
- **Behaviour:** Moves in straight formations, spawns in groups of 5-10
- **Counter:** Beam turrets + perpendicular dodging

---

## Progression

### Resource Economy

**EARN:**
- Triangle Kill: 5
- Square Kill: 25
- Line Kill: 15
- Wave Complete: +50 bonus
- Perfect Wave (no damage): +50 bonus

**SPEND:**
- Deploy Turret: 100-150
- Upgrade Turret: 50-100
- Unlock New Type: 200
- Increase Max Nodes: 150

### Wave Scaling

| Wave | Enemies | Duration | Health Multiplier |
|------|---------|----------|-------------------|
| 1    | 20      | 60s      | 1.0x              |
| 5    | 36      | 80s      | 1.75x             |
| 10   | 60      | 110s     | 2.5x              |

---

## Technical Stack

**Engine:** Unity 6000.2  
**Rendering:** Universal Render Pipeline (URP) 2D  
**Architecture:** Entity Component System (ECS)  
**Input:** New Input System 1.14.2  
**Language:** C# (.NET Standard 2.1)

### Performance Targets
- 60 FPS minimum on mid-range hardware
- 500+ active entities without frame drops
- ECS job system for parallel processing
- Object pooling for projectiles

---

## Roadmap

### ✅ Phase 1: Foundation (Current)
- [x] Project setup and architecture
- [x] Input handler implementation
- [x] Core data structures
- [x] Movement system
- [x] Dash system

### 🔄 Phase 2: Core Gameplay (Next)
- [x] Turret deployment system
- [x] Enemy spawning system
- [x] Collision detection
- [x] Basic enemy AI
- [ ] Resource collection
- [ ] Wave management

### 📅 Phase 3: Combat Depth
- [ ] All turret types functional
- [ ] All enemy types with behaviours
- [ ] Turret targeting systems
- [ ] Bullet-hell patterns
- [ ] Health and damage systems

### 📅 Phase 4: Progression & Polish
- [ ] Upgrade system
- [ ] UI/UX implementation
- [ ] Visual effects
- [ ] Audio
- [ ] Tutorial

### 📅 Phase 5: Content & Balance
- [ ] Additional turret/enemy types
- [ ] Wave variety
- [ ] Difficulty modes
- [ ] Achievements
- [ ] Balance pass

---

## Visual Direction

**Art Style:** Minimalist Geometric - Clean shapes with neon outlines

**Colour Palette:**
- Background: Deep void black (#0A0A0F)
- Player: Cyan (#00F0FF)
- Enemies: Red/Orange (#FF3366, #FF8833)
- Turrets: Green/Blue (#00FF88, #3388FF)

**Effects:** Glowing particles, trail effects, screen shake

---

## Contributing

This is a learning/portfolio project. Contributions and feedback welcome!

1. Clone the repository
2. Open in Unity 6000.2+
3. Review `/Assets/MyProjectStyle.md`
4. Check Issues for tasks

---

## Contact
📞 07515480932  
📧 uabhanu@gmail.com  
🌐 www.bhanu-upadhyayula.com

---

**Build:** Prototype Phase 1 | **Unity:** 6000.2 | **Last Updated:** 5th Nov 2025
