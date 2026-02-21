namespace HaveYouReadThis
{
    public static class Utilities
    {
        public static string GetStablePlayerId(EntityPlayer player)
        {
            if (player == null)
                return "";

            var world = GameManager.Instance.World;
            var ppd = world.GetGameManager()
                .GetPersistentPlayerList()
                .GetPlayerDataFromEntityID(player.entityId);

            return ppd?.PlatformData.PrimaryId.CombinedString;
        }

        public static EntityPlayer FindPlayerByStableId(string stableId)
        {
            foreach (var p in GameManager.Instance.World.Players.list)
            {
                if (GetStablePlayerId(p) == stableId)
                    return p;
            }

            return null;
        }

        public static bool LocalPlayerExists()
        {
            return GameManager.Instance.myEntityPlayerLocal != null;
        }
    }
}