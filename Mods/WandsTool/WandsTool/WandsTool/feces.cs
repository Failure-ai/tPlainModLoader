using Microsoft.Xna.Framework;
using System.Collections.Generic;
using tContentPatch;
using Terraria;
using Terraria.UI;
using WandsTool.Content;

namespace WandsTool
{
    /// <summary>
    /// 哥们别看了, 这里都是直接移植的陈年老屎
    /// 原本想优化下的但看了下还不如直接重写
    /// </summary>
    public class feces : PatchMain
    {
        private static wandsPanel ui = null;

        //不在静态构造里 new wandsPanel: 静态构造跑在 loader 的 CreateModInstance 阶段, 比 Mod.Loaded 还早,
        //而面板每个按钮都要同步 Main.Assets.Request 贴图, 那个时机资源系统未必就绪。

        public override void SetupDrawInterfaceLayersPostfix(List<GameInterfaceLayer> gameInterfaceLayers)
        {
            int index = gameInterfaceLayers.FindIndex(i => i.Name == "Vanilla: Laser Ruler");
            if (index != -1)
            {
                ++index;
                gameInterfaceLayers.Insert(index, new LegacyGameInterfaceLayer(
                    "StaticTile.WandsTool: Laser Ruler Postfix Game",
                    () =>
                    {
                        if (gameMain.Wand_isEnable)
                        {
                            Wands.Draw();
                        }
                        return true;
                    },
                    InterfaceScaleType.Game));

                gameInterfaceLayers.Insert(index, new LegacyGameInterfaceLayer(
                    "StaticTile.WandsTool: Laser Ruler Postfix UI",
                    () =>
                    {
                        if (ui != null && gameMain.UI_WandsPanel1_isOpen && gameMain.Wand_isEnable)
                        {
                            ui.Draw(Main.spriteBatch, Main.gameTimeCache);
                        }

                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        public override void UpdateUIStatesPostfix(GameTime gameTime)
        {
            if (Main.gameMenu)
            {
                gameMain.Wand_isEnable = false;
                return;
            }

            //进世界之后才建, 这时资源系统一定就绪
            if (ui == null) ui = new wandsPanel();

            if (gameMain.UI_WandsPanel1_isOpen && gameMain.Wand_isEnable)
            {
                ui.Update(gameTime);
                ui.update(gameTime);
            }
        }

        public override void DoUpdateInWorldPostfix()
        {
            if (gameMain.Wand_isEnable)
            {
                //右键面板只在手持魔杖时才允许开; 换了手持物品就把面板关掉
                if (WandItem.WandItemRegist.IsHeldByLocalPlayer() == false)
                {
                    gameMain.UI_WandsPanel1_isOpen = false;
                }
                else if (Main.mouseRight && Main.mouseRightRelease && Main.LocalPlayer.mouseInterface == false)
                {
                    gameMain.UI_WandsPanel1_isOpen = !gameMain.UI_WandsPanel1_isOpen;

                    //ui 要到本帧后面的 UpdateUIStatesPostfix 才建, 这里可能还是 null
                    if (ui != null)
                    {
                        if (gameMain.UI_WandsPanel1_isOpen) ui.isReset = true;
                        else ui.ItemPanel.Close();
                    }
                }

                Wands.Update();

                WandAction.Update();

            }
            else
            {
                gameMain.UI_WandsPanel1_isOpen = false;

                Wands.Reset();
            }
        }
    }
}
