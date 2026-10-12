using tContentPatch;
using Terraria;
using Terraria.GameContent.Items;
using Terraria.ID;

namespace WandsTool.WandItem
{
    /// <summary>
    /// 给固定 id 的魔杖填数值。
    /// 原版 Item.SetDefaults 里有 if (type >= ItemID.Count) type = 0, 越界 id 会被清成空气,
    /// 所以必须在后缀里用 ResetStats 把 type 写回来再赋值。
    /// </summary>
    internal class WandItemPatch : PatchItem
    {
        public override void SetDefaultsPostfix(Item This, int Type, ItemVariant variant)
        {
            if (Type != WandItemRegist.ItemId) return;

            This.ResetStats(Type);

            //必须在 ResetStats 之后: 它会把 _nameOverride 和 ToolTip 清成 null
            This.SetNameOverride(WandItemRegist.DisplayName);

            This.useStyle = 5;
            This.useTime = 20;
            This.useAnimation = 20;
            This.noMelee = true;
            This.favorited = true;

            This.RebuildTooltip();
        }
    }
}
