using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class GlacialRules {
  public const int FreezeTicks=300,ThawImmunity=180,Cooldown=2400,MaxVictims=16,PushTicks=9;
  public static bool Trigger(int enemies,bool ready,bool transformed){return enemies>=2&&ready&&transformed;}
  public static float HypothermiaGain(float current){return Math.Max(0,Math.Min(.15f,.30f-current));}
  public static int Frame(int age,int duration,int frames){return Math.Max(0,Math.Min(frames-1,(int)((long)Math.Max(0,age)*frames/Math.Max(1,duration))));}
 }
 public sealed class HediffCompProperties_GlacialExplosion:HediffCompProperties {public HediffCompProperties_GlacialExplosion(){compClass=typeof(HediffComp_GlacialExplosion);}}
 public sealed class HediffComp_GlacialExplosion:HediffComp {
  public int readyAt,nextCheck;
  public override void CompExposeData(){Scribe_Values.Look(ref readyAt,"EH_glacialExplosionReadyAt");Scribe_Values.Look(ref nextCheck,"EH_glacialExplosionNextCheck");}
  public static HediffComp_GlacialExplosion On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_GlacialExplosion>();if(c!=null)return c;}return null;}
  public bool CanAct{get{var d=HediffComp_WaterDefense.On(Pawn);return GlacialEvolution.Active(Pawn)&&d!=null&&d.Able&&!d.PendingCounter&&!WaterTechnique.Busy(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&Pawn.CurJobDef!=DefDatabase<JobDef>.GetNamed("EH_WaterCounter")&&HediffComp_ElementalTransform.Active(Pawn);}}
  public List<Pawn> Targets(){return WaterCombat.Targets(Pawn,Pawn.Position,4,GlacialRules.MaxVictims);}
  public int Crowd(){int n=0;foreach(var p in Targets())if(p.HostileTo(Pawn)&&!p.IsPrisoner&&!p.IsSlave){if(++n==2)break;}return n;}
  public bool TryCast(){int now=Find.TickManager.TicksGame;if(!CanAct||!GlacialRules.Trigger(Crowd(),now>=readyAt,HediffComp_ElementalTransform.Active(Pawn)))return false;readyAt=now+GlacialRules.Cooldown;nextCheck=now+12;var victims=Targets();var struck=new HashSet<int>();var profile=LightningCombat.Profile(Pawn,10);GlacialVfx.Emit(Pawn,5,Pawn.DrawPos,Pawn.DrawPos,36,9);ElementalAudio.Ability(Pawn,ElementalSound.Burst);foreach(var victim in victims){if(!WaterCombat.Eligible(Pawn,victim)||!struck.Add(victim.thingIDNumber))continue;bool immune=GlacialFreeze.Immune(victim,now);bool restrained=ElementalNonLethal.Enabled(Pawn);if(!restrained)WaterCombat.Strike(Pawn,victim,profile,2.5f);if(victim.Dead||victim.Downed||!victim.Spawned||immune)continue;GlacialFreeze.Apply(Pawn,victim,!restrained);}return true;}
  public bool Auto(){var d=HediffComp_WaterDefense.On(Pawn);if(d==null||!d.Active||!CanAct)return false;if(ElementalCombatPolicy.Controlled(Pawn)){if(Pawn.jobs.jobQueue.Count>0||Pawn.CurJob==null)return false;var job=Pawn.CurJob;if(job.def!=JobDefOf.Wait_Combat&&job.def!=JobDefOf.AttackMelee&&!SearchAndDestroyBridge.IsApproach(Pawn,job))return false;}if(Pawn.stances.FullBodyBusy&&!(Pawn.stances.curStance is Stance_Cooldown))return false;return TryCast();}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(!Pawn.Spawned)return;int now=Find.TickManager.TicksGame;if(now<nextCheck)return;nextCheck=now+12;Auto();}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn))yield break;var c=new Command_Action{defaultLabel="Glacial Explosion",defaultDesc="Automatically erupts when at least two visible hostiles surround you within four cells. Knocks surviving enemies up to three safe cells outward, freezes movement and attacks for five seconds, tints victims icy blue and applies mild real hypothermia. One impact per target; no collision damage or terrain changes. Forty-second cooldown. Non-lethal retains the push/freeze without wounds or hypothermia. Uses the shared Auto Combat setting. May also be pressed when the same crowd condition is met.",icon=GlacialVfx.Icon(4),groupable=false,action=()=>TryCast()};if(!CanAct)c.Disable("Transform and finish the current controlled action first.");else if(Find.TickManager.TicksGame<readyAt)c.Disable("Recharging: "+Math.Ceiling((readyAt-Find.TickManager.TicksGame)/60f)+"s");else if(Crowd()<2)c.Disable("Requires at least two nearby visible hostiles.");yield return c;}
 }
 public static class GlacialFreeze {
  public static HediffComp_GlacialFrozen On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_GlacialFrozen>();if(c!=null)return c;}return null;}
  public static bool Active(Pawn p){var c=On(p);return c!=null&&c.Active;}
  public static bool Immune(Pawn p,int now){var c=On(p);return c!=null&&now<c.immunityUntil;}
  public static bool Safe(Pawn p,IntVec3 from,IntVec3 to){if(!ThunderstormBlitz.Landing(p,to))return false;foreach(var cell in LightningGeometry.Trace(from,to)){if(!cell.InBounds(p.Map)||!LightningCombat.Open(cell,p.Map)||!cell.Standable(p.Map))return false;foreach(var t in cell.GetThingList(p.Map))if(t is Pawn&&t!=p)return false;}return true;}
  public static void Apply(Pawn source,Pawn victim,bool chill){int now=Find.TickManager.TicksGame;if(Immune(victim,now)||victim.Dead||victim.Downed)return;var start=victim.Position;var delta=(start-source.Position).ToVector3().normalized;var end=start;for(int i=1;i<=(EarthCombat.Anchored(victim)?0:3);i++){var cell=(start.ToVector3Shifted()+delta*i).ToIntVec3();if(cell==end)continue;if(!Safe(victim,end,cell))break;end=cell;}var h=victim.health.AddHediff(DefDatabase<HediffDef>.GetNamed("EH_GlacialFrozen"));var c=h.TryGetComp<HediffComp_GlacialFrozen>();c.priorStunUntil=now+victim.stances.stunner.StunTicksLeft;c.start=start;c.end=end;c.pushStart=now;c.pushUntil=now+(start==end?0:GlacialRules.PushTicks);c.freezeUntil=c.pushUntil+GlacialRules.FreezeTicks;c.immunityUntil=c.freezeUntil+GlacialRules.ThawImmunity;
   victim.pather.StopDead();victim.stances.CancelBusyStanceHard();victim.stances.stunner.StunFor(c.freezeUntil-now,source,false,false,true);if(chill&&!victim.RaceProps.IsMechanoid&&victim.GetStatValue(StatDefOf.ComfyTemperatureMin)>-100){var hyp=victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);float severity=hyp==null?0:hyp.Severity;float gain=GlacialRules.HypothermiaGain(severity);if(gain>0){if(hyp==null)hyp=victim.health.AddHediff(HediffDefOf.Hypothermia);hyp.Severity=severity+gain;}}GlacialVfx.Emit(victim,6,start.ToVector3Shifted(),end.ToVector3Shifted(),12,1.3f);c.Register();}
 }
 public sealed class HediffCompProperties_GlacialFrozen:HediffCompProperties {public HediffCompProperties_GlacialFrozen(){compClass=typeof(HediffComp_GlacialFrozen);}}
 public sealed class HediffComp_GlacialFrozen:HediffComp {
  public int pushStart,pushUntil,freezeUntil,immunityUntil,priorStunUntil;public IntVec3 start,end;
  public bool Active{get{return Pawn.Spawned&&!Pawn.Dead&&!Pawn.Downed&&Find.TickManager.TicksGame<freezeUntil;}}
  public Vector3 VisualAt{get{return Vector3.Lerp(start.ToVector3Shifted(),end.ToVector3Shifted(),WaterAbilityRules.StepProgress(Find.TickManager.TicksGame-pushStart,pushUntil-pushStart));}}
  public void Register(){if(Pawn.Spawned){Pawn.Map.GetComponent<MapComponent_GlacialVfx>().Register(Pawn);Pawn.Map.GetComponent<MapComponent_GlacialControl>().Register(Pawn);}}
  public override void Notify_Spawned(){int now=Find.TickManager.TicksGame;if(!Pawn.Spawned)return;Register();if(now<freezeUntil)Pawn.stances.stunner.StunFor(freezeUntil-now,null,false,false,true);}
  public override void CompExposeData(){Scribe_Values.Look(ref priorStunUntil,"EH_glacialPriorStunUntil");Scribe_Values.Look(ref pushDone,"EH_glacialPushDone");Scribe_Values.Look(ref pushStart,"EH_glacialPushStart");Scribe_Values.Look(ref pushUntil,"EH_glacialPushUntil");Scribe_Values.Look(ref freezeUntil,"EH_glacialFrozenUntil");Scribe_Values.Look(ref immunityUntil,"EH_glacialThawImmunityUntil");Scribe_Values.Look(ref start,"EH_glacialPushFrom");Scribe_Values.Look(ref end,"EH_glacialPushTo");}
  int lastAdvance=-1;bool pushDone;
  internal void Cancel(){int now=Find.TickManager.TicksGame;if(Pawn.stances!=null&&freezeUntil>now&&Pawn.stances.stunner.StunTicksLeft<=freezeUntil-now+1){Pawn.stances.stunner.StopStun();if(!Pawn.Dead&&priorStunUntil>now)Pawn.stances.stunner.StunFor(priorStunUntil-now,null,false,false,true);}pushUntil=freezeUntil=now;pushDone=true;}
  internal void Advance(int now){if(lastAdvance==now)return;lastAdvance=now;if(Pawn.Dead||Pawn.Downed||!Pawn.Spawned){Cancel();return;}if(now>=freezeUntil||LongEventHandler.ShouldWaitForEvent||Current.Game==null||!Current.Game.Maps.Contains(Pawn.Map))return;Pawn.pather.StopDead();if(!pushDone&&start!=end){var cell=now>=pushUntil?end:VisualAt.ToIntVec3();if(GlacialFreeze.Safe(Pawn,Pawn.Position,cell)){ThunderstormBlitz.Move(Pawn,cell);if(now>=pushUntil)pushDone=true;}else{end=Pawn.Position;pushDone=true;pushUntil=now;freezeUntil=now+GlacialRules.FreezeTicks;immunityUntil=freezeUntil+GlacialRules.ThawImmunity;}}if(Pawn.stances.stunner.StunTicksLeft<freezeUntil-now)Pawn.stances.stunner.StunFor(freezeUntil-now,null,false,false,true);}
  public override void CompPostTickInterval(ref float adjustment,int delta){int now=Find.TickManager.TicksGame;Advance(now);if(now>=immunityUntil)Pawn.health.RemoveHediff(parent);}
  public override string CompTipStringExtra{get{return Active?"Frozen: "+Math.Max(0,freezeUntil-Find.TickManager.TicksGame)/60f+" seconds; unable to move or attack.":"Briefly immune to refreezing.";}}
 }
 [HarmonyPatch(typeof(Pawn),"DeSpawn")]
 public static class GlacialDespawnCleanup {public static void Prefix(Pawn __instance){var c=GlacialFreeze.On(__instance);if(c!=null)c.Cancel();}}
 // Native health ticking may be batched for incapacitated pawns. Movement/control must still advance every game tick.
 public sealed class MapComponent_GlacialControl:MapComponent {
  readonly List<Pawn> active=new List<Pawn>();public MapComponent_GlacialControl(Map map):base(map){}
  public void Register(Pawn p){if(p!=null&&!active.Contains(p))active.Add(p);}
  public override void FinalizeInit(){foreach(var p in map.mapPawns.AllPawnsSpawned)if(GlacialFreeze.Active(p))Register(p);}
  public override void MapComponentTick(){if(Current.Game==null||!Current.Game.Maps.Contains(map)||LongEventHandler.ShouldWaitForEvent)return;int now=Find.TickManager.TicksGame;for(int i=active.Count-1;i>=0;i--){var p=active[i];var c=GlacialFreeze.On(p);if(c==null||p==null||!p.Spawned||p.Map!=map||p.Dead){if(c!=null)c.Cancel();active.RemoveAt(i);continue;}c.Advance(now);if(!c.Active)active.RemoveAt(i);}}
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class GlacialPushDraw {public static void Postfix(Pawn ___pawn,ref Vector3 __result){var c=GlacialFreeze.On(___pawn);if(c==null||!c.Active||Find.TickManager.TicksGame>=c.pushUntil||c.start==c.end)return;var at=c.VisualAt;at.y=__result.y;__result=at;}}
 [HarmonyPatch(typeof(PawnRenderer),"ParallelGetPreRenderResults")]
 public static class GlacialTintUncached {public static void Prefix(Pawn ___pawn,ref bool disableCache){if(GlacialFreeze.Active(___pawn))disableCache=true;}}
 [HarmonyPatch(typeof(PawnRenderNodeWorker),"GetMaterialPropertyBlock")]
 public static class GlacialFrozenTint {public static void Postfix(PawnRenderNode node,Material material,PawnDrawParms parms,ref MaterialPropertyBlock __result){if(parms.Portrait||parms.Statue||!GlacialFreeze.Active(parms.pawn))return;if(__result==null)__result=new MaterialPropertyBlock();Color original=material!=null&&material.HasProperty(ShaderPropertyIDs.Color)?material.color:Color.white;__result.SetColor(ShaderPropertyIDs.Color,Color.Lerp(original,new Color(.45f,.80f,1,original.a),.75f));}}
}
