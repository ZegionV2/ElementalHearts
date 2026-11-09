using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace ElementalHearts {
 public static class CourtRelicEvents {
  public static bool Ready(Map map){return CourtStories.Enabled&&ElementalHeartsMod.Settings.courtRaids&&CourtStories.Enemy!=null&&CourtStories.State!=null&&map!=null&&map.IsPlayerHome&&CourtStories.State.Pending<WorldComponent_CourtStories.OrderLimit&&!CourtStories.State.HasOrder(map)&&!Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_CourtRelic>().Any(l=>!l.Resolved&&l.map==map);}
 }
 public sealed class IncidentWorker_CourtCrucible:IncidentWorker {
  protected override bool CanFireNowSub(IncidentParms p){var map=p.target as Map;IntVec3 cell;return !float.IsNaN(p.points)&&!float.IsInfinity(p.points)&&CourtRelicEvents.Ready(map)&&Find.TickManager.TicksGame>=30*60000&&Find.TickManager.TicksGame-CourtStories.State.LastCrucible>=20*60000&&CourtStories.Entry(map,out cell);}
  protected override bool TryExecuteWorker(IncidentParms p){if(!CanFireNowSub(p))return false;int element=Rand.Range(0,5);var letter=(ChoiceLetter_CourtRelic)LetterMaker.MakeLetter("The severed crucible","A damaged Court supply crucible lies near your perimeter. Its seal contains twelve "+SeveredCourt.Elements[element].ToLower()+" essences and one Herald crucible sigil.\n\nSecure it and the materials can be condensed into elemental serum at a drug lab after Fabrication. Its tracking seal will summon an ordinary Court recovery force in one day (up to 3,600 points; no Herald). Ignore it and no pursuit is scheduled.\n\nYou have one day to decide.",DefDatabase<LetterDef>.GetNamed("EH_CourtRelicOffer"));letter.map=(Map)p.target;letter.faction=CourtStories.Enemy;letter.element=element;letter.crucible=true;letter.points=Math.Max(1200,Math.Min(3600,p.points));letter.StartTimeout(60000);Find.LetterStack.ReceiveLetter(letter);CourtStories.State.MarkCrucible();return true;}
 }
 public sealed class IncidentWorker_CourtUltimatum:IncidentWorker {
  protected override bool CanFireNowSub(IncidentParms p){return CourtRelicEvents.Ready(p.target as Map)&&CourtHeralds.Eligible(p.points,Find.TickManager.TicksGame)&&Find.TickManager.TicksGame-CourtStories.State.LastUltimatum>=30*60000&&(!CourtStrongholds.CampaignSeeded||CourtStrongholds.Bases().Any(b=>!b.HasMap));}
  protected override bool TryExecuteWorker(IncidentParms p){if(!CanFireNowSub(p))return false;int element;Settlement origin;if(!CourtStrongholds.TryBaseCommander(out element,out origin))return false;var map=(Map)p.target;var order=CourtStories.State.QueueOffer(map,CourtStories.Enemy,Math.Min(10000,p.points),true,element,120000,origin,"ultimatum");if(order==null)return false;
   var letter=(ChoiceLetter_CourtRelic)LetterMaker.MakeLetter("The Herald's ultimatum",CourtHeralds.Titles[element]+" demands six "+SeveredCourt.Elements[element].ToLower()+" essences and 600 silver as tribute. A Herald-led force is already marching"+(origin==null?".":" from "+origin.Label+".")+"\n\nPay from visible, unforbidden supplies on this map within two days to cancel this expedition, or defy the demand and face the announced assault. Payment cancels this force only; the Court remains hostile. Attacking or clearing its origin also recalls the expedition.",DefDatabase<LetterDef>.GetNamed("EH_CourtRelicOffer"));letter.map=map;letter.faction=CourtStories.Enemy;letter.element=element;letter.orderId=order.id;letter.StartTimeout(120000);Find.LetterStack.ReceiveLetter(letter);CourtStories.State.MarkUltimatum();return true;}
 }
 public sealed class ChoiceLetter_CourtRelic:ChoiceLetter {
  public Map map;public Faction faction;public int element,orderId;public float points;public bool crucible;bool resolved;
  public bool Resolved{get{return resolved;}}
  public override void ExposeData(){base.ExposeData();Scribe_References.Look(ref map,"EH_relicMap");Scribe_References.Look(ref faction,"EH_relicFaction");Scribe_Values.Look(ref element,"EH_relicElement");Scribe_Values.Look(ref orderId,"EH_relicOrderId");Scribe_Values.Look(ref points,"EH_relicPoints");Scribe_Values.Look(ref crucible,"EH_relicCrucible");Scribe_Values.Look(ref resolved,"EH_relicResolved");}
  bool Live{get{return !resolved&&map!=null&&Current.Game.Maps.Contains(map)&&faction!=null&&!faction.defeated&&faction.HostileTo(Faction.OfPlayer)&&CourtStories.Enabled&&ElementalHeartsMod.Settings.courtRaids&&(crucible||ElementalHeartsMod.Settings.courtHeralds);}}
  List<Thing> Supply(string name){var def=DefDatabase<ThingDef>.GetNamed(name);return map.listerThings.ThingsOfDef(def).Where(t=>t.Spawned&&!t.Position.Fogged(map)&&!t.IsForbidden(Faction.OfPlayer)).ToList();}
  bool Have(string name,int amount){return Supply(name).Sum(t=>t.stackCount)>=amount;}
  void Consume(string name,int amount){foreach(var t in Supply(name)){int n=Math.Min(amount,t.stackCount);t.SplitOff(n).Destroy();amount-=n;if(amount<=0)break;}}
  public bool AcceptCrucible(){IntVec3 cell;if(!crucible||!Live||CourtStories.State.HasOrder(map)||!CourtStories.Entry(map,out cell))return false;var order=CourtStories.State.QueueOffer(map,faction,points,false,element,60000,null,"crucible");if(order==null)return false;resolved=true;CourtSpoils.Drop(map,cell,"EH_Essence"+SeveredCourt.Elements[element],12);CourtSpoils.Drop(map,cell,"EH_CourtHeraldSigil",1);Find.LetterStack.ReceiveLetter("Crucible secured","The materials lie near the marked perimeter location. An ordinary Court recovery force is expected here in one day. The pursuit has no Herald.",LetterDefOf.ThreatSmall,new TargetInfo(cell,map));Find.LetterStack.RemoveLetter(this);return true;}
  public bool PayTribute(){if(crucible||!Live||CourtStories.State.FindOrder(orderId)==null||!Have("Silver",600)||!Have("EH_Essence"+SeveredCourt.Elements[element],6))return false;if(!CourtStories.State.CancelOrder(orderId))return false;resolved=true;Consume("Silver",600);Consume("EH_Essence"+SeveredCourt.Elements[element],6);Find.LetterStack.ReceiveLetter("Court expedition recalled","The tribute has been surrendered. This Herald expedition has been recalled; the Court remains hostile.",LetterDefOf.NeutralEvent,map.Parent);Find.LetterStack.RemoveLetter(this);return true;}
  public void Refuse(){if(resolved)return;resolved=true;Find.LetterStack.RemoveLetter(this);}
  public override IEnumerable<DiaOption> Choices{get{
   var accept=new DiaOption(crucible?"Secure the crucible":"Pay 6 essence and 600 silver"){action=()=>{if(crucible)AcceptCrucible();else PayTribute();},resolveTree=true};
   if(!Live)accept.Disable("This offer is no longer available.");else if(crucible&&CourtStories.State.HasOrder(map))accept.Disable("Resolve the pending Court encounter first.");else if(!crucible&&(CourtStories.State.FindOrder(orderId)==null||!Have("Silver",600)||!Have("EH_Essence"+SeveredCourt.Elements[element],6)))accept.Disable("The expedition must still be pending and the required visible, unforbidden supplies must be on this map.");
   yield return accept;yield return new DiaOption(crucible?"Leave it alone":"Defy the Herald"){action=Refuse,resolveTree=true};yield return Option_Postpone;
  }}
  public override void Removed(){resolved=true;base.Removed();}
 }
}
