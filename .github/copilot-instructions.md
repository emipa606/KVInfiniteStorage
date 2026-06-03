# GitHub Copilot Instructions for [KV] Infinite Storage (Continued)

## Mod Overview and Purpose

[KV] Infinite Storage (Continued) is an updated version of the original Infinite Storage mod by Kiame Vivacity. The purpose of this mod is to provide players with an efficient and compact storage solution for RimWorld that significantly reduces space requirements by utilizing a single 1x1 storage unit capable of holding vast quantities of items, excluding food. This continued version improves the graphical assets to better align with the vanilla aesthetic and introduces additional storage solutions for specific item types.

## Key Features and Systems

- **Infinite Storage Units:** 
  - **Size:** 1x1 grid space
  - **Cost:** 100 metallic materials + 4 components
  - **Power Requirement:** 1W per stored kg (can be adjusted via Mod Settings)
  - **Stores:** All items except food
  - **Placement:** Found under the "Misc" tab in the build menu
  - **Research Requirements:** Requires "Multi Analyzer" and "Infinite Storage" research

- **Specialty Storage Units:**
  - **Textile Storage:** Stores textiles, costs 50 wood/stone/steel
  - **Body Part Storage:** Stores body parts and medicines, costs 50 wood/stone/steel
  - **Silver Storage:** Stores silver, costs 50 wood/stone/steel

- **Energy Management:** Infinite Storage requires energy for item management, impacting gameplay dynamics by incorporating logistical decisions regarding power usage.

- **Compatibility and Mod Support:** Works with many other mods to enhance gameplay and ensure functionality, although there are compatibility notes listed under Unsupported Mods.

## Coding Patterns and Conventions

- **Class Structure:** Employs internal and public static classes to handle mod-related functionalities, optimizing for performance and organization.
- **Naming Conventions:** Follows standard C# naming conventions for classes and methods, ensuring clarity and maintainability. For instance, class names are noun-like `Building_InfiniteStorage` and methods use camelCase `TryGetFirstFilteredItemForMending`.

## XML Integration

The mod uses XML to define parts of the game's content, such as objects and configuration data, allowing flexibility in content customization. XML files define storage types, properties, and integrate seamlessly with the C# backend to dictate how these elements should behave in-game.

## Harmony Patching

The mod utilizes Harmony for patching core game methods to introduce new functionalities or alter existing ones. These changes are crucial for adding the infinite storage mechanics without modifying the actual game source code, maintaining compatibility and ease of updates with new versions of RimWorld.

## Suggestions for Copilot

1. **Automate Redundant Code:** Utilize Copilot to write repetitive functions or boilerplate code especially in parts like storage logic, UI drawing functions, and XML reading/writing methods.
   
2. **Expanded Features Detection:** Suggest automated checks and balances within the harmony patches to reduce errors and improve modularity.

3. **Integrate Error-Handling Routines:** Enhance error reporting and handling routines with Copilot's help to capture and manage mod-specific exceptions elegantly.

4. **Optimize Performance:** Suggest optimizations for large datasets when querying or processing items within the storage system.

5. **UI Enhancements:** Generate ideas for UI improvements such as tooltips and dynamic data visualization for stored items.

6. **Modular Extensions:** Recommend creating additional specialty storage units through XML and classes, enhancing mod extensibility.

Use this guide to maintain quality standards and implement new features effectively while keeping compatibility in mind.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
