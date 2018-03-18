namespace NP
{
    using System;

    internal class Helper
    {
        public static int GetDistance(Vector3 v1, Vector3 v2, int Divide)
        {
            Vector3 vector = new Vector3 {
                X = v1.X - v2.X,
                Y = v1.Y - v2.Y,
                Z = v1.Z - v2.Z
            };
            return (((int) Math.Sqrt((Math.Pow((double) vector.X, 2.0) + Math.Pow((double) vector.Y, 2.0)) + Math.Pow((double) vector.Z, 2.0))) / Divide);
        }

        public static int GetDistance2(Vector2 v1, Vector2 v2, int Divide)
        {
            Vector2 vector = new Vector2
            {
                X = v1.X - v2.X,
                Y = v1.Y - v2.Y
            };
            return (((int)Math.Sqrt((Math.Pow((double)vector.X, 2.0) + Math.Pow((double)vector.Y, 2.0)))) / Divide);
        }
    }
}

