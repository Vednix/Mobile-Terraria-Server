using Terraria.GameContent.NetModules;
using Terraria.Net;

namespace Terraria.Initializers
{
	internal static class NetworkInitializer
	{
		public static void Load()
		{
			NetManager.Instance.Register<NetLiquidModule>();
		}
	}
}
