namespace MrAmorphic
{
    [System.Serializable]
    public class PokeApiItem
    {
        public PokeApiNameUrl[] attributes;
        public PokeApiNameUrl baby_trigger_for;
        public PokeApiNameUrl category;
        public int cost;
        public PokeApiEffectEntry[] effect_entries;
        public PokeApiFlavorTextEntryItem[] flavor_text_entries;
        public PokeApiNameUrl fling_effect;
        public int fling_power;
        public PokeApiGameIndice[] game_indices;
        public string held_by_pokemon;
        public int id;
        public PokeApiMachine[] machines;
        public string name;
        public PokeApiName[] names;
        public PokeApiSprite sprites;
    }

    [System.Serializable]
    public class PokeApiItemHeldByPokemon
    {
        public PokeApiNameUrl pokemon;
        public PokeApiItemHeldVersionDetails[] version_details;
    }

    [System.Serializable]
    public class PokeApiItemHeldVersionDetails
    {
        public int rarity;
        public PokeApiNameUrl version;
    }

    [System.Serializable]
    public class PokeApiSpecies
    {
        public int base_happiness;
        public int capture_rate;
        public PokeApiFlavorTextEntry[] flavor_text_entries;
        public int gender_rate;
        public PokeApiNameUrl growth_rate;
        public PokeApiGenera[] genera;
        public PokeApiEvolutionChainUrl evolution_chain;
        public PokeApiEvolution evolution_data;
        public string name;
        public PokeApiSpeciesName[] names;
    }

    [System.Serializable]
    public class PokeApiSpeciesName
    {
        public PokeApiNameUrl language;
        public string name;
    }

    [System.Serializable]
    public class PokeApiEvolutionChainUrl
    {
        public string url;
    }

    [System.Serializable]
    public class PokeApiFlavorTextEntry
    {
        public string flavor_text;
        public PokeApiNameUrl language;
        public PokeApiNameUrl version;
    }

    [System.Serializable]
    public class PokeApiGenera
    {
        public string genus;
        public PokeApiNameUrl language;
    }

    [System.Serializable]
    public class PokeApiPokemon
    {
        public PokeApiAbilities[] abilities;
        public int base_experience;
        public PokeApiNameUrl[] forms;
        public PokeApiGameIndice[] game_indices;
        public int height;
        public PokemonHeldItem[] held_items;
        public int id;
        public bool is_default;
        public string location_area_encounters;
        public PokeApiMoves[] moves;
        public string name;
        public int order;
        public PokeApiPokemonPastType[] past_types;
        public PokeApiNameUrl species;
        public PokeApiSpecies species_data;
        public PokeApiPokemonSprites sprites;
        public PokeApiStats[] stats;
        public PokeApiType[] types;
        public int weight;
    }

    [System.Serializable]
    public class PokeApiPokemonPastType
    {
        public PokeApiNameUrl generation;
        public PokeApiType[] types;
    }

    [System.Serializable]
    public class PokemonHeldItem
    {
        public PokeApiNameUrl item;
        public PokeApiItemHeldVersionDetails[] version_details;
    }

    [System.Serializable]
    public class PokeApiType
    {
        public int slot;
        public PokeApiNameUrl type;
    }

    [System.Serializable]
    public class PokeApiStats
    {
        public int base_stat;
        public int effort;
        public PokeApiNameUrl stat;
    }

    // Sprites are not completely fetched, problems with naming in the json-file.
    [System.Serializable]
    public class PokeApiPokemonSprites
    {
        public string back_default;
        public string back_female;
        public string back_shiny;
        public string back_shiny_female;
        public string front_default;
        public string front_female;
        public string front_shiny;
        public string front_shiny_female;
    }

    [System.Serializable]
    public class PokeApiAbilities
    {
        public PokeApiNameUrl ability;
        public bool is_hidden;
        public int slot;
    }

    [System.Serializable]
    public class PokeApiSprite
    {
        public string defaul;
    }

    [System.Serializable]
    public class PokeApiGameIndice
    {
        public int game_index;
        public PokeApiNameUrl generation;
    }

    [System.Serializable]
    public class PokeApiMoves
    {
        public PokeApiNameUrl move;
        public PokeApiVersionGroupDetails[] version_group_details;
    }

