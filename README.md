# TurnBasedDemo
**Demo project | Unity | Ongoing**  

Primitive demo of the combat system from the bigger TRPG/CRPG project currently in development. Set to demonstrate the working grid movement system, simple enemy AI and turn/action manager. Code and project showcase.

##  Features showcased  
- **Grid movement**  
- **Game flow**:  
  - Initiative system and turn order.  
  - Command issuing to multiple units.  
- **Enemy AI**:  
  -  Movement.  
  -  Attack pattern. 
- **UI, combat log and victory/loss conditions**

##  Technical details
- Components-based architecture utilizing Unity's GameObject/Component system.
- State machine pattern for AI behavior management and game state transitions.
- Node-based pathfinding using A* algorithm for optimal movement calculation.
- Grid based around a Node system, with each node containing information about passability, elevation and stored elements (GridObject).
- Units implemented through a separate Character class, storing information about the unit's health, speed, armour class, attack capabilities, etc.  
- Combat flow based around a Turn Manager, tracking the current initiative order, game state, round, active unit and victory conditions.
- Action Point economy allowing multiple actions per turn with varying costs.
- AI implemented through a simple Finite State Machine, checking for unit's condition, other units and visible targets.
- Utility-based decision making scoring potential actions based on multiple factors.
- Pathfinding integration with obstacle avoidance and tactical positioning.

## Technology Stack
- Language: C#
- Engine: Unity 6

## Third-Party Assets
- [Lowpoly Cowboy RIO V1.1](https://assetstore.unity.com/packages/3d/characters/humanoids/lowpoly-cowboy-rio-v1-1-288965)
- [Lowpoly Magician RIO](https://assetstore.unity.com/packages/3d/characters/humanoids/lowpoly-magician-rio-288942)
- [PSX Misc. Gun Pack](https://doctor-sci3nce.itch.io/psx-misc-gun-pack)

# Play it here
- This repo contains just the code showcase. You can download and play the demo by following this [link](https://drive.google.com/file/d/1O6EEvbX6mWf7TUZhrbg76EUglt7ZQLwJ/view?usp=sharing)
