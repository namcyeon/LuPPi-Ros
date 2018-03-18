namespace NP
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class Maths
    {

        public static float[] ViewMatrix = new float[0x10];

        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }

        public static bool InsideCircle(int xc, int yc, int r, int x, int y)
        {
            int num = yc - y;
            int num1 = xc - x;
            return (((num1 * num1) + (num * num)) <= (r * r));
        }


        public static bool WorldToScreen(Vector3 pos, out Vector2 screen, int windowWidth, int windowHeight)
        {
            Vector4 vector;
            Vector3 vector2;
            screen = new Vector2();
            var a = Mem.ReadMemory<int>(Mem.BaseAddress + Offsets.ViewMatrix);
            var b = Mem.ReadMemory<int>(a + 4);
            ViewMatrix = Mem.ReadMatrix<float>(b + 0xc4, 0x10);
            vector.X = (((pos.X * ViewMatrix[0]) + (pos.Y * ViewMatrix[4])) + (pos.Z * ViewMatrix[8])) + ViewMatrix[12];
            vector.Y = (((pos.X * ViewMatrix[1]) + (pos.Y * ViewMatrix[5])) + (pos.Z * ViewMatrix[9])) + ViewMatrix[13];
            vector.Z = (((pos.X * ViewMatrix[2]) + (pos.Y * ViewMatrix[6])) + (pos.Z * ViewMatrix[10])) + ViewMatrix[14];
            vector.W = (((pos.X * ViewMatrix[3]) + (pos.Y * ViewMatrix[7])) + (pos.Z * ViewMatrix[11])) + ViewMatrix[15];
            /*string path = @"J:\WriteLines4.txt";
            File.AppendAllText(path, String.Join(" ", ViewMatrix.Select(f => f.ToString(CultureInfo.CurrentCulture))));*/
            if (vector.W < 0.100000001490116)
            {
                return false;
            }
            vector2.X = vector.X / vector.W;
            vector2.Y = vector.Y / vector.W;
            vector2.Z = vector.Z / vector.W;
            //screen.X = ((windowWidth / 2) * vector2.X) + vector2.X + ((windowWidth / 2));
            //screen.Y = -((windowHeight / 2) * vector2.Y) + ((windowHeight / 2));

            screen.X = windowWidth / 2 + vector2.X + vector2.X * (windowWidth / 2);
            screen.Y = vector2.Y + windowHeight / 2 - windowHeight / 2 * vector2.Y;
            if (screen.X <= 0.0f)
                screen.X = 0;
            if (screen.Y <= 0.0f)
                screen.Y = 0;
            return true;
        }

        public static bool WorldToScreen2(Vector3 pos, out Vector2 screen, int windowWidth, int windowHeight,float[] view)
        {
            Vector4 vector;
            Vector3 vector2;
            screen = new Vector2();
            ViewMatrix = view;
            vector.X = (((pos.X * ViewMatrix[0]) + (pos.Y * ViewMatrix[4])) + (pos.Z * ViewMatrix[8])) + ViewMatrix[12];
            vector.Y = (((pos.X * ViewMatrix[1]) + (pos.Y * ViewMatrix[5])) + (pos.Z * ViewMatrix[9])) + ViewMatrix[13];
            vector.Z = (((pos.X * ViewMatrix[2]) + (pos.Y * ViewMatrix[6])) + (pos.Z * ViewMatrix[10])) + ViewMatrix[14];
            vector.W = (((pos.X * ViewMatrix[3]) + (pos.Y * ViewMatrix[7])) + (pos.Z * ViewMatrix[11])) + ViewMatrix[15];
            if (vector.W < 0.100000001490116)
            {
                return false;
            }
            vector2.X = vector.X / vector.W;
            vector2.Y = vector.Y / vector.W;
            vector2.Z = vector.Z / vector.W;
            screen.X = ((windowWidth / 2) * vector2.X) + ((windowWidth / 2));
            screen.Y = -((windowHeight / 2) * vector2.Y) + ((windowHeight / 2));
            return true;
        }

    }
}

