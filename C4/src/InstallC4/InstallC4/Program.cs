using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InstallC4
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                if (!HasAdministratorPrivileges())
                {
                    Console.Write("Access denied! Start command processor as administrator? (Y/n)");
                    ConsoleKey key = Console.ReadKey().Key;
                    if (key == ConsoleKey.Y || key == ConsoleKey.Enter)
                    {
                        ProcessStartInfo PSI = new ProcessStartInfo(Process.GetCurrentProcess().Parent().MainModule.FileName);
                        PSI.Verb = "runas";
                        Process.Start(PSI);
                    }
                    return;
                }
                if (args.Length >= 1 && Path.GetFullPath(args[0]) == Path.GetFullPath(@"C:\C4"))
                {
                    try
                    {
                        if (Directory.Exists(@"C:\C4")) new Computer().FileSystem.DeleteDirectory(@"C:\C4", Microsoft.VisualBasic.FileIO.DeleteDirectoryOption.DeleteAllContents);
                        new Computer().FileSystem.CopyDirectory(@"C4", @"C:\C4");
                        try
                        {
                            if (!Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine).Contains(@"C:\C4"))
                            {
                                Environment.SetEnvironmentVariable("PATH",
                                    Environment.GetEnvironmentVariable("PATH",
                                    EnvironmentVariableTarget.Machine) + @";C:\C4\bin;", EnvironmentVariableTarget.Machine);
                            }
                        }
                        catch { }
                        try
                        {
                            if (!Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User).Contains(@"C:\C4"))
                            {
                                Environment.SetEnvironmentVariable("PATH",
                                    Environment.GetEnvironmentVariable("PATH",
                                    EnvironmentVariableTarget.User) + @";C:\C4\bin;", EnvironmentVariableTarget.User);
                            }
                        }
                        catch { }
                    }
                    catch { }
                    if (Directory.Exists(@"C:\C4\bin"))
                    {
                        SetFullControlPermissionsToEveryone(@"C:\C4\bin");
                        SetFullControlPermissionsToEveryone(@"C:\C4\src");
                        SetFullControlPermissionsToEveryone(@"C:\C4");
                        Console.WriteLine(@"Run C4 detonate to complete the installation.");
                    }
                    else
                    {
                        Console.WriteLine(@"The installation failed.");
                    }
                }
                else
                {
                    Console.WriteLine(@"Sorry, but you must install to 'C:\C4'.");
                }
            }
            else
            {
                Console.WriteLine(@"Invalid. Please run InstallC4 C:\C4 in your command processor.");
            }
        }

        static void SetFullControlPermissionsToEveryone(string path)
        {
            const FileSystemRights rights = FileSystemRights.FullControl;

            var allUsers = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);

            // Add Access Rule to the actual directory itself
            var accessRule = new FileSystemAccessRule(
                allUsers,
                rights,
                InheritanceFlags.None,
                PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow);

            var info = new DirectoryInfo(path);
            var security = info.GetAccessControl(AccessControlSections.Access);

            bool result;
            security.ModifyAccessRule(AccessControlModification.Set, accessRule, out result);

            if (!result)
            {
                throw new InvalidOperationException("Failed to give full-control permission to all users for path " + path);
            }

            // add inheritance
            var inheritedAccessRule = new FileSystemAccessRule(
                allUsers,
                rights,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.InheritOnly,
                AccessControlType.Allow);

            bool inheritedResult;
            security.ModifyAccessRule(AccessControlModification.Add, inheritedAccessRule, out inheritedResult);

            if (!inheritedResult)
            {
                throw new InvalidOperationException("Failed to give full-control permission inheritance to all users for " + path);
            }

            info.SetAccessControl(security);
        }

        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static class ProcessExtensions
    {
        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++)
            {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int)processId.NextValue() == pid)
                {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int)parentId.NextValue());
        }

        public static Process Parent(this Process process)
        {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }
    }
}
