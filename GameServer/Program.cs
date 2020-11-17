using System;
using System.Threading;

namespace GameServer
{
	public class Program
	{
		private static bool isRunning = false;

		private static void Main(string[] args)
		{
			Console.Title = "GameServer";
			isRunning = true;

			Thread mainThread = new Thread(new ThreadStart(MainThread));
			mainThread.Start();

			Server.StartServer(3, 1986);
		}

		private static void MainThread()
		{
			Console.WriteLine($"Main thread started. Running at {ConstantValues.TICKS_PER_SECOND} ticks per second.");

			var nextLoop = DateTime.Now;
			while (isRunning)
			{
				while (nextLoop < DateTime.Now)
				{
					GameLogic.Update();
					nextLoop = nextLoop.AddMilliseconds(ConstantValues.MS_PER_TICK);
					
					if (nextLoop > DateTime.Now)
						Thread.Sleep(nextLoop - DateTime.Now);
				}
			}
		}
	}
}
