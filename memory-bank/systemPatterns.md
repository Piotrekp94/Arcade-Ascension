# System Patterns *Optional*

This file documents recurring patterns and standards used in the project.
It is optional, but recommended to be updated as the project evolves.
2025-08-28 17:29:44 - Log of updates made.

*

## Coding Patterns

*   **Singleton Pattern:** Used for core managers like `GameManager` and `UpgradeManager` to ensure a single instance throughout the game, providing global access to their functionalities.
*   **Component-Based Design:** Unity's inherent component-based architecture is utilized, where game objects are composed of various scripts (components) like `PlayerPaddle`, `Ball`, and `Block`, each handling specific behaviors.

## Architectural Patterns

*   **Unity's New Input System:** Player input for paddle movement is handled using Unity's `InputSystem` (specifically `Keyboard.current`), allowing for flexible and modern input management.

## Testing Patterns

*   **Unity Test Runner:** Unit tests are organized in a dedicated 'Tests' folder, mirroring the project's main folder structure, and are executed using the Unity Test Runner. Focus is on core game logic and non-MonoBehaviour components, with consideration for integration/playmode tests for interactions. Mocking frameworks are employed for true unit isolation.