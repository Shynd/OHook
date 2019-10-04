using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OHook;

namespace OLoader
{
    class Program
    {
        private static ServerInterface _interface;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "OLoader - Idle";

            _interface = new ServerInterface();

            _interface.BackgroundImage = Environment.CurrentDirectory + "/bg.png";

            var targetPID = 0;
            var targetExe = "";

            // Will contain the name of the IPC server channel
            string channelName = null;

            ProcessArgs(args, out targetPID, out targetExe);

            if (targetPID <= 0 && string.IsNullOrEmpty(targetExe))
                return;

            // Create the IPC server using the OHook.ServerInterface class as a singleton
            EasyHook.RemoteHooking.IpcCreateServer<OHook.ServerInterface>(ref channelName, WellKnownObjectMode.SingleCall, _interface);

            // Get the full path to the assembly we want to inject into the target process
            var injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "OHook.dll");

            try
            {
                // Injecting into existing process by Id
                if (targetPID > 0)
                {
                    Console.WriteLine("Attempting to inject into process: {0}", targetPID);

                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // The parameters to pass into the injected library
                                            // ...
                    );
                }
                // Create a new process and then inject into it.
                else if (!string.IsNullOrEmpty(targetExe))
                {
                    // TODO
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
        }

        static void ProcessArgs(string[] args, out int targetPid, out string targetExe)
        {
            targetPid = 0;
            targetExe = "";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("OHook!");
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("    Multi-purpose hook for a rhythm based game!");
            Console.WriteLine("");
            Console.ResetColor();

            while (args.Length != 1)
            {
                if (targetPid > 0)
                {
                    break;
                }

                if (args.Length != 1)
                {
                    Console.WriteLine("Usage: OLoader ProcessID");
                    Console.WriteLine("   or: OLoader ProcessName");
                    Console.WriteLine("");
                    Console.WriteLine("e.g  : OLoader 1234");
                    Console.WriteLine("           to monitor an existing process with PID 1234");
                    Console.WriteLine(@"   or: OLoader ""Notepad.exe""");
                    Console.WriteLine("           monitor an existing process with the name notepad.exe");
                    Console.WriteLine("");
                    Console.WriteLine("Enter a process Id or name");
                    Console.Write("> ");

                    args = new string[] { Console.ReadLine() };

                    if (string.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetExe = args[0];
                    break;
                }

                var p = Process.GetProcessesByName(args[0]);
                targetPid = p[0].Id;
            }
        }
    }
}
