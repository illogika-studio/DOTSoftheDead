# DOTSoftheDead

DOTS of the Dead is a DOTS sample game project that's meant to be an example of "real gameplay" implemented fully in DOTS. It is a simple local-multiplayer top-down zombie shooter.


## How to Play
On start, the game will automatically spawn one Player for the keyboard, and one more
Player for each connected gamepad

### Controls
- 1: spawn a wave of zombies (you can press this many times for more zombies)
- Escape: reset the game
- WASD/LeftStick: move
- IJKL/RIghtStick: aim
- Space/RightTrigger: Shoot
- LeftCTRL/LeftTrigger: Melee attack

### Pickups
- Small red orbs are healthpacks
- White boxes increase your fire rate
- Red boxes add to your number of projectiles (but you need to pick up more and
more of them to gain a bonus)


## Overview of Implementation

### Game Initialization
Game Initialization is handled in a Monobehaviour called GameInitializer in the game scene.
This behaviour does the following on start:
- Convert all game prefabs to their Entity representation (characters, weapons, etc...)
- Spawn one player+character per input device
- Setup some data in the ZombieSpawningSystem, to make it ready to spawn batches

### Player inputs
This is handled in PlayerInputSystem.

This system has a CreatePlayer function that spawns a Player entity, which holds a PlayerTag
component, a GameplayInput component, and a buffer of associated input device IDs. It then
uses the new input system’s callbacks to build arrays of all inputs with associated devices that
were made during the frame. Finally, it launches a job whose purpose is to go fill the
GameplayInput struct of each player with the inputs corresponding to the devices those players
were given.

We end up with a bunch of Player entities who all have their GameplayInputs components up
to date.

In order to use those inputs for character movement, weapon shooting, etc…. We use the
OwningPlayer component in combination with jobs whose purpose is to give an entity the
inputs it needs based on its owning player entity. An example of this is the
PlayerInputsToCharacterInputsJob in CharacterMoveSystem. This job will, for each
character with an OwningPlayer component, find the associated player entity and feed the
player’s inputs into the character movement components

### Character Movement
The character movement system, used both by player characters and zombies, has two main
components: CharacterInputs and Character.

CharacterInputs stores the intention of the character: Move vector, look direction, whether
we’re attacking or not.

Character stores the characteristics of the character: Move speed, move sharpness, orientation
sharpness, etc….

CharacterMoveSystem launches a job that, for each <Character, CharacterInputs,
PhysicsVelocity, Rotation> entities, will translate character inputs and character data into a
velocity to apply to PhysicsVelocity, and a rotation to apply to Rotation
CharacterCollisionSystem launches a ITriggerEventsJob, which is how trigger overlaps are
processed. For each character-to-character trigger events, this job will apply a decollision
impulse to both characters.

CharacterAnimationSystem handles making the character mesh entities bob up and down
based on their current velocity

### Zombies
Zombies are extremely simple, and require only the following systems.Each of those systems
will launch a job that will iterate on every zombie, every frame.

AssignTargetClosestPlayerSystem finds the closest player to each zombie in a certain range,
and assign them as it’s target
AssignTargetRandomSystem if a zombie doesn’t have a player in it’s range as a target, this
system will choose a location around the zombie, and use it as a target. This will be the
zombie’s destination until it reaches it, or a player comes in it’s range.

MoveTowardTargetSystem moves every zombie toward its target, at full speed if it’s a player,
slower (1/3) if not.

ZombieAutoAttackSystem if the zombie has a player target and is close enough, it will change
its AttackInputs so that a melee attack will be triggered

### Weapons
Weapons are assigned to the corresponding character when it is spawned. A melee weapon is
assigned to the zombies, and both a range and melee weapon is assigned to the player

AttackSystem launches 4 jobs:
- PlayerInputsToRangeWeaponInputsJob feeds player inputs to weapon
- PlayerInputsToMeleeWeaponInputsJob feeds player inputs to weapon
- WeaponShootJob tests for every range weapon if the shoot input is held, and spawn a
bullet if needed
- MeleeAttackJob same thing but for melee weapons

### Camera
The Camera is a bit special because it’s not yet converted automatically to a DOTS entity. We
use an orthographic Camera and the goal is to be able to move it and change its orthographic
size so we can see all the players at the same time on screen.

First, a monobehaviour CameraManager is in charge of creating the camera entity and its main
component CameraData.

Also, there is another monobehaviour CameraEntityBridge. It makes the bridge between the
entity and the monobehaviour world, by setting the values of the CameraData component and
getting the Translation from the entity to place the GameObject in the world.

Two entities are actually used for the camera, one for the pivot gameobject and one for the
actual Camera gameobject (a system uses its world position in the entity world)
To be able to focus the camera any number of entities, a CameraFocus component is assigned
to the entities.

The CameraFocusSystem launches a job that gets the position of all entities with the
CameraFocus component and uses them to set the position and the orthographic size of the
camera.

To get those, it uses an EntityQuery which retrieve the entities with a CameraFocus and
Translation component.

The BillboardSystem launches a IJobForEach<Billboard, LocalToWorld, Rotation,
Translation, Parent>, making every entity with a Billboard component face the camera.

### Pickups
PickUps are prefabs transformed into entities by the GameInitializer.
There are two types : some are moving toward the player, others are static and the player
needs to get to them to collect them.

The Health pickups are attracted to the player in a certain radius. For this, they just have the
MoveToTarget and FollowingPickUp component added to them. The
MoveTowardTargetSystem launches an additional job for the entities with FollowingPickUp
component because they have a configurable speed.

The PickupSystem launches an ITriggerEventsJob for each type of pickup, foreach
player-pickup event, the modifications to the corresponding player character components are
done.

The pickup also use the DestroyAfterTime component which is used by the
DestroyAfterTimeSystem jobs to destroy entities after a certain time.
