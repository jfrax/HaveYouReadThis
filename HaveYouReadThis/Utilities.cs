using System;
using System.Collections.Generic;
using System.IO;
using UniLinq;

namespace HaveYouReadThis
{
    public static class Utilities
    {
        public static bool MOD_DISABLED { get; set; }

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
        
        public static readonly HashSet<string> IgnoredBuffs = new HashSet<string>(StringComparer.Ordinal)
        {
            ".ArmorMediumWorn",
            "_biomeradiation",
            "_carrycapacity",
            "_coretemp",
            "_degreesabsorbed",
            "_encumberedslots",
            "_encumbrance",
            "_expdeficit",
            "_lightlevel",
            "_noiselevel",
            "_outsidetemp",
            "_shaded",
            "_sheltered",
            "_underwater",
            "_wetnessrate",
            ".ArmorHeavyMobility",
            ".ArmorHeavyStaminaRun",
            ".ArmorHeavyStaminaWalk",
            ".ArmorHeavyWorn",
            ".ArmorLightLevel",
            ".ArmorLightTotal",
            ".ArmorLightWorn",
            ".ArmorMediumMobility",
            ".ArmorMediumStaminaRun",
            ".ArmorMediumStaminaWalk",
            ".BurntHazardTimerDisplay",
            ".DesertHazardTimerDisplay",
            ".insulationT1Total",
            ".insulationT2Total",
            ".insulationT3Total",
            ".insulationTotal",
            ".SnowHazardTimerDisplay",
            ".WastelandHazardTimerDisplay",
            "$BiomeBadgeLevel",
            "$BurntHazardTimer",
            "$critHitNaturalHealingRate",
            "$DesertHazardTimer",
            "$DrowningTimerMax",
            "$encumbranceEffect",
            "$maxBleedCounter",
            "$medicRegHealthIncreaseSpeed",
            "$MetabolismDuration",
            "$MetabolismResist",
            "$parkourBonus",
            "$perkBookwormChance",
            "$PlayerLevelBonus",
            "$SnowHazardTimer",
            "$treatedCritHealing",
            "$treatedCritHealingBonuses",
            "$WastelandHazardTimer",
            "foodHealthFarmer",
            "foodHealthFarmerMulti",
            "foodHealthFarmerSub",
            "smell",
            "_notAlerted",
            "_difficulty",
            "_equipReload",
            "$BurntHazardTimerMax",
            "$DesertHazardTimerMax",
            "$SnowHazardTimerMax",
            "$WastelandHazardTimerMax",
            "$infectionMaxDuration",
            "$dysenteryMaxDuration",
            "$doingHealingWrong",
            "$BiomeProgressionOn",
            "$xpFromLootThisLevel",
            "$xpFromHarvestingThisLevel",
            "$xpFromKillThisLevel",
            "$xpFromLootLast",
            "$xpFromHarvestingLast",
            "$xpFromKillLast",
            "$LastPlayerLevel",
            "_xpOther",
            "modGunRetractingStock",
            "_difficulty"
        };
    }
}