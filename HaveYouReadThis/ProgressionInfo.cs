using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UniLinq;
using UnityEngine;

namespace HaveYouReadThis
{
    public class ProgressionInfo
    {
        // playerStableId -> (skillName -> ProgressionInfo)
        private static readonly Dictionary<string, Dictionary<string, ProgressionInfo>> AllPlayerProgressions
            = new Dictionary<string, Dictionary<string, ProgressionInfo>>();

        [JsonProperty] private int currentLevel;
        [JsonProperty] private int maxLevel;
        [JsonIgnore] private bool IsMaxed => currentLevel >= maxLevel;

        public static void AddForPlayer(string playerStableId, Dictionary<string, ProgressionInfo> progressionInfos)
        {
            AllPlayerProgressions[playerStableId] = progressionInfos;
            RefreshLocalIconInfo();
        }

        public static void UpdateForPlayer(string playerStableId, string skillName, int newLevel)
        {
            if (!AllPlayerProgressions.TryGetValue(playerStableId, out var playerData))
            {
                Log.Warning("Unable to find player by Stable ID in local AllPlayerProgressions cache");
                return;
            }

            if (playerData != null &&
                playerData.TryGetValue(skillName, out var skill) &&
                skill != null)
            {
                skill.currentLevel = newLevel;
            }

            RefreshLocalIconInfo();
        }

        public static void RemovePlayer(string playerStableId)
        {
            AllPlayerProgressions.Remove(playerStableId);
        }

        public static void RefreshLocalIconInfo()
        {
            if (!Utilities.LocalPlayerExists())
                return;

            var localPlayer = GameManager.Instance.myEntityPlayerLocal;
            var myId = Utilities.GetStablePlayerId(localPlayer);

            if (!AllPlayerProgressions.TryGetValue(myId, out var myProgressions))
                return;

            var otherPlayers = GetOtherPlayers(myId);

            foreach (var itemClass in ItemClass.list.Where(HasUnlock))
            {
                var unlockKey = itemClass.Unlocks.ToLower();

                if (!myProgressions.TryGetValue(unlockKey, out var myInfo) || !myInfo.IsMaxed)
                    continue; // fallback to vanilla behavior

                if (HasUnMaxedAlly(unlockKey, itemClass, otherPlayers))
                {
                    SetIcon(itemClass, "allies", Color.cyan);
                }
                else
                {
                    SetIcon(itemClass, "check", Color.green);
                }
            }

            localPlayer?.PlayerUI?.xui?.RefreshAllWindows();
        }

        public static Dictionary<string, ProgressionInfo> GetPlayerProgressionInfo(EntityPlayer player)
        {
            var result = new Dictionary<string, ProgressionInfo>();

            foreach (var itemClass in ItemClass.list.Where(HasUnlock))
            {
                var pv = player?.Progression?.ProgressionValues?.Get(itemClass.Unlocks);

                if (pv?.Name == null)
                    continue;

                result[pv.Name.ToLower()] = new ProgressionInfo
                {
                    currentLevel = pv.Level,
                    maxLevel = pv.CalculatedMaxLevel(player)
                };
            }

            return result;
        }

        public static string GetPartyProgressString(string unlockKey)
        {
            if (string.IsNullOrEmpty(unlockKey) || !Utilities.LocalPlayerExists())
                return string.Empty;

            var localPlayer = GameManager.Instance.myEntityPlayerLocal;
            var myId = Utilities.GetStablePlayerId(localPlayer);
            var normalizedKey = unlockKey.ToLower();

            if (!AllPlayerProgressions.TryGetValue(myId, out var myData))
                return string.Empty;

            var sb = new StringBuilder();

            if (myData.TryGetValue(normalizedKey, out var myInfo))
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"{localPlayer.PlayerDisplayName} : {myInfo.currentLevel} / {myInfo.maxLevel}");
            }

            foreach (var other in GetOtherPlayers(myId))
            {
                var otherPlayer = Utilities.FindPlayerByStableId(other.Key);

                if (!(otherPlayer?.IsInPartyOfLocalPlayer ?? false))
                    continue;

                if (other.Value.TryGetValue(normalizedKey, out var otherInfo))
                {
                    sb.AppendLine(
                        $"{otherPlayer.PlayerDisplayName} : {otherInfo.currentLevel} / {otherInfo.maxLevel}");
                }
            }

            return sb.ToString();
        }

        private static bool HasUnlock(ItemClass itemClass)
            => !string.IsNullOrEmpty(itemClass?.Unlocks);

        private static IEnumerable<KeyValuePair<string, Dictionary<string, ProgressionInfo>>>
            GetOtherPlayers(string myId)
            => AllPlayerProgressions.Where(x => x.Key != myId);

        private static bool HasUnMaxedAlly(
            string unlockKey,
            ItemClass itemClass,
            IEnumerable<KeyValuePair<string, Dictionary<string, ProgressionInfo>>> others)
        {
            foreach (var other in others)
            {
                var otherPlayer = Utilities.FindPlayerByStableId(other.Key);
                if (!(otherPlayer?.IsInPartyOfLocalPlayer ?? false))
                    continue;

                if (other.Value.TryGetValue(unlockKey, out var otherInfo))
                {
                    if (!otherInfo.IsMaxed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void SetIcon(ItemClass itemClass, string icon, Color color)
        {
            itemClass.AltItemTypeIcon = icon;
            itemClass.AltItemTypeIconColor = color;
        }
    }
}