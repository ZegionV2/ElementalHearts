using System;using HarmonyLib;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class CycloneControlRules {
  public const int MaxActive=64,MaxVictims=6,CandidateLimit=24,SearchThings=384,FormationTicks=24,HoldTicks=120,PushTicks=12,RecoveryTicks=60,Cooldown=2400;
  public static int Duration(int ticks){return Math.Max(1,Math.Min(HoldTicks,ticks));}
  public static bool Trigger(int enemies,bool ready){return enemies>=2&&ready;}
  public static float Lift(int now,int start,int until){if(now>=until||until<=start)return 0;return Mathf.Clamp01((now-start)/8f)*Mathf.Clamp01((until-now)/10f)*(.7f+.06f*Mathf.Sin((now-start)*.16f));}
 }
 public static class CycloneAirbound {
  public static HediffComp_CycloneAirbound On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_CycloneAirbound>();if(c!=null)return c;}return null;}
  public static bool Active(Pawn p){var c=On(p);return c!=null&&c.Controlling;}
  public static bool Apply(Pawn source,Pawn target,int ticks,int cast){
   if(EarthCombat.Anchored(target)||!WindCombat.AliveTarget(source,target)||GlacialFreeze.Active(target)||ElementalAnimationBridge.ExternallyControlled(target))return false;
   int now=Find.TickManager.TicksGame;var c=On(target);if(c!=null&&(c.Controlling||now<c.immunityUntil))return false;
   if(!target.Map.GetComponent<MapComponent_CycloneControl>().Register(target))return false;
   if(c==null)c=target.health.AddHediff(DefDatabase<HediffDef>.GetNamed("EH_CycloneAirbound")).TryGetComp<HediffComp_CycloneAirbound>();
   c.source=source;c.cast=cast;c.holdStart=now;c.holdUntil=now+CycloneControlRules.Duration(ticks);c.pushStart=c.pushUntil=now;c.immunityUntil=c.holdUntil+CycloneControlRules.RecoveryTicks;c.from=c.to=target.Position;c.priorStunUntil=now+target.stances.stunner.StunTicksLeft;c.ownedStunUntil=c.holdUntil;
   target.pather.StopDead();target.stances.CancelBusyStanceHard();target.stances.stunner.StunFor(c.holdUntil-now,source,false,false,true);CycloneVfx.TrackAirbound(target,c.holdUntil);return true;
  }
 }
 public sealed class HediffCompProperties_CycloneAirbound:HediffCompProperties {public HediffCompProperties_CycloneAirbound(){compClass=typeof(HediffComp_CycloneAirbound);}}
 public sealed class HediffComp_CycloneAirbound:HediffComp {
  public Pawn source;public int cast,holdStart,holdUntil,pushStart,pushUntil,immunityUntil,priorStunUntil,ownedStunUntil;public IntVec3 from,to;int lastAdvance=-1;
  public bool Controlling{get{return Pawn.Spawned&&!Pawn.Dead&&!Pawn.Downed&&Find.TickManager.TicksGame<Math.Max(holdUntil,pushUntil);}}
  public bool Held{get{return Controlling&&Find.TickManager.TicksGame<holdUntil;}}
  public bool Owned(Pawn owner,int token){return source==owner&&cast==token;}
  public override void CompExposeData(){Scribe_References.Look(ref source,"EH_cycloneSource");Scribe_Values.Look(ref cast,"EH_cycloneOwnerCast");Scribe_Values.Look(ref holdStart,"EH_airboundStart");Scribe_Values.Look(ref holdUntil,"EH_airboundUntil");Scribe_Values.Look(ref pushStart,"EH_cyclonePushStart");Scribe_Values.Look(ref pushUntil,"EH_cyclonePushUntil");Scribe_Values.Look(ref immunityUntil,"EH_airboundImmunityUntil");Scribe_Values.Look(ref priorStunUntil,"EH_airboundPriorStunUntil");Scribe_Values.Look(ref ownedStunUntil,"EH_airboundOwnedStunUntil");Scribe_Values.Look(ref from,"EH_cyclonePushFrom");Scribe_Values.Look(ref to,"EH_cyclonePushTo");}
  public override void Notify_Spawned(){if(!Controlling)return;if(!Pawn.Map.GetComponent<MapComponent_CycloneControl>().Register(Pawn)){Cancel();return;}Pawn.stances.stunner.StunFor(Math.Max(holdUntil,pushUntil)-Find.TickManager.TicksGame,source,false,false,true);if(Held)CycloneVfx.TrackAirbound(Pawn,holdUntil);}
  void StopOwnedStun(){int now=Find.TickManager.TicksGame;if(Pawn.stances!=null&&Pawn.stances.stunner.StunTicksLeft<=Math.Max(0,ownedStunUntil-now)+1){Pawn.stances.stunner.StopStun();if(!Pawn.Dead&&priorStunUntil>now)Pawn.stances.stunner.StunFor(priorStunUntil-now,null,false,false,true);}}
  public void Cancel(){int now=Find.TickManager.TicksGame;StopOwnedStun();holdUntil=pushUntil=now;source=null;cast=0;immunityUntil=Math.Max(immunityUntil,now+CycloneControlRules.RecoveryTicks);if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_CycloneVfx>().UntrackAirbound(Pawn);}
  public void Release(bool push){int now=Find.TickManager.TicksGame;StopOwnedStun();holdUntil=now;if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_CycloneVfx>().UntrackAirbound(Pawn);if(!push||Pawn.Dead||Pawn.Downed||!Pawn.Spawned||source==null||!source.Spawned||source.Map!=Pawn.Map){pushUntil=now;immunityUntil=Math.Max(immunityUntil,now+CycloneControlRules.RecoveryTicks);return;}
   from=to=Pawn.Position;var direction=(Pawn.Position-source.Position).ToVector3().normalized;for(int i=1;i<=3;i++){var cell=(from.ToVector3Shifted()+direction*i).ToIntVec3();if(cell==to)continue;if(!GlacialFreeze.Safe(Pawn,to,cell))break;to=cell;}
   pushStart=now;pushUntil=now+(to==from?0:CycloneControlRules.PushTicks);if(pushUntil>now&&!Pawn.Map.GetComponent<MapComponent_CycloneControl>().Register(Pawn))pushUntil=now;ownedStunUntil=pushUntil;immunityUntil=pushUntil+CycloneControlRules.RecoveryTicks;if(pushUntil>now)Pawn.stances.stunner.StunFor(pushUntil-now,source,false,false,true);
  }
  internal void Advance(int now){if(lastAdvance==now)return;lastAdvance=now;
   if(Pawn.Dead||Pawn.Downed||!Pawn.Spawned||source==null||source.Dead||source.Downed||!source.Spawned||source.Map!=Pawn.Map||GlacialFreeze.Active(Pawn)||ElementalAnimationBridge.ExternallyControlled(Pawn)||cast!=0&&(!SovereignCyclone.Busy(source)||source.CurJob.loadID!=cast)){Cancel();return;}
   if(now>=Math.Max(holdUntil,pushUntil)){if(pushUntil>pushStart&&Pawn.Position!=to&&GlacialFreeze.Safe(Pawn,Pawn.Position,to))ThunderstormBlitz.Move(Pawn,to);StopOwnedStun();if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_CycloneVfx>().UntrackAirbound(Pawn);return;}
   Pawn.pather.StopDead();if(now<pushUntil&&now>=holdUntil){var at=Vector3.Lerp(from.ToVector3Shifted(),to.ToVector3Shifted(),WaterAbilityRules.StepProgress(now-pushStart,pushUntil-pushStart)).ToIntVec3();if(GlacialFreeze.Safe(Pawn,Pawn.Position,at))ThunderstormBlitz.Move(Pawn,at);else{to=Pawn.Position;pushUntil=now;StopOwnedStun();}}
   if(now>=holdUntil&&pushUntil>pushStart&&now>=pushUntil&&Pawn.Position!=to&&GlacialFreeze.Safe(Pawn,Pawn.Position,to))ThunderstormBlitz.Move(Pawn,to);
   if(now>=Math.Max(holdUntil,pushUntil)){StopOwnedStun();if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_CycloneVfx>().UntrackAirbound(Pawn);return;}
   int left=Math.Max(holdUntil,pushUntil)-now;if(Pawn.stances.stunner.StunTicksLeft<left)Pawn.stances.stunner.StunFor(left,source,false,false,true);
  }
  public override void CompPostTickInterval(ref float adjustment,int delta){int now=Find.TickManager.TicksGame;if(Controlling)Advance(now);if(now>=immunityUntil&&!Controlling)Pawn.health.RemoveHediff(parent);}
  public override void CompPostPostRemoved(){Cancel();}
  public override string CompTipStringExtra{get{return Held?"Held aloft; movement and attacks suspended briefly.":Controlling?"Being pushed safely by sovereign wind.":"Briefly resistant to another airbound capture.";}}
 }
 public sealed class MapComponent_CycloneControl:MapComponent {
  readonly Pawn[] active=new Pawn[CycloneControlRules.MaxActive];
  public MapComponent_CycloneControl(Map map):base(map){}
  public bool Register(Pawn p){int free=-1;for(int i=0;i<active.Length;i++){if(active[i]==p)return true;if(active[i]==null&&free<0)free=i;}if(free<0)return false;active[free]=p;return true;}
  public override void FinalizeInit(){foreach(var p in map.mapPawns.AllPawnsSpawned)if(CycloneAirbound.Active(p)){if(!Register(p))CycloneAirbound.On(p).Cancel();}}
  public override void MapComponentTick(){if(Current.Game==null||!Current.Game.Maps.Contains(map)||LongEventHandler.ShouldWaitForEvent)return;int now=Find.TickManager.TicksGame;for(int i=0;i<active.Length;i++){var p=active[i];if(p==null)continue;var c=CycloneAirbound.On(p);if(c==null){active[i]=null;continue;}c.Advance(now);if(!c.Controlling)active[i]=null;}}
 }
 [HarmonyPatch(typeof(Pawn),"DeSpawn")]
 public static class CycloneAirboundDespawn {public static void Prefix(Pawn __instance){var c=CycloneAirbound.On(__instance);if(c!=null)c.Cancel();}}
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class CycloneAirboundDraw {public static void Postfix(Pawn ___pawn,ref Vector3 __result){var c=CycloneAirbound.On(___pawn);if(c==null||!c.Controlling)return;int now=Find.TickManager.TicksGame;if(c.Held)__result.z+=CycloneControlRules.Lift(now,c.holdStart,c.holdUntil);else if(now<c.pushUntil){var at=Vector3.Lerp(c.from.ToVector3Shifted(),c.to.ToVector3Shifted(),WaterAbilityRules.StepProgress(now-c.pushStart,c.pushUntil-c.pushStart));at.y=__result.y;__result=at;}}}
}
