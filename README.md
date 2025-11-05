🌌 VOID WEAVE

Tactical Bullet-Hell Tower Defence
Platform: PC (Unity 6)
Type: Single-Player Action Strategy
Status: 🔧 Prototype Phase 1 - Architecture

Weave Your Defence. Navigate the Chaos.

📖 Table of Contents
Overview
Core Concept
Game Loop
Player Mechanics
Turret System
Enemy Types
Progression System
Technical Stack
Development Roadmap
🎮 Overview
Void Weave is a tactical bullet-hell tower defence game where players strategically deploy autonomous turret nodes while dodging geometric enemy patterns in a minimalist sci-fi arena.

Key Features
✨ Hybrid Gameplay - Strategic turret placement meets skill-based bullet-hell dodging
🌊 Wave-Based Survival - Defend against increasingly complex enemy formations
🎯 Tactical Decisions - Limited resources force meaningful deployment choices
⚡ High-Performance ECS - Built on Unity's Entity Component System
🎨 Geometric Aesthetic - Clean, minimalist visual design with procedural patterns

💡 Core Concept
Players face waves of geometric enemies in a contained arena. Success requires mastering three pillars:

DODGE → Navigate bullet-hell patterns with precise movement and dashing
DEPLOY → Place turret nodes in tactical positions during preparation
ADAPT → React to enemy compositions and adjust strategies

The core tension comes from balancing active survival with strategic planning.

🔄 Game Loop
Three-Phase Cycle
1️⃣ PREPARATION PHASE (30 seconds)

Navigate the arena freely
Deploy or relocate turret nodes
Review enemy intel for the next wave
Spend resources on upgrades
⬇️

2️⃣ COMBAT WAVE (60+ seconds)

Geometric enemies spawn in patterns
Player dodges bullet-hell attacks
Deployed turrets engage automatically
Collect resources from defeated enemies
⬇️

3️⃣ UPGRADE PHASE

Review wave performance
Spend collected resources
Unlock new turret types
Upgrade existing nodes
Difficulty scales up
⬇️ LOOP BACK TO PREPARATION

🎯 Player Mechanics
Movement System
WASD - 8-directional movement (5 units/sec)
Responsive - Instant direction changes for precision
No Acceleration - Direct control for bullet-hell gameplay
Dash Ability
Property	Value
Keybind	Spacebar / Shift
Distance	3 units
Duration	0.2 seconds
Cooldown	1 second
Special	Invincibility frames during dash
Tactical Use: Escape tight bullet patterns, reposition quickly

Deployment System
Keybind: E / Left Click
Range: 2 units from the player
Max Nodes: 5 active (base limit)
Cost: Resources from defeated enemies
Restrictions: Cannot place during combat or overlapping existing nodes
🏰 Turret System
Three Core Types
🔺 STRIKER - Tank Killer
Damage: High (20)
Fire Rate: Slow (0.5/sec)
Range: 5 units
Special: Single-target precision
Cost: 100 resources
Best Against: Square enemies
🔷 SCATTER - Crowd Control
Damage: Low (5 per projectile)
Fire Rate: Fast (3/sec)
Range: 4 units
Special: 5-projectile spread (30° cone)
Cost: 100 resources
Best Against: Triangle swarms
▬ BEAM - Formation Breaker
Damage: Medium (12)
Fire Rate: Continuous
Range: 6 units
Special: Pierces 3 enemies
Cost: 150 resources
Best Against: Line formations
Upgrade Path
Level 1 (Base)  →  Level 2 (+50%)  →  Level 3 (+100%)
   100 cost          +50 cost            +100 cost
👾 Enemy Types
🔻 Triangle - The Swarm
Stats

Health: 50 HP
Speed: 4 units/sec (Fast)
Damage: Collision only
Reward: 5 resources
Behavior

Erratic, unpredictable movement
Spawns in large groups (10-20)
Overwhelmed by numbers
Weak individually, deadly in swarms
Counter: Scatter turrets + dash through gaps

🟥 Square - The Tank
Stats

Health: 200 HP
Speed: 1.5 units/sec (Slow)
Damage: 10 per projectile (shoots back!)
Fire Rate: 0.5/sec
Reward: 25 resources
Behavior

Slow, deliberate movement
Shoots slow-moving projectiles
High health pool
Priority target
Counter: Striker turrets + kiting

▬ Line - The Formation
Stats

Health: 100 HP
Speed: 2.5 units/sec (Medium)
Damage: Collision only
Reward: 15 resources
Behavior

Moves in straight-line formations
Predictable but coordinated
Creates wall-like patterns
Spawns in groups of 5-10
Counter: Beam turrets + perpendicular dodging

📈 Progression System
Resource Economy
EARN Resources

🔻 Triangle Kill: 5
🟥 Square Kill: 25
▬ Line Kill: 15
✅ Wave Complete: +50 bonus
💎 Perfect Wave (no damage): +50 bonus
SPEND Resources

