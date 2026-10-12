using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using WandsTool.WandItem;

namespace WandsTool.Content
{
    public partial class Wands
    {
        public enum Shapes
        {
            line,
            circular,
            rectangle,
        }


        private static Vector2 position1;
        private static Vector2 position2;
        private static bool selecting = false;
        private static List<Point> shapes = null;
        private static Shapes shapes_s = Shapes.line;


        public static void Reset()
        {
            selecting = false;
            shapes = null;
        }

        public static void Update()
        {
            Update_Select();
        }

        private static void Update_Select()
        {
            if (Main.mouseRight == true && selecting == true)
            {
                selecting = false;

                Terraria.Player player = Main.LocalPlayer;
                if (player == null) return;

                CombatText.NewText(player.getRect(), Color.Red,
                    $"取消{(gameMain.Wand_isPlace ? "放置" : "破坏")}", true, false);

                return;
            }

            //悬停在任何界面上(背包/物品栏/魔杖面板自己)时不开始选区, 和原版"丢出物品"的判定一致
            if (Main.mouseLeft == true && Main.mouseLeftRelease == true && Main.mouseRight == false && selecting == false
                && WandItemRegist.IsHeldByLocalPlayer() && Main.LocalPlayer.mouseInterface == false)
            {
                selecting = true;
                position1 = Main.MouseWorld;
            }

            if (selecting == false) return;

            Update_UseAnimation();

            position2 = Main.MouseWorld;

            if (Main.mouseLeft == false)
            {
                Update_Shapes();
                selecting = false;

                bool wire =
                    gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Red) ||
                    gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Green) ||
                    gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Blue) ||
                    gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Yellow) ||
                    gameMain.Wand_ToolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Actuator);

                bool wireLine = shapes_s == Shapes.rectangle;

                //换出手就丢弃这次选区, 不创建任务(收尾照常走)
                bool canCreate = WandItemRegist.IsHeldByLocalPlayer();

                if (canCreate && (gameMain.Wand_Tile || gameMain.Wand_Wall))
                {
                    if (gameMain.Wand_isPlace)
                    {
                        WandAction.AddTile(shapes, gameMain.Wand_Tile, gameMain.Wand_Wall, gameMain.Wand_BlockType);
                    }
                    else
                    {
                        WandAction.DelTile(shapes, gameMain.Wand_Tile, gameMain.Wand_Wall);
                    }
                }

                if (canCreate && wire)
                {
                    if (wireLine)
                    {
                        Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode toolMode = gameMain.Wand_ToolMode;

                        if (gameMain.Wand_isPlace == false) toolMode |= Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Cutter;

                        WandAction.AddWireLine(position1, position2, toolMode);
                    }
                    else
                    {
                        if (gameMain.Wand_isPlace)
                        {
                            WandAction.AddWire(shapes, gameMain.Wand_ToolMode);
                        }
                        else
                        {
                            WandAction.DelWire(shapes, gameMain.Wand_ToolMode);
                        }
                    }
                }

                shapes = null;
            }
        }

        /// <summary>
        /// 按住左键期间每帧把使用动画顶满, 否则人物只是站着(选区是这里自己轮询鼠标驱动的, 不走原版物品使用流程)。
        /// </summary>
        private static void Update_UseAnimation()
        {
            Terraria.Player player = Main.LocalPlayer;
            Terraria.Item held = player.HeldItem;

            if (held.type != WandItemRegist.ItemId) return;

            player.itemAnimation = held.useAnimation;
            player.itemTime = held.useTime;

            //和原版 ItemCheck_Shoot 同一套算法, 让法杖指向鼠标
            Vector2 to = Main.MouseWorld - player.MountedCenter;

            //原版靠 ItemCheck_Shoot 里的 ChangeDir 翻面, 我们不走那条路, 得自己跟着鼠标转向
            player.ChangeDir(to.X >= 0 ? 1 : -1);

            player.itemRotation = (float)Math.Atan2(to.Y * player.direction, to.X * player.direction) - player.fullRotation;
        }

        private static void Update_Shapes()
        {
            shapes_s = gameMain.Wand_Shapes;

            switch (shapes_s)
            {
                case Shapes.line: shapes = WandUtils.GetShapes_line(position1, position2); break;
                case Shapes.circular: shapes = WandUtils.GetShapes_Circular(position1, position2); break;
                case Shapes.rectangle: shapes = WandUtils.GetShapes_Rectangle(position1, position2); break;
                default: break;
            }
        }

        public static void Draw()
        {
            if (selecting == false) return;

            if (Main.GameUpdateCount % 5 == 0 || shapes == null) Update_Shapes();

            if (shapes?.Count > 0 == false) return;

            //Color borderColor = new Color(135, 0, 180);//紫色
            //Color borderColor = new Color(250, 40, 80);//红色
            //Color borderColor = new Color(40, 250, 80);//绿色
            Color borderColor = gameMain.Wand_isPlace ? new Color(40, 250, 80) : new Color(250, 40, 80);
            Color backgroundColor = borderColor * 0.35f;

            //
            int w = (int)Math.Abs(Math.Floor(position1.X / 16) - Math.Floor(position2.X / 16)) + 1;
            int h = (int)Math.Abs(Math.Floor(position1.Y / 16) - Math.Floor(position2.Y / 16)) + 1;
            Terraria.Utils.DrawBorderString(Main.spriteBatch, $"{w} x {h}", new Vector2(Main.mouseX, Main.mouseY + 50), borderColor, anchorx: 0.5f, anchory: 0.5f);
            //

            switch (shapes_s)
            {
                case Shapes.line: WandUtils.Draw_line(shapes, position1, position2, borderColor, backgroundColor); break;
                case Shapes.circular: WandUtils.Draw_circular(shapes, position1, position2, borderColor, backgroundColor); break;
                case Shapes.rectangle: WandUtils.Draw_rectangle(shapes, position1, position2, borderColor, backgroundColor); break;
                default: break;
            }
        }
    }
}
