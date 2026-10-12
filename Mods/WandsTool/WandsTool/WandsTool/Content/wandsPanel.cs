using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace WandsTool.Content
{
    public class wandsPanel_btn1 : UIState
    {
        private UIImage back = null;
        private Asset<Texture2D> back_img1 = null;
        private Asset<Texture2D> back_img2 = null;
        private UIImage ico = null;
        public string mouseText = null;
        public bool isBack = false;


        public wandsPanel_btn1(Texture2D img1, string mouseText, string badge = null)
        {
            back_img1 = Main.Assets.Request<Texture2D>("Images/UI/Wires_0", AssetRequestMode.ImmediateLoad);
            back_img2 = Main.Assets.Request<Texture2D>("Images/UI/Wires_1", AssetRequestMode.ImmediateLoad);
            this.mouseText = mouseText;

            back = new UIImage(back_img1);

            Width.Set(40, 0);
            Height.Set(Width.Pixels, 0);

            back.Width.Set(Width.Pixels, 0);
            back.Height.Set(Height.Pixels, 0);

            //img1 为 null 时只画角标文字(速度 -/+ 没有合适的原版图标)
            if (img1 != null)
            {
                ico = new UIImage(img1);

                ico.Width.Set(16, 0);
                ico.Height.Set(16, 0);
                ico.HAlign = 0.5f;
                ico.VAlign = 0.5f;
                ico.ScaleToFit = true;
            }

            OnLeftClick += (e, s) => SoundEngine.PlaySound(SoundID.MenuTick);

            Append(back);

            if (ico != null) Append(ico);

            //角标文字, 给没有现成图标的按钮(速度+/-)用
            if (badge != null)
            {
                UIText text = new UIText(badge, 1.2f);
                text.HAlign = 0.5f;
                text.VAlign = 0.5f;
                Append(text);
            }
        }
        public wandsPanel_btn1(string img1, string mouseText, string badge = null) :
            this(Main.Assets.Request<Texture2D>(img1, AssetRequestMode.ImmediateLoad)?.Value, mouseText, badge)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Terraria.Player player = Main.LocalPlayer;
                if (player != null) player.mouseInterface = true;

                if (mouseText != null) Main.instance.MouseText(mouseText);

                //back.SetImage(back_img2);
                back.SetImage(isBack ? back_img2 : back_img1);
            }
            else
            {
                back.SetImage(isBack ? back_img2 : back_img1);
            }

            base.Update(gameTime);
        }

        public void SetIco(Texture2D img1)
        {
            if (ico == null) return;

            ico.SetImage(img1);
        }

        public void SetIco(string img1)
        {
            SetIco(Main.Assets.Request<Texture2D>(img1, AssetRequestMode.ImmediateLoad)?.Value);
        }
    }

    public class wandsPanel : UserInterface
    {
        protected UIState container = null;
        protected UIState btns = null;
        protected UIState btns_2 = null;
        protected UIState btns_3 = null;
        protected UIState btns_4 = null;
        protected UIState btns_5 = null;
        protected wandsPanel_btn1 btn1 = null;//破坏放置
        protected wandsPanel_btn1 btn2 = null;//方块墙壁
        protected wandsPanel_btn1 btn2_tile = null;
        protected wandsPanel_btn1 btn2_wall = null;
        protected wandsPanel_btn1 btn2_wire_red = null;
        protected wandsPanel_btn1 btn2_wire_green = null;
        protected wandsPanel_btn1 btn2_wire_blue = null;
        protected wandsPanel_btn1 btn2_wire_yellow = null;
        protected wandsPanel_btn1 btn2_wire_actuator = null;
        protected wandsPanel_btn1 btn3 = null;//形状
        protected wandsPanel_btn1 btn3_line = null;
        protected wandsPanel_btn1 btn3_circular = null;
        protected wandsPanel_btn1 btn3_rectangle = null;
        protected wandsPanel_btn1 btn4 = null;//方向
        protected wandsPanel_btn1 btn4_Solid = null;
        protected wandsPanel_btn1 btn4_HalfBlock = null;
        protected wandsPanel_btn1 btn4_SlopeUpLeft = null;
        protected wandsPanel_btn1 btn4_SlopeUpRight = null;
        protected wandsPanel_btn1 btn4_SlopeDownLeft = null;
        protected wandsPanel_btn1 btn4_SlopeDownRight = null;
        protected wandsPanel_btn1 btn5_more = null;//其他设置
        protected wandsPanel_btn1 btn5_speed_minus = null;//速度-
        protected wandsPanel_btn1 btn5_speed_plus = null;//速度+
        protected wandsPanel_btn1 btn5_teleport = null;//联机传送放置
        protected wandsPanel_btn1 btn6_item = null;//物品选择
        protected wandsItemPanel itemPanel = null;

        /// <summary>给 feces 在关面板时一起关掉选择窗口用。</summary>
        public wandsItemPanel ItemPanel => itemPanel;
        public bool isReset = true;

        /// <summary>速度上限, 每帧处理的格子数。</summary>
        public const int SpeedMax = 200;


        public wandsPanel()
        {
            container = new UIState();
            btns = new UIState();
            btns_2 = new UIState();
            btns_3 = new UIState();
            btns_4 = new UIState();
            btns_5 = new UIState();
            btn1 = new wandsPanel_btn1("Images/Item_1", "破坏放置");
            btn2 = new wandsPanel_btn1("Images/Item_2", "方块墙壁");
            btn2_tile = new wandsPanel_btn1("Images/Item_2", "方块");
            btn2_wall = new wandsPanel_btn1("Images/Item_30", "墙壁");
            btn2_wire_red = new wandsPanel_btn1("Images/UI/Wires_2", "红线");
            btn2_wire_green = new wandsPanel_btn1("Images/UI/Wires_3", "绿线");
            btn2_wire_blue = new wandsPanel_btn1("Images/UI/Wires_4", "蓝线");
            btn2_wire_yellow = new wandsPanel_btn1("Images/UI/Wires_5", "黄线");
            btn2_wire_actuator = new wandsPanel_btn1("Images/UI/Wires_10", "制动器");
            btn3 = new wandsPanel_btn1(Resources.Images_ShapesLine, "形状");
            btn3_line = new wandsPanel_btn1(Resources.Images_ShapesLine, "线");
            btn3_circular = new wandsPanel_btn1(Resources.Images_ShapesCircular, "圆");
            btn3_rectangle = new wandsPanel_btn1(Resources.Images_ShapesRectangle, "矩形");
            btn4 = new wandsPanel_btn1(Resources.Images_SlopeSolid, "方向");
            btn4_Solid = new wandsPanel_btn1(Resources.Images_SlopeSolid, null);
            btn4_HalfBlock = new wandsPanel_btn1(Resources.Images_SlopeHalfBlock, null);
            btn4_SlopeUpLeft = new wandsPanel_btn1(Resources.Images_SlopeUpLeft, null);
            btn4_SlopeUpRight = new wandsPanel_btn1(Resources.Images_SlopeUpRight, null);
            btn4_SlopeDownLeft = new wandsPanel_btn1(Resources.Images_SlopeDownLeft, null);
            btn4_SlopeDownRight = new wandsPanel_btn1(Resources.Images_SlopeDownRight, null);
            btn5_more = new wandsPanel_btn1((Texture2D)null, "其他设置", "...");
            btn5_speed_minus = new wandsPanel_btn1((Texture2D)null, null, "-");
            btn5_speed_plus = new wandsPanel_btn1((Texture2D)null, null, "+");
            btn5_teleport = new wandsPanel_btn1("Images/Item_2222", "联机传送放置");
            btn6_item = new wandsPanel_btn1(Resources.Images_WandCreate, "物品选择");
            itemPanel = new wandsItemPanel();

            btn5_more.OnLeftClick += (e, s) => onClick(4);
            btn5_speed_minus.OnLeftClick += (e, s) => SetSpeed(gameMain.Wand_UpdateCount + SpeedStep(-1));
            btn5_speed_plus.OnLeftClick += (e, s) => SetSpeed(gameMain.Wand_UpdateCount + SpeedStep(1));
            btn5_teleport.OnLeftClick += (e, s) => gameMain.Wand_TeleportPlace = !gameMain.Wand_TeleportPlace;
            btn6_item.OnLeftClick += (e, s) =>
            {
                if (itemPanel.isOpen) itemPanel.Close();
                else itemPanel.Open();
            };

            btn1.OnLeftClick += (e, s) => onClick(0);
            btn2.OnLeftClick += (e, s) => onClick(1);
            btn3.OnLeftClick += (e, s) => onClick(2);
            btn4.OnLeftClick += (e, s) => onClick(3);

            Action<Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode> btn2_wire_action = (v) =>
            {
                if (gameMain.Wand_ToolMode.HasFlag(v)) gameMain.Wand_ToolMode &= ~v;
                else gameMain.Wand_ToolMode |= v;
            };
            btn2_tile.OnLeftClick += (e, s) => gameMain.Wand_Tile = !gameMain.Wand_Tile;
            btn2_wall.OnLeftClick += (e, s) => gameMain.Wand_Wall = !gameMain.Wand_Wall;
            btn2_wire_red.OnLeftClick += (e, s) => btn2_wire_action(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Red);
            btn2_wire_green.OnLeftClick += (e, s) => btn2_wire_action(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Green);
            btn2_wire_blue.OnLeftClick += (e, s) => btn2_wire_action(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Blue);
            btn2_wire_yellow.OnLeftClick += (e, s) => btn2_wire_action(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Yellow);
            btn2_wire_actuator.OnLeftClick += (e, s) => btn2_wire_action(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Actuator);
            btn3_line.OnLeftClick += (e, s) => gameMain.Wand_Shapes = Wands.Shapes.line;
            btn3_circular.OnLeftClick += (e, s) => gameMain.Wand_Shapes = Wands.Shapes.circular;
            btn3_rectangle.OnLeftClick += (e, s) => gameMain.Wand_Shapes = Wands.Shapes.rectangle;
            btn4_Solid.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.Solid;
            btn4_HalfBlock.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.HalfBlock;
            btn4_SlopeUpLeft.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.SlopeUpLeft;
            btn4_SlopeUpRight.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.SlopeUpRight;
            btn4_SlopeDownLeft.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.SlopeDownLeft;
            btn4_SlopeDownRight.OnLeftClick += (e, s) => gameMain.Wand_BlockType = WandAction.BlockType.SlopeDownRight;

            SetState(container);
            container.Append(btns);
            container.Append(btn1);
            container.Append(btn2);
            container.Append(btn3);
            container.Append(btn4);
            container.Append(btn5_more);
            container.Append(btn6_item);
            container.Append(itemPanel);
            btns_2.Append(btn2_tile);
            btns_2.Append(btn2_wall);
            btns_2.Append(btn2_wire_red);
            btns_2.Append(btn2_wire_green);
            btns_2.Append(btn2_wire_blue);
            btns_2.Append(btn2_wire_yellow);
            btns_2.Append(btn2_wire_actuator);
            btns_3.Append(btn3_line);
            btns_3.Append(btn3_circular);
            btns_3.Append(btn3_rectangle);
            btns_4.Append(btn4_Solid);
            btns_4.Append(btn4_HalfBlock);
            btns_4.Append(btn4_SlopeUpLeft);
            btns_4.Append(btn4_SlopeUpRight);
            btns_4.Append(btn4_SlopeDownLeft);
            btns_4.Append(btn4_SlopeDownRight);
            btns_5.Append(btn5_speed_minus);
            btns_5.Append(btn5_speed_plus);
            btns_5.Append(btn5_teleport);
        }


        public void update(GameTime time)
        {
            if (isReset)
            {
                isReset = false;
                Reset();
            }

            btn1.SetIco(gameMain.Wand_isPlace ? "Images/Item_129" : "Images/Item_1");

            switch (gameMain.Wand_Shapes)
            {
                case Wands.Shapes.line: btn3.SetIco(Resources.Images_ShapesLine); break;
                case Wands.Shapes.circular: btn3.SetIco(Resources.Images_ShapesCircular); break;
                case Wands.Shapes.rectangle: btn3.SetIco(Resources.Images_ShapesRectangle); break;
                default: break;
            }

            switch (gameMain.Wand_BlockType)
            {
                case WandAction.BlockType.Solid: btn4.SetIco(Resources.Images_SlopeSolid); break;
                case WandAction.BlockType.HalfBlock: btn4.SetIco(Resources.Images_SlopeHalfBlock); break;
                case WandAction.BlockType.SlopeUpLeft: btn4.SetIco(Resources.Images_SlopeUpLeft); break;
                case WandAction.BlockType.SlopeUpRight: btn4.SetIco(Resources.Images_SlopeUpRight); break;
                case WandAction.BlockType.SlopeDownLeft: btn4.SetIco(Resources.Images_SlopeDownLeft); break;
                case WandAction.BlockType.SlopeDownRight: btn4.SetIco(Resources.Images_SlopeDownRight); break;
                default: break;
            }

            btn2_tile.isBack = gameMain.Wand_Tile;
            btn2_wall.isBack = gameMain.Wand_Wall;
            btn2_wire_red.isBack = gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Red);
            btn2_wire_green.isBack = gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Green);
            btn2_wire_blue.isBack = gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Blue);
            btn2_wire_yellow.isBack = gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Yellow);
            btn2_wire_actuator.isBack = gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Actuator);
            btn3_line.isBack = gameMain.Wand_Shapes == Wands.Shapes.line;
            btn3_circular.isBack = gameMain.Wand_Shapes == Wands.Shapes.circular;
            btn3_rectangle.isBack = gameMain.Wand_Shapes == Wands.Shapes.rectangle;
            btn4_Solid.isBack = gameMain.Wand_BlockType == WandAction.BlockType.Solid;
            btn4_HalfBlock.isBack = gameMain.Wand_BlockType == WandAction.BlockType.HalfBlock;
            btn4_SlopeUpLeft.isBack = gameMain.Wand_BlockType == WandAction.BlockType.SlopeUpLeft;
            btn4_SlopeUpRight.isBack = gameMain.Wand_BlockType == WandAction.BlockType.SlopeUpRight;
            btn4_SlopeDownLeft.isBack = gameMain.Wand_BlockType == WandAction.BlockType.SlopeDownLeft;
            btn4_SlopeDownRight.isBack = gameMain.Wand_BlockType == WandAction.BlockType.SlopeDownRight;

            btn5_teleport.isBack = gameMain.Wand_TeleportPlace;

            btn5_speed_minus.mouseText = $"速度-  当前每帧 {gameMain.Wand_UpdateCount} 格";
            btn5_speed_plus.mouseText = $"速度+  当前每帧 {gameMain.Wand_UpdateCount} 格 (上限 {SpeedMax}, 按 Ctrl 每次 5)";
        }

        /// <summary>按着 Ctrl 时步进 5。</summary>
        private static int SpeedStep(int step)
        {
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) ||
                Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)) step *= 5;

            return step;
        }

        private static void SetSpeed(int v)
        {
            if (v < 1) v = 1;
            if (v > SpeedMax) v = SpeedMax;

            gameMain.Wand_UpdateCount = v;
        }

        public void Reset()
        {
            int margin = 4;

            btn1.Left.Set(Main.mouseX - btn1.Width.Pixels - margin, 0);
            btn1.Top.Set(Main.mouseY - btn1.Height.Pixels - margin, 0);

            btn2.Left.Set(Main.mouseX + margin, 0);
            btn2.Top.Set(Main.mouseY - btn2.Height.Pixels - margin, 0);

            btn3.Left.Set(Main.mouseX - btn3.Width.Pixels - margin, 0);
            btn3.Top.Set(Main.mouseY + margin, 0);

            btn4.Left.Set(Main.mouseX + margin, 0);
            btn4.Top.Set(Main.mouseY + margin, 0);

            //第 5 个主按钮放正上方, 紧贴 btn1/btn2 那排, 点开是下面那一横排
            btn5_more.Left.Set(Main.mouseX - btn5_more.Width.Pixels / 2, 0);
            btn5_more.Top.Set(Main.mouseY - btn1.Height.Pixels * 2 - margin, 0);

            //物品选择按钮放正下方, 和上方的"..."对称(下方第一格是速度横排)
            btn6_item.Left.Set(Main.mouseX - btn6_item.Width.Pixels / 2, 0);
            btn6_item.Top.Set(Main.mouseY + btn3.Height.Pixels * 2 + margin * 3, 0);

            //速度- / 速度+ / 传送 排一横排, 放在 btn3/btn4 下面
            float pitch = btn5_speed_minus.Width.Pixels + margin;
            float row = Main.mouseY + margin + btn3.Height.Pixels + margin;

            btn5_speed_minus.Left.Set(Main.mouseX - pitch * 1.5f, 0);
            btn5_speed_minus.Top.Set(row, 0);
            btn5_speed_plus.Left.Set(Main.mouseX - pitch / 2, 0);
            btn5_speed_plus.Top.Set(row, 0);
            btn5_teleport.Left.Set(Main.mouseX + pitch / 2, 0);
            btn5_teleport.Top.Set(row, 0);

            Action<int, int, Terraria.UI.UIElement> action = (c, i, ui) =>
            {
                float rad = MathHelper.TwoPi / c;
                Vector2 p = (rad * i).ToRotationVector2() * 80;
                p += Main.MouseScreen;

                ui.Left.Set(p.X - ui.Width.Pixels / 2, 0);
                ui.Top.Set(p.Y - ui.Height.Pixels / 2, 0);
            };

            action.Invoke(7, 0, btn2_tile);
            action.Invoke(7, 1, btn2_wall);
            action.Invoke(7, 2, btn2_wire_red);
            action.Invoke(7, 3, btn2_wire_green);
            action.Invoke(7, 4, btn2_wire_blue);
            action.Invoke(7, 5, btn2_wire_yellow);
            action.Invoke(7, 6, btn2_wire_actuator);

            action.Invoke(3, 0, btn3_line);
            action.Invoke(3, 1, btn3_circular);
            action.Invoke(3, 2, btn3_rectangle);

            action.Invoke(6, 0, btn4_Solid);
            action.Invoke(6, 1, btn4_HalfBlock);
            action.Invoke(6, 2, btn4_SlopeUpLeft);
            action.Invoke(6, 3, btn4_SlopeUpRight);
            action.Invoke(6, 4, btn4_SlopeDownLeft);
            action.Invoke(6, 5, btn4_SlopeDownRight);
        }

        private void onClick(int index)
        {
            btns.RemoveAllChildren();

            switch (index)
            {
                case 0: gameMain.Wand_isPlace = !gameMain.Wand_isPlace; break;
                case 1: btns.Append(btns_2); break;
                case 2: btns.Append(btns_3); break;
                case 3: btns.Append(btns_4); break;
                case 4: btns.Append(btns_5); break;
                default: break;
            }
        }
    }
}