Deploy Turret: 100-150 (type dependent)
Upgrade Turret: 50-100 (level dependent)
Unlock New Type: 200
Increase Max Nodes: 150 (+1 capacity)
Wave Scaling
Wave	Enemies	Duration	Enemy Health Multiplier
1	20	60s	1.0x
5	36	80s	1.75x
10	60	110s	2.5x
Formula:

Enemy Count = Base × (1 + Wave × 0.2)
Enemy Health = Base × (1 + Wave × 0.15)
Duration = 60s + (Wave × 5s)
🏗️ Technical Stack
Technologies
Engine: Unity 6000.2
Rendering: Universal Render Pipeline (URP) 2D
Architecture: Entity Component System (ECS)
Input: New Input System 1.14.2
Language: C# (.NET Standard 2.1)
Project Structure
Assets/
├── Scripts/
│   ├── Input/
│   │   ├── PlayerInputHandler.cs
│   │   └── PlayerInputActions.cs
│   ├── Components/ (ECS)
│   │   ├── PlayerMovementComponent.cs
│   │   ├── DashComponent.cs
│   │   └── TurretDeploymentComponent.cs
│   ├── Systems/ (ECS)
│   │   ├── PlayerMovementSystem.cs
│   │   ├── DashSystem.cs
│   │   └── TurretTargetingSystem.cs
│   ├── Data/
│   │   ├── GameConfig.cs
│   │   ├── TurretNodeData.cs
│   │   └── EnemyData.cs
│   └── Utilities/
├── Prefabs/
│   ├── Player.prefab
│   ├── Turrets/
│   └── Enemies/
├── ScriptableObjects/
│   ├── Turrets/
│   └── Enemies/
└── Scenes/
    ├── MainMenu.unity
    └── GameArena.unity
Code Style
Following MyProjectStyle.md guidelines:

✅ PascalCase for classes, methods, public fields
✅ _camelCase for private fields
✅ ScriptableObjects for static configuration
✅ ECS architecture (no SOAP pattern)
✅ Industry Standard Input Manager pattern

Performance Targets
⚡ 60 FPS minimum on mid-range hardware
🎯 500+ entities without frame drops
🔧 ECS job system for parallel processing
♻️ Object pooling for projectiles
🛣️ Development Roadmap
✅ Phase 1: Foundation (Current)
 Project setup and architecture
 Input handler implementation
 Core data structures
 ECS component definitions
 Basic player movement system
 Dash ability implementation
🔄 Phase 2: Core Gameplay (Next)
 Turret deployment system
 Enemy spawning system
 Collision detection
 Basic enemy AI (Triangle)
 Resource collection
 Wave management
📅 Phase 3: Combat Depth
 All turret types functional
 All enemy types with behaviours
 Turret targeting and firing
 Bullet-hell patterns
 Health and damage systems
 Wave progression
📅 Phase 4: Progression & Polish
 Upgrade system
 Resource economy balancing
 UI/UX implementation
 Visual effects
 Audio
 Tutorial
📅 Phase 5: Content & Balance
 Additional turret types
 Additional enemy types
 Wave variety
 Difficulty modes
 Achievements
 Playtesting and balance
🎨 Visual Direction
Art Style
Minimalist Geometric - Clean shapes with neon outlines

Color Palette
🖤 Background: Deep void black (#0A0A0F)
💠 Player: Cyan (#00F0FF)
🔴 Enemies: Red/Orange (#FF3366, #FF8833)
💚 Turrets: Green/Blue (#00FF88, #3388FF)
Effects
Glowing particle trails
Screen shake on impacts
Hit indicators and damage numbers
Audio feedback cues
UI/UX
Minimal HUD (health, resources, timer)
Diegetic information (range indicators)
Clear visual feedback
🤝 Contributing
This is a learning/portfolio project. Contributions and feedback welcome!

Getting Started
Clone the repository
Open in Unity 6000.2+
Review /Assets/MyProjectStyle.md
Check Issues for tasks
Guidelines
Follow the project style guide
Write clean, documented ECS systems
Test performance benchmarks
Submit PRs with clear descriptions
📄 License
[To be determined - MIT / GPL / Proprietary]

🙏 Acknowledgments
Unity ECS Documentation
Bullet-hell inspirations: Enter the Gungeon, Vampire Survivors
Tower Defence classics: Bloons TD, Kingdom Rush
📞 Contact
07515480932
uabhanu@gmail.com
www.bhanu-upadhyayula.com

Build: Prototype Phase 1 - Architecture
Unity Version: 6000.2
Last Updated: 2024

This version is much cleaner with:

✅ Proper line spacing and breathing room
✅ Simplified structures (no complex ASCII art)
✅ Working Table of Contents links
✅ Clean header that doesn't run together
✅ Better use of emojis as visual breaks
✅ Tables and lists that render properly on GitHub
✅ Consistent section styling
