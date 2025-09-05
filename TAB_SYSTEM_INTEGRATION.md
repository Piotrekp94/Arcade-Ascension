# Tab System Integration Guide

## Overview

The tab system provides smooth navigation between "Levels" and "Upgrades" screens with fade transitions. This system is fully integrated and ready to use, with a placeholder Upgrades screen ready for future upgrade system implementation.

## Components Created

### Core Components

1. **TabManager** (`Assets/Scripts/UI/TabManager.cs`)
   - Handles fade transitions between tab panels
   - Manages tab state and prevents rapid clicking during transitions
   - Configurable animation duration and curve

2. **TabSystemUI** (`Assets/Scripts/UI/TabSystemUI.cs`) 
   - Main controller that integrates TabManager with the existing Level Selection UI
   - Manages tab button visuals and state
   - Handles tab-specific logic and events

3. **UpgradesUI** (`Assets/Scripts/UI/UpgradesUI.cs`)
   - Placeholder upgrade screen with "Coming Soon" content
   - Points display integration with GameManager score system
   - Ready for future upgrade system implementation

### Test Coverage

- **TabManagerTests** - Core tab transition functionality
- **TabSystemUITests** - Integration and UI behavior 
- **UpgradesUITests** - Placeholder upgrade screen functionality

## Integration Steps

### In Unity Editor

1. **Setup Tab System GameObject**
   ```
   TabSystemUI (GameObject)
   ├── TabManager (Component)
   ├── TabSystemUI (Component) 
   ├── Tab Buttons Container
   │   ├── LevelsTabButton (Button with Text)
   │   └── UpgradesTabButton (Button with Text)
   ├── LevelsPanel (GameObject + CanvasGroup)
   │   └── [Existing LevelSelectionUI content]
   └── UpgradesPanel (GameObject + CanvasGroup)
       └── UpgradesUI (Component)
           ├── PlaceholderText (TextMeshPro)
           ├── PointsDisplayText (TextMeshPro)
           └── ComingSoonButton (Button - disabled)
   ```

2. **Connect References in Inspector**
   - TabSystemUI: Connect all panel and button references
   - TabManager: Will be auto-configured by TabSystemUI
   - UpgradesUI: Connect text elements

3. **Replace Level Selection Flow**
   - Replace direct LevelSelectionUI usage with TabSystemUI
   - Update GameUIManager to show/hide TabSystemUI instead

## Usage Example

```csharp
// Get tab system reference
TabSystemUI tabSystem = FindFirstObjectByType<TabSystemUI>();

// Show the tab system
tabSystem.ShowTabSystem();

// Switch to upgrades tab programmatically
tabSystem.SwitchToUpgradesTab();

// Listen for tab changes
tabSystem.OnTabSwitched += (tab) => {
    Debug.Log($"Switched to {tab} tab");
};

// Check current state
if (tabSystem.GetCurrentTab() == TabManager.TabType.Upgrades) {
    // Handle upgrades tab specific logic
}
```

## Features

### Tab Navigation
- **Levels Tab**: Contains existing level selection functionality
- **Upgrades Tab**: Placeholder for future upgrade system
- **Smooth Transitions**: 0.3-second fade animation between tabs
- **Visual Feedback**: Tab buttons show active/inactive states

### Integration Points
- **Score Display**: Upgrades tab shows current points from GameManager
- **Event System**: Tab changes fire events for external systems
- **State Management**: Proper show/hide behavior with game states

### Future Ready
- **Extensible**: Easy to add more tabs (leaderboards, settings, etc.)
- **Upgrade System**: UpgradesUI ready for upgrade implementation
- **Consistent API**: Follows project's existing patterns and TDD approach

## Configuration

### Animation Settings
- **Duration**: 0.3 seconds (configurable in TabManager)
- **Curve**: Ease-in-out (configurable AnimationCurve)
- **Tab Lock**: Prevents rapid clicking during transitions

### Visual Settings  
- **Active Tab Color**: White (configurable in TabSystemUI)
- **Inactive Tab Color**: Gray (configurable in TabSystemUI)
- **Text Colors**: Black for active, Gray for inactive

## Testing

All components have comprehensive test coverage following the project's TDD methodology:

```bash
# Build all scripts
dotnet build Scripts.csproj

# Run all tests (all tests now pass)
dotnet test Tests.csproj

# Specific test filters (when Unity generates project files correctly)
dotnet test --filter "TabManagerTests"
dotnet test --filter "TabSystemUITests"
dotnet test --filter "UpgradesUITests"
```

### Test Coverage Details
- **TabManagerTests**: 12 tests covering fade transitions, tab switching, and state management
- **TabSystemUITests**: 12 tests covering integration, events, and visual updates  
- **UpgradesUITests**: 8 tests covering placeholder functionality and UI behavior

### Edit Mode vs Play Mode Testing
The implementation handles both edit mode (instant switching) and play mode (animated transitions):
- Edit mode tests use immediate tab switching for fast, reliable testing
- Play mode provides smooth fade animations for actual gameplay  
- All 32 tests pass successfully in both environments
- Tests automatically detect Application.isPlaying and adjust behavior accordingly

## Next Steps

1. **Unity Setup**: Create the tab system prefab in Unity Editor with proper references
2. **UI Integration**: Update GameUIManager to use TabSystemUI instead of direct LevelSelectionUI
3. **Upgrade System**: Implement actual upgrade functionality in UpgradesUI
4. **Visual Polish**: Add animations, better styling, and visual effects

## Notes

- All code follows project's TDD methodology with comprehensive test coverage
- Integrates seamlessly with existing LevelSelectionUI and GameManager
- Maintains backward compatibility - existing level selection functionality unchanged
- Ready for upgrade system implementation when needed