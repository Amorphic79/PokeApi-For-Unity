# PokeApi-For-Unity

This is a data fetcher/asset creator for Unity, specifically Game Dev Experiences How to make a Pokemon Game series on youtube.<br>

MrAmorphic.cs needs to be in a folder called Editor. PokeApi.cs can be in anything other than the editor folder.<br>
After importing these two files into your project you will see many errors, so let's fix them.<br>

<b>!! If you have my previous PokeApi.cs script, remove it first !!</b><br>

First we need to install the Unity package EditorCoroutines
Window -> Package Manager<br>

This is a pre-release package so you need to enable pre-releases by going to<br>
Gear -> Advanced project settings -> Enable pre-release and then
Install Editor Coroutines -> restart Unity<br>
<br>
Now we need to add an integer called id to MoveBase, ItemBase and PokemonBase and give them getters/setters<br>

<b>PokemonBase.cs add</b><br>
[SerializeField] MrAmorphic.PokeApiPokemon pokeApiPokemon; 

<b>MoveBase.cs add</b><br>
[SerializeField] private MrAmorphic.PokeApiMove pokeApiMove; 

<b>ItemBase.cs</b><br>
[SerializeField] private MrAmorphic.PokeApiItem pokeApiItem;

all three should also have get/set so we can access the fields.
These allow us to see all the data from the API, incase you want to add more functionality later.<p>

Next rename conditions to full names <br>
  <b>none, poison, burn, sleep, paralysis, freeze, confusion</b> (use F2 in visual studio)<br>
there will still be errors when fetching the moves, this is because we haven't implemented all conditions, like trap or yawn.<br>
So these moves will not be added until you add the condition to the enum but you'll still need to implement the condition too.<br>
Full list of conditions missing is:<br>
<b>yawn, trap, disable, leech_seed, unknown,nightmare, no_type_immunity, perish_song,<br>
infatuation, torment, ingrain, embargo, heal_block, silence, tar_shot</b><br>
<p>
Rename stats SpAttack and SpDefense, to Special_attack, Special_defense (again use F2 in Visual Studio)
<p>

When this is done, we need to add setters for ALL fields in
MoveBase,  MoveEffects, SecondaryEffects<p>
Add setters for all fields in ItemBase
and here we also remove virtual from CanUseInBattle and CanUseOutsideBattle and removing the overrides for them in PokeballItem and TmItem
<br>
And we also need setters for all fields in PokemonBase, LearnableMove and Evolution
<br>
Add Steel, Dark, Fairy to PokemonType in PokemonBase.cs
<br>
This should conclude everything :)<br>
When compiled, you should see a new Menu called MrAmorphic in Unity. <p><br>
First you can check the settings in MrAmorphic.cs around line 180. <br>
These are the folders where everything is created, wich language to use and wich game to get texts from.<br>
And the last bools are if you want the sprites to be downloaded and added for pokemons and items.<br>
and if you want to blank files for Pokemons/Items that are missing when adding evolutions. If a Pokemon/Item is missing, the evolution won't be added.<br>

<h2>Changelog</h2>
<ul>
<li><b>2022-02-01</b><br>
<ul>
<li>Added enums for Version and VersionGroup, making it easier to select which game so fetch data from. There is also a selection for "Lastest" which goes through all games and finds the latest game in which the data was present.</li>
<li>Fixed that moves don't get duplicates in LearnableMove or LearnableByItem</li>
<li>Added a settings window instead of having the settings hidden in code. Window -> MrAmorphic -> PokeAPI Settings.</li>
<li>Added custom editors for subitems (pokeball, recovery etc) this only updates the current item.
</ul>
</li>
</ul>
  
<h2>Known errors</h2>
<ul>
<li>Gen 8 Pokemon don't get moves added. They don't exist in the API.</li>
<li>Other evolutions than Level-Up or Use-Item doesn't get added. No support in code yet.</li>
<li>Item sprites get duplicates downloaded. Many TMs use the same sprite, but in the API they are unique.</li>
<li>Some data is missing from the assets. Not in API. Example is how much health a Potion gives, or if the item can be used in battle. Same for TMs, this doesn't fill in the move just creates the asset and fills in the text.</li>
</ul>
    
