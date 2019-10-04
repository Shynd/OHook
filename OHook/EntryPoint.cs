using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;

namespace OHook
{
    public class EntryPoint : IEntryPoint
    {
        #region -- Local Properties --

        private LocalHook CreateFileHook;
        public static string ChannelName;
        public readonly ServerInterface Interface;
        private Queue<string> _messageQueue = new Queue<string>();

        #endregion -- Local Properties --

        #region -- Threads --



        #endregion -- Threads --

        #region -- Static Properties --



        #endregion -- Static Properties --

        #region -- Function Pointers --

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate IntPtr CreateFile_Delegate(
            string fileName,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttributes,
            IntPtr templateFile
        );

        #endregion -- Function Pointers --

        #region -- Hooks --

        IntPtr CreateFile_Hook(
            string fileName,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttributes,
            IntPtr templateFile)
        {
            try
            {
                lock (_messageQueue)
                {
                    if (_messageQueue.Count < 1000)
                    {
                        var mode = string.Empty;
                        switch (creationDisposition)
                        {
                            case 1:
                                mode = "CREATE_NEW";
                                break;
                            case 2:
                                mode = "CREATE_ALWAYS";
                                break;
                            case 3:
                                mode = "OPEN_ALWAYS";
                                break;
                            case 4:
                                mode = "OPEN_EXISTING";
                                break;
                            case 5:
                                mode = "TRUNCATE_EXISTING";
                                break;
                        }

                        // Add message to send to loader
                        _messageQueue.Enqueue(
                            string.Format("[{0}:{1}]: CREATE ({2}) \"{3}\"",
                            RemoteHooking.GetCurrentProcessId(), RemoteHooking.GetCurrentThreadId(),
                            mode, fileName));
                    }
                }
            }
            catch
            {
                // Swallow exceptions so that any issues caused by this code does not crash the target process.
            }

            // Modify filename IF the file is in the songs folder
            if (fileName.Contains(@"\Songs\") && (fileName.Contains(".png") || fileName.Contains(".jpg") || fileName.Contains(".jpeg")))
            {
                if (!string.IsNullOrEmpty(Interface.BackgroundImage))
                {
                    //_messageQueue.Enqueue("Changed BG for beatmap!");
                    var bg = fileName.Split('\\').Last();
                    Interface.ReportMessage("Changed beatmap background '" + bg + "'");
                    fileName = Interface.BackgroundImage;
                }
            }

            return CreateFileW(
                fileName,
                desiredAccess,
                shareMode,
                securityAttributes,
                creationDisposition,
                flagsAndAttributes,
                templateFile
            );
        }

        #endregion -- Hooks --

        #region -- DLL Methods --

        public EntryPoint(RemoteHooking.IContext context, string channelName)
        {
            // Connect to the server object using provided channel name
            Interface = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);


            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext contect, string channelName)
        {
            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            // Install hooks

            // CreateFile https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
            CreateFileHook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                new CreateFile_Delegate(CreateFile_Hook),
                this
            );


            // Activate hooks on all threads except the current thread.
            CreateFileHook.ThreadACL.SetExclusiveACL(new int[] { 0 });


            Interface.ReportMessage("Hook 'CreateFile' has been installed");

            RemoteHooking.WakeUpProcess();

            try
            {
                // Loop until the loader closes (i.e. IPC fails)
                while (true)
                {
                    Thread.Sleep(500);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                    {
                        Interface.ReportMessages(queued);
                    }
                    else
                    {
                        Interface.Ping();
                    }
                }
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable.
            }

            // Remove hooks
            CreateFileHook.Dispose();


            // Finalize cleanup of hooks
            LocalHook.Release();
        }

        #endregion -- DLL Methods --

        #region CreateFileW

        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFileW(
            String filename,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttributes,
            IntPtr templateFile
        );

        #endregion CreateFileW Hook
    }
}
