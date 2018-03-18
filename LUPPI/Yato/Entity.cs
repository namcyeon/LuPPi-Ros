using NP;
using System;

namespace Yato
{
    internal class Entity
    {
        public int pEntity;
        public string PlayerName;
        public string TypeName;
        public int Type;
        public int ObjectId;
        public bool isPlayer;
        public Vector3 GetEncryptedPosition()
        {
            Vector3 Position;
            var xPos = Mem.ReadMemory<float>(pEntity + 0x10);
            var yPos = Mem.ReadMemory<float>(pEntity + 0x18);
            var zPos = Mem.ReadMemory<float>(pEntity + 0x20);

            var keyXAddr = Mem.ReadMemory<int>(pEntity + 0x14);
            var keyYAddr2 = Mem.ReadMemory<int>(pEntity + 0x1C);
            var keyZAddr3 = Mem.ReadMemory<int>(pEntity + 0x24);

            var a1 = Mem.ReadMemory<UInt32>(keyXAddr);
            var a2 = Mem.ReadMemory<UInt32>(keyYAddr2);
            var a3 = Mem.ReadMemory<UInt32>(keyZAddr3);



            uint xEnc = BitConverter.ToUInt32(BitConverter.GetBytes(xPos), 0); //Converting to Hex
            uint yEnc = BitConverter.ToUInt32(BitConverter.GetBytes(yPos), 0);
            uint zEnc = BitConverter.ToUInt32(BitConverter.GetBytes(zPos), 0);

            uint xKey = BitConverter.ToUInt32(BitConverter.GetBytes(a1), 0); //Converting to Hex
            uint yKey = BitConverter.ToUInt32(BitConverter.GetBytes(a2), 0);
            uint zKey = BitConverter.ToUInt32(BitConverter.GetBytes(a3), 0);

            uint xDec = xEnc ^ xKey;
            uint yDec = yEnc ^ yKey;
            uint zDec = zEnc ^ zKey;

            var xDecrypted = BitConverter.ToSingle(BitConverter.GetBytes(xDec), 0);
            var yDecrypted = BitConverter.ToSingle(BitConverter.GetBytes(yDec), 0);
            var zDecrypted = +BitConverter.ToSingle(BitConverter.GetBytes(zDec), 0);
            Position = new Vector3(xDecrypted, yDecrypted, zDecrypted);

            return Position;
        }
        public Vector3 Coordinates;
        public float Yaw;
        public float Pitch;
        public bool isVehicle;
        public int playerAdress;
        public float[] boneMatrix;
        public float[] m_pSkeleton;
        public int hp;
        public bool isRobot;
        public bool isItem;
        public int SpaceID;
        public Vector3 test2;
        public Vector3 test3;
        public float test4;
        public int test5;
        public int bonelist;
        public float[] bonelist2;
        public float[] bonelist3;
        public string itemName;
        public bool isItemDie;
        public int dropID;
        public int entityaddress;
        public int op1;
        public int op2;
        public Pose pose;
    }

    public enum Pose
    {
        // Token: 0x04000020 RID: 32
        Standing,
        // Token: 0x04000021 RID: 33
        Crouching,
        // Token: 0x04000022 RID: 34
        Prone
    }
}