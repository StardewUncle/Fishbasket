using System.Collections.Generic;
using HarmonyLib;
using StardewValley;
using StardewValley.Inventories;
using SObject = StardewValley.Object;

namespace FishBasket.Patches
{
    internal static class CraftingRecipePatch
    {
        private static readonly Dictionary<string, int> QualityRequired = new()
        {
            { "StardewUncle.FishBasket_Basic", 0 },
            { "StardewUncle.FishBasket_Silver", 1 },
            { "StardewUncle.FishBasket_Gold", 2 },
            { "StardewUncle.FishBasket_Iridium", 4 },
        };

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.doesFarmerHaveIngredientsInInventory)),
                postfix: new HarmonyMethod(typeof(CraftingRecipePatch), nameof(DoesFarmerHaveIngredientsPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                postfix: new HarmonyMethod(typeof(CraftingRecipePatch), nameof(ConsumeIngredientsPostfix))
            );
        }

        private static void DoesFarmerHaveIngredientsPostfix(CraftingRecipe __instance, IList<Item> extraToCheck, ref bool __result)
        {
            if (!__result)
                return;

            int requiredQuality = GetRequiredQuality(__instance);
            if (requiredQuality < 0)
                return;

            bool hasFish = false;
            foreach (var item in extraToCheck)
            {
                if (item is SObject obj && obj.Category == -4 && obj.Quality >= requiredQuality && obj.Stack > 0)
                {
                    hasFish = true;
                    break;
                }
            }

            if (!hasFish)
                __result = false;
        }

        private static void ConsumeIngredientsPostfix(CraftingRecipe __instance, List<IInventory> additionalMaterials)
        {
            int requiredQuality = GetRequiredQuality(__instance);
            if (requiredQuality < 0)
                return;

            Farmer who = Game1.player;
            if (who == null)
                return;

            foreach (var item in who.Items)
            {
                if (item is SObject obj && obj.Category == -4 && obj.Quality >= requiredQuality && obj.Stack > 0)
                {
                    obj.Stack--;
                    if (obj.Stack <= 0)
                        who.removeItemFromInventory(obj);
                    return;
                }
            }
        }

        private static int GetRequiredQuality(CraftingRecipe recipe)
        {
            if (recipe.itemToProduce == null || recipe.itemToProduce.Count == 0)
                return -1;
            string itemId = recipe.itemToProduce[0];
            if (QualityRequired.TryGetValue(itemId, out int quality))
                return quality;
            return -1;
        }
    }
}
