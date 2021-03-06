using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Pathing;
using Styx.CommonBot.POI;
using Styx.CommonBot.Routines;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System.IO;

namespace BloodyMessNG
{
    class DeathKnight : CombatRoutine
    {
        private string vNum = "v1.0.0";
        public override sealed string Name { get { return "Joystick's BloodyMess (unofficial NG version) PVP " + vNum; } }
        public override WoWClass Class { get { return WoWClass.DeathKnight; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"Routines/BloodyMessNG/"));
        private bool BloodPresenceSwitch = false;

        private BloodyMessNGForm BloodyMessNGConfig;

        private bool UseBloodTap
        {
            get { return BloodyMessNGConfig.Settings.UseBloodTap; }
        }
        private bool UsePath
        {
            get { return BloodyMessNGConfig.Settings.UsePath; }
        }
        private int DeathSiphonPercent
        {
            get { return BloodyMessNGConfig.Settings.DeathSiphonPercent; }
        }
        private bool DisableMovement
        {
            get { return BloodyMessNGConfig.Settings.DisableMovement; }
        }
        private bool DisableTargeting
        {
            get { return BloodyMessNGConfig.Settings.DisableTargeting; }
        }
        private bool BloodPresence
        {
            get { return BloodyMessNGConfig.Settings.BloodPresence; }
        }
        private bool FrostPresence
        {
            get { return BloodyMessNGConfig.Settings.FrostPresence; }
        }
        private bool UnholyPresence
        {
            get { return BloodyMessNGConfig.Settings.UnholyPresence; }
        }
        private bool UseMindFreeze
        {
            get { return BloodyMessNGConfig.Settings.UseMindFreeze; }
        }
        private bool UseStrangulate
        {
            get { return BloodyMessNGConfig.Settings.UseStrangulate; }
        }
        private bool UseDeathGripInterrupt
        {
            get { return BloodyMessNGConfig.Settings.UseDeathGripInterrupt; }
        }
        private bool UseDeathGrip
        {
            get { return BloodyMessNGConfig.Settings.UseDeathGrip; }
        }
        private bool UseERW
        {
            get { return BloodyMessNGConfig.Settings.UseERW; }
        }
        private bool UseDRW
        {
            get { return BloodyMessNGConfig.Settings.UseDRW; }
        }
        private bool UseBoneShield
        {
            get { return BloodyMessNGConfig.Settings.UseBoneShield; }
        }
        private bool UseHorn
        {
            get { return BloodyMessNGConfig.Settings.UseHorn; }
        }
        private int LichbornePercent
        {
            get { return BloodyMessNGConfig.Settings.LichbornePercent; }
        }
        private int DeathCoilPercent
        {
            get { return BloodyMessNGConfig.Settings.DeathCoilPercent; }
        }
        private int ConversionPercent
        {
            get { return BloodyMessNGConfig.Settings.ConversionPercent; }
        }
        private int VampiricBloodPercent
        {
            get { return BloodyMessNGConfig.Settings.VampiricBloodPercent; }
        }
        private int RuneTapPercent
        {
            get { return BloodyMessNGConfig.Settings.RuneTapPercent; }
        }
        private int AMSPercent
        {
            get { return BloodyMessNGConfig.Settings.AMSPercent; }
        }
        private int IBFPercent
        {
            get { return BloodyMessNGConfig.Settings.IBFPercent; }
        }
        private int DeathStrikePercent
        {
            get { return BloodyMessNGConfig.Settings.DeathStrikePercent; }
        }

