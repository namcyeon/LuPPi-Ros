using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NP;
using Yato;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using SharpDX.DirectWrite;

namespace LUPPI
{
    public partial class Luppi : Form
    {
        private static RECT rect;
        private static IntPtr Handle = new IntPtr(0);
        private static int HandleID = 0;
        private static int LocalPlayer = 0;
        private static Vector3 MyPosition = new Vector3(0,0,0);
        private static bool tele = false;
        public static float X, Y, Z = 0;
        private static Thread np;
        private static int enemyCount = 0;
        private bool isGrass = false;
        private bool isWall = false;

        public static string WINDOW_NAME = "null";

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int nVirtKey);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public static IntPtr WinGetHandle(string wName)
        {
            IntPtr zero = IntPtr.Zero;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.MainWindowTitle.Contains(wName))
                {
                    zero = process.MainWindowHandle;
                    HandleID = process.Id;
                }
            }
            return zero;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public Luppi()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            np.Abort();
            this.Close();
        }


        int subHeight = 0;
        int subWidth = 0;
        private void Luppi_Load(object sender, EventArgs e)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && (process.ProcessName == "ros"))
                {
                    WINDOW_NAME = process.MainWindowTitle;
                }
            }
            Mem.Initialize("ros");
            if (Mem.m_pProcessHandle == IntPtr.Zero)
            {
                Console.WriteLine("Game Not Found");
            }
            else
            {
                Handle = WinGetHandle(WINDOW_NAME);
                GetWindowRect(FindWindow(null, WINDOW_NAME), out rect);
                rect.left = 0;
                rect.right = 1366;
                rect.top = 21;
                rect.bottom = 726;
                SetWindowLong(Handle, -4, GetWindowLong(Handle, -0) | 524288 | 32);
                np = new Thread(new ThreadStart(Nampham));
                np.Start();
            }

        }

        private List<Entity> ReadAllEntity()
        {
            var EntityList = new List<Entity>();
            var GameObject = Mem.ReadMemory<int>((Mem.BaseAddress + Offsets.Client)) +Offsets.m_ppObjects;
            GameObject = Mem.ReadMemory<int>(GameObject + 0x0);
            var pCurrentItem = Mem.ReadMemory<int>(Mem.ReadMemory<int>(GameObject + 0x0));
            pCurrentItem = Mem.ReadMemory<int>(pCurrentItem + 0x0);
            pCurrentItem = Mem.ReadMemory<int>(pCurrentItem + 0x0);
            var End = Mem.ReadMemory<int>(GameObject + 0x4);
            var i = 0;
            enemyCount = 0;
            do
            {
                var encryptedEntity = Mem.ReadMemory<int>(pCurrentItem + 0xC);
                var entityDecryptKey =
                    Mem.ReadMemory<int>(Mem.ReadMemory<int>(pCurrentItem + 0x10) + 0x0);
                var Entity = encryptedEntity ^ entityDecryptKey;
                //var Entity = Mem.ReadMemory<int>(pCurrentItem + 0xC);
                var m_pMeta = Mem.ReadMemory<int>(Entity + 0x4);
                //var SpaceID = Mem.ReadMemory<int>(Entity + 0x48 );
                //var YAW = Mem.ReadMemory<int>(Entity + 0x38);
                //var PITCH = Mem.ReadMemory<int>(Entity + 0x3C);
                //var currStageName = Mem.ReadMemory<int>(Entity + 0x11BC);
                //var name = Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Entity + 0x100) + 0x14) + 0x3EC);
                //var naemsize = Mem.ReadMemory<int>(name + 0x8);
                var playerName = "";
                /*if (naemsize > 50 || naemsize < 0)
                    playerName = "";
                else
                    playerName = Mem.ReadString(name + 0x14, naemsize);*/
                var EntityType = Mem.ReadString(Mem.ReadMemory<int>(m_pMeta + 0xC), 12);
                var ObjectId = Mem.ReadMemory<int>(Entity + 0xC);
                var hp = Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Entity + 0x120) + 0x14) + 0x98) + 8);
                byte b = Mem.ReadMemory<byte>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Entity + 0x120) + 0x14) + 376 + 4) + 20);
                Pose pose1 = Pose.Standing;
                byte b2 = b;
                if (b2 != 0)
                {
                    if (b2 != 1)
                    {
                        if (b2 == 255)
                        {
                            pose1 = Pose.Standing;
                        }
                    }
                    else
                    {
                        pose1 = Pose.Crouching;
                    }
                }
                else
                {
                    pose1 = Pose.Prone;
                }
                var isPlayer = false;
                if (EntityType.Contains("Avatar"))
                {
                    isPlayer = true;
                    enemyCount += 1;
                }
                var isItem = EntityType.Contains("DtsProp");
                var dropID = 0;
                if (isItem)
                {
                    dropID = Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(Entity + 0x120) + 0x14) + 0x2C) + 8);
                }
                var isItemDie = EntityType.Contains("DtsPlayer");
                EntityList.Add(new Entity() { pEntity = Entity,entityaddress = Entity,dropID = dropID, isItem = isItem, isItemDie = isItemDie, TypeName = EntityType, ObjectId = ObjectId, isPlayer = isPlayer, hp = hp,pose = pose1 });
                pCurrentItem = Mem.ReadMemory<int>(pCurrentItem + 0x0);
                i++;
            } while (pCurrentItem != End);
            return EntityList;
        }

        private static bool autoaim = true;
        private static List<Entity> target = new List<Entity>();
        private static int heightAim = 0;
        private static bool isBoxEsp = false;
        private static bool isBoxNew = true;
        private static bool aimLeg = false;
        private static bool Showbone = true;
        public Vector3 GetEncryptedPosition(int pEntity)
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

        private void Nampham()
        {
            rect.left = 0;//
            rect.right = 1366;//
            rect.top = 21;//
            rect.bottom = 726;//
            int Width = rect.right - rect.left;
            int Height = rect.bottom - rect.top;
            int Width2 = 1382;//
            int Height2 = 744;//
            var overlay = new OverlayWindow(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            var rendererOptions = new Direct2DRendererOptions()
            {
                AntiAliasing = false,
                Hwnd = overlay.WindowHandle,
                MeasureFps = true,
                VSync = true
            };

            var d2d = new Direct2DRenderer(rendererOptions);
            var trains = d2d.CreateBrush(0, 0, 0, 0);
            var blackBrush = d2d.CreateBrush(0, 0, 0, 255);
            var redBrush = d2d.CreateBrush(242, 14, 14, 255);
            var greenBrush = d2d.CreateBrush(33, 208, 43, 255);
            var whiteBrush = d2d.CreateBrush(255, 255, 255, 200);
            var blueBrush = d2d.CreateBrush(0, 0, 255, 255);
            var grenBrush = d2d.CreateBrush(33, 208, 43, 180);
            var greenBrush2 = d2d.CreateBrush(0, 188, 0, 255);
            var font = d2d.CreateFont("Tahoma", 8,true);
            var bigfont = d2d.CreateFont("Tahoma", 14,true);
            Vector2 center = new Vector2();
            while (true)
            {
                center.X = Width / 2;
                center.Y = (Height / 2) + 20;
                d2d.BeginScene();
                d2d.ClearScene(trains);
                if(Showbone)
                {
                    var m_pWorld = Mem.ReadMemory<int>(Mem.BaseAddress + Offsets.PyGame + 0x410);
                    List<LUPPI.NP.Word> modal = new List<NP.Word>();
                    var m_pSceneContext = Mem.ReadMemory<int>(m_pWorld + 0x8);
                    var cameraBase = Mem.ReadMemory<int>(m_pSceneContext + 0x4);
                    var viewMatrix = Mem.ReadMatrix<float>(cameraBase + 0xC4, 16);
                    var pSkeletonList = Mem.ReadMemory<int>(m_pWorld + 0x290);
                    int visibleCount = Mem.ReadMemory<int>(m_pWorld + 0x278);
                    int coutene = 0;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        int r_pModel = Mem.ReadMemory<int>(pSkeletonList + i);
                        int m_pAnimator = Mem.ReadMemory<int>(r_pModel + 0x328);
                        if (m_pAnimator > 1)
                        {
                            var intt = Mem.ReadMemory<int>(m_pAnimator + 0x528);
                            //var bon = Mem.ReadMemory<int>(m_pAnimator + 0x970);
                            var name = Mem.ReadString(intt, 35);
                            //float[] b = Mem.ReadMatrix<float>(r_pModel + 0x3B0, 16);
                            if (name.Contains("_male"))
                            {
                                coutene += 1;
                                modal.Add(new NP.Word() { baseAdd = m_pAnimator, baseModal = r_pModel, isMen = true });
                            }
                            else if (name.Contains("_female"))
                            {
                                coutene += 1;
                                modal.Add(new NP.Word() { baseAdd = m_pAnimator, baseModal = r_pModel, isMen = false });
                            }

                        }
                    }
                    d2d.DrawTextWithBackground("ENERMY : " + (coutene - 1) + " 💩", 10, 400, bigfont, redBrush, whiteBrush);
                    for (int i = 0; i < modal.Count; i++)
                    {
                        if (i == 0)//LOCALPLAYER POS
                        {
                            var m_Position1 = modal[i].pos;
                            MyPosition.X = m_Position1[12];
                            MyPosition.Y = m_Position1[13];
                            MyPosition.Z = m_Position1[14];
                        }
                        //string name = modal[i].TypeName;
                        //if (name.Contains("dataosha_male") || name.Contains("dataosha_female"))
                        {
                            var m_Position = modal[i].pos;
                            Vector3 position;
                            position.X = m_Position[12];
                            position.Y = m_Position[13];
                            position.Z = m_Position[14];
                            var p = 0;
                            for (int j = 0; j < 0xE80; j += 0x40)
                            {
                                var ab = Mem.ReadMemory<int>(modal[i].baseAdd + 0x970);
                                var boneMatrix = Mem.ReadMatrix<float>(ab + j, 16);
                                var bone4 = new LUPPI.NP.Matrix(boneMatrix);
                                var bone24 = new LUPPI.NP.Matrix(m_Position);
                                var result = LUPPI.NP.Matrix.Multiply(bone4, bone24);
                                var vec3a = new Vector3(result.M41, result.M42, result.M43);
                                Maths.WorldToScreen3(vec3a, viewMatrix, out var testeee, Width, Height);
                                d2d.DrawText(p.ToString(), testeee.X, testeee.Y, font, whiteBrush);
                                p++;
                            }
                            Maths.WorldToScreen(position, out var testee2, Width, Height);
                            int khoangCach = Helper.GetDistance(MyPosition, position, 20);
                            string tea = "[" + khoangCach + "m]";
                            if (khoangCach < 150)
                                d2d.DrawText(tea, testee2.X - tea.Length, testee2.Y, font, greenBrush2);
                            else
                                d2d.DrawText(tea, testee2.X - tea.Length, testee2.Y, font, whiteBrush);
                        }
                    }
                }
                if (isBoxEsp)
                {
                    Vector2 vector3;
                    LocalPlayer = Mem.ReadMemory<int>(Mem.BaseAddress + Offsets.LocalPlayer);
                    Mem.ReadMemory<int>(Mem.BaseAddress + 0x22);
                    MyPosition = GetEncryptedPosition(LocalPlayer);
                    List<Entity> ls = ReadAllEntity();
                    d2d.DrawTextWithBackground("ENERMY : " + enemyCount.ToString() + " 💩", 10, 400, bigfont, redBrush, whiteBrush);
                    d2d.DrawTextWithBackground("AIM LEG: " + aimLeg.ToString(), 10, 370, bigfont, redBrush, whiteBrush);
                    for (int i = 0; i < ls.Count; i++)
                    {
                        //ls[i].Coordinates.Y += 15f;
                        ls[i].Coordinates = ls[i].GetEncryptedPosition();
                        if (Maths.WorldToScreen(ls[i].Coordinates, out vector3, Width2, Height2))
                        {
                            int khoangCach = Helper.GetDistance(MyPosition, ls[i].Coordinates, 10);
                            var widthhp = 0f;
                            var widthhp2 = 0f;
                            float numaim = 2f;
                            if (ls[i].isPlayer && ls[i].hp > 0)
                            {
                                float heiadd = 0f;
                                bool flag3 = ls[i].pose == Pose.Standing;
                                if (flag3)
                                {
                                    heiadd += 18.5f;
                                }
                                bool flag4 = ls[i].pose == Pose.Prone;
                                if (flag4)
                                {
                                    heiadd += 12.5f;
                                    numaim = 1.6f;
                                }
                                bool flag5 = ls[i].pose == Pose.Crouching;
                                if (flag5)
                                {
                                    heiadd += 4f;
                                    numaim = 1.1f;
                                }
                                Vector2 line1, line2, line3, line4, line5, line6, line7, line8;
                                if (isBoxEsp)
                                {
                                    var a1 = ls[i].Coordinates.X;
                                    var a2 = ls[i].Coordinates.Y;
                                    var a3 = ls[i].Coordinates.Z;
                                    var v7 = a1 - 5.5f;
                                    var v8 = a2 - 2.5f;
                                    var v9 = a3 - 5.5f;
                                    var v10 = a1 + 5.5f;
                                    var v12 = a3 + 5.5f;
                                    if (Maths.WorldToScreen(new Vector3(v7, v8, v9), out line1, Width, Height))
                                    {
                                        var v4 = a2 + heiadd;
                                        var v11 = a2 + heiadd;
                                        var v13 = v4;
                                        var v14 = v4;
                                        if (Maths.WorldToScreen(new Vector3(v10, v4, v12), out line2, Width, Height))
                                        {
                                            if (Maths.WorldToScreen(new Vector3(v10, v8, v9), out line3, Width, Height))
                                            {
                                                if (Maths.WorldToScreen(new Vector3(v7, v11, v9), out line4, Width, Height))
                                                {
                                                    if (Maths.WorldToScreen(new Vector3(v7, v8, v12), out line5, Width, Height))
                                                    {
                                                        if (Maths.WorldToScreen(new Vector3(v7, v13, v12), out line6, Width, Height))
                                                        {
                                                            if (Maths.WorldToScreen(new Vector3(v10, v8, v12), out line7, Width, Height))
                                                            {
                                                                if (Maths.WorldToScreen(new Vector3(v10, v14, v9), out line8, Width, Height))
                                                                {
                                                                    d2d.DrawLine(line1.X, line1.Y, line4.X, line4.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line3.X, line3.Y, line8.X, line8.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line7.X, line7.Y, line2.X, line2.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line5.X, line5.Y, line6.X, line6.Y, 1, whiteBrush);

                                                                    //Chan
                                                                    d2d.DrawLine(line1.X, line1.Y, line3.X, line3.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line3.X, line3.Y, line7.X, line7.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line7.X, line7.Y, line5.X, line5.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line5.X, line5.Y, line1.X, line1.Y, 1, whiteBrush);

                                                                    //Dau
                                                                    d2d.DrawLine(line4.X, line4.Y, line8.X, line8.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line8.X, line8.Y, line2.X, line2.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line2.X, line2.Y, line6.X, line6.Y, 1, whiteBrush);
                                                                    d2d.DrawLine(line6.X, line6.Y, line4.X, line4.Y, 1, whiteBrush);

                                                                    widthhp = (float)Helper.GetDistance2(line4, line2, 1);
                                                                    widthhp2 = (float)Helper.GetDistance2(line6, line8, 1);
                                                                    if (widthhp < widthhp2)
                                                                        widthhp = widthhp2;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                var dy = ls[i].Coordinates.X;
                                var dy_4 = ls[i].Coordinates.Y;
                                var v46 = ls[i].Coordinates.Z;
                                var v27 = dy_4 + 27.0f;
                                Vector2 aimpoint, aimpoint2;
                                if (Maths.WorldToScreen(new Vector3(dy, v27, v46), out aimpoint, Width, Height))
                                {
                                    string tea = ls[i].PlayerName + " [" + khoangCach + " m]";
                                    if (khoangCach < 150)
                                    {
                                        d2d.DrawText(tea, aimpoint.X - tea.Length * 2, aimpoint.Y - 10, font, redBrush);
                                    }
                                    else
                                    {
                                        d2d.DrawText(tea, aimpoint.X - tea.Length * 2, aimpoint.Y - 10, font, whiteBrush);
                                    }

                                    //Player HP
                                    if (ls[i].hp == 100)
                                        d2d.DrawVerticalBar(ls[i].hp, aimpoint.X - widthhp / 2, aimpoint.Y - 15f, widthhp, 1, 3, greenBrush2, blackBrush);
                                    else
                                        d2d.DrawVerticalBar(ls[i].hp, aimpoint.X - widthhp / 2, aimpoint.Y - 15f, widthhp, 1, 3, redBrush, blackBrush);
                                }

                                if (Maths.WorldToScreen(new Vector3(dy, v27, v46), out aimpoint, Width, Height2))
                                {
                                    var v41 = dy_4 + heiadd;
                                    if (Maths.WorldToScreen(new Vector3(dy, v41, v46), out aimpoint2, Width, Height2))
                                        if ((Maths.InsideCircle((int)center.X, (int)center.Y, 80, (int)aimpoint2.X, (int)aimpoint2.Y)))
                                        {
                                            if (Keyboard.IsKeyDown(Keys.LShiftKey))
                                            {
                                                Cursor.Position = new Point((int)(aimpoint2.X), (int)(aimpoint2.Y));
                                                if (Keyboard.IsKeyDown(Keys.LButton))
                                                {
                                                    Cursor.Position = new Point((int)(aimpoint2.X), (int)(aimpoint2.Y + ls[i].Pitch));
                                                }
                                            }
                                        }
                                }
                            }
                            if (ls[i].isItem)
                            {
                                if (ls[i].dropID == 1001 || ls[i].dropID == 1002 || ls[i].dropID == 1007 || ls[i].dropID == 1026)
                                {
                                    d2d.DrawText2("[GUN]", vector3.X, vector3.Y, font, whiteBrush, greenBrush2);
                                }
                                else if (ls[i].dropID == 1273 || ls[i].dropID == 1274 || ls[i].dropID == 1275)
                                {
                                    d2d.DrawText2("[SCOPE]", vector3.X, vector3.Y, font, whiteBrush, greenBrush2);
                                }
                                else if (khoangCach < 100)
                                {
                                    //d2d.DrawText("[I]", vector3.X, vector3.Y, font, whiteBrush);
                                }
                            }
                            if (ls[i].isItemDie && khoangCach < 100)
                            {
                                d2d.DrawText2("[DIE]", vector3.X, vector3.Y, font, whiteBrush, greenBrush2);
                            }
                        }
                    }
                }
                
                d2d.EndScene();
                //Thread.Sleep(1);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isBoxEsp = !isBoxEsp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private static Thread newThre;
        private static Thread newThre2;
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            isWall = !isWall;
            ChangeValue();
        }

        private void ChangeValue()
        {
            if (isWall)
                Mem.WriteMemory<float>(Mem.BaseAddress + 0x15DF918, -0.8f);
            else
                Mem.WriteMemory<float>(Mem.BaseAddress + 0x15DF918, -0.5f);
        }
        
        private void LuPhACK()
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            isGrass = !isGrass;
            if (isGrass)
                Mem.WriteMemory<float>(Mem.ReadMemory<int>(
                    Mem.ReadMemory<int>(
                        Mem.ReadMemory<int>(
                            Mem.ReadMemory<int>(
                                Mem.ReadMemory<int>(Mem.BaseAddress + 0x1C2519C)+0x0)+0x100)+0x10)+0x210)+0x2C, 0);
            else
                Mem.WriteMemory<float>(Mem.ReadMemory<int>(
                    Mem.ReadMemory<int>(
                        Mem.ReadMemory<int>(
                            Mem.ReadMemory<int>(
                                Mem.ReadMemory<int>(
                                    Mem.BaseAddress + 0x1C2519C) + 0x0) + 0x100) + 0x10) + 0x210) + 0x2C, 100000);

        }

        private void ChangeValue2()
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetWindowRect(FindWindow((string)null, WINDOW_NAME), out rect);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            autoaim = !autoaim;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            np.Abort();
            np = new Thread(new ThreadStart(Nampham));
            np.Start();
        }
    }


    public abstract class Keyboard
    {
        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }

        public static bool IsKeyToggled(Keys key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }
    }
    public class AOBScan
    {
        protected uint ProcessID;
        public AOBScan(uint ProcessID)
        {
            this.ProcessID = ProcessID;
        }

        [DllImport("kernel32.dll")]
        protected static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [StructLayout(LayoutKind.Sequential)]
        protected struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        protected List<MEMORY_BASIC_INFORMATION> MemoryRegion { get; set; }

        protected void MemInfo(IntPtr pHandle)
        {
            IntPtr Addy = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION MemInfo = new MEMORY_BASIC_INFORMATION();
                int MemDump = VirtualQueryEx(pHandle, Addy, out MemInfo, Marshal.SizeOf(MemInfo));
                if (MemDump == 0) break;
                if ((MemInfo.State & 0x1000) != 0 && (MemInfo.Protect & 0x100) == 0)
                    MemoryRegion.Add(MemInfo);
                Addy = new IntPtr(MemInfo.BaseAddress.ToInt32() + (int)MemInfo.RegionSize);
            }
        }
        protected IntPtr Scan(byte[] sIn, byte[] sFor)
        {
            int[] sBytes = new int[256]; int Pool = 0;
            int End = sFor.Length - 1;
            for (int i = 0; i < 256; i++)
                sBytes[i] = sFor.Length;
            for (int i = 0; i < End; i++)
                sBytes[sFor[i]] = End - i;
            while (Pool <= sIn.Length - sFor.Length)
            {
                for (int i = End; sIn[Pool + i] == sFor[i]; i--)
                    if (i == 0) return new IntPtr(Pool);
                Pool += sBytes[sIn[Pool + End]];
            }
            return IntPtr.Zero;
        }
        public IntPtr AobScan(byte[] Pattern)
        {
            Process Game = Process.GetProcessById((int)this.ProcessID);
            if (Game.Id == 0) return IntPtr.Zero;
            MemoryRegion = new List<MEMORY_BASIC_INFORMATION>();
            MemInfo(Game.Handle);
            for (int i = 0; i < MemoryRegion.Count; i++)
            {
                byte[] buff = new byte[MemoryRegion[i].RegionSize];
                ReadProcessMemory(Game.Handle, MemoryRegion[i].BaseAddress, buff, MemoryRegion[i].RegionSize, 0);

                IntPtr Result = Scan(buff, Pattern);
                if (Result != IntPtr.Zero)
                    return new IntPtr(MemoryRegion[i].BaseAddress.ToInt32() + Result.ToInt32());
            }
            return IntPtr.Zero;
        }
    }
}
