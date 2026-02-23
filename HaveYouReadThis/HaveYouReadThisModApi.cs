using System.Linq;
using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace HaveYouReadThis
{
    public class HaveYouReadThisModApi : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            new Harmony(this.GetType().ToString()).PatchAll(Assembly.GetExecutingAssembly());
        }

        private void GameStartDone(ref ModEvents.SGameStartDoneData data)
        {
            if (ConnectionManager.Instance.IsServer)
            {
                ProgressionInfo.LoadProgressionsFromDisk();
            }
        }

        private static bool _mySpawnDone;

        private void PlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData data)
        {
            _mySpawnDone = true;
            if (!ConnectionManager.Instance.IsServer)
                return;

            ProgressionInfo.LoadProgressionsFromDisk();

            var spawningEntityId = data.EntityId;
            var spawnedPlayer =
                GameManager.Instance.World.Players.list.FirstOrDefault(x => x.entityId == spawningEntityId);
            if (spawnedPlayer == null)
                return;

            var spawnedPlayerProgressionInfo = ProgressionInfo.GetPlayerProgressionInfo(spawnedPlayer);

            //add it to our own server cache
            ProgressionInfo.AddForPlayer(Utilities.GetStablePlayerId(spawnedPlayer), spawnedPlayerProgressionInfo);

            //broadcast this player's info to everyone
            NetHelpers.ServerBroadcastFullSkillState(spawnedPlayer, spawnedPlayerProgressionInfo);

            //broadcast everyone else's info to this player
            foreach (var player in GameManager.Instance.World.Players.list)
            {
                var pi = ProgressionInfo.GetPlayerProgressionInfo(player);
                NetHelpers.ServerSendFullSkillStateToClient(player, spawnedPlayer, pi);
            }
        }

        [HarmonyPatch(typeof(Party), nameof(Party.AddPlayer))]
        public static class Party_AddPlayer_Patch
        {
            static void Postfix(Party __instance, EntityPlayer player)
            {
                ProgressionInfo.RefreshLocalIconInfo();
            }
        }

        [HarmonyPatch(typeof(Party), nameof(Party.RemovePlayer))]
        public static class Party_RemovePlayer_Patch
        {
            static void Postfix(Party __instance, EntityPlayer player)
            {
                ProgressionInfo.RefreshLocalIconInfo();
            }
        }

        [HarmonyPatch(typeof(Party), nameof(Party.UpdateMemberList))]
        public static class Party_UpdateMemberList_Patch
        {
            static void Postfix(Party __instance, World world, int[] partyMembers)
            {
                ProgressionInfo.RefreshLocalIconInfo();
            }
        }

        [HarmonyPatch(typeof(ProgressionValue), "set_Level")]
        public static class ProgressionValue_Level_Patch
        {
            static void Postfix(ProgressionValue __instance)
            {
                if (!_mySpawnDone)
                    return;

                if (!ProgressionValueIsForLocalPlayer(__instance))
                    return;

                if (Utilities.LocalPlayerExists() && ConnectionManager.Instance.IsServer)
                    ProgressionInfo.UpdateForPlayer(
                        Utilities.GetStablePlayerId(GameManager.Instance.myEntityPlayerLocal),
                        __instance.Name.ToLower(), __instance.Level);

                NetHelpers.ClientSendIndividualSkillState(__instance.Name.ToLower(), __instance.Level);
            }

            private static bool ProgressionValueIsForLocalPlayer(ProgressionValue progressionValue)
            {
                if (!Utilities.LocalPlayerExists())
                    return false;

                return GameManager.Instance.myEntityPlayerLocal.Progression.ProgressionValueQuickList.Any(x =>
                    x == progressionValue);
            }
        }

        [HarmonyPatch(typeof(XUiC_ItemInfoWindow), nameof(XUiC_ItemInfoWindow.GetBindingValueInternal))]
        public static class XUiC_ItemInfoWindow_GetBindingValueInternal_Patch
        {
            static void Postfix(XUiC_ItemInfoWindow __instance, ref string value, string bindingName)
            {
                if (bindingName == "itemdescription" && value != null)
                {
                    if (!string.IsNullOrEmpty(__instance?.itemClass?.Unlocks))
                    {
                        value += ProgressionInfo.GetPartyProgressString(__instance?.itemClass?.Unlocks);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveWorld))]
        public static class GameManager_SaveWorld_Patch
        {
            private static void Postfix()
            {
                ProgressionInfo.SaveProgressionsToDisk();
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.PersistentPlayerEvent))]
        public static class GameManager_PersistentPlayerEvent_Patch
        {
            private static void Postfix(GameManager __instance, PlatformUserIdentifierAbs playerID,
                PlatformUserIdentifierAbs otherPlayerID, EnumPersistentPlayerDataReason reason)
            {
                ProgressionInfo.RefreshLocalIconInfo();
            }
        }
    }
}