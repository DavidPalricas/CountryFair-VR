Act as a senior Unity engineer working on the Unity VR game (Meta Quest, Meta XR SDK, C#) of this project.

## Task
Remove the temporary `[FormerlySerializedAs("oldName")]` attributes (added by the documentation/naming-fix skill) from the C# scripts, and remove the `using UnityEngine.Serialization;` import when it becomes unused.

Removing these attributes is only safe **after** the scenes, prefabs and ScriptableObjects were reserialized with the new field names. Otherwise Unity can no longer map the old name in the YAML to the renamed field and **the values stored in the assets are lost**. For that reason this skill asks for confirmation first and verifies the assets before removing anything.

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
- Never touch anything outside `.cs` files: no scenes, prefabs, ScriptableObject assets, `.meta` files, TypeScript files or `NamingReport.md`

## Step 0 — Mandatory confirmation (before reading or editing anything)

Before doing anything else, ask the user exactly this, and wait for the answer:

> Já abriste o Unity e fizeste **Assets > Force Reserialize Assets** (isto reescreve todos os YAML com os nomes novos)? Responde "sim" para avançar.

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

For each attribute found, search the old name as a YAML key in the Unity text assets:

- Search in `CountryFair/Assets/**/*.unity`, `*.prefab`, `*.asset` (never in `Library/` or `Packages/`)
- Match the old name as a whole YAML key: a line matching `^\s*(- )?oldName:` (for example `crowdNoise:`)
- No match → the attribute is **safe to remove**
- Any match → the attribute is **kept** and reported. List the asset file paths where it matched (maximum 5 per attribute, then "and N more")
- A match can be a false positive: another script may legitimately have a field with the same name. Do not try to decide that yourself — keep the attribute and let the user decide

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
- Number of attributes **removed** and the list of files changed
- List of files where the `using UnityEngine.Serialization;` was removed
- Attributes **kept**, each with: file, field, old name, and the asset files where the old name was still found (or the reason, if the serialization mode could not be verified)
- If anything was kept: tell the user to open the listed assets in Unity, save them, and run the skill again
- Final reminder: open Unity, confirm the values are still in the Inspector of the affected scenes/prefabs, then commit. `NamingReport.md` was not modified and can be deleted manually when everything is done