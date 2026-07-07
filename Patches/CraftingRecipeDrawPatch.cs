using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FishBasket.Patches
{
    internal static class CraftingRecipeDrawPatch
    {
        private static bool _logged;

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), "drawMenuView"),
                postfix: new HarmonyMethod(typeof(CraftingRecipeDrawPatch), nameof(Postfix))
            );
        }

        private static void Postfix(CraftingRecipe __instance, SpriteBatch b, int x, int y)
        {
            List<string> items = __instance.itemToProduce;
            if (!_logged)
            {
                _logged = true;
                ModEntry.StaticMonitor?.Log($"[FishBasket] drawMenuView called. itemToProduce=({string.Join(",", items ?? new List<string>())}) count={items?.Count}", LogLevel.Info);
            }

            string? itemId = items?.FirstOrDefault();
            if (itemId == null || !itemId.Contains("FishBasket")) return;

            string? tier = null;
            if (itemId.EndsWith("Basic")) tier = "Basic";
            else if (itemId.EndsWith("Silver")) tier = "Silver";
            else if (itemId.EndsWith("Gold")) tier = "Gold";
            else if (itemId.EndsWith("Iridium")) tier = "Iridium";
            if (tier == null) return;

            try
            {
                int idx = 691;
                Texture2D objects = Game1.content.Load<Texture2D>("Maps/springobjects");
                Rectangle hookRect = new(idx % 24 * 16, idx / 24 * 16, 16, 16);
                Vector2 hookPos = new(x + 36, y + 4);
                b.Draw(objects, hookPos, hookRect, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.871f);
            }
            catch { }

            Point pt = tier switch
            {
                "Silver" => new Point(338, 400),
                "Gold" => new Point(346, 400),
                "Iridium" => new Point(346, 392),
                _ => new Point(0, 0)
            };

            if (pt.X > 0)
            {
                Vector2 starPos = new(x + 16, y + 36);
                b.Draw(Game1.mouseCursors, starPos, new Rectangle(pt.X, pt.Y, 8, 8), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.872f);
            }
        }
    }
}