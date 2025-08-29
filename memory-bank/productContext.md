# Product Context

This file provides a high-level overview of the project and the expected product that will be created. Initially it is based upon projectBrief.md (if provided) and all other available project-related information in the working directory. This file is intended to be updated as the project evolves, and should be used to inform all other modes of the project's goals and context.
2025-08-28 17:26:48 - Log of updates made will be appended as footnotes to the end of this file.

*

## Project Goal

*   Build an arcade game similar to Pong, where players destroy blocks to earn points and currency, which can then be used to purchase upgrades to enhance gameplay.

## Key Features

*   Pong-like core gameplay with a player-controlled paddle and a ball.
*   Blocks with hit points that award score and currency upon destruction.
*   An upgrade system allowing players to purchase enhancements (e.g., Paddle Speed, Paddle Size, Ball Speed, Multi-Ball, Extra Life) using earned currency.
*   Management of distinct game states (Start, Playing, Game Over).
*   User Interface (UI) for displaying score and currency.

## Overall Architecture

*   Unity game engine, C# scripting.
*   Singleton pattern for core managers (`GameManager`, `UpgradeManager`).
*   Input handling via Unity's new Input System (keyboard for paddle).
*   Component-based design for game entities (`PlayerPaddle`, `Ball`, `Block`).
*   UI elements managed by TextMeshPro.