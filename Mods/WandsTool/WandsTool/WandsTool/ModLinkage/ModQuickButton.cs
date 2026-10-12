using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using tContentPatch;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using WandsTool.Content;

namespace WandsTool.ModLinkage
{
    public class ModQuickButton : Mod
    {
        public static bool IsLinkage = true;

        /// <summary>加载时:记下模组目录, 魔杖贴图要从那里读。</summary>
        public override void Load(tContentPatch.ModLoad.ModObject mo)
        {
            WandItem.WandItemRegist.ModPath = mo.modPath;
        }

        public override void Loaded()
        {
            if (Main.dedServ) return;

            //物品注册和 QuickButton 联动无关, 必须排在下面的提前 return 之前
            WandItem.WandItemRegist.Load();

            if (IsLinkage == false) return;

            System.Collections.Generic.List<tContentPatch.ModLoad.ModObject> mos = ContentPatch.GetModObjects();
            if (mos == null) return;

            tContentPatch.ModLoad.ModObject mo = mos.FirstOrDefault(i => i.assembly?.GetName().Name == "QuickButton");
            if (mo == null) return;

            Type type = mo.assembly.GetType("QuickButton.QuickButton.QuickButton");
            if (type == null) return;

            MethodInfo mi = type.GetMethod("Add", BindingFlags.Static | BindingFlags.Public);
            if (mi == null) return;

            UIImage ui_img = new UIImage(tContentPatch.Utils.AssemblyResource.Load<Texture2D>(
                $"{nameof(WandsTool)}.Resources.Wand.png",
                Assembly.GetExecutingAssembly()));
            ui_img.Width.Pixels = 32;
            ui_img.Height.Pixels = 32;
            ui_img.ScaleToFit = true;
            ui_img.OnUpdate += _ =>
            {
                if (ui_img.IsMouseHovering) Main.instance.MouseText($"魔杖-{(gameMain.Wand_isEnable ? "启用" : "禁用")}({WandAction.Count})");
            };
            ui_img.OnLeftClick += (e, s) =>
            {
                SoundEngine.PlaySound(12);

                if (gameMain.Wand_isEnable)
                {
                    WandItem.WandItemRegist.RemoveAllFromInventory();
                    gameMain.Wand_isEnable = false;
                }
                else
                {
                    WandItem.WandItemRegist.GiveToInventory();
                    gameMain.Wand_isEnable = true;
                }
            };

            mi.Invoke(null, new object[] { "WandsTool.SwitchOpenOrClose", ui_img });
        }
    }
}
