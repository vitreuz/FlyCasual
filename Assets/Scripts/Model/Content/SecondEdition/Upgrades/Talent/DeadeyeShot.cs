using Upgrade;
using SubPhases;
using System.Collections.Generic;
using Ship;
using System.Linq;
using System;
using Arcs;

namespace UpgradesList.SecondEdition
{
    public class DeadeyeShot : GenericUpgrade
    {
        public DeadeyeShot() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
                "Deadeye Shot",
                UpgradeType.Talent,
                cost: 1,
                abilityType: typeof(Abilities.SecondEdition.DeadeyeShotAbility),
                restriction: new BaseSizeRestriction(BaseSize.Small, BaseSize.Medium)
            );

            ImageUrl = "https://sb-cdn.fantasyflightgames.com/card_images/en/99f10f4dd059aae2529ec0863a6cc47e.png";
        }
    }
}

namespace Abilities.SecondEdition
{
    //While you perform a primary attack, if the defender is in your bullseye
    //firing arc, you may spend 1 success result or change 1 critical success
    //result to a success result. If you do, the defender exposes 1 of its
    //damage cards.

    public class DeadeyeShotAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            AddDiceModification(
                HostUpgrade.UpgradeInfo.Name + " Crit",
                IsCritAvailable,
                () => 35,
                DiceModificationType.Change,
                1,
                new List<DieSide> { DieSide.Crit },
                DieSide.Success,
                payAbilityCost: ExposeFaceDownCard
            );

            AddDiceModification(
                HostUpgrade.UpgradeInfo.Name + " Hit",
                IsHitAvailable,
                () => 20,
                DiceModificationType.Spend,
                1,
                new List<DieSide> { DieSide.Success },
                payAbilityCost: ExposeFaceDownCard
            );

            HostShip.OnAttackFinish += ResetUsedFlag;
        }

        public override void DeactivateAbility()
        {
            RemoveDiceModification();
            HostShip.OnAttackFinish -= ResetUsedFlag;
        }

        public void ResetUsedFlag(GenericShip ship)
        {
            ClearIsAbilityUsedFlag();
        }
        public bool IsCritAvailable()
        {
            if (IsAbilityUsed) return false;
            if (Combat.Attacker != HostShip) return false;
            if (Combat.ChosenWeapon.WeaponType != WeaponTypes.PrimaryWeapon) return false;
            if (!HostShip.SectorsInfo.IsShipInSector(Combat.Defender, ArcType.Bullseye)) return false;
            if (Combat.DiceRollAttack.CriticalSuccesses < 1) return false;
            if (!Combat.Defender.Damage.HasFacedownCards) return false;

            return true;
        }
        public bool IsHitAvailable()
        {
            if (IsAbilityUsed) return false;
            if (Combat.Attacker != HostShip) return false;
            if (Combat.ChosenWeapon.WeaponType != WeaponTypes.PrimaryWeapon) return false;
            if (!HostShip.SectorsInfo.IsShipInSector(Combat.Defender, ArcType.Bullseye)) return false;
            if (Combat.DiceRollAttack.RegularSuccesses < 1) return false;
            if (!Combat.Defender.Damage.HasFacedownCards) return false;

            return true;
        }

        private void ExposeFaceDownCard(Action<bool> callback)
        {
            if (Combat.Defender.Damage.HasFacedownCards)
            {
                IsAbilityUsed = true;
                Combat.Defender.Damage.ExposeRandomFacedownCard(delegate { callback(true); });
            }
            else
            {
                callback(false);
            }
        }
    }
}