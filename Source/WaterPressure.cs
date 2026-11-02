using System;using System.Collections.Generic;using Verse;
namespace ElementalHearts {
 public sealed class WaterPressureEntry:IExposable {
  public Pawn source;public int stacks,until;
  public void ExposeData(){Scribe_References.Look(ref source,"source");Scribe_Values.Look(ref stacks,"stacks");Scribe_Values.Look(ref until,"until");}
 }
 public sealed class HediffCompProperties_WaterPressure:HediffCompProperties {public HediffCompProperties_WaterPressure(){compClass=typeof(HediffComp_WaterPressure);}}
 public sealed class HediffComp_WaterPressure:HediffComp {
  public List<WaterPressureEntry> entries=new List<WaterPressureEntry>();
  public override void CompExposeData(){Scribe_Collections.Look(ref entries,"EH_waterPressureOwners",LookMode.Deep);if(entries==null)entries=new List<WaterPressureEntry>();}
  public int Stacks(Pawn source,int now){foreach(var e in entries)if(e!=null&&e.source==source&&e.until>now)return Math.Max(0,Math.Min(3,e.stacks));return 0;}
  public void Add(Pawn source,int now){Expire(now);foreach(var e in entries)if(e.source==source){e.stacks=Math.Min(3,e.stacks+1);e.until=now+480;Refresh(now);return;}if(entries.Count>=8){int oldest=0;for(int i=1;i<entries.Count;i++)if(entries[i].until<entries[oldest].until)oldest=i;entries.RemoveAt(oldest);}entries.Add(new WaterPressureEntry{source=source,stacks=1,until=now+480});Refresh(now);}
  public void Expire(int now){for(int i=entries.Count-1;i>=0;i--)if(entries[i]==null||entries[i].source==null||entries[i].source.Destroyed||entries[i].until<=now)entries.RemoveAt(i);while(entries.Count>8)entries.RemoveAt(0);}
  public void Refresh(int now){int maximum=0;foreach(var e in entries)if(e.until>now)maximum=Math.Max(maximum,Math.Min(3,e.stacks));parent.Severity=Math.Max(1,maximum);}
  public int Consume(Pawn source,int now){int count=Stacks(source,now);entries.RemoveAll(e=>e==null||e.source==source);Refresh(now);return count;}
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){int now=Find.TickManager.TicksGame;Expire(now);if(entries.Count==0){Pawn.health.RemoveHediff(parent);return;}Refresh(now);}
  public override string CompTipStringExtra{get{return "Pressure: the strongest active mark slows movement by 5% per stack, up to 15%. Each Water caster owns their own stacks; Ocean's Judgement consumes only its caster's stacks. Marks expire after eight seconds.";}}
 }
 public static class WaterPressure {
  public static HediffComp_WaterPressure On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName=="EH_WaterPressure")return h.TryGetComp<HediffComp_WaterPressure>();return null;}
  public static int Stacks(Pawn source,Pawn target){var c=On(target);return c==null?0:c.Stacks(source,Find.TickManager.TicksGame);}
  public static void Apply(Pawn source,Pawn target){if(source==null||target==null||target.Dead||target.Downed||!target.Spawned)return;var c=On(target);if(c==null)c=target.health.AddHediff(DefDatabase<HediffDef>.GetNamed("EH_WaterPressure")).TryGetComp<HediffComp_WaterPressure>();if(c!=null){c.Add(source,Find.TickManager.TicksGame);WaterAbilityVfx.Emit(target,4,target.DrawPos,target.DrawPos,12,1.1f);}}
  public static void Consume(Pawn source,Pawn target){var c=On(target);if(c!=null){c.Consume(source,Find.TickManager.TicksGame);if(c.entries.Count==0)target.health.RemoveHediff(c.parent);}}
 }
}
