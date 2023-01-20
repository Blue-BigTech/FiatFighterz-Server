using System;
using System.Timers;
using Utils.NET.Modules;
using WebServer;
using World;
using Utils.NET.Logging;
using System.Threading.Tasks;
namespace Run.Local.All
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debugin = true;
            Log.Write(" ______   ______  " + "\n" +
                      "/\\  ___\\ /\\  ___\\ " + "\n" +
                      "\\ \\  __\\ \\ \\  __\\ " + "\n" +
                      " \\ \\_\\    \\ \\_\\   " + "\n" +
                      "  \\/_/     \\/_/   "
                 );






            ModularProgram.Run(new WebServerModule(), new WorldModule());

            

            if (!debugin)
            {
                Console.Beep();
                Console.Clear();
            }
            Log.Write(" ______   ______  " + "\n" +
                      "/\\  ___\\ /\\  ___\\ " + "\n" +
                      "\\ \\  __\\ \\ \\  __\\ " + "\n" +
                      " \\ \\_\\    \\ \\_\\   " + "\n" +
                      "  \\/_/     \\/_/   "
                 );
            Log.Write("Server Running ................. ");
        }

        public void callasyncs()
        {
            Distributor distributor = new Distributor();

            var task = Task.Run(async () => await distributor.SendToken("", 10));

            Task t = distributor.SendToken("", 10);
            //await t;
        }
    }
}
