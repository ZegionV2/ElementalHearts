using System;using System.Collections.Generic;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class AdamantRules {
  public static readonly string[] Names={"Adamant Severance","Mountain King's Advance","Sovereign Quake","Worldspine Eruption","Adamant Bulwark"};
  public static readonly int[] Cooldowns={300,420,900,1500,2100};
  public static readonly float[] Ranges={10,8,5,10,3};
  public static readonly string[] Details={
   "A ten-cell fault cut. One powerful blunt impact per eligible enemy, stopped by walls; builds one Adamant Resolve per successful hostile cast.",
   "Charge along an eight-cell navigation envelope to a clear melee flank with smooth checked steps, then deliver one crushing impact. One bounded route correction; no wall, corner or pawn crossing.",
   "A ground stab releases a five-cell earthquake. Each eligible enemy is struck once, staggered for 1.5 seconds and pushed up to three safe cells. Automatically used against two nearby enemies.",
   "Mark a fixed ground position within ten cells and stab down. After 0.7 seconds, a 3.5-cell crown of adamant spires strikes each enemy once, staggers and pushes survivors up to four clear cells. Spend up to three Resolve for +15% damage each. Spires are brief visuals.",
   "When at least two enemies pressure you within three cells, brace for 0.3 seconds and raise a three-second Bulwark: 40% less remaining personal damage and 15% allied protection within three clear cells. Strongest protection applies once. No reflected damage. Explicit movement, reversion, incapacitation or external animation control ends it."};
  public static List<int> Deadlines(List<int> saved,bool evolved){int size=evolved?5:4;if(saved!=null&&saved.Count==size)return saved;var result=new List<int>(size);for(int i=0;i<size;i++)result.Add(saved!=null&&i<saved.Count?saved[i]:0);return result;}
  public static bool RepathAllowed(int attempts,int spent,int remaining,int distance){return attempts<1&&spent+remaining<=16&&remaining>0&&distance<=64;}
  public static bool GuardOrder(Pawn p,IntVec3 origin){if(p==null||p.Position!=origin||HediffComp_ElementalTransform.Transforming(p)||AzureEvolution.Evolving(p)||ElementalAnimationBridge.Controlled(p))return false;var job=p.CurJobDef;return job==null||job==JobDefOf.AttackMelee||job.defName.StartsWith("Wait")||job.defName=="EH_EarthTechnique";}
 }
 public static class AdamantEvolution {
  public const string DefName="EH_AdamantAscendant";
  public static bool Active(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName==DefName)return true;return false;}
  public static bool Earth(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName=="EH_EarthHeart")return true;return false;}
  public static HediffComp_AdamantMastery Mastery(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_AdamantMastery>();if(c!=null)return c;}return null;}
  public static string Problem(Pawn p){var c=Mastery(p);if(c==null||!Earth(p))return "Requires normal Earth ascension.";if(!AzureRules.Ready(c.kills,c.parent.ageTicks))return "Requires 150 unique hostile non-animal mastery kills and ten Earth Ascendant days.";if(!ElementalTeaching.Ready(p)||ElementalTeachingSession.Busy(p)||EarthCombat.Busy(p)||ElementalAnimationBridge.Controlled(p)||HediffComp_ElementalTransform.Transforming(p)||AzureEvolution.Evolving(p))return "Finish the current action, lesson or transformation first.";return null;}
  public static bool Upgrade(Pawn p){if(p==null||p.health==null)return false;HediffWithComps root=null;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName=="EH_EarthHeart")root=h as HediffWithComps;if(root==null)return Active(p);var c=root.TryGetComp<HediffComp_EarthAbilities>();if(c!=null)c.EndGuard();AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));c=root.TryGetComp<HediffComp_EarthAbilities>();c.EnsureSlots();p.health.Notify_HediffChanged(root);var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();if(p.Spawned)foreach(var comp in root.comps)comp.Notify_Spawned();return true;}
  public static bool Complete(Pawn p){var c=Mastery(p);if(c==null||!Earth(p)||!AzureRules.Ready(c.kills,c.parent.ageTicks)||!ElementalTeaching.Ready(p))return false;if(!Upgrade(p))return false;p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Ability(p,ElementalSound.Impact);Messages.Message(p.LabelShort+" has evolved into an Adamant Ascendant!",p,MessageTypeDefOf.PositiveEvent,false);return true;}
 }
 public sealed class HediffCompProperties_AdamantMastery:HediffCompProperties {public HediffCompProperties_AdamantMastery(){compClass=typeof(HediffComp_AdamantMastery);}}
 public sealed class HediffComp_AdamantMastery:HediffComp {
  public int kills;HashSet<int> victims=new HashSet<int>();
  public bool RecordKill(int id,bool eligible){if(AdamantEvolution.Active(Pawn)||!eligible||!victims.Add(id))return false;kills++;return true;}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){if(victim!=null)RecordKill(victim.thingIDNumber,AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn));}
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_adamantMasteryKills");Scribe_Collections.Look(ref victims,"EH_adamantMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned&&Pawn.CurJobDef!=null&&Pawn.CurJobDef.defName=="EH_EvolveAdamant")EvolutionCinematic.Track(Pawn);}
  public override string CompTipStringExtra{get{return AdamantEvolution.Active(Pawn)?"Adamant evolution complete.":"Adamant mastery: "+kills+" / 150 unique hostile non-animal kills; "+(parent.ageTicks/60000f).ToString("0.0")+" / 10 Earth Ascendant days.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn)||!AdamantEvolution.Earth(Pawn))yield break;var c=new Command_Action{defaultLabel="Evolve: Adamant",defaultDesc=CompTipStringExtra+"\nA free, interruptible 4.5-second coronation grants five Adamant techniques, evolved defense and recovery, helmetless regal armor, Worldbreaker and an amber halo. Saved controls, cooldowns and personal clothing carry over.",icon=AdamantVfx.Icon(5),groupable=false,action=()=>{if(AdamantEvolution.Problem(Pawn)==null)Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_EvolveAdamant"),Pawn),JobTag.Misc);}};var reason=AdamantEvolution.Problem(Pawn);if(reason!=null)c.Disable(reason);yield return c;if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>AdamantEvolution.Earth(Pawn)&&AdamantEvolution.Mastery(Pawn)==this,defaultLabel="DEV: Evolution-Ready",defaultDesc="God Mode only. Fulfill Adamant milestones; press Evolve: Adamant to complete the ceremony.",icon=AdamantVfx.Icon(5),groupable=false,action=()=>{if(DebugSettings.godMode){kills=Math.Max(150,kills);parent.ageTicks=Math.Max(600000,parent.ageTicks);}}};}
 }
 [StaticConstructorOnStartup] public static class AdamantVfx {
  static readonly string[] sequences={"AdamantSeverance","MountainKingsAdvance","SovereignQuake","WorldspineEruption","BulwarkFormation","BulwarkFade","Halo"};
  static readonly string[] icons={"AdamantSeverance","MountainKingsAdvance","SovereignQuake","WorldspineEruption","AdamantBulwark","EvolveAdamant"};
  static readonly Material[,] materials=new Material[7,8];static readonly Texture2D[] commands=new Texture2D[6];
  public static Material Frame(int sequence,int frame){frame=Math.Max(0,Math.Min(sequence==6?5:7,frame));return materials[sequence,frame]??(materials[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Adamant/Frames/"+sequences[sequence]+"_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  public static Texture2D Icon(int i){return commands[i]??(commands[i]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Adamant/"+icons[i]));}
  public static int MapSequence(int earth){return earth==7||earth==3?3:earth==8||earth==2?2:earth==1?1:0;}
 }
}