        public override bool WantButton
        {
            get
            {
                return true;
            }
        }
        public override void OnButtonPress()
        {
            if (BloodyMessNGConfig != null)
                BloodyMessNGConfig.ShowDialog();
            else
            {
                BloodyMessNGConfig = new BloodyMessNGForm();
                BloodyMessNGConfig.ShowDialog();
            }
        }
        public override void Initialize()
        {
            BloodyMessNGConfig = new BloodyMessNGForm();
            Logging.Write(LogLevel.Normal, Colors.White, "Joystick's BloodyMess (unofficial NG version) PVP Started");
//            Updater.CheckForUpdate();
        }
        public override bool NeedRest { get { return false; } }
        public override void Rest()
        {
        }
        public override void Pull()
        {
            if (!DisableTargeting)
            {
                GetTarget();
                if (Me.CurrentTarget == null || !Me.GotTarget || !Me.CurrentTarget.Attackable || Me.Stunned || Me.Fleeing || Me.IsDead)
                    return;
                GetFace();
            }
            else
            {
                if (Me.Mounted)
                    return;
            }
            if (!DisableMovement)
            {
                GetMelee();
            }
            if (CheckHealth())
            {
                if (MustHeal())
                    return;
            }
            if (CheckBuffs())
            {
                if (Buff())
                    return;
            }
            if (InterruptRotation())
                return;
            if (!SpellManager.GlobalCooldown)
            {
                if (DiseasesRotation())
                    return;
                if (UseCooldowns())
                    return;
                if (!Me.CurrentTarget.IsWithinMeleeRange)
                {
                    if (RangedRotation())
                        return;
                }
                else if (Me.CurrentTarget.IsWithinMeleeRange)
                {
                    if (MeleeRotation())
                        return;
                }
            }

        }
        public override bool NeedPullBuffs
        {
            get
            {
                    return false;
            }
        }
        public override void PullBuff() 
        {
        }
        public override bool NeedPreCombatBuffs
        {
            get
            {
                return false;
            }
        }
        public override void PreCombatBuff()
        {
        }
        public override bool NeedCombatBuffs
        {
            get
            {
                return false;
            }
        }
        public override void CombatBuff()   
        {
        }
        public override bool NeedHeal { get { return false; } }
        public override void Heal()
        {

        }
        public void HandleFalling() { }
        public override void Combat()
        {
            if (!DisableTargeting)
            {
                GetTarget();
                if (Me.CurrentTarget == null || !Me.GotTarget || !Me.CurrentTarget.Attackable || Me.Stunned || Me.Fleeing || Me.IsDead)
                    return;
                GetFace();
            }
            else
            {
                if (Me.Mounted)
                    return;
            }
            if (!DisableMovement)
            {
                GetMelee();
            }
            if (CheckHealth())
            {
                if (MustHeal())
                    return;
            }
            if (CheckBuffs())
            {
                if (Buff())
                    return;
            }
            if (InterruptRotation())
                return;
            if (!SpellManager.GlobalCooldown)
            {
                if (DiseasesRotation())
                    return;
                if (UseCooldowns())
                    return;
                if (Me.GotTarget)
                {
                    if (!Me.CurrentTarget.IsWithinMeleeRange)
                    {
                        if (RangedRotation())
                            return;
                    }
                    else if (Me.CurrentTarget.IsWithinMeleeRange)
                    {
                        if (MeleeRotation())
                            return;
                    }
                }
            }
        }
        private void AutoAttack()
        {
            if (!Me.IsAutoAttacking)
            {
                Lua.DoString("StartAttack()");
            }

        }
        private bool CanCast(string spellName)
        {
            if (SpellManager.CanCast(spellName))
                return true;
            else
            {
                return false;
            }
        }
        private bool CanBuff(string spellName)
        {
            if (SpellManager.CanCast(spellName))
                return true;
            else
            {
                return false;
            }
        }
        private void Cast(string spellName)
        {
            Logging.Write(LogLevel.Normal, Colors.Yellow, "[BloodyMessNG] Casting " + spellName);
            if (Me.GotTarget)
                SpellManager.Cast(spellName);
        }
        private void Buff(string spellName)
        {
            Logging.Write(LogLevel.Normal, Colors.Red, "[BloodyMessNG] Buffing " + spellName);
            SpellManager.Cast(spellName);
        }
        private void CastMe(string spellName)
        {
            Logging.Write(LogLevel.Normal, Colors.Yellow, "[BloodyMessNG] Casting " + spellName + " on Myself");
            SpellManager.Cast(spellName, Me);
        }
        private void Interrupt(String spellName)
        {
            Logging.Write(LogLevel.Normal, Colors.Blue, "[BloodyMessNG] Interrupting " + Me.CurrentTarget.Class + "'s " + Me.CurrentTarget.CastingSpell.ToString() + " with " + spellName);
            if (Me.GotTarget)
                SpellManager.Cast(spellName);
        }
        private bool CCTC(string spellName)
        {
            if (CanCast(spellName) && Me.GotTarget)
            {
                Cast(spellName);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CCTCMe(string spellName)
        {
            if (CanCast(spellName))
            {
                CastMe(spellName);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CCBC(string spellName)
        {
            if (CanBuff(spellName))
            {
                Buff(spellName);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CCIC(string spellName)
        {
            if (CanCast(spellName) && Me.GotTarget)
            {
                Interrupt(spellName);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void GetMelee()
        {
            if (Me.GotTarget)
            {
                if (!Me.CurrentTarget.IsWithinMeleeRange && !Me.IsCasting && Styx.CommonBot.BotManager.Current.Name != "LazyRaider")
                {
                    Navigator.MoveTo(Me.CurrentTarget.Location);
                }
                else if (Me.CurrentTarget.IsWithinMeleeRange && Styx.CommonBot.BotManager.Current.Name != "LazyRaider")
                {
                    Navigator.PlayerMover.MoveStop();
                }
            }
        }
        private void GetFace()
        {
            if (Me.GotTarget)
            {
                if (!Me.IsFacing(Me.CurrentTarget.Location))
                    Me.CurrentTarget.Face();
            }
        }
        private bool DiseasesRotation()
        {
            if (Me.GotTarget)
            {

                if (!Me.CurrentTarget.HasAura("Frost Fever") && !Me.CurrentTarget.HasAura("Blood Plague"))
                {
                    if (Me.CurrentTarget.Distance < 30)
                    {
                        if (CCTC("Outbreak"))
                            return true;
                    }
                    if (Me.CurrentTarget.Distance < 20)
                    {
                        if (CCTC("Chains of Ice"))
                            return true;
                    }

                }
                if (!Me.CurrentTarget.HasAura("Frost Fever"))
                {
                    if (Me.CurrentTarget.Distance < 30)
                    {
                        if (CCTC("Outbreak"))
                            return true;
                    }
                    if (Me.CurrentTarget.Distance < 20)
                    {
                        if (CCTC("Chains of Ice"))
                            return true;
                    }
                }
                if (!Me.CurrentTarget.HasAura("Blood Plague"))
                {
                    if (Me.CurrentTarget.Distance < 30)
                    {
                        if (CCTC("Outbreak"))
                            return true;
                    }
                    if (Me.CurrentTarget.IsWithinMeleeRange)
                    {
                        if (CCTC("Plague Strike"))
                            return true;
                    }
                }
            }

            return false;
        }
        private void GetTarget()
        {
            if (!Me.GotTarget)
            {
                FindTarget();
            }
            else
            {
                if (Me.CurrentTarget.IsPet)
                {
                    (Me.CurrentTarget.CreatedByUnit).Target();
                    BotPoi.Current = new BotPoi(Me.CurrentTarget, PoiType.Kill);
                    WoWMovement.ConstantFace(Me.CurrentTargetGuid);
                }

                if (Me.CurrentTarget.Distance > 30 || !Me.CurrentTarget.IsAlive || !Me.CurrentTarget.Attackable
                    || Me.CurrentTarget.HasAura("Spirit of Redemption") || Me.CurrentTarget.HasAura("Ice Block"))
                {
                    Me.ClearTarget();
                    FindTarget();
                }
            }
        }
        private void FindTarget()
        {
            if (Styx.CommonBot.BotManager.Current.Name != "BG Bot [Beta]" && Styx.CommonBot.BotManager.Current.Name != "PvP")
                return;
            WoWPlayer newTarget = ObjectManager.GetObjectsOfType<WoWPlayer>(false, false).
                Where(p => p.IsHostile && !p.IsTotem && !p.IsPet && !p.IsDead && p.DistanceSqr <= (10 * 10) && !p.Mounted
                    && !p.HasAura("Ice Block") && !p.HasAura("Spirit of Redemption")).
                OrderBy(u => u.HealthPercent).FirstOrDefault();
            if (newTarget != null)
            {
                newTarget.Target();
                BotPoi.Current = new BotPoi(Me.CurrentTarget, PoiType.Kill);
                WoWMovement.ConstantFace(Me.CurrentTargetGuid);
            }

        }
        private bool InterruptRotation()
        {
            if (Me.GotTarget)
            {
                if (Me.CurrentTarget.CanInterruptCurrentSpellCast && Me.CurrentTarget.IsCasting)
                {
                    if (!SpellManager.GlobalCooldown)
                    {
                        if (Me.CurrentTarget.Distance < 30 && UseStrangulate)
                        {
                            if (CCIC("Strangulate"))
                                return true;
                        }
                    }
                    if (Me.CurrentTarget.IsWithinMeleeRange && UseMindFreeze)
                    {
                        if (CCIC("Mind Freeze"))
                            return true;
                    }
                    if (Me.CurrentTarget.Distance < 30 && UseDeathGripInterrupt)
                    {
                        if (CCIC("Death Grip"))
                            return true;
                    }
                }
            }
            return false;
        }
        private bool RangedRotation()
        {
            if (Me.GotTarget)
            {
                if (Me.CurrentTarget.Distance < 30 && UseDeathGrip && !IsPVPBoss())
                {
                    if (CCTC("Death Grip"))
                        return true;
                }
                if (Me.CurrentTarget.Distance < 30)
                {
                    if (Me.CurrentRunicPower == Me.MaxRunicPower)
                    {
                        if (CCTC("Death Coil"))
                            return true;
                    }
                }
                if (Me.CurrentTarget.Distance < 20)
                {
                    if (CCTC("Chains of Ice"))
                        return true;
                }
            }
            return false;
        }
        private bool MeleeRotation()
        {
            if (Me.GotTarget)
            {
                if (Me.CurrentTarget.HealthPercent < 40 && !Me.CurrentTarget.HasAura("Soul Reaper"))
                {
                    if (CCTC("Soul Reaper"))
                        return true;
                }
                if (Me.CurrentRunicPower == Me.MaxRunicPower)
                {
                    if (CCTC("Rune Strike"))
                        return true;
                }
                if (CCTC("Death Strike"))
                    return true; 
                if (CCTC("Heart Strike"))
                    return true;
                if (CCTC("Necrotic Strike"))
                    return true;
            }

            return false;
        }
        private bool UseCooldowns()
        {
            if (UseERW && !(Me.BloodRuneCount > 0 || Me.UnholyRuneCount > 0 || Me.DeathRuneCount > 0 || Me.FrostRuneCount > 0))
            {
                if (CCTC("Empower Rune Weapon"))
                    return true;
            }
            if (UseDRW)
            {
                if (CCTC("Dancing Rune Weapon"))
                    return true;
            }

            if (UseBloodTap && Me.BloodRuneCount < 2 && Me.HasAura("Blood Charge"))
            {
                if (Me.Auras["Blood Charge"].StackCount >= 5)
                {
                    if (CCTC("Blood Tap"))
                        return true;
                }
            }

            return false;
        }
        private bool CheckHealth()
        {
            if (Me.HealthPercent < LichbornePercent && CanCast("Lichborne"))
            {
                return true;
            }
            else if (Me.HealthPercent < DeathCoilPercent && CanCast("Death Coil") && Me.HasAura("Lichborne") && !SpellManager.GlobalCooldown)
            {
                return true;
            }
            else if (Me.HealthPercent < IBFPercent && CanCast("Icebound Fortitude"))
            {
                return true;
            }
            else if (Me.HealthPercent < AMSPercent && CanCast("Anti-Magic Shell"))
            {
                return true;
            }
            else if (Me.HealthPercent < VampiricBloodPercent && CanCast("Vampiric Blood"))
            {
                return true;
            }
            else if ((Me.HealthPercent < RuneTapPercent || (Me.HasAura("Will of the Necropolis") && Me.HealthPercent < RuneTapPercent)) && CanCast("Rune Tap"))
            {
                return true;
            }
            else if (Me.HealthPercent < DeathStrikePercent && CanCast("Death Strike") && Me.CurrentTarget.IsWithinMeleeRange && !SpellManager.GlobalCooldown)
            {
                return true;
            }
            else if (Me.HealthPercent < DeathSiphonPercent && CanCast("Death Siphon") && !SpellManager.GlobalCooldown)
            {
                return true;
            }
            else if (Me.HealthPercent < ConversionPercent && !Me.HasAura("Conversion") && Me.CurrentRunicPower >= 5 && CanCast("Conversion"))
            {
                return true;
            }

            return false;
        }
        private bool MustHeal()
        {
            if (Me.HealthPercent < VampiricBloodPercent)
            {
                if (CCTC("Vampiric Blood"))
                    return true;
            }
            if (Me.HealthPercent < LichbornePercent && CanBuff("Lichborne"))
            {
                if (CCTC("Lichborne"))
                    return true;
            }
            if (Me.HasAura("Lichborne") && Me.HealthPercent < DeathCoilPercent && !SpellManager.GlobalCooldown)
            {
                if (CCTCMe("Death Coil"))
                    return true;
            }
            if (Me.HealthPercent < RuneTapPercent || (Me.HasAura("Will of the Necropolis") && Me.HealthPercent < RuneTapPercent))
            {
                if (CCTC("Rune Tap"))
                    return true;
            }
            if (Me.HealthPercent < AMSPercent)
            {
                if (CCTC("Anti-Magic Shell"))
                    return true;
            }
            if (Me.HealthPercent < IBFPercent)
            {
                if (CCTC("Icebound Fortitude"))
                    return true;
            }
            if (Me.CurrentTarget.IsWithinMeleeRange && !SpellManager.GlobalCooldown && Me.HealthPercent < DeathStrikePercent)
            {
                if (CCTC("Death Strike"))
                    return true;
            }
            if (Me.CurrentTarget.Distance < 40 && Me.HealthPercent < DeathSiphonPercent && !SpellManager.GlobalCooldown && Me.GotTarget)
            {
                if (CCTC("Death Siphon"))
                    return true;
            }
            if (Me.HealthPercent < ConversionPercent && !Me.HasAura("Conversion") && Me.CurrentRunicPower >= 5 && CanCast("Conversion"))
            {
                if (CCTC("Conversion"))
                    return true;
            }

            return false;
        }
        private bool IsPVPBoss()
        {
            if (Me.CurrentTarget.Name.ToString().Equals("Drek'Thar")
             || Me.CurrentTarget.Name.ToString().Equals("Vanndar Stormpike")
             || Me.CurrentTarget.Name.ToString().Equals("Captain Balinda Stonehearth")
             || Me.CurrentTarget.Name.ToString().Equals("Captain Galvangar")
             || Me.CurrentTarget.Name.ToString().Equals("High Commander Halford Wyrmbane")
             || Me.CurrentTarget.Name.ToString().Equals("Overlord Agmar"))
            {
                return true;
            }


            return false;
        }
        public override void Pulse()
        {
            if (!Me.Combat)
            {
                if (CheckBuffs())
                {
                    if (Buff())
                        return;
                }
            }
        }
        private bool CheckBuffs()
        {
            if (!Me.HasAura("Path of Frost") && UsePath && !Me.Combat)
                return true;
	        if ((!BloodPresenceSwitch || Me.HasAura("Unholy Presence") || Me.HasAura("Frost Presence")) && BloodPresence)
                return true;
            if (!Me.HasAura("Frost Presence") && FrostPresence)
                return true;
            if (!Me.HasAura("Unholy Presence") && UnholyPresence)
                return true;
            if (Me.Mounted)
                return false;
            if (!Me.HasAura("Bone Shield") && UseBoneShield)
                return true;
            if ((!Me.HasAura("Horn of Winter") && !Me.HasAura("Battle Shout")) && UseHorn)
                return true;
            
            return false;
        }
        private bool Buff()
        {
            if (!SpellManager.GlobalCooldown)
            {
                if (!Me.HasAura("Bone Shield") && UseBoneShield && !Me.Mounted)
                {
                    if (CCBC("Bone Shield"))
                        return true;
                }
                if ((!Me.HasAura("Horn of Winter") && !Me.HasAura("Battle Shout")) && UseHorn && !Me.Mounted)
                {
                    if (CCBC("Horn of Winter"))
                        return true;
                }
                if (!Me.HasAura("Path of Frost") && UsePath && !Me.Combat)
                {
                    if (CCBC("Path of Frost"))
                        return true;
                }
                if ((!BloodPresenceSwitch || Me.HasAura("Unholy Presence") || Me.HasAura("Frost Presence")) && BloodPresence)
                {
                    if (CCBC("Blood Presence"))
                    {
                        BloodPresenceSwitch = true;
                        return true;
                    }
                }
                if (!Me.HasAura("Frost Presence") && FrostPresence)
                {
                    if (CCBC("Frost Presence"))
                    {
                        BloodPresenceSwitch = false;
                        return true;
                    }
                }
                if (!Me.HasAura("Unholy Presence") && UnholyPresence)
                {
                    if(CCBC("Unholy Presence"))
                    {
                        BloodPresenceSwitch = false;
                        return true;
                    }
                }

            }

            return false;
        }
    }
  
}
