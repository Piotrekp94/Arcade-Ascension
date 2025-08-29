# System Patterns

This file documents recurring patterns and standards used in the project.
Updated 2025-08-29 to reflect current implementation patterns and best practices.

## Coding Patterns

*   **Singleton Pattern:** Used for core managers like `GameManager` to ensure a single instance throughout the game, providing global access and state management.
*   **Component-Based Design:** Unity's component-based architecture with game objects composed of focused scripts (`PlayerPaddle`, `Ball`, `Block`, `Wall`, `DeathZone`) each handling specific behaviors.
*   **State Machine Pattern:** GameManager uses clear state transitions (Start → Playing → GameOver) with event-driven state changes.
*   **Event-Driven Architecture:** Components communicate via C# Actions/Events (e.g., `OnGameOver`, `OnBallLost`, `OnWallHit`) for loose coupling.

## Architectural Patterns

*   **Unity's New Input System:** Player input handled via `Keyboard.current` and `Mouse.current` for modern input management.
*   **Physics-Based Movement:** Ball movement uses Unity's Rigidbody2D with `linearVelocity` for realistic physics simulation.
*   **Timer Systems:** Configurable timer pattern for respawn delay using manual update methods for testability.
*   **Attachment System:** Ball-to-paddle attachment using kinematic state switching and position tracking.

## Testing Patterns

*   **Test-Driven Development (TDD):** Strict RED-GREEN-REFACTOR cycle for all new features.
*   **Comprehensive Test Structure:** Tests organized in dedicated 'Tests' folder mirroring main structure:
    - Unit tests for individual components
    - Integration tests for system interactions
    - Both Edit Mode and Play Mode test coverage
*   **Test Lifecycle Management:** Proper Setup/TearDown with GameObject creation/destruction handling for both runtime and edit mode.
*   **Deterministic Testing:** Separate deterministic methods (e.g., `SimulateLeftClickDeterministic()`) for reliable test results.
*   **Manual Timer Testing:** `UpdateRespawnTimer()` method for testing timer-based systems without relying on Unity's Update() cycle.

## Unity-Specific Patterns

*   **MonoBehaviour Lifecycle:** Proper use of Awake, Start, Update methods with clear responsibilities.
*   **Physics Materials:** Separate physics materials for ball bouncing and wall interactions.
*   **Tag-Based Detection:** Object identification using Unity tags ("Ball", "Wall", "DeathZone").
*   **Layer-Based Organization:** Physics layers for different game object types.
*   **Context Menu Testing:** `[ContextMenu]` attributes for debugging and manual testing in editor.

## Code Organization Patterns

*   **Assembly Definitions:** Separate assemblies for main scripts and tests for clean separation.
*   **Namespace Convention:** All scripts use `Scripts` namespace for organization.
*   **File Structure:** Clear folder hierarchy (Game/, Player/, Ball/, Environment/, UI/, Tests/).
*   **Serialized Fields:** Unity-specific serialization with `[SerializeField]` for inspector configuration.

## Error Handling Patterns

*   **Null Checks:** Defensive programming with null checks for singleton access and component references.
*   **Graceful Degradation:** Systems continue functioning when optional components are missing.
*   **Test Safety:** Error handling in tests for both runtime and edit mode execution environments.