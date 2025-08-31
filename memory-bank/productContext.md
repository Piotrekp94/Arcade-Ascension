# Product Context

This file provides a high-level overview of the project and the expected product that will be created. Initially it is based upon projectBrief.md (if provided) and all other available project-related information in the working directory. This file is intended to be updated as the project evolves, and should be used to inform all other modes of the project's goals and context.

**CRITICAL DEVELOPMENT PRINCIPLE: This project STRICTLY follows Test-Driven Development (TDD) methodology. ALL new features and changes MUST follow the RED-GREEN-REFACTOR cycle.**

2025-08-28 17:26:48 - Log of updates made will be appended as footnotes to the end of this file.
2025-08-31 22:25:00 - Updated to reflect level selection system implementation and TDD emphasis.

*

## Project Goal

*   Build a classic Breakout/Arkanoid-style arcade game with 10-level progression system where players control a paddle to bounce a ball and destroy blocks to advance through increasingly challenging levels.

## Key Features

*   **Level Progression System**: 10 levels with sequential unlock mechanics (Level 1 unlocked by default)
*   **Dynamic Level Selection UI**: Interactive button grid showing locked/unlocked states
*   **Configurable Level Difficulty**: Progressive block configurations with varying rows, columns, and spacing
*   **Level Completion Detection**: Automatic detection when all blocks destroyed, unlocks next level
*   **Classic Breakout Gameplay**: Player-controlled paddle with physics-based ball movement
*   **Destructible Blocks**: Hit points system that awards score points upon destruction
*   **Pure Arcade Mechanics**: Focus on skill-based gameplay without upgrades
*   **Complete Game Flow**: Level Selection → Playing → Level Completion → Return to Selection
*   **Clean User Interface**: Score display and intuitive level selection system
*   **Comprehensive TDD Coverage**: All systems built using strict Test-Driven Development methodology

## Overall Architecture

*   **Unity Game Engine**: C# scripting with Unity 2D physics system
*   **Singleton Pattern**: Core managers (`GameManager`, `LevelManager`) for centralized state management
*   **ScriptableObject Data System**: Level configurations stored as designer-friendly data assets
*   **Input System**: Unity's new Input System with keyboard controls for paddle movement
*   **Component-Based Design**: Focused scripts for game entities (`PlayerPaddle`, `Ball`, `Block`, `BlockManager`, `LevelSelectionUI`)
*   **Event-Driven Architecture**: Loose coupling via C# Actions/Events for system communication
*   **Progressive Difficulty**: Data-driven level scaling through configurable parameters
*   **UI Management**: TextMeshPro-based interface with state-managed panel system
*   **Test-Driven Development**: Comprehensive unit and integration test coverage following TDD principles
*   **Unity Editor Integration**: Custom integrators for seamless level data management