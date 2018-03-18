namespace NP
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4
    {
        public float W;
        public float X;
        public float Y;
        public float Z;
    }
}

