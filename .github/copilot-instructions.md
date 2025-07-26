# Copilot Instructions for RimWorld Mod: Infinite Storage

## Mod Overview and Purpose
The "Infinite Storage" mod for RimWorld enhances the storage capabilities within the game by introducing a building that can store an unlimited number of items. The primary goal of this mod is to offer players an efficient way to manage their inventory, reducing clutter and increasing the accessibility of resources within their colonies.

## Key Features and Systems
- **Building_InfiniteStorage**: A core class that defines the building's behavior, allowing endless items to be stored, managed, and retrieved efficiently.
    - Methods to add, remove, and manage items, as well as applying filters and handling user interactions.
- **Harmony Patches**: Numerous patches to integrate the infinite storage functionality seamlessly with existing game mechanics like caravans, reservations, and trade systems.
- **Settings and UI Enhancements**: Configurable settings for the mod and a user-friendly interface to manage the items stored.

## Coding Patterns and Conventions
- **Class & Method Definitions**: Following a consistent naming convention using PascalCase for classes and methods, and camelCase for local variables.
- **Public vs. Internal Classes**: Use of public static classes for globally accessed patches, and internal classes for more encapsulated functionality.
- **Usage of Extensions**: `DefModExtension` is used to extend definitions, adhering to RimWorld's modding architecture.

## XML Integration
- [Include any XML-related details here if applicable, such as settings definitions, item categories, or any custom XML utilized by your mod. This section would typically detail how XML files control in-game content such as items, buildings, or settings.]

## Harmony Patching
- **Patch Class Structure**: Each patch is structured within a dedicated class, aiding in modularity and specific functionality targeting. Example:
    - `Patch_Building_Storage_Accepts`: Handles changes to how buildings accept and store items.
    - `Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan`: Modifies the behavior of exiting maps with caravans.
- **Adding Prefixes and Postfixes**: Harmony is used to inject code before and/or after the original methods to extend or alter functionality without modifying the game's core code directly.
- **Error Handling**: Incorporate error handling within patches to maintain game stability, using try-catch blocks where necessary.

## Suggestions for Copilot
- **Automatic Implementation Suggestions**: Encourage Copilot to propose method signatures, especially when adding new methods consistent with existing ones, to maintain functionality and structure.
- **Pattern Recognition**: Use common patterns for defining Harmony patches such as prefixes, postfixes, and transpilers.
- **Code Refactoring**: Aid in refactoring code to improve performance, readability, and maintainability.
- **Integration with Game Mechanics**: Ensure the suggested code works harmoniously with existing game systems, particularly when dealing with complex mechanics like caravans and trade systems.

By following these copilot guidelines, you can maintain a consistent coding standard across your RimWorld modding project, ensuring reliable integration and seamless gameplay experience.
