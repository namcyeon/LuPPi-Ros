using NP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUPPI.NP
{
    internal class Word
    {
        public int baseAdd;
        public int baseModal;
        public bool isMen;
        public string TypeName
        {
            get { return Mem.ReadString(Mem.ReadMemory<int>(baseAdd + 0x528), 30); }
        }

        public float[] pos
        {
            get { return Mem.ReadMatrix<float>(baseModal + 0x3B0, 16); }
        }

    }
}
