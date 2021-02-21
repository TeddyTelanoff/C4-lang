using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace C4
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!HasAdministratorPrivileges())
            {
                Console.WriteLine("Please only use this command in cmd4! To open cmd4, open cmd and run cmd4, or press win+r and enter cmd4.");
                return;
            }
            if (File.Exists(@"C:\C4\bin\C4_Status") && File.ReadAllText(@"C:\C4\bin\C4_Status") == "OK")
            {
                if (args.Length <= 0 || (args.Length >= 1 &&
                    args[0].ToLower().StartsWith("help") ||
                    args[0].StartsWith(@"/?") || args[0].StartsWith(@"\?") || args[0].StartsWith(@"?")))
                {
                    Console.WriteLine("Build and run: C4 run filename.C4");
                    Console.WriteLine("Build only: C4 build filename.C4");
                }
                else if (args.Length >= 1)
                {
                }
            }
            else if (!(args.Length >= 1 && args[0] == "detonate"))
            {
                Console.WriteLine("Run C4 detonate to complete the installation.");
            }
            if (args.Length >= 1 && args[0] == "detonate")
            {
                if (File.Exists(@"C:\C4\bin\C4_Status")) File.Delete(@"C:\C4\bin\C4_Status");
                File.WriteAllText(@"C:\C4\bin\C4_Status", "OK");
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("KABOOM!!! C4 is now installed on your machine!");
                Console.ForegroundColor = color;
            }
        }

        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
