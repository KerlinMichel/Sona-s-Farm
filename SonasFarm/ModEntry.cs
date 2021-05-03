using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;

namespace SonasFarm
{

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        // Reference to Sona character
        private static Cat sona = null;

        private static Random rand = new Random();
        // probabilty that Sona will catch something and then give it to the owner in the morning
        private float SonaCatchEvenRate = 0.15f;

        enum SonaState
        {
            Normal, // Sona is just a normal Stardew Valley cat
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            // Version 0.0.2 and below has a bug where a new Sona is created every new day. This command removes the extra Sonas
            helper.ConsoleCommands.Add("too_many_sonas", "Removes extra Sonas.\n\nUsage: too_many_sonas <number of sonas>\n- value: number of extra sonas.", this.TooManySonas);


            // bootstrap Sona into the game
            EventHandler<DayStartedEventArgs> sonaEntersTheMatrix = null;
            sonaEntersTheMatrix = (s, e) =>
            {
                SonaEntersTheMatrix();
                helper.Events.GameLoop.DayStarted -= sonaEntersTheMatrix;
            };
            helper.Events.GameLoop.DayStarted += sonaEntersTheMatrix;

            // everyday there is chance that Sona catches something from the night before and leaves it in the farmhouse in the morning
            helper.Events.GameLoop.DayStarted += SonaCatchEvent;

            // Sona will randomly meow when the farmer is in the same location as Sona
            helper.Events.GameLoop.OneSecondUpdateTicking += SonaRandomeMewo;
        }

        private void SonaRandomeMewo(object sender, OneSecondUpdateTickingEventArgs e)
        {
            Cat sona = GetSona();

            if (sona == null)
            {
                return;
            }

            foreach (GameLocation location in Game1.locations)
            {
                if (location is Farm || location is FarmHouse)
                {
                    if (IsSonaThere(location) && Game1.currentLocation == location && rand.NextDouble() < 0.05f)
                    {
                        sona.playContentSound();
                    }
                }
            }
        }

        private void TooManySonas(string command, string[] args)
        {
            int num_sonas = int.Parse(args[0]);

            int i = 0;

            foreach (GameLocation location in Game1.locations)
            {
                if (location is Farm || location is FarmHouse)
                {
                    location.characters.Filter(npc =>
                    {
                        bool remove = npc is Cat && npc.name == "Cat" && ((Cat)npc).whichBreed == 0;
                        if (remove)
                        {
                            i++;
                        }

                        return !remove && i < num_sonas;
                    });
                }
            }
        }

        private bool IsSonaThere(GameLocation location)
        {
            foreach(Character npc in location.characters)
            {
                if(npc is Cat && npc.name == "Sona" && ((Cat)npc).whichBreed == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private Cat GetSona()
        {
            foreach (GameLocation location in Game1.locations)
            {
                if (location is Farm || location is FarmHouse)
                {
                    foreach (Character npc in location.characters)
                    {
                        if (npc is Cat && npc.name == "Sona" && ((Cat)npc).whichBreed == 0)
                        {
                            return (Cat) npc;
                        }
                    }
                }
            }

            return null;
        }

        // Add Sona to the FarmHouse location
        private void SonaEntersTheMatrix()
        {
            if (sona == null)
            {
                sona = new Cat(6, 6, 0);
                sona.Name = "Sona";
            }

            if (!IsSonaThere(Game1.getLocationFromName("FarmHouse")) && !IsSonaThere(Game1.getLocationFromName("FarmHouse")))
            {
                Game1.getLocationFromName("FarmHouse").addCharacter(sona);
            }
        }

        private void SonaCatchEvent(object sender, DayStartedEventArgs e)
        {
            var r = rand.NextDouble();

            if (r < SonaCatchEvenRate)
            {
                Game1.chatBox.addMessage("Sona: \"MEOOOOWOWOW!\" (Translation: Sona caught something and wants you to check it out.)", Color.Orange);

                int idx = rand.Next(itemIds.Count);
                var item = new StardewValley.Object(new Vector2(5, 8), itemIds[idx], null, false, true, false, true);
                Game1.getLocationFromName("FarmHouse").objects.Add(new Vector2(5, 8), item);

                Game1.chatBox.addMessage($"Sona found {itemIdToName(itemIds[idx])}", Color.Orange);
            }
        }

        private static List<int> itemIds = new List<int> { 
            446, // Rabbit's Foot
            800, // Blob Fish
            767, // Bat Wing
            440, // Wool
            440,
            440,
            168, // Trash,
            168,
            168,
            168,
            168,
            168,
            168,
            168,
            747, // Rotten Plant
            747,
            747,
            747,
            172, // Soggy newspaper
            172,
            172,
            172,
            64, // Ruby
            60, // Emerald
        };

        private String itemIdToName(int id)
        {
            switch(id)
            {
                case 446:
                    return "a Rabbit's Foot for you!";
                case 800:
                    return "a Blob Fish for you!";
                case 767:
                    return "a Bat Wing for you!";
                case 440:
                    return "a piece of wool for you!";
                case 168:
                    return "some trash for you! Sona < Trash."; // '<' is a heart in Stardew Valley
                case 747:
                    return "a rotten plant for you! Sona needs to stop eating random plants.";
                case 172:
                    return "a soggy newspaper for you! Breaking News: Sona loves all the trash in the world!";
                case 64:
                    return "a ruby! Has she been going in the caves?!";
                case 60:
                    return "an emerald! Where did she get that?";
                default:
                    return "item";
            }
        }
    }
}