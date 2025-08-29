# Product Context

This file provides a high-level overview of the project and the expected product that will be created. Initially it is based upon projectBrief.md (if provided) and all other available project-related information in the working directory. This file is intended to be updated as the project evolves, and should be used to inform all other modes of the project's goals and context.
2025-08-28 17:26:48 - Log of updates made will be appended as footnotes to the end of this file.

*

## Project Goal

*   Build a classic Breakout/Arkanoid-style arcade game where players control a paddle to bounce a ball and destroy blocks to earn points.

## Key Features

*   Classic Breakout gameplay with a player-controlled paddle and physics-based ball.
*   Destructible blocks with hit points that award score points upon destruction.
*   Pure arcade mechanics without upgrades - focus on skill-based gameplay.
*   Management of distinct game states (Start, Playing, Game Over).
*   Clean User Interface (UI) for displaying score and game information.

## Overall Architecture

*   Unity game engine, C# scripting.
*   Singleton pattern for core managers (`GameManager`).
*   Input handling via Unity's new Input System (keyboard for paddle).
*   Component-based design for game entities (`PlayerPaddle`, `Ball`, `Block`).
*   UI elements managed by TextMeshPro.
*   Pure arcade mechanics without progression systems - classic Breakout experience.