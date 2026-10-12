using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Items;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace WandsTool.WandItem
{
    /// <summary>
    /// 自己注册一个固定物品 id(8405), 不进 ExtenContent 的注册表。
    /// 这里负责"把按 item type 索引的数组扩到这个 id" + 填贴图/名字 + 背包的给与删; 数值在 WandItemPatch。
    /// </summary>
    internal static class WandItemRegist
    {
        /// <summary>固定物品 id, 要和 PlayerWatch 的 8404 错开。</summary>
        public const int ItemId = 8405;

        /// <summary>模组目录下 Resources\Wand.png, 不带扩展名。</summary>
        private const string Texture = "Resources/Wand";

        public const string DisplayName = "魔杖";
        public const string TipText = "[c/ff0000:右键打开魔杖面板]";

        private const string NameKey = "WandsTool.WandItem.Name";
        private const string TipKey = "WandsTool.WandItem.Tooltip";

        private const BindingFlags STATIC = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>模组目录, 由 ModQuickButton.Load 传入, 贴图从那里读。</summary>
        public static string ModPath { get; set; }

        public static bool Registered { get; private set; }

        /// <summary>
        /// 必须在 ExtenContent 的 Loaded 之后调用(同阶段按模组目录序), 它会重建 ItemID.Sets 的数组。
        /// 每次重载都要重跑, 所以不做"已注册就跳过"。
        /// </summary>
        public static void Load()
        {
            // 别人(ExtenContent)已认领的区间, 我们的 id 不能落在里面
            int claimed = (int)typeof(SetFactory)
                .GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(ItemID.Sets.Factory);

            if (ItemId < claimed)
                throw new Exception($"物品 id[{ItemId}] 落在别人已扩容区间内(当前长度 {claimed}), 请改大 ItemId");

            foreach (Type sets in new[] { typeof(ItemID.Sets), typeof(AmmoID.Sets), typeof(PrefixLegacy.ItemSets) })
                foreach (FieldInfo fi in sets.GetFields(STATIC))
                {
                    // Factory(SetFactory) / List<int> 这类字段不是按 type 索引的数组
                    if (!fi.FieldType.IsArray) continue;

                    fi.SetValue(null, Grow((Array)fi.GetValue(null)));
                }

            GrowField(typeof(TextureAssets), "Item");
            GrowField(typeof(TextureAssets), "ItemFlame");
            GrowField(typeof(Item), "staff");
            GrowField(typeof(Item), "claw");
            GrowField(typeof(Item), "cachedItemSpawnsByType");
            //原版 Player.cs:55317 只有 staff 标记为 true 的物品才会沿 itemRotation 把手里的法杖伸出去
            Item.staff[ItemId] = true;
            GrowField(typeof(Lang), "_itemNameCache");
            GrowField(typeof(Lang), "_itemTooltipCache");
            GrowField(typeof(Main), "itemAnimations");
            GrowField(typeof(ArmorSetBonuses), "SetsContaining");

            TextureAssets.Item[ItemId] = LoadTexture(Texture);

            //tooltip 正文: Lang.GetTooltip 直接下标 _itemTooltipCache, 没有越界守卫
            SetSlot(typeof(Lang), "_itemTooltipCache", ItemTooltip.FromLanguageKey(Text(TipKey, TipText).Key));
            //名字表头走 Item.Name 的 _nameOverride(见 WandItemPatch); 这里只给少数直接查 Lang 的路径兜底
            SetSlot(typeof(Lang), "_itemNameCache", Text(NameKey, DisplayName));

            Item sample = new Item();
            sample.SetDefaults(ItemId);
            ContentSamples.ItemsByType[ItemId] = sample;

            Registered = true;
        }

        /// <summary>把物品放进背包第一个空格, 已有则不重复给。</summary>
        public static void GiveToInventory()
        {
            if (!Registered) return;

            Item[] inv = Main.LocalPlayer.inventory;

            for (int i = 0; i < 50; ++i)
            {
                if (inv[i].type == ItemId) return;
            }

            for (int i = 0; i < 50; ++i)
            {
                if (inv[i].IsAir)
                {
                    inv[i].SetDefaults(ItemId);
                    inv[i].stack = 1;
                    return;
                }
            }

            Main.NewText("WandsTool: 背包已满, 放不下魔杖");
        }

        /// <summary>删掉背包里所有该物品。</summary>
        public static void RemoveAllFromInventory()
        {
            Item[] inv = Main.LocalPlayer.inventory;

            for (int i = 0; i < 50; ++i)
            {
                if (inv[i].type == ItemId) inv[i].TurnToAir();
            }
        }

        /// <summary>当前是否手持本物品。</summary>
        public static bool IsHeldByLocalPlayer()
        {
            return Main.LocalPlayer.HeldItem.type == ItemId;
        }

        private static AssetRepository _assets;

        /// <summary>以模组目录为资源源。</summary>
        private static Asset<Texture2D> LoadTexture(string name)
        {
            if (_assets == null)
            {
                AssetReaderCollection readers = new AssetReaderCollection();
                readers.RegisterReader(new XnbReader(Main.instance.Services), ".xnb");
                readers.RegisterReader(new PngReader(Main.instance.GraphicsDevice), ".png");

                _assets = new AssetRepository(new AssetLoader(readers), new AsyncAssetLoader(readers, 20));
                _assets.SetSources(new IContentSource[] { new FileSystemContentSource(ModPath) });
            }

            Asset<Texture2D> asset = _assets.Request<Texture2D>(name, AssetRequestMode.ImmediateLoad);
            if (asset == null || asset.Value == null)
                throw new Exception($"魔杖贴图加载失败: {ModPath}\\{name}.png");

            return asset;
        }

        private static void GrowField(Type owner, string field)
        {
            FieldInfo fi = owner.GetField(field, STATIC);
            if (fi == null) throw new Exception($"找不到字段 {owner.Name}.{field}, 原版结构变了");
            if (!fi.FieldType.IsArray) throw new Exception($"{owner.Name}.{field} 不是数组, 原版结构变了");

            fi.SetValue(null, Grow((Array)fi.GetValue(null)));
        }

        /// <summary>
        /// 只扩"按 item type 索引"的数组(长度至少到原版边界), 且已经覆盖到 ItemId 的不动 ——
        /// 别的模组可能已经把它扩得更大, 所以判据不能是"长度等于某个固定值"。
        /// 新槽用 src[0] 的值填(item 0 = 空气, 语义就是"无特殊处理")。
        /// </summary>
        private static Array Grow(Array src)
        {
            if (src == null || src.Length < ItemID.Count || src.Length > ItemId) return src;

            Array dst = Array.CreateInstance(src.GetType().GetElementType(), ItemId + 1);
            Array.Copy(src, dst, src.Length);

            object fill = src.GetValue(0);
            for (int i = src.Length; i < dst.Length; ++i) dst.SetValue(fill, i);

            return dst;
        }

        private static void SetSlot(Type owner, string field, object val)
        {
            ((Array)owner.GetField(field, STATIC).GetValue(null)).SetValue(val, ItemId);
        }

        /// <summary>注册本地化文本并写值。</summary>
        private static LocalizedText Text(string key, string val)
        {
            LocalizedText lt = Language.GetText(key);
            typeof(LocalizedText)
                .GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(lt, new object[] { val });

            return lt;
        }
    }
}
