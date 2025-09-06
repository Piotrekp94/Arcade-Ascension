# Decision Log

This file records architectural and implementation decisions using a list format.
2025-08-28 17:27:31 - Log of updates made.

*

## Decision

*

## Rationale

*

## Implementation Details

*
[2025-08-28 17:37:18] - Technical Stack Decision
## Decision
The technical stack for the project will be Unity game engine with C# scripting.

## Rationale
Unity provides a robust and widely-used platform for 2D game development, offering extensive tools for physics, rendering, and scripting. C# is the native scripting language for Unity, ensuring seamless integration and access to the engine's full capabilities. This choice aligns with the project's goal of building a game similar to Pong with an upgrade system, as Unity's component-based architecture is well-suited for managing game objects, player mechanics, and upgrade functionalities.

## Implementation Details
*   Utilize Unity's 2D physics system for ball and paddle interactions.
*   Implement game logic, player controls, scoring, enemy AI, and upgrade mechanics using C# scripts.
*   Leverage Unity's UI system for displaying scores, upgrade menus, and other in-game information.
*   Manage game assets (sprites, sounds) within the Unity editor.
2025-08-28 17:38:41 - Decision: Unit Testing Approach for Arcade Ascension.
Rationale: To ensure code quality, maintainability, and reduce regressions in the Unity game project.
Implementation Details:
- Utilize Unity Test Runner for all unit tests.
- Focus unit tests on core game logic, utility classes, and non-MonoBehaviour components.
- For MonoBehaviour components, test isolated logic where possible, and consider integration/playmode tests for interactions.
- Employ mocking frameworks (e.g., NSubstitute, Moq) for external dependencies to ensure true unit isolation.
- Organize tests in a dedicated 'Tests' folder, mirroring the project's main folder structure.
- Aim for high code coverage, especially for critical game mechanics and upgrade systems.

2025-08-29 - Decision: Component Architecture and Upgrade System Design
Rationale: Establish clear separation of concerns and maintainable upgrade system for the arcade game.
Implementation Details:
- Each game component (PlayerPaddle, Ball, Block) handles its own behavior and upgrade integration
- UpgradeManager uses FindObjectOfType for simplicity but should evolve to use references for better performance
- Input handled directly in PlayerPaddle using Unity's new Input System with keyboard polling
- Upgrade system uses enum-based typing with generic float values for flexibility
- GameManager serves as central coordinator for game state, score, and currency management

2025-08-29 - Decision: Complete Upgrade System Removal
Rationale: User requested complete removal of upgrade system to simplify game to pure Breakout/Arkanoid mechanics.
Implementation Details:
- Removed all upgrade-related files: UpgradeManager.cs, UpgradeMenu.cs, UpgradeButton.cs, UpgradeManagerTests.cs
- Cleaned GameManager to remove currency system, keeping only score tracking
- Removed upgrade methods from PlayerPaddle and Ball components
- Updated UIManager to remove upgrade menu references
- Fixed game balance with paddle speed=5f, ball speed=5f, blocks award 10 points
- Created backup branch 'backup-upgrade-system' before removal for potential future restoration

2025-08-29 - Decision: Complete Boundary System Implementation
Rationale: Implement proper Breakout-style boundary behavior where ball bounces off walls but is lost at bottom.
Implementation Details:
- Created Wall component with WallType enum (Top, Left, Right) for bouncing collisions
- Created DeathZone component for bottom boundary that triggers ball loss detection
- Added physics materials: BallMaterial (friction=0, bounce=1) and WallMaterial for proper bouncing
- Ball component enhanced with wall collision detection and events (OnWallHit)
- Comprehensive unit and integration tests for all boundary interactions

2025-08-29 - Decision: Ball Respawn System with Timer
Rationale: User requested configurable timer system where lost balls respawn attached to paddle instead of immediate game over.
Implementation Details:
- GameManager enhanced with respawn timer system (configurable delay, default 3 seconds)
- Ball spawning system that creates balls attached to paddle at paddle position + offset
- Respawn only triggers during Playing state, not during Start or GameOver states
- Ball attachment system with kinematic physics when attached, dynamic when launched
- PlayerPaddle enhanced with ball attachment/detachment and left-click launch mechanics

2025-08-29 - Decision: Manual Game Control with UI System
Rationale: User explicitly chose manual game control over automatic start, requiring UI buttons for game initiation.
Implementation Details:
- Created GameUIManager for handling UI state transitions and button interactions
- Manual Start button to begin game and spawn initial ball attached to paddle
- Restart button to reset game state, score, and start fresh
- UI panels for different game states (Start, Playing, GameOver) with proper show/hide logic
- Event-driven UI updates responding to GameManager state changes

2025-08-29 - Decision: Comprehensive Test-Driven Development Approach
Rationale: Ensure high code quality and prevent regressions with extensive test coverage using strict TDD methodology.
Implementation Details:
- RED-GREEN-REFACTOR cycle for all new features and bug fixes
- Unit tests for all core components: GameManager, PlayerPaddle, Ball, Wall, DeathZone
- Integration tests for boundary system interactions and complete gameplay flows
- Test utility methods for manual timer updates and deterministic behavior
- Proper test lifecycle management with GameObject creation/destruction
- Both Edit Mode and Play Mode test coverage for comprehensive validation

2025-08-31 - Decision: Complete Level Selection System Implementation
Rationale: Transform basic Breakout game into full progression-based experience with 10 levels and unlock system.
Implementation Details:
- LevelManager singleton for level progression logic, unlock tracking, and data management
- ScriptableObject-based level data system for designer-friendly configuration (10 level assets)
- LevelSelectionUI with interactive button grid showing locked/unlocked states and level selection events
- BlockManager integration for dynamic block spawning with level-specific parameters (rows, columns, spacing)
- Level completion detection in GameManager with automatic next level unlocking
- Complete UI flow: Level Selection → Playing → Level Completion → Return to Selection
- Unity Editor integration via LevelSelectionIntegrator for seamless level data binding
- Comprehensive TDD coverage for ALL level system components following strict RED-GREEN-REFACTOR methodology
- Progressive difficulty scaling through configurable level parameters for enhanced gameplay experience

2025-09-06 - Decision: Global Timer System Implementation
Rationale: User requested centralized timer management where all levels use the same configurable time limit instead of per-level timing.
Implementation Details:
- Created GlobalGameConfig ScriptableObject for centralized timer configuration with singleton access pattern
- Removed levelTimeLimit field from LevelData class to eliminate per-level timer complexity
- Updated GameManager to use GlobalGameConfig.Instance.GlobalTimeLimit for all timer operations
- Modified all level asset files to remove individual time limit settings
- Enhanced GameUIManager timer display with percentage-based color coding (Green >50%, Yellow 25-50%, Red <25%)
- Updated LevelSelectionIntegrator to log global time limit instead of per-level limits
- Comprehensive test suite updates: TimerIntegrationTests, GameUIManagerTimerTests, GameManagerTests
- Added proper test infrastructure with GlobalGameConfig setup/teardown and helper methods
- Implemented proper state synchronization between global config changes and GameManager internal state
- Created GlobalGameConfig asset in Resources folder for easy Unity Editor configuration