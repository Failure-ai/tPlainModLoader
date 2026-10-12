namespace WandsTool.Content
{
    public partial class gameMain
    {
        public static bool UI_WandsPanel1_isOpen = false;

        public static bool Wand_isEnable = false;
        public static int Wand_UpdateCount = 1;//更新次数
        public static bool Wand_TeleportPlace = false;//联机放置图格/墙壁时先把自己传送到放置点左边3格
        public static bool Wand_isPlace = true;//是放置
        public static bool Wand_Tile = true;
        public static bool Wand_Wall = false;
        public static Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode Wand_ToolMode = 0;
        public static Wands.Shapes Wand_Shapes = Wands.Shapes.rectangle;//形状
        public static WandAction.BlockType Wand_BlockType = WandAction.BlockType.Solid;//方向
        public static int Wand_SelectItemType = 0;//物品选择窗口里选中的物品 type, 0 = 还没选
    }
}
