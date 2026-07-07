using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace FishBasket.Patches
{
    internal static class ObjectDrawInMenuPatch
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), "drawInMenu", new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(ObjectDrawInMenuPatch), nameof(Postfix))
            );
        }

        private static string? GetTier(Item obj)
        {
            string? id = obj.ItemId;
            if (id == null) return null;
            if (id.EndsWith("Basic")) return "Basic";
            if (id.EndsWith("Silver")) return "Silver";
            if (id.EndsWith("Gold")) return "Gold";
            if (id.EndsWith("Iridium")) return "Iridium";
            return null;
        }

        private static Point StarSource(string? tier)
        {
            return tier switch
            {
                "Silver" => new Point(338, 400),
                "Gold" => new Point(346, 400),
                "Iridium" => new Point(346, 392),
                _ => new Point(0, 0),
            };
        }

        private static void DrawOverlay(SpriteBatch spriteBatch, Vector2 location, float transparency, float layerDepth, Color color)
        {
            try
            {
                int idx = 691;
                Texture2D objects = Game1.content.Load<Texture2D>("Maps/springobjects");
                Rectangle hookRect = new(idx % 24 * 16, idx / 24 * 16, 16, 16);
                Vector2 hookPos = location + new Vector2(36f, 4f);
                spriteBatch.Draw(objects, hookPos, hookRect, color * transparency, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, layerDepth + 0.001f);
            }
            catch { }
        }

        private static void DrawStar(SpriteBatch spriteBatch, Vector2 location, float transparency, float layerDepth, string? tier)
        {
            Point pt = StarSource(tier);
            if (pt.X > 0)
            {
                Vector2 starPos = location + new Vector2(16f, 36f);
                spriteBatch.Draw(Game1.mouseCursors, starPos, new Rectangle(pt.X, pt.Y, 8, 8), Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.002f);
            }
        }

        private static void Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            string? tier = GetTier(__instance);
            if (tier == null) return;
            DrawOverlay(spriteBatch, location, transparency, layerDepth, color);
            DrawStar(spriteBatch, location, transparency, layerDepth, tier);
        }


    }
}