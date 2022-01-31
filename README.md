# PokeApi-For-Unity

This is a data fetcher/asset creator for Unity, specifically Game Dev Experiences How to make a Pokemon Game series on youtube.

MrAmorphic.cs needs to be in a Editor folder, and PokeApi.cs cannot be in one.
After importing these two files into your project you will see many errors, so lets fix them first.

If you have my previous PokeAPi.cs script, remove it first!

Firstly we need to install the Unity package EditorCoroutines
Window -> Package Manager
Gear -> Advanced project settings -> Enable pre-release
Install Editor Coroutines -> restart Unity

Now we need to add an integer called id to movebase, itembase and pokemonbase and give them getters/setters

PokemonBase.cs add
[SerializeField] MrAmorphic.PokeApiPokemon pokeApiPokemon; 

MoveBase.cs add
[SerializeField] private MrAmorphic.PokeApiMove pokeApiMove; 

ItemBase.cs
[SerializeField] private MrAmorphic.PokeApiItem pokeApiItem;

all three with get/set so we can access the fields.
These allow us to see all the data from the API, incase you want to add more functionality later.

When this is done, we need to add setters for all fields in
MoveBase,  MoveEffects, SecondaryEffects

Add setters for all fields in ItemBase
and here we also remove virtual from CanUseInBattle and CanUseOutsideBattle and removing the overrides for them in PokeballItem and TmItem

And we also need setters for all fields in PokemonBase, LearnableMove and Evolution

Add Steel, Dark, Fairy to PokemonType in PokemonBase.cs

This should conclude everything :)
When compiled, you should see a new Menu called MrAmorphic in Unity. 
First you can check the settings in MrAmorphic.cs around line 180. 
These are the folders where everything is created, wich language to use and wich game to get texts from.
And the last bools are if you want the sprites to be downloaded and added for pokemons and items.
and if you want to blank files for Pokemons/Items that are missing when adding evolutions. If a Pokemon/Item is missing, the evolution won't be added.
