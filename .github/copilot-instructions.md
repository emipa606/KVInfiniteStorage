# GitHub Copilot Instructions for [KV] Infinite Storage (Continued)

## Mod Overview and Purpose
[KV] Infinite Storage (Continued) is an updated version of Kiame Vivacity's original Infinite Storage mod. The mod allows for the creation of storage structures that can hold an infinite amount of items, except for food. It requires energy to compress and make available the stored items, with the energy requirement increasing linearly as the weight stored increases. The mod includes several specialty storage units for specific item types, such as Body Parts, Textiles, and Silver.

## Key Features and Systems
- **Infinite Storage**: A storage structure that can hold an infinite amount of items except for food. It requires energy to operate.
- **Textile Storage**: A storage unit specifically for textiles.
- **Body Part Storage**: A storage unit for body parts and medicines.
- **Silver Storage**: A storage unit for silver.
- **Harmony Patching**: The mod uses Harmony to patch various RimWorld methods to integrate the new storage systems.
- **XML Integration**: The mod defines new ThingDefs, ResearchProjectDefs, and minifiedDefs in XML files to create the new storage structures.

## Coding Patterns and Conventions
- **Harmony Patches**: All Harmony patches are located in the `HarmonyPatches` directory. Each patch is a separate class with a `Prefix` or `Postfix` method that modifies the behavior of the patched method.
- **XML Definitions**: XML files are located in the `Defs` directory. Each file contains definitions for new ThingDefs, ResearchProjectDefs, and minifiedDefs.
- **Mod Settings**: The mod includes settings in the Mod Settings menu to adjust the power consumption per stored kg.

## XML Integration
The mod defines new ThingDefs, ResearchProjectDefs, and minifiedDefs in XML files located in the `Defs` directory. Here is an example of a ThingDef for Infinite Storage:

xml
<ThingDef>
    <defName>InfiniteStorage</defName>
    <label>Infinite Storage</label>
    <description>Stores everything except food.</description>
    <thingClass>Building_Storage</thingClass>
    <category>Building</category>
    <costList>
        <count>100</count>
        <thingDef>Steel</thingDef>
    </costList>
    <size>
        <x>1</x>
        <y>1</y>
    </size>
    <researchPrerequisites>
        <li>Multianalyzer</li>
    </researchPrerequisites>
    <statBases>
        <PowerConsumption>1</PowerConsumption>
    </statBases>
</ThingDef>


## Harmony Patching
The mod uses Harmony to patch various RimWorld methods to integrate the new storage systems. Here is an example of a Harmony patch that modifies the behavior of the `Dialog_FormCaravan_PostOpen` method:

csharp
public class Dialog_FormCaravan_PostOpen
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "PostOpen")]
    public static class Patch_PostOpen
    {
        public static void Prefix(Dialog_FormCaravan __instance)
        {
            // Custom code to handle the post-open behavior
        }
    }
}


## Suggestions for Copilot
- **Harmony Patching**: When working with Harmony patches, Copilot can suggest the correct Harmony patch class and method names based on the patched method.
- **XML Integration**: When working with XML definitions, Copilot can suggest the correct XML structure and field names based on the existing definitions.
- **Coding Patterns and Conventions**: Copilot can suggest coding patterns and conventions based on the existing codebase, such as the use of Harmony patches and XML definitions.

By following these guidelines, you can ensure that the Copilot suggestions are relevant and useful for developing [KV] Infinite Storage (Continued).

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

