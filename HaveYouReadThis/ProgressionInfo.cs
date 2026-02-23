using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UniLinq;
using UnityEngine;

namespace HaveYouReadThis
{
    public class ProgressionInfo
    {
        // playerStableId -> (skillName -> ProgressionInfo)
        private static Dictionary<string, ProgressionInfoPlayerEntry> AllPlayerProgressions
            = new Dictionary<string, ProgressionInfoPlayerEntry>();

        private static string ProgressionsFile =>
            Path.Combine(Utilities.ModSaveDir, "playerprogression.json");

        private static bool _isDirty;

        [JsonProperty] private int currentLevel;
        [JsonProperty] private int maxLevel;
        [JsonIgnore] private bool IsMaxed => currentLevel >= maxLevel;


        public static void AddForPlayer(string playerStableId, Dictionary<string, ProgressionInfo> progressionInfos)
        {
            var player = Utilities.FindPlayerByStableId(playerStableId);
            AllPlayerProgressions[playerStableId] = new ProgressionInfoPlayerEntry()
            {
                PlayerDisplayName = player?.PlayerDisplayName,
                Progressions = progressionInfos
            };
            RefreshLocalIconInfo();
            _isDirty = true;
        }

        public static void UpdateForPlayer(string playerStableId, string skillName, int newLevel)
        {
            if (!AllPlayerProgressions.TryGetValue(playerStableId, out var playerData))
            {
                Log.Warning("Unable to find player by Stable ID in local AllPlayerProgressions cache");
                return;
            }

            if (playerData != null &&
                playerData.Progressions.TryGetValue(skillName, out var skill) &&
                skill != null)
            {
                skill.currentLevel = newLevel;
                _isDirty = true;
            }

            RefreshLocalIconInfo();
        }

        public static void RemovePlayer(string playerStableId)
        {
            AllPlayerProgressions.Remove(playerStableId);
            _isDirty = true;
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

                if (!myProgressions.Progressions.TryGetValue(unlockKey, out var myInfo) || !myInfo.IsMaxed)
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

            if (myData.Progressions.TryGetValue(normalizedKey, out var myInfo))
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"{localPlayer.PlayerDisplayName} : {myInfo.currentLevel} / {myInfo.maxLevel}");
            }


            foreach (var other in GetOtherPlayers(myId))
            {
                if (!Utilities.IsAlliesWithLocalPlayer(other.Key))
                    continue;

                if (other.Value?.Progressions?.TryGetValue(normalizedKey, out var otherInfo) ?? false)
                {
                    sb.AppendLine(
                        $"{other.Value.PlayerDisplayName} : {otherInfo.currentLevel} / {otherInfo.maxLevel}");
                }
            }


            return sb.ToString();
        }

        private static bool HasUnlock(ItemClass itemClass)
            => !string.IsNullOrEmpty(itemClass?.Unlocks);

        private static IEnumerable<KeyValuePair<string, ProgressionInfoPlayerEntry>> GetOtherPlayers(string myId)
            => AllPlayerProgressions.Where(x => x.Key != myId);

        private static bool HasUnMaxedAlly(
            string unlockKey,
            ItemClass itemClass,
            IEnumerable<KeyValuePair<string, ProgressionInfoPlayerEntry>> others)
        {
            if (others == null)
                return false;

            foreach (var other in others)
            {
                if (!Utilities.IsAlliesWithLocalPlayer(other.Key))
                    continue;

                if (other.Value?.Progressions?.TryGetValue(unlockKey, out var otherInfo) ?? false)
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

        public static void LoadProgressionsFromDisk()
        {
            if (!ConnectionManager.Instance.IsServer)
                return;

            if (AllPlayerProgressions != null && AllPlayerProgressions.Count > 0)
                return;

            if (!Directory.Exists(Utilities.ModSaveDir))
                Directory.CreateDirectory(Utilities.ModSaveDir);

            if (File.Exists(ProgressionsFile))
            {
                string json = File.ReadAllText(ProgressionsFile);
                AllPlayerProgressions =
                    JsonConvert.DeserializeObject<Dictionary<string, ProgressionInfoPlayerEntry>>(json);
            }

            if (AllPlayerProgressions == null)
                AllPlayerProgressions = new Dictionary<string, ProgressionInfoPlayerEntry>();
        }

        public static void SaveProgressionsToDisk()
        {
            if (!ConnectionManager.Instance.IsServer)
                return;

            if (!_isDirty || AllPlayerProgressions == null)
                return;

            if (!Directory.Exists(Utilities.ModSaveDir))
                Directory.CreateDirectory(Utilities.ModSaveDir);

            string json = JsonConvert.SerializeObject(AllPlayerProgressions);
            File.WriteAllText(ProgressionsFile, json);

            _isDirty = false;
        }
    }
}