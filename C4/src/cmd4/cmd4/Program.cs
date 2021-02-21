using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmd4
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo PSI = new ProcessStartInfo("cmd");
            PSI.Verb = "runas";
            Process.Start(PSI);
        }
    }
}
