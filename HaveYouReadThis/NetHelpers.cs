using System.Collections.Generic;

namespace HaveYouReadThis
{
    public class NetHelpers
    {
        public static void ClientSendIndividualSkillState(string skillName, int newSkillLevel)
        {
            var pkg = NetPackageManager.GetPackage<NetPackageIndividualSkillState>().Setup(
                Utilities.GetStablePlayerId(GameManager.Instance.myEntityPlayerLocal), skillName, newSkillLevel);
            
            //if we aren't on the server, we need to send it to the server and they will broadcast to everyone.
            if (!ConnectionManager.Instance.IsServer)
            {
                ConnectionManager.Instance.SendToServer(pkg);    
            }
            else //otherwise, we can just directly broadcast to everyone else.
            {
                ConnectionManager.Instance.SendPackage(pkg);
            }
        }

        public static void ServerBroadcastFullSkillState(EntityPlayer sourcePlayer, Dictionary<string, ProgressionInfo> progressionInfos)
        {
            var pkg = NetPackageManager.GetPackage<NetPackageFullSkillState>().Setup(sourcePlayer, progressionInfos);
            ConnectionManager.Instance.SendPackage(pkg);
        }

        public static void ServerSendFullSkillStateToClient(
            EntityPlayer sourcePlayer,
            EntityPlayer targetPlayer,
            Dictionary<string, ProgressionInfo> progressionInfos)
        {
            var pkg = NetPackageManager.GetPackage<NetPackageFullSkillState>().Setup(sourcePlayer, progressionInfos);
            ConnectionManager.Instance.SendPackage(
                _package: pkg,
                _onlyClientsAttachedToAnEntity: true,
                _attachedToEntityId: targetPlayer.entityId
            );
        }
    }
}