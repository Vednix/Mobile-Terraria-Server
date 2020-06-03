using System.Diagnostics;
using Terraria.Social;

namespace Terraria.Initializers
{
	public static class LaunchInitializer
	{
		public static void LoadParameters(Main game)
		{
			LoadSharedParameters(game);
			LoadServerParameters(game);
		}

		private static void LoadSharedParameters(Main game)
		{
			string path;
			if ((path = TryParameter("-loadlib")) != null)
			{
				game.loadLib(path);
			}
			string s;
			if ((s = TryParameter("-p", "-port")) != null && int.TryParse(s, out int result))
			{
				Netplay.ListenPort = result;
			}
		}

		private static void LoadClientParameters(Main game)
		{
			string iP;
			if ((iP = TryParameter("-j", "-join")) != null)
			{
				game.AutoJoin(iP);
			}
			string serverPassword;
			if ((serverPassword = TryParameter("-pass", "-password")) != null)
			{
				Netplay.ServerPassword = serverPassword;
				game.AutoPass();
			}
			if (HasParameter("-host"))
			{
				game.AutoHost();
			}
		}

		private static void LoadServerParameters(Main game)
		{
			try
			{
				string s;
				if ((s = TryParameter("-forcepriority")) != null)
				{
					Process currentProcess = Process.GetCurrentProcess();
					if (int.TryParse(s, out int result))
					{
						switch (result)
						{
						case 0:
							currentProcess.PriorityClass = ProcessPriorityClass.RealTime;
							break;
						case 1:
							currentProcess.PriorityClass = ProcessPriorityClass.High;
							break;
						case 2:
							currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
							break;
						case 3:
							currentProcess.PriorityClass = ProcessPriorityClass.Normal;
							break;
						case 4:
							currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
							break;
						case 5:
							currentProcess.PriorityClass = ProcessPriorityClass.Idle;
							break;
						default:
							currentProcess.PriorityClass = ProcessPriorityClass.High;
							break;
						}
					}
					else
					{
						currentProcess.PriorityClass = ProcessPriorityClass.High;
					}
				}
				else
				{
					Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
				}
			}
			catch
			{
			}
			string s2;
			if ((s2 = TryParameter("-maxplayers", "-players")) != null && int.TryParse(s2, out int result2))
			{
				game.SetNetPlayers(result2);
			}
			string serverPassword;
			if ((serverPassword = TryParameter("-pass", "-password")) != null)
			{
				Netplay.ServerPassword = serverPassword;
			}
			string s3;
			if ((s3 = TryParameter("-lang")) != null && int.TryParse(s3, out int result3))
			{
				Lang.lang = result3;
			}
			string worldName;
			if ((worldName = TryParameter("-worldname")) != null)
			{
				game.SetWorldName(worldName);
			}
			string newMOTD;
			if ((newMOTD = TryParameter("-motd")) != null)
			{
				game.NewMOTD(newMOTD);
			}
			string banFilePath;
			if ((banFilePath = TryParameter("-banlist")) != null)
			{
				Netplay.BanFilePath = banFilePath;
			}
			if (HasParameter("-autoshutdown"))
			{
				game.EnableAutoShutdown();
			}
			if (HasParameter("-secure"))
			{
				Netplay.spamCheck = true;
			}
			string worldSize;
			if ((worldSize = TryParameter("-autocreate")) != null)
			{
				game.autoCreate(worldSize);
			}
			if (HasParameter("-noupnp"))
			{
				Netplay.UseUPNP = false;
			}
			string world;
			if ((world = TryParameter("-world")) != null)
			{
				game.SetWorld(world, cloud: false);
			}
			else if (SocialAPI.Mode == SocialMode.Steam && (world = TryParameter("-cloudworld")) != null)
			{
				game.SetWorld(world, cloud: true);
			}
			string configPath;
			if ((configPath = TryParameter("-config")) != null)
			{
				game.LoadDedConfig(configPath);
			}
		}

		private static bool HasParameter(params string[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (Program.LaunchParameters.ContainsKey(keys[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static string TryParameter(params string[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (Program.LaunchParameters.TryGetValue(keys[i], out string value))
				{
					if (value == null)
					{
						return "";
					}
					return value;
				}
			}
			return null;
		}
	}
}
