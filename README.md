# PokeApi-For-Unity

This is a data fetcher/asset creator for Unity, specifically Game Dev Experiences How to make a Pokemon Game series on youtube.

`!! If you have my previous PokeApi.cs script, REMOVE IT FIRST !!`

**MrAmorphic.cs** needs to be in a folder called **Editor**. **PokeApi.cs** can be in anything other than the editor folder.
After importing these two files into your project you will see many errors, so let's fix them.

## EditorCoroutines
First we need to install the Unity package **EditorCoroutines**

`Window -> Package Manager`

This is a pre-release package so you need to enable pre-releases by going to

`Gear -> Advanced project settings -> Enable pre-release and then
Install Editor Coroutines -> restart Unity`

## PokemonBase.cs
- Add the following lines:
```
  [SerializeField] private int id;
  [SerializeField] private MrAmorphic.PokeApiPokemon pokeApiPokemon;
  public int Id { get => id; set => id = value; }
  public PokeApiPokemon PokeApiPokemon { get => pokeApiPokemon; set => pokeApiPokemon = value; }
```
- Rename in the **enum Stats** the fields `SpAttack and SpDefense`, to `Special_attack, Special_defense` (use F2 in Visual Studio)
- Add setters for **ALL** fields in the following classes:
  `PokemonBase, LearnableMove, Evolution`
- Add `Steel, Dark, Fairy` to the **enum PokemonType**


## MoveBase.cs
- Add the following lines:
```
[SerializeField] private int id;
[SerializeField] private MrAmorphic.PokeApiMove pokeApiMove; 
public int Id { get => id; set => id = value; }
public PokeApiMove PokeApiMove { get => pokeApiMove; set => pokeApiMove = value; }
```
- Add setters for **ALL** fields in the following classes:
`MoveBase,  MoveEffects, SecondaryEffects`

## ItemBase.cs
- Add the following lines:
```
[SerializeField] private int id;
[SerializeField] private MrAmorphic.PokeApiItem pokeApiItem;
public int Id { get => id; set => id = value; }
public PokeApiItem PokeApiItem { get => pokeApiItem; set => pokeApiItem = value; }
```
- Add setters for **ALL** fields in this class.
- Remove **virtual** from `CanUseInBattle` and `CanUseOutsideBattle`

## ConditionsDB.cs
Next rename conditions to full names in **ConditionID** (use F2 in visual studio)
  
  `none, poison, burn, sleep, paralysis, freeze, confusion,`
  
There will still be errors when fetching the moves, this is because we haven't implemented all conditions, like trap or yawn.
So these moves will not be added until you add the condition to the enum but you'll still need to implement the condition too.
Full list of conditions missing is:
```
yawn, trap, disable, leech_seed, unknown,nightmare, no_type_immunity, perish_song,
infatuation, torment, ingrain, embargo, heal_block, silence, tar_shot,
```

## PokeballItem.cs
- Remove the line
`public override bool CanUseOutsideBattle => false;`
  
## TmItem.cs
- Remove the line
  `public override bool CanUseInBattle => false;`
- Add setters for `move and isHM`
  
## Conclusion 
When compiled, you should see a new Menu called **MrAmorphic** in Unity and you'll have a new window under

`Window -> MrAmorhic -> PokeAPI Settings`

First you can check the settings in **MrAmorphic.cs** around line 490.
These are the folders where everything is created, wich language to use and which game to get texts from.
These show up in the window too, but for now these don't get saved! So keep that in mind.

And the last bools are if you want the sprites to be downloaded and added for pokemons and items.
and if you want to blank files for Pokemons/Items that are missing when adding evolutions.
If a Pokemon/Item is missing, the evolution won't be added.

## Changelog
- **2022-05-04**
  - Updated GetItems() so it fetches all items from PokeAPI. (Almost 1000 more items now)
- **2022-02-20**
  - Added "healing" to PokeApiMoveMeta
  - Added more missing data in PokeApiSpecies, Pokemons should now be complete (except all the special sprites)
- **2022-02-19**
  - Added missing data to the API
- **2022-02-04**
  - Added function to set moves to TMs and HMs. You find it under the Moves menu. (Need to add setters for move and isHM under TmItem.cs)
- **2022-02-02**
  - Rewrote this ReadME, adding clarifications and reformatting the text.
- **2022-02-01**
  - Added enums for Version and VersionGroup, making it easier to select which game so fetch data from. There is also a selection for "Lastest" which goes through all games and finds the latest game in which the data was present.
  - Fixed that moves don't get duplicates in LearnableMove or LearnableByItem
  - Added a settings window instead of having the settings hidden in code. Window -> MrAmorphic -> PokeAPI Settings.
  - Added custom editors for subitems (pokeball, recovery etc) this only updates the current item.
  
## Known errors
- Gen 8 Pokemon don't get moves added. They don't exist in the API.
- Other evolutions than Level-Up or Use-Item doesn't get added. No support in code yet.
- Item sprites get duplicates downloaded. Many TMs use the same sprite, but in the API they are unique.
- Some data is missing from the assets. Not in API. Example is how much health a Potion gives, or if the item can be used in battle. Same for TMs, this doesn't fill in the move just creates the asset and fills in the text.
- Paths in the Settings window don't get saved. Resets everytime.
    
