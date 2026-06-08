# Hollow Knight Bonfire mod

This is a fork of [TheodoreChristianRadu](https://github.com/TheodoreChristianRadu/Bonfire)'s repository which itself 
is a fork of [original BonfireMod by ricardosouzag](https://github.com/ricardosouzag/BonfireMod).

## What is it

Bonfire mod adds a geo-based leveling system to Hollow Knight. This replicates the idea from Dark Souls games where 
souls work both as currency and experience points for leveling character stats. 

This way player can choose between raw stats/utility and usual upgrades/items. It also keeps geo useful even in the 
end game: improve character power instead of saving up and spending it on unbreakable charms.

While leveling certainly makes game easier, it can offer some refreshing variety to standard gameplay.


## Changes to original mod

1. **Enemy Hp scaling has been removed entirely.**  
Yup, there's scaling which itself is kinda counter-intuitive as it's 
never explained anywhere. Scaling formula is also pretty bad as you can see below:

    ```csharp
    hm.hp *= (int)((1.25 + (double)Dreamers / 3) * (2.5 / (1.0 + Math.Exp(-0.05 * Status.CurrentLv))));
    ```

    This formula was applied to all enemies with less than 5000 hp which is actually just all enemies because nothing 
    gets close to this value: for example HK wiki (https://hollowknight.wiki/) says 
    - Absolute Radiance has 2181 hp (highest single entity)
    - Sister of Battle on Radiant difficulty have 750+2 * 950 = 2650, but game loads them as separate entities so 
    it's actually just the 750/950 values

    Following table shows how Knight's level and the amount of dreamers killed affected enemy hp multiplier:

    |  |0 killed  | 1 killed | 2 killed | 3 killed |
    |---|:-:|:-:|:-:|:-:|
    | 2x | 12 | 1  | 1  | 1 |
    | 3x | 64 | 23 | 11 | 3 | 
    | 4x | -  | -  | 33 | 19 |
    | 5x | -  | -  | -  | 44 |

    How to read this:
    - pick the column based on how many dreamers have been killed
    - then pick the equal or next lowest level value on that column e.g. 1 killed, your level is 20 -> 20 is less 
    than 23 -> pick the row above it
    - then look at the multiplier corresponding to that row on the left-most column. 
    In this case it's the first row which means 2x hp

    As you can see it's very harsh and increments would get applied suddenly:
    - leveling up from level 11 -> 12 even with 0 dreamers causes enemies hp to double
    - with all 3 dreamers killed and just hitting level 3 is already a 3x multiplier. And as you probably kept still 
    leveling, you'd have a whopping 4x multiplier on late/end game bosses after reaching lvl 19

2. **Bench bonfire visuals have been removed** because in my personal opinion they didn't fit the aesthetic of 
Hollow Knight

3. **All spells have now intelligence scaling**. Previously only howling wraiths/abyss shriek had the bonus damage applied.

4. **Extended bench menu to include toggle buttons** for
    - simple enemy health bars
        - attached to each enemy individually
        - for enemies: only displayed if enemy is damaged (= not full hp), has red color
        - for bosses: always displayed, has orange color, larger than normal bar
    - void heart soul regen (regen multiplier can be adjusted)
        - only applies to void hearth -> you need to have obtained this charm
        - use this at your own discretion: for balanced gameplay keep it at a low 
        multiplier or disable entirely because you already have wisdom stat for base regen

5. **Other changes**

    Gameplay:
    - ui menu now opens only when sitting on a bench (previously just being next to it was enough)

    Code:
    - removed unused code like public fields and some LevellingSystem functions
    - changed most public interfaces to private because they are not accessed outside the class
    - combined some code e.g. BonfireMod.HeroUpdate handles soul regen and crit rolls without creating separate hooks
    - update readability: code is longer/more verbose, but also formatted better and includes a lot of comments for explanations


## Installation

*Mod only compatible with Hollow Knight 1.5.*

This mod is not part of Lumafly mod manager so you need to perform manual installation:
- Install the [modding API](https://github.com/hk-modding/api) or download [Lumafly](https://themulhima.github.io/Lumafly/) mod manager 
- Download the [latest release](https://github.com/j-miet/BonfireRevamped/releases/latest) of the mod
- Extract `BonfireRevamped.zip` into a `BonfireRevamped` folder. This folder should contain just two files:
     - `BonfireRevamped.dll`
     - `BonfireRevamped.pdb` 

    Then move this folder to `Hollow Knight/Hollow Knight_Data/Managed/Mods` to finish installation. On Steam you'd end up with path

    `C:/Program Files (x86)/Steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed/Mods/BonfireRevamped`

**If you use modding API:** launch the game and mod should be enabled  
**If you use Lumafly:** you will find *BonfireRevamped* in installed mods (it should say Version not from modlinks). Just enable it and launch modded game

**If you use mods such as Enemy HP Bar and Better Void Heart, or anything else with custom hp bars and/or void heart soul regen:** 
- remove such mods, or 
- use them but keep BonfireRevamped's own implementations toggled off to avoid conflicts

