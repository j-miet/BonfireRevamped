# Hollow Knight BonfireRelit mod

This is a fork of [TheodoreChristianRadu's Bonfire](https://github.com/TheodoreChristianRadu/Bonfire) repository 
which itself is a fork of [original BonfireMod by ricardosouzag](https://github.com/ricardosouzag/BonfireMod).

---

Original Bonfire mod adds a geo-based levelling system to Hollow Knight. This replicates the idea from Dark Souls 
games where 
souls work both as currency and experience points for levelling character stats. 

This way player can choose between raw stats/utility and usual upgrades/items. It also keeps geo useful even in the 
end game: improve character power instead of saving up and spending it on unbreakable charms.

While levelling certainly makes game easier, it can offer some refreshing variety to standard gameplay.

Goal of *BonfireRelit* is to
1. slightly improve the original mod by adding some quality of life changes and update existing systems 
(but not too much to keep the same feel)
2. update the project's codebase to be easier to approach and update


## Changes to original mod

- **Enemy Hp scaling has been removed entirely.**  
Yup, there's scaling which itself is kinda counter-intuitive. Scaling formula is also pretty bad as you can see below:

    ```csharp
    hm.hp *= (int)((1.25 + (double)Dreamers / 3) * (2.5 / (1.0 + Math.Exp(-0.05 * Status.CurrentLv))));
    ```

    The formula will always apply integer multiplier which means user would get sudden multiplier changes: 
    no smooth curve, just 1x -> 2x -> 3x etc. Notice how hp also scales from both level ups and dreamers killed!

    This formula was applied to all enemies with less than 5000 hp which is actually just all enemies because nothing 
    gets close to this value, even on Ascended/Radiant difficulty in pantheons. For example the HK wiki 
    (https://hollowknight.wiki/) says 
    - Absolute Radiance has 2181 hp (highest single entity)
    - other multi-boss battles such as Sisters of Battle and Watcher Knights have more in total, but each entity 
    gets checked separately

- **Bench bonfire visuals have been removed** because in my personal opinion they didn't fit the aesthetic of 
Hollow Knight

- **All spells have now intelligence stat scaling**. Previously only howling wraiths/abyss shriek had the bonus 
damage applied.

- **All nail arts now scale from strength stat and also get the 50% damage bonus from Fragile/Unbreakable 
Strength charms**.

- **Extended bench menu to include toggle buttons** for
    - simple enemy health bars
        - very simple visually, use other mods for fancier ones and keep these disabled
        - attached to each enemy individually, hovering over them
        - for enemies: only displayed if enemy is damaged (= not full hp), has red color. For bosses: always 
        displayed, has orange color, larger than normal bar
        - can also toggle progressive bar colors (green -> yellow -> red). Then both bosses and enemies use this same 
        color system.
    - void heart soul regen (regen multiplier can be adjusted)
        - only applies to void heart -> you need to have obtained this charm
        - use this at your own discretion: for balanced gameplay keep it at a low 
        multiplier or disable entirely because you already have wisdom stat for base regen

- **Other changes**

    Gameplay:
    - ui menu now opens only when sitting on a bench (previously just being next to it was enough)
    - respec system no longer refunds the King's Idols/Arcane Eggs as King's Idols. Instead it now keeps internal
    counter of all free level-ups from relics. So stat respecs works just the same except you can no longer convert the
    free level-ups back to sellable relics
    - all stats now start from 0 instead of 1. This makes it easier for player to see how many points they've 
    allocated: 0 indeed means 0 added points in stat. It also simplifies codebase logic if any stat 
    changes/adjustments are needed in the future.

    Code:
    - removed unused code like public fields and some LevellingSystem functions
    - changed most public interfaces to private because they are not accessed outside the class
    - combined some code e.g. BonfireRelit.HeroUpdate handles soul regen and crit rolls without creating 
    separate hooks
    - moved all GUI menu logic into a separate BonfireGUI.cs file
    - update readability: code is longer/more verbose, but also formatted better and includes plenty of comments


## Installation

*Mod is only compatible with Hollow Knight 1.5.78.11833 because modding API is not available yet for the newest 
public version* 

**BonfireRelit is not part of Lumafly mod manager (at least yet)** so you need to perform manual installation:
- Install modding api either
    - manually from [official Github page](https://github.com/hk-modding/api) (download newest release then unzip 
    contents into Hollow Knight's `Hollow Knight/Hollow Knight_Data/Managed` folder)
    
        or 

    - download [Lumafly](https://themulhima.github.io/Lumafly/) mod manager (which automatically installs 
    the modding API)
- Download the [latest release](https://github.com/j-miet/BonfireRelit/releases/latest) of the mod
- Extract `BonfireRelit.zip` into a `BonfireRelit` folder. This folder should contain just two files:
     - `BonfireRelit.dll`
     - `BonfireRelit.pdb` 

    Then move this folder to `Hollow Knight/Hollow Knight_Data/Managed/Mods` to finish installation. On Steam you'd 
    end up with a path similar to this

    `C:/Program Files (x86)/Steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed/Mods/BonfireRelit`

**If you use just modding API:** launch the game and mod should be enabled  
**If you use Lumafly:** you will find *BonfireRelit* in installed mods (it should say "Version not from modlinks"). 
Just enable it and launch modded game

### Using other mods with BonfireRelit

If you use mods such as *Enemy HP Bar* and *Better Void Heart*, or anything else with custom hp bars and/or void heart 
soul regen:
- remove such mods/disable them, or 
- use them, but keep BonfireRelit's own implementations toggled off to avoid conflicts

In general -> mods which affect leveling stats indirectly (e.g. modify nail/spell damage values) could cause issues: 
some could scale existing values, others override earlier changes entirely depending in which order updating is 
executed. Keep this in mind if you plan to mix other mods with BonfireRelit.

#### Bonfire Teleport (by *kassent*)

> Despite its name: Bonfire Teleport is an entirely separate mod project from Bonfire leveling system mods, 
> made by different author.

If you'd like to keep the Dark Souls theme of warping between bonfires, install the **Bonfire Teleport**. This mod 
allows you to effortlessly fast travel from bench to bench using map pins. Or just freely teleport to
anywhere at any point if that's what you want instead.

Bonfire Teleport in NexusMods: https://www.nexusmods.com/hollowknight/mods/129?tab=description
- make sure you download the HK modding API compatible version (not BepInEx)

