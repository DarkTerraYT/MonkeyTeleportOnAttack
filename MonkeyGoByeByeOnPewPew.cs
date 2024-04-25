using MelonLoader;
using BTD_Mod_Helper;
using MonkeyGoByeByeOnPewPew;
using BTD_Mod_Helper.Api.ModOptions;
using System;
using System.Reflection.Metadata.Ecma335;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons;
using Il2CppSystem.IO;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(MonkeyGoByeByeOnPewPew.MonkeyGoByeByeOnPewPew), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace MonkeyGoByeByeOnPewPew;

public class MonkeyGoByeByeOnPewPew : BloonsTD6Mod
{
    // Where the camera edge on the X axis is (top + bottom, bottom is this but -109 instead)
    public static readonly int MaxAllowedTop = 114;
    // Where the camera edge on the Y axis is (left + right, left is -145 instead)
    public static readonly int MaxAllowedX = 145;

    public static readonly ModSettingBool LimitTeleportDistance = new(false);

    static readonly AbilityModel teleportAbility = new("AbilityModel_Unstuck", "Unstuck", "Teleports your tower into a new spot", 0, 0, ModContent.GetSpriteReference<MonkeyGoByeByeOnPewPew>("Icon"), 0, new(0), false, false, null, 1, 0, -1, false, false);

    public override void OnNewGameModel(GameModel result)
    {
        foreach(var tower in result.GetDescendants<TowerModel>().ToArray())
        {
            if (!tower.baseId.Contains("BananaFarm"))
            {
                tower.AddBehavior(teleportAbility);
            }
        }
    }

    public static readonly ModSettingInt MaxTeleportDistance = new(114)
    {
        min = 1,
        description = "Max distance on each axis combined"
    };
    public static readonly ModSettingInt MinTeleportDistance = new(1)
    {
        min = 1,
        description = "Minimum distance on each axis combined"
    };

    public override void OnApplicationStart()
    {
        ModHelper.Msg<MonkeyGoByeByeOnPewPew>("MonkeyGoByeByeOnPewPew loaded!");
    }

    public static int RandomInt(int min, int max)
    {
        Random rand = new();
        return rand.Next(min, max);
    }

    public static void CalculateNewSpot(Tower tower, int maxDistance, int minDistance, bool limitDistance)
    {
        Vector2 oldPos = tower.Position.ToVector2();

        Vector2 newPos = new(RandomInt(-MaxAllowedX, MaxAllowedX), RandomInt(-MaxAllowedTop + 5, MaxAllowedTop));

        if (limitDistance)
        {
            float travelledDistance = newPos.Distance(oldPos);

            while(travelledDistance > maxDistance || travelledDistance < minDistance)
            {
                newPos = new(RandomInt(-MaxAllowedX, MaxAllowedX), RandomInt(-MaxAllowedTop + 5, MaxAllowedTop));
                travelledDistance = newPos.Distance(oldPos);
            }
        }

        tower.PositionTower(newPos);
        tower.Position.Z = 100;
    }

    public override void OnAbilityCast(Ability ability)
    {
        if (ability.abilityModel.name.Contains("AbilityModel_Unstuck"))
        {
            CalculateNewSpot(ability.tower, MaxTeleportDistance, MinTeleportDistance, LimitTeleportDistance);
        }
    }

    public override void OnWeaponFire(Weapon weapon)
    {
        Tower tower = weapon.attack.tower;

        if (!tower.towerModel.baseId.Contains("BananaFarm"))
        { CalculateNewSpot(tower, MaxTeleportDistance, MinTeleportDistance, LimitTeleportDistance); }
    }
}