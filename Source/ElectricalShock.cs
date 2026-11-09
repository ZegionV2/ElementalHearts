using System;
using RimWorld;
using Verse;

namespace ElementalHearts {
 public static class ElectricalShock {
  public const string DefName="EH_ElectricalShock";
  public const int RecoveryTicks=180;

  public static bool TryApply(Pawn source,Pawn victim){
   if(source==null||victim==null||victim==source||victim.Dead||victim.Downed||!victim.Spawned||
    !victim.HostileTo(source)||ElementalNonLethal.Enabled(source)||HediffComp_LightningPassives.On(source)==null)return false;
   var def=DefDatabase<HediffDef>.GetNamed(DefName);
   if(victim.health.hediffSet.HasHediff(def))return false;
   bool incarnate=ThunderIncarnate.Active(source),evolved=ThunderstormEvolution.Active(source)||incarnate;
   var resistance=HediffComp_LightningPassives.On(victim);
   float factor=resistance==null?1f:Math.Max(0f,Math.Min(1f,resistance.Props.resistanceFactor));
   if(!Rand.Chance((incarnate?.45f:evolved?.40f:.25f)*factor))return false;
   int duration=Math.Max(1,(int)Math.Round((evolved?90:60)*factor));
   var shock=victim.health.AddHediff(def);
   var state=shock.TryGetComp<HediffComp_ElectricalShock>();
   state.stunnedUntil=Find.TickManager.TicksGame+duration;
   state.recoveryUntil=state.stunnedUntil+RecoveryTicks;
   victim.stances.stunner.StunFor(duration,source);
   return true;
  }
 }

 public sealed class HediffCompProperties_ElectricalShock : HediffCompProperties {
  public HediffCompProperties_ElectricalShock(){compClass=typeof(HediffComp_ElectricalShock);}
 }
 public sealed class HediffComp_ElectricalShock : HediffComp {
  public int stunnedUntil,recoveryUntil;
  public override void CompExposeData(){
   Scribe_Values.Look(ref stunnedUntil,"EH_shockStunnedUntil");
   Scribe_Values.Look(ref recoveryUntil,"EH_shockRecoveryUntil");
  }
  public override bool CompShouldRemove {get{return Find.TickManager.TicksGame>=recoveryUntil;}}
  public override string CompLabelInBracketsExtra {
   get{return Find.TickManager.TicksGame<stunnedUntil?"stunned":"discharge recovery";}
  }
  public override string CompTipStringExtra {
   get{return "Electrical shock briefly disrupts movement and attacks. Further electrical shock is blocked for three seconds after the stun. Other injuries and statuses resolve normally.";}
  }
 }
}
