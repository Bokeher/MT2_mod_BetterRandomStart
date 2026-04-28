# BetterRandomStart | Monster Train 2 Mod

**BetterRandomStart** is a Quality of Life (QoL) extension for *Monster Train 2*, developed using the **BepInEx** framework and **C#**. It introduces a smart selection system that allows players to pursue 100% completion while maintaining the variety of randomized gameplay.

[![Thunderstore](https://img.shields.io/badge/Thunderstore-Available-blue?style=for-the-badge&logo=thunderstore)](https://thunderstore.io/c/monster-train-2/p/Bokeher/BetterRandomStart_PreventLogbookDuplicates/)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-Available-orange?style=for-the-badge&logo=nexusmods)](https://www.nexusmods.com/monstertrain2/mods/10)

---

## 🔍 The Problem
*Monster Train 2* features a **Logbook** system that tracks victories across **264 unique clan combinations**. While the base game includes a "Random Start" feature, it often selects combinations the player has already mastered. 

For completionists, this creates a friction point: they must either manually search through menus to find missing combinations (breaking the "random" flow) or waste time restarting runs until a new pairing appears.

## 💡 The Solution
This mod introduces a **custom functional UI button** into the clan selection screen. When triggered, the mod:
1.  **Scans Player Progress:** Automatically checks the player's save file to identify which clan combinations are not completed.
2.  **Smart Randomization:** Filters the pool of 264 pairings to isolate only those without a victory flag.
3.  **UI Automation:** Programmatically selects a valid pairing from the remaining pool, directly updating the game state and streamlining the user experience.

---

## 💻 Tech Stack & Engineering Tools
* **Language:** C#
* **Target Framework:** .NET Standard 2.1
* **Modding Framework:** BepInEx (Plugin architecture for Unity games)
* **Runtime Patching:** Harmony (Used for method hooking and code injection)
* **Reverse Engineering:** dnSpy (Used to analyze the game's assembly and UI hierarchy)

## 🛠 Engineering Highlights
* **Method Hooking:** Patched `RunSetupScreen.Start` via Harmony Postfix to trigger custom UI initialization and data parsing.
* **Runtime UI Injection:** Programmatically constructed a new UI hierarchy (`GameObject`, `RectTransform`, `UnityEngine.UI.Button`) and injected it into the active Unity Canvas.
* **Reflection & Accessing Internals:** Utilized `BindingFlags.NonPublic` to invoke private game methods such as `RefreshCharacters` and `RefreshClanCovenantUI`, enabling seamless state updates without an official API.
* **Edge-Case Handling:** Implemented logic to respect game rules, such as verifying Clan Level (level 5+) before selecting alternative champion variants.


## 📦 Dependencies

This mod requires **BepInEx** to run properly. BepInEx is a plugin framework and mod loader for Unity games like Monster Train 2.

Before using this mod, make sure to:

- Download and install **BepInEx** for *Monster Train 2*:
  1. Go to the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases).
  2. Download the recommended version for your game (usually the x64 version).
  3. Extract the contents of the archive into your Monster Train 2 game folder (the folder where the game executable resides).
  4. Run the game once to complete the BepInEx installation, then close it.

Without BepInEx installed, this mod will not work.
---
## 🎮 Using the Mod
To use this mod in your game:
- Download the compiled `.dll` file of this mod.
- Place the `.dll` file into the `BepInEx/plugins` folder inside your Monster Train 2 installation directory.
- Launch the game and enjoy the mod.

---


## 🏗 Development and Build (For Developers Only)

The steps below are **only necessary for developers** who want to build and modify the mod. If you're not a programmer, you do **not** need to follow these — just use the provided `.dll` file.

---

### Automatically Copy DLL to Game Plugins Folder After Build

The project `.csproj` file contains a `<PostBuildEvent>` that copies the compiled DLL to the game’s plugins directory.  

You **must update** the destination path in this line to match your local Monster Train 2 installation folder.

Locate the following snippet in the `.csproj` file:

```xml
<PropertyGroup>
  copy /Y "C:\Users\Szymon\source\repos\MT2_BETTER_RANDOM\MT2_BETTER_RANDOM\bin\Debug\netstandard2.1\MT2_BETTER_RANDOM.dll" "C:\Program Files (x86)\Steam\steamapps\common\Monster Train 2\BepInEx\plugins\"
</PropertyGroup>
```

#### Replace  
```xml
"C:\Users\Szymon\source\repos\MT2_BETTER_RANDOM\MT2_BETTER_RANDOM\bin\Debug\netstandard2.1\MT2_BETTER_RANDOM.dll"
```  
with the path to the DLL file produced by your build (the output of your project).
#### Replace  
```xml
"C:\Program Files (x86)\Steam\steamapps\common\Monster Train 2\BepInEx\plugins\"
```  
with the path to your game's plugins folder.

### Update Project Dependencies

To successfully compile the project, you also **need to update project dependencies** in your local development environment:

- Make sure the project references the correct path to **BepInEx** and **UnityEngine** DLLs.
- These are typically located in your installed Monster Train 2 game folder under `BepInEx/core` and `MonoBleedingEdge/lib/mono/unityjit`.

Update your project references accordingly in Visual Studio (or your preferred IDE), or directly in the `.csproj` file if needed.

Without correcting these paths, the project may fail to build due to missing or unresolved references.


#### Replace  
```xml
"C:\Program Files (x86)\Steam\steamapps\common\Monster Train 2\BepInEx\plugins\"
```  
with the path to the `BepInEx\plugins` folder inside your Monster Train 2 game directory on your machine.
