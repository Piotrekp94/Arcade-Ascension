# Level Selection System Implementation Plan

## Requirements Summary
- 10 levels total with sequential unlock progression
- Level 1 unlocked by default, others unlock after completing previous level
- Level completion = destroying all blocks
- Players return to level selection after beating a level
- Level selection replaces current "Start Game" button flow
- Start with basic level differences (BlockManager parameters only)
- Persistence to be added later (not in MVP)

## UI Placement Decision
**Chosen: New Panel in Existing UI System**
- Maintains single scene architecture
- Reuses existing UIManager/GameUIManager infrastructure
- Easier to maintain and test incrementally

## Phase 1: Core Infrastructure (TDD)

### 1. Create LevelData ScriptableObject
- Define level configuration structure:
  - Level ID/number
  - Block rows and columns
  - Block spacing
  - Score multiplier
  - Level name/description
- Create 10 basic level configurations with varying difficulty
- Store as ScriptableObject assets for easy designer modification

### 2. Create LevelManager System
- Singleton pattern following existing GameManager architecture
- Core responsibilities:
  - Track current selected level
  - Track unlocked levels (array/list of booleans)
  - Provide level data access
  - Handle level progression logic
- Key methods:
  - `GetUnlockedLevels()` - returns list of unlocked level IDs
  - `SetCurrentLevel(int levelId)` - sets active level
  - `UnlockNextLevel()` - unlocks next level in sequence
  - `IsLevelUnlocked(int levelId)` - check if level accessible
  - `GetLevelData(int levelId)` - retrieve level configuration
- Event system:
  - `OnLevelUnlocked` event for UI updates
  - `OnCurrentLevelChanged` event

### 3. Integrate Level Completion Logic
- Extend GameManager to detect level completion:
  - Monitor BlockManager for "all blocks destroyed" state
  - Add `OnLevelCompleted` event
  - Integrate with LevelManager to auto-unlock next level
- Add new GameState for level completion handling
- Ensure proper event flow: Level Complete → Unlock Next → Return to Level Selection

## Phase 2: UI Implementation

### 4. Create LevelSelectionUI Component
- New UI panel integrated with existing GameUIManager system
- UI elements:
  - Grid layout for 10 level buttons (2x5 or 3x4 layout)
  - Each button shows: level number, locked/unlocked state
  - Back button to return to main menu (if needed)
- Button states:
  - Unlocked: clickable, normal appearance
  - Locked: grayed out, non-interactive, lock icon
- Integration with LevelManager for data and state updates

### 5. Modify Game Flow
- Replace current "Start Game" button with "Select Level" button
- Update GameUIManager state management:
  - Add `LevelSelection` state to UI flow
  - Handle transitions: Start → Level Selection → Playing → Level Complete → Level Selection
- Modify GameManager.StartGame() to accept level parameter
- Add return-to-level-selection logic after level completion

## Phase 3: BlockManager Integration

### 6. Update BlockManager for Level System
- Modify BlockManager to accept LevelData configuration
- Add method: `ConfigureForLevel(LevelData levelData)`
- Apply level-specific parameters:
  - Block rows/columns from level data
  - Block spacing from level data  
  - Score multiplier integration with GameManager
- Maintain backward compatibility with existing functionality
- Ensure proper cleanup between level transitions

## Phase 4: Testing & Polish

### 7. Comprehensive Test Coverage
Following existing TDD patterns:

#### LevelManager Unit Tests
- Level unlocking progression logic
- Level data retrieval and validation
- Event firing verification
- Edge cases (invalid level IDs, etc.)

#### Level Progression Integration Tests  
- End-to-end level completion flow
- BlockManager integration with level data
- UI state transitions
- GameManager integration

#### UI Interaction Tests
- Level button click handling
- Locked/unlocked state display
- Proper navigation flow

#### BlockManager Level Configuration Tests
- Level data application
- Parameter validation
- Cleanup between level switches

### 8. Basic Polish
- Level button visual design:
  - Clear locked/unlocked indicators
  - Level number display
  - Hover/click feedback
- Smooth UI panel transitions
- Level completion celebration/feedback
- Proper loading states during transitions

## Architecture Decisions Summary

### Design Patterns
- **Singleton**: LevelManager follows GameManager pattern
- **ScriptableObjects**: Level data storage for designer-friendly configuration
- **Event-Driven**: Loose coupling between systems via events
- **State Management**: Integration with existing GameManager states

