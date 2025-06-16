# MT2_BETTER_RANDOM

Better Random is a BepInEx mod for *Monster Train 2* designed to enhance randomization by tracking completed clan combinations and avoiding duplicates.

## Dependencies

This mod requires **BepInEx** to run properly. BepInEx is a plugin framework and mod loader for Unity games like Monster Train 2.

Before using this mod, make sure to:

- Download and install **BepInEx** for *Monster Train 2*:
  1. Go to the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases).
  2. Download the recommended version for your game (usually the x64 version).
  3. Extract the contents of the archive into your Monster Train 2 game folder (the folder where the game executable resides).
  4. Run the game once to complete the BepInEx installation, then close it.

Without BepInEx installed, this mod will not work.
---
## Using the Mod
To use this mod in your game:
- Download the compiled `.dll` file of this mod.
- Place the `.dll` file into the `BepInEx/plugins` folder inside your Monster Train 2 installation directory.
- Launch the game and enjoy the mod.

---


## Development and Build (For Developers Only)

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
with the path to the `BepInEx\plugins` folder inside your Monster Train 2 game directory on your machine.
