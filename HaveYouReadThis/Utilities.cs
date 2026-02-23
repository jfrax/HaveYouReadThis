using System.IO;
using UniLinq;

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

        public static bool IsAlliesWithLocalPlayer(string playerStableId)
        {
            if (!LocalPlayerExists())
                return false;
            
            return GameManager.Instance.myEntityPlayerLocal?.persistentPlayerData?.ACL?.Any(acl => acl.CombinedString == playerStableId) ?? false;
            
        }

        public static bool LocalPlayerExists()
        {
            return GameManager.Instance.myEntityPlayerLocal != null;
        }
        
        public static string ModSaveDir =>
            Path.Combine(ConnectionManager.Instance.IsServer ? GameIO.GetSaveGameDir() : GameIO.GetSaveGameLocalDir(),
                "Mods", "HaveYouReadThis");
    }
}