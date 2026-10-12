using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WandsTool.Content
{
    /// <summary>
    /// 物品选择窗口: 列出背包里当前可放置的物品(跟随面板的 方块/墙壁 开关过滤), 点击高亮选中。
    /// 这一步只记录 gameMain.Wand_SelectItemType, 不改放置时的取物逻辑。
    /// </summary>
    public class wandsItemPanel : UIState
    {
        private const int Col = 10;
        private const int Slot = 40;
        private const int Gap = 2;
        private const int Pad = 8;

        private static Asset<Texture2D> back_on;
        private static Asset<Texture2D> back_off;

        public bool isOpen = false;
        private bool built = false;

        public wandsItemPanel()
        {
            //UIState 的构造默认把 Width/Height.Precent 设成 1f(撑满父级), 而 GetElementAt 是从最后
            //Append 的子元素往前找、命中就 break —— 不归零的话这个浮层会把全屏点击都吃掉
            Width.Set(0, 0);
            Height.Set(0, 0);

            if (back_off == null)
            {
                back_off = Main.Assets.Request<Texture2D>("Images/UI/Wires_0", AssetRequestMode.ImmediateLoad);
                back_on = Main.Assets.Request<Texture2D>("Images/UI/Wires_1", AssetRequestMode.ImmediateLoad);
            }
        }

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;

            //关掉必须把尺寸也归零: GetElementAt 从最后 Append 的子元素往前找, 命中的顶层元素会吸收点击,
            //留着空矩形会把压在下面的面板按钮全吃掉
            Width.Set(0, 0);
            Height.Set(0, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (isOpen && built == false) Build();
            else if (isOpen == false && built)
            {
                RemoveAllChildren();
                built = false;
                return;
            }

            base.Update(gameTime);
        }

        /// <summary>每次打开都按当前背包和 方块/墙壁 开关重建格子, 同一种物品只占一格。</summary>
        private void Build()
        {
            List<int> types = new List<int>();
            Item[] inv = Main.LocalPlayer.inventory;

            for (int i = 0; i < inv.Length; ++i)
            {
                if (inv[i].IsAir) continue;
                if (types.Contains(inv[i].type)) continue;

                if (gameMain.Wand_Tile && inv[i].createTile >= 0) types.Add(inv[i].type);
                else if (gameMain.Wand_Wall && inv[i].createWall > 0) types.Add(inv[i].type);
            }

            int row = Math.Max(1, (types.Count + Col - 1) / Col);

            //当前类别下没有选中项时默认选第一个, 和放置时的回退保持一致
            if (types.Count > 0 && types.Contains(gameMain.Wand_SelectItemType) == false)
                gameMain.Wand_SelectItemType = types[0];

            Width.Set(Col * (Slot + Gap) + Gap + Pad * 2, 0);
            Height.Set(row * (Slot + Gap) + Gap + Pad * 2, 0);
            Left.Set((Main.screenWidth - Width.Pixels) / 2, 0);
            Top.Set((Main.screenHeight - Height.Pixels) / 2, 0);

            RemoveAllChildren();

            UIPanel bg = new UIPanel();
            bg.Width.Set(1f, 1f);
            bg.Height.Set(1f, 1f);
            Append(bg);

            for (int i = 0; i < types.Count; ++i)
            {
                itemSlot s = new itemSlot(types[i], back_on, back_off, OnClick);

                s.Left.Set(Pad + Gap + i % Col * (Slot + Gap), 0);
                s.Top.Set(Pad + Gap + i / Col * (Slot + Gap), 0);

                Append(s);
            }

            built = true;
        }

        private void OnClick(int type)
        {
            gameMain.Wand_SelectItemType = type;
        }

        private class itemSlot : UIState
        {
            private readonly UIImage back;
            private readonly UIImage ico;
            private readonly Asset<Texture2D> on;
            private readonly Asset<Texture2D> off;
            private readonly Action<int> click;
            private readonly int type;

            public itemSlot(int type, Asset<Texture2D> on, Asset<Texture2D> off, Action<int> click)
            {
                this.type = type;
                this.on = on;
                this.off = off;
                this.click = click;

                Width.Set(Slot, 0);
                Height.Set(Slot, 0);

                back = new UIImage(off);
                back.Width.Set(Slot, 0);
                back.Height.Set(Slot, 0);

                //TextureAssets.Item[type] 是懒加载的, 没加载时 .Value 为 null, UIImage 画它必炸
                //(Main.LoadItem 发的是异步请求, 同帧也拿不到)。扩展物品的槽位已由对方填好, 直接用。
                Asset<Texture2D> tex = TextureAssets.Item[type];

                if (tex == null || tex.State == AssetState.NotLoaded)
                    tex = Main.Assets.Request<Texture2D>("Images/Item_" + type, AssetRequestMode.ImmediateLoad);

                if (tex?.Value != null)
                {
                    ico = new UIImage(tex);
                    ico.HAlign = 0.5f;
                    ico.VAlign = 0.5f;
                }

                OnLeftClick += (e, s) => click?.Invoke(type);

                Append(back);

                if (ico != null) Append(ico);
            }

            public override void Update(GameTime gameTime)
            {
                if (IsMouseHovering)
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Main.instance.MouseText(Lang.GetItemNameValue(type));
                }

                //悬停和"已选中"都用亮背景, 和面板按钮同一套视觉
                back.SetImage(IsMouseHovering || type == gameMain.Wand_SelectItemType ? on : off);

                base.Update(gameTime);
            }
        }
    }
}
