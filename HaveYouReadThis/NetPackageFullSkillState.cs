using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace HaveYouReadThis
{
    public class NetPackageFullSkillState : NetPackage
    {

        private string playerStableId;
        private string progressionInfoJson;
        
        public NetPackage Setup(EntityPlayer player, Dictionary<string, ProgressionInfo> myProgressionInfo)
        {
            playerStableId = Utilities.GetStablePlayerId(player);
            progressionInfoJson = JsonConvert.SerializeObject(myProgressionInfo);

            return this;
        }
        
        public override void write(PooledBinaryWriter writer)
        {
            base.write(writer);
            writer.Write(playerStableId);
            writer.Write(progressionInfoJson);
        }

        public override void read(PooledBinaryReader reader)
        {
            playerStableId = reader.ReadString();
            progressionInfoJson = reader.ReadString();
        }

        public override void ProcessPackage(World world, GameManager callbacks)
        {
            var pi = JsonConvert.DeserializeObject<Dictionary<string, ProgressionInfo>>(progressionInfoJson);
            ProgressionInfo.AddForPlayer(playerStableId, pi);
        }

        public override int GetLength() => Encoding.UTF8.GetByteCount(playerStableId) + Encoding.UTF8.GetByteCount(progressionInfoJson);
    }
}