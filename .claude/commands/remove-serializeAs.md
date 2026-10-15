Act as a senior Unity engineer working on the Unity VR game (Meta Quest, Meta XR SDK, C#) of this project.

## Task
Remove the temporary `[FormerlySerializedAs("oldName")]` attributes (added by the documentation/naming-fix skill) from the C# scripts, and remove the `using UnityEngine.Serialization;` import when it becomes unused.

Removing these attributes is only safe **after** the scenes, prefabs and ScriptableObjects were reserialized with the new field names. Otherwise Unity can no longer map the old name in the YAML to the renamed field and **the values stored in the assets are lost**. For that reason this skill asks for confirmation first and verifies the assets before removing anything.

**Known gap of Force Reserialize:** it rewrites the fields stored directly on a component (`oldName: value` → `newName: value`), but it does **not** rename the `propertyPath:` of the overrides stored on prefab instances (in scenes and in nested prefabs). Those keep `propertyPath: oldName`, and once the attribute is gone they no longer match any field, so the override is silently dropped and the field falls back to the base prefab value (`null`, `0`, empty UnityEvent…). Step 2 migrates those overrides before any attribute is removed.

## Arguments

The argument is matched case-insensitively, in this order:

| Argument | Files processed |
|----------|-----------------|
| *(none)* | All `.cs` files under `CountryFair/Assets/Scripts/` (recursively) |
| One or more `.cs` file paths | Only those files |
| One or more folder paths | Every `.cs` file under that folder, recursively |

- File paths and folder paths may be mixed in the same call
- Always apply the exclusions: `Library/`, `Temp/`, `Assets/Plugins/`, `node_modules/`, generated/third-party code
- If an argument is not an existing `.cs` file or an existing folder, stop and ask instead of guessing
- A path to a file that is not `.cs` is an unrecognized argument: stop and ask
- The only non-`.cs` edits allowed are the `propertyPath:` renames of Step 2 inside `.unity` / `.prefab` / `.asset` files. Never touch anything else in those assets, and never edit `.meta` files or TypeScript files
- `NamingReport.md` is read-only context for this skill (Step 2's "Old" names), except for one case: if the user explicitly asks to delete it or clean it up as part of this run, and every attribute this run processed ends up **removed** (none kept), you may delete the file (or the rows/sections it tracked). Never delete or edit it without that explicit request

## Step 0 — Mandatory confirmation (before reading or editing anything)

Before doing anything else, ask the user exactly this, and wait for the answer:

> Já abriste o Unity e correste o **Tools > Force Reserialize All Assets** (o script `ReserializeAll`, que reescreve os YAML com os nomes novos)? Responde "sim" para avançar.

Rules:

- Do **not** read project files, search the assets or edit anything before the answer
- Proceed **only** if the user clearly answers yes (e.g. "sim", "já fiz", "yes", "done")
- If the answer is no, "not yet", unsure, or does not answer the question, **stop**: do not edit any file, and reply with one short line telling the user to run Force Reserialize Assets first and call the skill again
- Ask only once per run; never infer the answer from earlier messages in the conversation

## Step 1 — Collect the targets

- Resolve the file list from the argument (see Arguments)
- In each file, find every occurrence of the attribute, in all its forms:
  - `[FormerlySerializedAs("oldName")]` on its own line
  - Combined with other attributes: `[SerializeField, FormerlySerializedAs("oldName")]`
  - Fully qualified: `[UnityEngine.Serialization.FormerlySerializedAs("oldName")]`
  - Property backing fields: `[field: FormerlySerializedAs("oldName")]`
  - A field may have **several** of them (a field renamed more than once): handle each one separately
- If no file in the resolved list has any occurrence, **do nothing**: edit no file and reply with a single line saying there are no `[FormerlySerializedAs]` attributes to remove

## Step 2 — Safety check on the assets (per attribute)

Check that the project stores assets as text: `ProjectSettings/EditorSettings.asset` must have `m_SerializationMode: 2` (Force Text). If it does not, or the file cannot be read, you cannot verify the assets: **do not remove anything**, and tell the user why.

All searches below run in `CountryFair/Assets/**/*.unity`, `*.prefab`, `*.asset` (never in `Library/` or `Packages/`).

### 2a — Migrate prefab-instance overrides (`propertyPath`)

For each attribute (script `S`, `oldName` → `newName`):

1. Read the script's GUID from `S.cs.meta` (`guid:` line)
2. Find every override entry whose path **starts** with the old name as its first segment: `propertyPath: oldName` followed by end of line, `.` (e.g. `oldName.Array.size`, `oldName.m_PersistentCalls.m_Calls.Array.data[0].m_Target`) or `[`. Each entry is the block `- target: {fileID: F, guid: G, type: 3}` + `propertyPath:` + `value:` + `objectReference:`
3. Confirm the override really belongs to `S` before renaming it:
   - Find the source prefab whose `.meta` has `guid: G`
   - In that prefab, find the object `--- !u!114 &F` and check that its `m_Script:` has `guid:` equal to the script GUID from step 1. **A subclass counts**: if `S` is a base class (e.g. `OrderableTentElement`), the component's script may be a subclass (`MiniGameTent`, `TentPanel`); accept it when that subclass's `.cs` inherits from `S` (directly or indirectly)
   - If `F` is not found directly in that prefab (nested prefab / variant), follow the chain: the object will be a stripped object or come from a `PrefabInstance` inside it — resolve it in the next source prefab the same way, up to the original
   - If it cannot be resolved with certainty → **do not rename**; mark it as unresolved (attribute kept, see 2c)
4. If it belongs to `S`:
   - If the same `target` already has an entry with the corresponding `newName` path (Unity sometimes writes both) → leave the old entry untouched (it is an orphan Unity drops on the next save) and do not count it as a blocker
   - Otherwise → rename only the first segment of that `propertyPath` line: `oldName…` → `newName…`. Change nothing else on the line or in the block, and preserve the file's line endings (CRLF/LF)
5. Old names used as a **later** segment (e.g. `someStruct.oldName`, fields of a `[Serializable]` class) are not migrated automatically: report them and keep the attribute

### 2b — Direct YAML keys

Search the old name as a whole YAML key: a line matching `^\s*(- )?oldName:` (for example `crowdNoise:`).

- A match can be a false positive: another script may legitimately have a field with the same name. Do not try to decide that yourself — treat it as a blocker and let the user decide

### 2c — Decision per attribute

- No direct-key match (2b) **and** no unresolved / unmigrated override left (2a) → the attribute is **safe to remove**
- Otherwise → the attribute is **kept** and reported. List the asset file paths where it matched (maximum 5 per attribute, then "and N more") and why (direct key / unresolved override / nested segment)

## Step 3 — Remove

For every attribute marked safe:

- Alone on its line → delete the whole line (do not leave an empty line behind)
- Inside a list of attributes (`[SerializeField, FormerlySerializedAs("x")]`) → remove only that item and keep the other attributes and the brackets valid
- A field with several `[FormerlySerializedAs]`: remove only the ones that passed the check
- Never touch the field, its `/// <summary>` documentation, other attributes, or any other code
- After removing, if the file no longer contains **any** use of `FormerlySerializedAs`, remove the line `using UnityEngine.Serialization;`. If another symbol from that namespace is still used, or any attribute was kept, leave the `using` in place
- Do not reformat, reorder or rename anything. If a file has no safe attribute, do not edit it

## Output

- State at the top which argument was resolved and how many `.cs` files were processed
- Overrides migrated in Step 2a: a table with asset file, `oldName` → `newName`, number of `propertyPath` lines renamed
- Number of attributes **removed** and the list of files changed
- List of files where the `using UnityEngine.Serialization;` was removed
- Attributes **kept**, each with: file, field, old name, and the asset files where the old name was still found (or the reason, if the serialization mode could not be verified)
- If anything was kept: tell the user to open the listed assets in Unity, save them, and run the skill again
- Final reminder: open Unity, confirm the values are still in the Inspector of the affected scenes/prefabs (especially the ones whose overrides were migrated), save them, then commit. `NamingReport.md` was not modified (unless the user explicitly asked for cleanup, see Arguments) and can otherwise be deleted manually when everything is done