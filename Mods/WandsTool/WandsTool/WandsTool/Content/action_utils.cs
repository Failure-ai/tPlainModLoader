using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Items;
using Terraria.ID;
using Terraria.Net.Sockets;

namespace WandsTool.Content
{
    public partial class action
    {
        public static void updateData_placeTile(int x, int y, int style)
        {
            if (WorldGen.InWorld(x, y) == false) return;

            Tile tile = Main.tile[x, y];
            if (tile == null) return;

            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, x, y, tile.type, style);

            if (tile.halfBrick())
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, x, y, 1f, 0, 0, 0);
            else if (tile.slope() > 0)
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, x, y, tile.slope());
        }

        public static void updateData_placeWall(int x, int y)
        {
            if (WorldGen.InWorld(x, y) == false) return;

            Tile tile = Main.tile[x, y];
            if (tile == null) return;

            if (tile.wall > 0)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 3, x, y, tile.wall);
            }
        }
    }
    internal static class ClientSendToServer
    {
        public static void TrySend(byte msgType, Action<BinaryWriter> action)
        {
            try
            {
                if (Main.netMode != 1 || !Netplay.Connection.IsConnected())
                {
                    return;
                }
                MessageBuffer val = NetMessage.buffer[256];
                lock (val)
                {
                    if (val.writer == null)
                    {
                        val.ResetWriter();
                    }
                    BinaryWriter writer = val.writer;
                    writer.BaseStream.Position = 0L;
                    long position = writer.BaseStream.Position;
                    writer.BaseStream.Position += 2L;
                    writer.Write(msgType);
                    action(writer);
                    int num = (int)writer.BaseStream.Position;
                    if (num <= 65535)
                    {
                        writer.BaseStream.Position = position;
                        writer.Write((ushort)num);
                        writer.BaseStream.Position = num;
                        try
                        {
                            ushort num2 = BitConverter.ToUInt16(val.writeBuffer, 0);
                            Netplay.Connection.Socket.AsyncSend(val.writeBuffer, 0, (int)num2, new SocketSendCallback(Netplay.Connection.ClientWriteCallBack), (object)null);
                            return;
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void SendSyncEquipment(int type, int slot, int whoAmi)
        {
            TrySend(5, delegate (BinaryWriter writer)
            {
                writer.Write((byte)whoAmi);
                writer.Write((short)slot);
                Item val = new Item();
                val.SetDefaults(type, (ItemVariant)null);
                val.stack = 1;
                writer.Write((short)val.stack);
                writer.Write(val.prefix);
                writer.Write((short)val.type);
                BitsByte val2 = default(BitsByte);
                val2[0] = val.favorited;
                val2[1] = false;
                writer.Write((byte)val2);
            });
        }
    }
}
