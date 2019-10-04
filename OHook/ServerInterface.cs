using System;

namespace OHook
{
    /**
     * \class   ServerInterface
     *
     * \brief   Provides an interface for communicating from the client (target) to the server (injector)
     *
     * \author  Shynd
     * \date    21.07.2018
     */

    public class ServerInterface : MarshalByRefObject
    {
        private string _backgroundImage;
        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set { _backgroundImage = value; }
        }

        public void IsInstalled(int clientPid)
        {
            Console.WriteLine("OHook has been injected into process: {0}.\r\n", clientPid);
        }

        /**
         * \fn  public void ReportMessages(string[] messages)
         *
         * \brief   Output the message to the console.
         *
         * \author  Shynd
         * \date    21.07.2018
         *
         * \param   messages    The messages.
         */
        public void ReportMessages(string[] messages)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        public void ReportMessage(string message)
        {
            var time = DateTime.Now.ToString();
            Console.WriteLine("[*] [" + time + "]: " + message);
        }

        /**
         * \fn  public void ReportException(Exception e)
         *
         * \brief   Reports an exception
         *
         * \author  Shynd
         * \date    21.07.2018
         *
         * \param   e   An Exception to process.
         */
        public void ReportException(Exception e)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + e);
        }

        private int _count = 0;

        /**
         * \fn  public void Ping()
         *
         * \brief   Called to confirm that the IPC channel is still open / host application has not closed.
         *
         * \author  Shynd
         * \date    21.07.2018
         */

        public void Ping()
        {
            var oldTop = Console.CursorTop;
            var oldLeft = Console.CursorLeft;
            Console.CursorVisible = false;

            var chars = "\\|/-";
            Console.SetCursorPosition(Console.WindowWidth - 1, oldTop - 1);
            var loader = chars[_count++ % chars.Length];
            Console.Write(loader);
            Console.Title = "OLoader - Connected " + loader;

            Console.SetCursorPosition(oldLeft, oldTop);
            Console.CursorVisible = true;
        }
    }
}
