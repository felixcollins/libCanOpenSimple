using System;
using libCanOpenSimple;
namespace libCanOpenSimpleCLI
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0) 
			{
				Console.WriteLine("Please supply one argument being the name of the CAN port to connect to");
				return;
			}

			Console.WriteLine($"CanOpenSimple - connecting to {args[0]}");

			var canopen = new CanOpenSimpleMaster();

			canopen.open(args[0], BUSSPEED.BUS_500Kbit, "SocketCan");

			canopen.emcyevent += (packet, time) =>
			{
				Console.WriteLine ($"Emergency event received : {packet}");
			};

			canopen.nmtevent += (packet, time) => 
			{
				Console.WriteLine($"nmt event received  : {packet}");
			};

			canopen.nmtecevent += (packet, time) =>
			{
				Console.WriteLine($"nmtec event received  : {packet}");
			};

			canopen.lssevent += (packet, time) =>
			{
				Console.WriteLine($"ls event received  : {packet}");
			};

			canopen.pdoevent += (packet, time) =>
			{
				Console.WriteLine($"pdo event received  : {packet}");
			};

			canopen.packetevent += (packet, time) =>
			{
				Console.WriteLine($"packet event received  : {packet}");
			};

			canopen.sdoevent += (packet, time) =>
			{
				Console.WriteLine($"sdo event received  : {packet}");
			};

			canopen.syncevent += (packet, time) =>
			{
				Console.WriteLine($"sync event received  : {packet}");
			};

			canopen.connectionevent += (packet, time) =>
			{
				Console.WriteLine($"connection event received  : {packet}");
			};


			Console.WriteLine("Started - hit key  to continue");
			Console.ReadKey();

			canopen.NMT_ResetNode(1);

			Console.WriteLine("Reset node 1 - hit key  to continue");
			Console.ReadKey();

			canopen.NMT_preop(1);

			StartDrive(canopen);

			canopen.NMT_start(1);
			while (true) 
			{
				var k = Console.ReadKey();

				switch(k.Key)
				{
					case ConsoleKey.Q:
						canopen.close();
						return;
					case ConsoleKey.F:
						canopen.SDOwrite(1, 0x60ff, 0x00, 2000, null);
						break;
					case ConsoleKey.S:
						canopen.SDOwrite(1, 0x60ff, 0x00, 300, null);
						break;
				}
			}
		}

		// Start the drive by transitioning the control state
		public static void StartDrive(libCanOpenSimple.CanOpenSimpleMaster can)
		{
			can.SDOwrite(1, 0x6060, 0, 3, null); //write velocity mode

			// Send control word to switch the drive to "Operational Enabled"
			WriteControlWord(can, 0x06); // Shutdown
			WriteControlWord(can, 0x07); // Switch On
			WriteControlWord(can, 0x0F); // Enable Operation
		}

		private static void WriteControlWord(libCanOpenSimple.CanOpenSimpleMaster can, ushort controlWord)
		{
			byte[] controlWordBytes = BitConverter.GetBytes(controlWord);
			var sdo = can.SDOwrite(1, 0x6040, 0x00, controlWordBytes, null);
		}
	}
}