    [System.Serializable]
    public class PokeApiVersionGroupDetails
    {
        public int level_learned_at;
        public PokeApiNameUrl move_learn_method;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiMove
    {
        public int accuracy;
        public PokeApiMoveContestCombos contest_combos;
        public PokeApiUrl contest_effect;
        public PokeApiNameUrl contest_type;
        public PokeApiNameUrl damage_class;
        public int effect_chance;
        public PokeApiMoveEffectChange[] effect_changes;
        public PokeApiEffectEntry[] effect_entries;
        public PokeApiFlavorTextEntryMove[] flavor_text_entries;
        public PokeApiNameUrl generation;
        public int id;
        public PokeApiNameUrl[] learned_by_pokemon;
        public PokeApiMachine[] machines;
        public PokeApiMoveMeta meta;
        public string name;
        public PokeApiName[] names;
        public PokeApiMovePastValue[] past_values;
        public int power;
        public int pp;
        public int priority;
        public PokeApiMoveStatChange[] stat_changes;
        public PokeApiUrl super_contest_effect;
        public PokeApiNameUrl target;
        public PokeApiNameUrl type;
    }

    [System.Serializable]
    public class PokeApiMoveEffectChange
    {
        public PokeApiEffectEntry[] effect_entries;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiMovePastValue
    {
        public int accuracy;
        public int effect_chance;
        public PokeApiEffectEntry[] effect_entries;
        public int power;
        public int pp;
        public PokeApiNameUrl type;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiMachine
    {
        public PokeApiUrl machine;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiEffectEntry
    {
        public string effect;
        public PokeApiNameUrl language;
        public string short_effect;
    }

    [System.Serializable]
    public class PokeApiUrl
    {
        public string url;
    }

    [System.Serializable]
    public class PokeApiMoveContestCombos
    {
        public PokeApiMoveContestCombo normal;
        public PokeApiMoveContestCombo super;
    }

    [System.Serializable]
    public class PokeApiMoveContestCombo
    {
        public PokeApiNameUrl[] use_after;
        public PokeApiNameUrl[] use_before;
    }

    [System.Serializable]
    public class PokeApiFlavorTextEntryMove
    {
        public string flavor_text;
        public PokeApiNameUrl language;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiFlavorTextEntryItem
    {
        public PokeApiNameUrl language;
        public string text;
        public PokeApiNameUrl version_group;
    }

    [System.Serializable]
    public class PokeApiName
    {
        public PokeApiNameUrl language;
        public string name;
    }

    [System.Serializable]
    public class PokeApiMoveMeta
    {
        public PokeApiNameUrl ailment;
        public int ailment_chance;
        public PokeApiNameUrl category;
        public int crit_rate;
        public int drain;
        public int flinch_chance;
        public int max_hits;
        public int max_turns;
        public int min_hits;
        public int min_turns;
        public int stat_chance;
    }

    [System.Serializable]
    public class PokeApiMoveStatChange
    {
        public int change;
        public PokeApiNameUrl stat;
    }

    [System.Serializable]
    public class PokeApiNameUrl
    {
        public string name;
        public string url;
    }

    [System.Serializable]
    public class PokeApiEvolution
    {
        public string baby_trigger_item;
        public PokeApiEvolutionChain chain;
        public int id;
    }

    [System.Serializable]
    public class PokeApiEvolutionChain
    {
        public PokeApiEvolutionDetails[] evolution_details;
        public PokeApiEvolvesTo[] evolves_to;
        public string is_baby;
        public PokeApiNameUrl species;
    }

    [System.Serializable]
    public class PokeApiEvolvesTo
    {
        public PokeApiEvolutionDetails[] evolution_details;
        public PokeApiEvolvesToSecondStage[] evolves_to;
        public string is_baby;
        public PokeApiNameUrl species;
    }

    [System.Serializable]
    public class PokeApiEvolvesToSecondStage
    {
        public PokeApiEvolutionDetails[] evolution_details;
        public string is_baby;
        public PokeApiNameUrl species;
    }

    [System.Serializable]
    public class PokeApiEvolutionDetails
    {
        public PokeApiNameUrl item;
        public PokeApiNameUrl trigger;
        public int min_level;
    }
}