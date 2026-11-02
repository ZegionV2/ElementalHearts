using System;
using Verse;
using Verse.Sound;
namespace ElementalHearts {
 public sealed class HediffCompProperties_FlameRecovery:HediffCompProperties {
  public int sealAfterTicks=180;
  public float healPerSecond=2f;
  public HediffCompProperties_FlameRecovery(){compClass=typeof(HediffComp_FlameRecovery);}
 }
 public sealed class HediffComp_FlameRecovery:HediffComp {
  private int elapsed;
  public HediffCompProperties_FlameRecovery Props {get{return (HediffCompProperties_FlameRecovery)props;}}
  public override void CompExposeData(){Scribe_Values.Look(ref elapsed,"EH_recoveryElapsed",0);}
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){
   if(Pawn.Dead)return;elapsed+=delta;if(elapsed<60)return;
   int seconds=elapsed/60;elapsed%=60;
   // Seal wounds first; normal tending persists through saves and implant removal.
   Hediff_Injury worst=null;
   foreach(Hediff h in Pawn.health.hediffSet.hediffs){
    if(h.ageTicks<Props.sealAfterTicks)continue;
    if((h is Hediff_Injury || h is Hediff_MissingPart)&&h.Bleeding&&h.TendableNow())h.Tended(.75f,1f,0);
    var injury=h as Hediff_Injury;
    if(injury!=null&&!injury.IsPermanent()&&(worst==null||injury.Severity>worst.Severity))worst=injury;
   }
   if(worst!=null)worst.Heal(Props.healPerSecond*seconds);
  }
 }
 public static class FlameAudio {
  public static void Play(Pawn pawn,params string[] sounds){
   ElementalAudio.Play(pawn,sounds);
  }
 }
}
