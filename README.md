🌌 VOID WEAVE
Genre: Tactical Bullet-Hell Tower Defence
Platform: PC (Unity 6)
Target: Single-Player Action Strategy
Status: Prototype Phase 1 - Architecture

"Weave Your Defence. Navigate the Chaos."

📖 Table of Contents
Overview
Core Concept
Game Loop
Player Mechanics
Turret System
Enemy Types
Progression System
Technical Architecture
Development Roadmap
Contributing

🎮 Overview
Void Weave is a tactical bullet-hell tower defence game where players must strategically deploy autonomous turret nodes while dodging geometric enemy patterns in a minimalist sci-fi arena.

Key Features
Hybrid Gameplay: Combine strategic turret placement with skill-based bullet-hell dodging
Wave-Based Survival: Defend against increasingly complex enemy formations
Tactical Decision Making: Limited resources force meaningful turret deployment choices
High-Performance ECS: Built on Unity's Entity Component System for optimal performance
Geometric Aesthetic: Clean, minimalist visual design with procedural patterns

💡 Core Concept
Players face waves of geometric enemies in a contained arena. Between waves, they strategically place Turret Nodes at key positions. During combat, players must:

Dodge - Navigate bullet-hell patterns using precise movement and dashing
Deploy - Place turret nodes in tactical positions during preparation phases
Adapt - React to enemy compositions and adjust turret placement strategies
The core tension comes from balancing active survival (dodging) with strategic planning (turret placement).

🔄 Game Loop
┌─────────────────────────────────────────────────────┐
│                 PREPARATION PHASE                   │
│  • Navigate the arena freely                           │
│  • Deploy/Relocate turret nodes                    │
│  • Review enemy intel for next wave                │
│  • Spend resources on upgrades                     │
│  • 30-second countdown timer                       │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│                  COMBAT WAVE                        │
│  • Geometric enemies spawn in patterns             │
│  • Player dodges bullet-hell attacks                │
│  • Deployed turrets engage automatically           │
│  • Collect resources from defeated enemies         │
│  • Survive for wave duration (60s base)            │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│                 UPGRADE PHASE                       │
│  • Review wave performance                          │
│  • Spend collected resources                       │
│  • Unlock new turret types                         │
│  • Upgrade existing turret nodes                   │
│  • Difficulty scales for next wave                 │
└──────────────────┬──────────────────────────────────┘
                   │
                   └──────► LOOP BACK TO PREPARATION
🎯 Player Mechanics
Movement
WASD: 8-directional movement at base speed of 5 units/sec
Responsive: Instant direction changes for precise dodging
No Acceleration: Direct control for bullet-hell precision
Dash Ability
Keybind: Spacebar / Shift
Function: Short-range teleport (3 units)
Duration: 0.2 seconds
Cooldown: 1 second
I-Frames: Invincibility during dash animation
Tactical Use: Escape tight patterns, reposition quickly
Deployment System
Keybind: E / Left Click
Function: Place turret node at cursor position
Range: 2 units from the player
Limit: 5 active nodes maximum (base)
Cost: Resources collected from enemies
Placement Rules:
Cannot overlap with existing nodes
Cannot place during the combat wave
Requires sufficient resources

🏰 Turret System
Three Core Turret Types
Type	Icon	Damage	Fire Rate	Range	Special Ability	Cost
STRIKER	🔺	High (20)	Slow (0.5/s)	5 units	Single-target precision	100
SCATTER	🔷	Low (5)	Fast (3/s)	4 units	5-projectile spread (30°)	100
BEAM	▬	Medium (12)	Continuous	6 units	Pierces 3 enemies	150
Turret Behavior
STRIKER - Tank Killer

Targets the highest-health enemy in range
Perfect accuracy with travel-time projectiles
Best against Square enemies
Long cooldown between shots
SCATTER - Crowd Control

Fire cone of 5 projectiles
Targets nthe earest enemy group
Effective area denial
Best against Triangle swarms
BEAM - Formation Breaker

Continuous laser that locks onto the target
Pierces through multiple enemies
Effective against Line formations
High energy cost
Upgrade Paths
Each turret has 3 upgrade levels:

Level 1 (Base) → Level 2 (+50% stats) → Level 3 (+100% stats) → MAX
     100 cost        +50 cost               +100 cost
👾 Enemy Types
Triangle - The Swarm
Stats:
  Health: 50 HP
  Speed: 4 units/sec (Fast)
  Damage: 0 (Collision only)
  Resources: 5 per kill
  
Behavior:
  • Erratic, unpredictable movement
  • Spawns in large groups (10-20 units)
  • Overwhelms through numbers
  • Weak individually, deadly in swarms
Counter Strategy: Scatter turrets for area denial, dash through gaps

Square - The Tank
Stats:
  Health: 200 HP
  Speed: 1.5 units/sec (Slow)
  Damage: 10 per projectile
  Fire Rate: 0.5/sec
  Resources: 25 per kill
  
Behavior:
  • Slow, deliberate movement toward the player
  • Shoots slow-moving projectiles
  • High health pool
  • Priority target for focused fire
Counter Strategy: Striker turrets for focused damage, kite while turrets engage

Line - The Formation
Stats:
  Health: 100 HP
  Speed: 2.5 units/sec (Medium)
  Damage: 0 (Collision only)
  Resources: 15 per kill
  