### Data Flow
```
LevelSelectionUI → LevelManager → GameManager → BlockManager
                ↓                      ↓
            UI Updates ←← Events ←← Level Completion
```

### Level Progression Flow
1. Player selects unlocked level from UI
2. LevelManager sets current level and provides data
3. GameManager starts game with level configuration
4. BlockManager applies level settings
5. Game plays with level-specific parameters
6. On level completion: unlock next level, return to selection

## Implementation Strategy

### TDD Approach
1. Write failing tests for each component
2. Implement minimal code to pass tests
3. Refactor for quality while maintaining tests
4. Integrate with existing systems incrementally

### Integration Points
- **GameManager**: Level completion detection, game state management
- **BlockManager**: Level configuration application  
- **UI System**: New panel integration with existing GameUIManager
- **Event System**: Leverage existing event patterns for loose coupling

### Future Extensibility
This foundation supports future enhancements:
- Different block types per level
- Special obstacles or power-ups
- Advanced scoring systems
- Level-specific mechanics
- Persistence system
- Level editor tools

## Unity Editor Setup Guidelines

### ScriptableObject Setup
1. **Create Level Data Assets Directory**:
   - Create folder: `Assets/Data/Levels/`
   - Right-click → Create → [Custom LevelData ScriptableObject]
   - Create 10 level data assets: `Level01.asset` through `Level10.asset`

2. **Configure Level Data Assets**:
   - Level 1: 3 rows, 5 columns, 0.1f spacing, 1.0f score multiplier
   - Level 2: 4 rows, 6 columns, 0.1f spacing, 1.2f score multiplier
   - Level 3: 5 rows, 7 columns, 0.08f spacing, 1.4f score multiplier
   - Continue progression pattern for remaining levels
   - Assign each asset a unique level ID (1-10)

### Scene Setup
1. **LevelManager GameObject**:
   - Create empty GameObject named "LevelManager"
   - Add LevelManager script component
   - Assign all 10 LevelData assets to the manager's level data array
   - Position in scene hierarchy near GameManager

2. **UI Panel Setup**:
   - In existing UI Canvas, duplicate existing panel as template
   - Rename to "LevelSelectionPanel"
   - Add Grid Layout Group component for level buttons
   - Configure grid: 2 columns, 5 rows (or preferred layout)
   - Add 10 Button GameObjects as children

3. **Level Button Configuration**:
   - Each button needs: Image, Button component, Text (level number)
   - Create button prefab for consistent styling
   - Assign locked/unlocked materials or sprites
   - Configure button states (normal, highlighted, pressed, disabled)

4. **GameUIManager Updates**:
   - Add LevelSelectionPanel reference in inspector
   - Add LevelSelectionUI component reference
   - Ensure proper panel ordering in hierarchy

### Prefab Setup
1. **Level Button Prefab**:
   - Create button with Image, Text components
   - Add hover/click animations if desired
   - Save as prefab: `Assets/Prefabs/UI/LevelButton.prefab`

2. **Update Block Prefab** (if needed):
   - Ensure Block prefab has proper components for level-specific configuration
   - Verify Block script can handle dynamic point value assignment

### Component Integration
1. **GameManager Integration**:
   - Add LevelManager reference field
   - Update inspector to assign LevelManager instance
   - Ensure event subscriptions work in inspector

2. **BlockManager Integration**:
   - Add LevelData reference field for current level
   - Update inspector to show current level configuration
   - Ensure runtime level switching works properly

### Testing Setup
1. **Test Scene Configuration**:
   - Create minimal test scene with required managers
   - Set up test doubles/mocks for UI testing
   - Configure test data assets for different test scenarios

2. **Play Mode Testing**:
   - Verify level progression works in play mode
   - Test level button interactions
   - Confirm BlockManager receives level data correctly
   - Validate UI state transitions

### Development Workflow
1. **Scene Organization**:
   - Keep all managers in "Systems" hierarchy folder
   - Keep UI elements in "UI" hierarchy folder
   - Use prefab variants for different level configurations

2. **Inspector Debugging**:
   - Add [Header] attributes for better inspector organization
   - Add context menu items for testing (like existing debug methods)
   - Use [SerializeField] for private fields that need inspector access

3. **Version Control**:
   - Include all ScriptableObject assets in version control
   - Use .meta files for proper asset references
   - Consider using prefab variants for level-specific configurations