using System.Text;

namespace HaveYouReadThis
{
    public class NetPackageIndividualSkillState : NetPackage
    {
        private string playerStableId;
        private string skillName;
        private int skillNewLevel;
        
        public NetPackage Setup(string stableId, string skillNameParam, int skillNewLevelParam)
        {
            playerStableId = stableId;
            skillName = skillNameParam;
            skillNewLevel = skillNewLevelParam;

            return this;
        }
        
        public override void write(PooledBinaryWriter writer)
        {
            base.write(writer);
            writer.Write(playerStableId);
            writer.Write(skillName);
            writer.Write(skillNewLevel);
        }

        public override void read(PooledBinaryReader reader)
        {
            playerStableId = reader.ReadString();
            skillName = reader.ReadString();
            skillNewLevel = reader.ReadInt32();
        }

        public override void ProcessPackage(World world, GameManager callbacks)
        {
            if (ConnectionManager.Instance.IsServer)
            {
                //we are the server - need to queue a package to all other players with this same info
                var pkg = NetPackageManager.GetPackage<NetPackageIndividualSkillState>().Setup(playerStableId, skillName, skillNewLevel);

                if (Utilities.LocalPlayerExists())
                {
                    //need to ensure we don't send this to the server player; we will update that right here
                    ConnectionManager.Instance.SendPackage(
                        pkg,
                        false,
                        -1,
                        GameManager.Instance.myEntityPlayerLocal.entityId
                    );
                }
                else
                {
                    //no server player; send to everyone
                    ConnectionManager.Instance.SendPackage(pkg);
                }
            }
            
            ProgressionInfo.UpdateForPlayer(playerStableId, skillName, skillNewLevel);
        }

        public override int GetLength() => Encoding.UTF8.GetByteCount(playerStableId) + Encoding.UTF8.GetByteCount(skillName) + 4;
    }
}