Behavior:
  • Moves in straight-line formations
  • Predictable but coordinated
  • Creates wall-like patterns
  • Spawns in groups of 5-10 aligned units
Counter Strategy: Beam turrets to pierce formations, dodge perpendicular to lines

📈 Progression System
Resource Economy
EARN Resources:
  • Triangle Kill: 5 resources
  • Square Kill: 25 resources
  • Line Kill: 15 resources
  • Wave Completion Bonus: 50 resources
  • Perfect Wave (No Damage): +50 bonus

SPEND Resources:
  • Deploy Turret Node: 100-150 (type-dependent)
  • Upgrade Turret: 50-100 (level-dependent)
  • Unlock New Turret Type: 200
  • Increase Max Nodes: 150 (+1 max node)
Wave Scaling
Wave Difficulty Formula:
  Enemy Count = Base Count × (1 + Wave × 0.2)
  Enemy Health = Base Health × (1 + Wave × 0.15)
  Wave Duration = 60s + (Wave × 5s)
  
Example:
  Wave 1: 20 enemies, 60s duration
  Wave 5: 36 enemies, 80s duration
  Wave 10: 60 enemies, 110s duration

🏗️ Technical Architecture
Technology Stack
Engine: Unity 6000.2
Rendering: Universal Render Pipeline (URP) 2D
Architecture: Entity Component System (ECS)
Input: New Input System (1.14.2)
Language: C# (.NET Standard 2.1)
Project Structure
/Assets
├── /Scripts
│   ├── /Input
│   │   ├── PlayerInputHandler.cs          # Input manager (Singleton)
│   │   └── PlayerInputActions.cs          # Generated input class
│   ├── /Components (ECS)
│   │   ├── PlayerMovementComponent.cs
│   │   ├── DashComponent.cs
│   │   └── TurretDeploymentComponent.cs
│   ├── /Systems (ECS)
│   │   ├── PlayerMovementSystem.cs
│   │   ├── DashSystem.cs
│   │   └── TurretTargetingSystem.cs
│   ├── /Data
│   │   ├── GameConfig.cs                  # ScriptableObject config
│   │   ├── TurretNodeData.cs
│   │   └── EnemyData.cs
│   └── /Utilities
│       └── Constants.cs
├── /Prefabs
│   ├── Player.prefab
│   ├── /Turrets
│   └── /Enemies
├── /ScriptableObjects
│   ├── /Turrets
│   └── /Enemies
└── /Scenes
    ├── MainMenu.unity
    ├── GameArena.unity
    └── TestArena.unity
Code Style
Following MyProjectStyle.md guidelines:

✅ PascalCase for classes, methods, public fields
✅ _camelCase for private fields
✅ ScriptableObjects for static configuration
✅ ECS for runtime systems (no SOAP pattern)
✅ Industry Standard Input Manager pattern

Performance Targets
60 FPS minimum on mid-range hardware
500+ active entities without frame drops
ECS job system for parallel processing
Object pooling for projectiles and enemies

🛣️ Development Roadmap

✅ Phase 1: Foundation (Current)
 Project setup and architecture
 Input handler implementation
 Core data structures (ScriptableObjects)
 ECS component definitions
 Basic player movement system
 Dash ability implementation

🔄 Phase 2: Core Gameplay (Next)
 Turret deployment system
 Enemy spawning system
 Collision detection
 Basic enemy AI (Triangle)
 Resource collection
 Wave management system

📅 Phase 3: Combat Depth
 All three turret types are functional
 All three enemy types with behaviours
 Turret targeting and firing systems
 Bullet-hell patterns for Square enemies
 Health and damage systems
 Wave progression and scaling

📅 Phase 4: Progression & Polish
 Upgrade system implementation
 Resource economy balancing
 UI/UX implementation
 Visual effects and juice
 Audio implementation
 Tutorial/onboarding

📅 Phase 5: Content & Balance
 Additional turret types
 Additional enemy types
 Wave variety and challenges
 Difficulty modes
 Achievements
 Balance pass and playtesting

🎨 Visual Direction
Art Style
Minimalist Geometric: Clean shapes, neon outlines
Colour Palette:
Background: Deep void black (#0A0A0F)
Player: Cyan (#00F0FF)
Enemies: Red/Orange spectrum (#FF3366, #FF8833)
Turrets: Green/Blue spectrum (#00FF88, #3388FF)
Effects: Glowing particles, trail effects, screen shake on impacts
UI/UX
Minimal HUD: Health bar, resource counter, wave timer
Diegetic Information: Turret range indicators, deployment preview
Feedback: Clear hit indicators, damage numbers, audio cues

🤝 Contributing
This is a learning/portfolio project. Contributions, feedback, and suggestions are welcome!

Getting Started
Clone the repository
Open in Unity 6000.2 or later
Review MyProjectStyle.md for code conventions
Check Issues for current tasks
Code Guidelines
Follow the project style guide in /Assets/MyProjectStyle.md
Write clean, documented ECS systems
Test on target performance benchmarks
Submit PRs with clear descriptions


🙏 Acknowledgments
Unity ECS Documentation - Architecture patterns
Bullet-hell genre inspirations - Enter the Gungeon, Vampire Survivors
Tower Defence classics - Bloons TD, Kingdom Rush

📞 Contact
07515480932
uabhanu@gmail.com

Current Build: Prototype Phase 1 - Architecture Foundation
Last Updated: 5th Nov 2025
Unity Version: 6000.2
