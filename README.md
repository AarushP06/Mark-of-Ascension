# Mark of Ascension

Mark of Ascension is a 2D top-down action game built in Unity. The player fights through a sequence of dark fantasy stages, defeats enemy waves, clears boss encounters, and progresses deeper into the fortress.

## Overview

The game currently includes:

- a main menu and lobby flow
- persistent player spawning across scenes
- player movement, melee attack, and health
- enemy damage, simple enemy AI, and boss progression
- stage portals for scene-to-scene progression
- hazard zones that damage the player
- three playable combat stages:
  - `Stage01`
  - `Stage02`
  - `Stage03`

## Game Flow

The current progression is:

1. `MainMenu`
2. `SC_Lobby`
3. `Stage01`
4. `Stage02`
5. `Stage03`

Each stage uses a simple progression loop:

- clear the enemy wave
- unlock the path to the boss
- defeat the boss
- use the portal to move forward or return

## Stage Themes

- `Stage01`: early dungeon/combat introduction
- `Stage02`: fire-themed fortress stage
- `Stage03`: dark fortress final-stage area with stronger hazards and a larger boss approach

## Controls

- `WASD` or movement input: move the player
- `B`: melee attack

## Current Systems

### Player

- health and damage handling
- death/game-over screen
- return to main menu on death
- persistent player handling between scenes

### Combat

- short-range melee attack
- enemy contact damage
- boss spawning after wave clear
- scene portals unlocked by progression

### Hazards

- red hazard blocks damage the player on contact
- hazard placement is used to shape stage difficulty

## Project Structure

Important folders:

- `Assets/Scenes`
- `Assets/Scripts/Gameplay`
- `Assets/Scripts/UI`

Important scenes:

- [MainMenu.unity](Assets/Scenes/MainMenu.unity)
- [SC_Lobby.unity](Assets/Scenes/SC_Lobby.unity)
- [Stage01.unity](Assets/Scenes/Stage01.unity)
- [Stage02.unity](Assets/Scenes/Stage02.unity)
- [Stage03.unity](Assets/Scenes/Stage03.unity)

## Unity Setup

This project is a Unity project. To run it:

1. Open the folder in Unity Hub.
2. Open the project with the correct Unity version used by the course/project setup.
3. Open `MainMenu` or start from the first gameplay scene you want to test.
4. Press Play in the Unity Editor.

## Current Status

The project is in active development. The main gameplay loop is playable, and the current focus has been:

- building stage progression
- improving collision and boundaries
- polishing Stage03
- making bosses, hazards, and portals work reliably

## Notes

- `Ground` should be used for walkable floor.
- `Decor` should be used for visual-only wall art.
- collision should come from real blockers, boss gate colliders, hazards, and perimeter boundaries rather than decorative tile art.

## Author

Created as a game development project by Aarush Patel.
