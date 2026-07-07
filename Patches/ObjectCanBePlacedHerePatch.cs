using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace FishBasket.Patches
{
    internal static class ObjectCanBePlacedHerePatch
    {
        private static readonly string[] FishBasketIds = new[]
        {
            "StardewUncle.FishBasket_Basic",
            "StardewUncle.FishBasket_Silver",
            "StardewUncle.FishBasket_Gold",
            "StardewUncle.FishBasket_Iridium",
        };

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), "canBePlacedHere"),
                prefix: new HarmonyMethod(typeof(ObjectCanBePlacedHerePatch), nameof(Prefix))
            );
        }

        private static bool Prefix(SObject __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            bool isFishBasket = false;
            foreach (string id in FishBasketIds)
            {
                if (__instance.ItemId == id)
                {
                    isFishBasket = true;
                    break;
                }
            }

            if (!isFishBasket)
                return true; // kein Fischkorb -> Original entscheidet normal

            __result = CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y);
            return false; // Original überspringen, wir haben entschieden
        }
    }
}