# PokeApi-For-Unity

This is a data fetcher/asset creator for Unity, specifically Game Dev Experiences How to make a Pokemon Game series on youtube.

MrAmorphic.cs needs to be in a Editor folder, and PokeAPi.cs cannot be in one.
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


In MoveBase.cs
Lets add None to MoveCategory this is used as a failsafe when getting moves.

Add Steel, Dark, Fairy to PokemonType in PokemonBase.cs
