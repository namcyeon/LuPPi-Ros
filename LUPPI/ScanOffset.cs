using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LUPPI.NP;

namespace LUPPI
{
    public partial class ScanOffset : Form
    {
        public ScanOffset()
        {
            InitializeComponent();
        }

        SigScanSharp Sigscan;
        private void button1_Click(object sender, EventArgs e)
        {
            Process TargetProcess = Process.GetProcessesByName("ros")[0];
            Sigscan = new SigScanSharp(TargetProcess.Handle);
            Sigscan.SelectModule(TargetProcess.MainModule);
            //long lTime, lTime2;
            /*var offset = Sigscan.FindPattern("? ? ? ? ? ? ? ? 78 6D 6C ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 64 65 66 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 69 6E 74 65 72 66 61 63 65 73", out lTime);
            offset = offset + 0 - (uint)TargetProcess.MainModule.BaseAddress;
            string hexValue2 = offset.ToString("X");
            txt_Offset.Text += "LocalPlayer: 0x" + hexValue2;


            var offset2 = Sigscan.FindPattern("69 43 6C 69 65 6E 74 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 74 70 73", out lTime2);
            offset2 = offset2 + 0 - (uint)TargetProcess.MainModule.BaseAddress - 8;
            string hexValue3 = offset2.ToString("X");
            txt_Offset.Text += "Client: 0x" + hexValue3;*/

            string Local = HexFind("? ? ? ? ? ? ? ? 78 6D 6C ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 64 65 66 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 69 6E 74 65 72 66 61 63 65 73", (uint)TargetProcess.MainModule.BaseAddress, 0);
            txt_Offset.Text += "LocalPlayer: 0x" + Local;


            Local = HexFind("69 43 6C 69 65 6E 74 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 74 70 73", (uint)TargetProcess.MainModule.BaseAddress, 8);
            txt_Offset.Text += "Client: 0x" + Local;

            Local = HexFind("64 79 6E 74 65 78 2E 64 61 74 61 5F 70 72 6F 76 69 64 65 72 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 69 6E 69 74", (uint)TargetProcess.MainModule.BaseAddress, 1192);
            txt_Offset.Text += "ViewMatrix: 0x" + Local;

            Local = HexFind("B9 ? ? ? ? C7 45 ? ? ? ? ? E8 ? ? ? ? 68 ? ? ? ? E8 ? ? ? ? 68 ? ? ? ? E8 ? ? ? ? 83 C4 08 C7 05 ? ? ? ? ? ? ? ? B8 ? ? ? ? 8B 4D F4 64 89 0D ? ? ? ? 59 8B E5 5D C3", (uint)TargetProcess.MainModule.BaseAddress, 0);
            txt_Offset.Text += "PyGame: 0x" + Local;

           /* Local = HexFind("? ? ? ? ? 3F 41 56 4D 65 73 68 43 6F 6D 6D 61 6E 64 40 63 6F 63 6F 73 32 64 40 40", (uint)TargetProcess.MainModule.BaseAddress, 0);
            txt_Offset.Text += "Render: 0x" + Local;*/

           /* Local = HexFind("F0 41 ?  ? 70 42 ? ? ? ? ? ? ? ? ? ? ? ?", (uint)TargetProcess.MainModule.BaseAddress, 0);
            txt_Offset.Text += "Speed: 0x" + Local;*/


        }

        private string HexFind(string text,uint baseadd,uint sub)
        {
            long lTime;
            var offset = Sigscan.FindPattern(text, out lTime);
            offset = offset + 0 - baseadd - sub;
            string hexValue2 = offset.ToString("X");
            return hexValue2 + Environment.NewLine;
        }

        private void ScanOffset_Load(object sender, EventArgs e)
        {

        }
    }
}
