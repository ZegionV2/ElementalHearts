using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public static class ConcordRules {
  public const int Window=240,Duration=240,Cooldown=360,PrimerLimit=64,MemberLimit=128,PendingLimit=24,VisualLimit=24;
  public const float Radius=8,Recovery=4,FannedHit=1.15f;
  public static bool Pair(string a,string b,string first,string second){return a==first&&b==second||a==second&&b==first;}
 }
 public struct ConcordContext {public Pawn source,target;}
 public struct ConcordHit {public bool entered;public int priorDepth;public Pawn source,target,partner;public bool fanned;}
 public sealed class ConcordPrimer:IExposable {
  public Pawn target,source;public int until;
  public void ExposeData(){Scribe_References.Look(ref target,"target");Scribe_References.Look(ref source,"source");Scribe_Values.Look(ref until,"until");}
 }
 public sealed class ConcordMember:IExposable {
  public Pawn pawn;public int readyAt;
  public void ExposeData(){Scribe_References.Look(ref pawn,"pawn");Scribe_Values.Look(ref readyAt,"readyAt");}
 }
 public sealed class ConcordPending:IExposable {
  public Pawn first,second,target;public string firstElement,secondElement;public int tick;public bool solar;
  public void ExposeData(){Scribe_References.Look(ref first,"first");Scribe_References.Look(ref second,"second");Scribe_References.Look(ref target,"target");Scribe_Values.Look(ref firstElement,"firstElement");Scribe_Values.Look(ref secondElement,"secondElement");Scribe_Values.Look(ref tick,"tick");Scribe_Values.Look(ref solar,"solar");}
 }
 public static class ElementalConcord {
  [ThreadStatic]public static ConcordContext current;
  [ThreadStatic]public static int damageDepth;
  public static bool Enabled{get{return ElementalHeartsMod.Settings!=null&&ElementalHeartsMod.Settings.enableConcord;}}
  public static bool Active(Pawn p){return p!=null&&p.Spawned&&!p.Dead&&!p.Downed&&p.Awake()&&p.Faction!=null&&ElementalHeartUtility.HasHeart(p)&&HediffComp_ElementalTransform.Active(p)&&!HediffComp_ElementalTransform.Transforming(p)&&!AzureEvolution.Evolving(p);}
  public static bool Allied(Pawn a,Pawn b){return a!=b&&Active(a)&&Active(b)&&a.Map==b.Map&&(a.Faction==b.Faction||a.Faction.RelationKindWith(b.Faction)==FactionRelationKind.Ally&&b.Faction.RelationKindWith(a.Faction)==FactionRelationKind.Ally);}
  public static bool Enemy(Pawn source,Pawn target){return source!=null&&target!=null&&target.Spawned&&!target.Dead&&!target.Downed&&!target.IsPrisoner&&source.Map==target.Map&&target.HostileTo(source);}
  // Non-lethal techniques replace injury before TakeDamage, but still count as elemental control.
  public static void Suppression(Pawn source,Pawn target){
   if(!Enabled||current.source!=source||current.target!=target||damageDepth>1||!Active(source)||!Enemy(source,target))return;
   var map=source.Map.GetComponent<MapComponent_ElementalConcord>();var hit=map.Prepare(source,target);map.Complete(hit);
  }
  public static void Control(Pawn source,Pawn target){if(Enabled&&Active(source)&&Enemy(source,target)){var map=source.Map.GetComponent<MapComponent_ElementalConcord>();map.Complete(map.Prepare(source,target));}}
 }
 // Scope only existing elemental strike methods. Native ordinary attacks, awakening,
 // cosmetics and damage from unrelated mods never create a synchronization primer.
 [HarmonyPatch] public static class ConcordStrikeScope {
  static IEnumerable<MethodBase> TargetMethods(){foreach(var type in new[]{typeof(LightningCombat),typeof(FlameCombat),typeof(WindCombat),typeof(WaterCombat),typeof(EarthCombat),typeof(LunarCombat),typeof(EclipseCombat),typeof(CelestialCombat)})yield return AccessTools.DeclaredMethod(type,"Strike");yield return AccessTools.DeclaredMethod(typeof(WaterDefense),"Counter");}
  static void Prefix(Pawn __0,Pawn __1,out ConcordContext __state){__state=ElementalConcord.current;ElementalConcord.current=ElementalConcord.Enabled?new ConcordContext{source=__0,target=__1}:default(ConcordContext);}
  static Exception Finalizer(Exception __exception,ConcordContext __state){ElementalConcord.current=__state;return __exception;}
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")] public static class ConcordDamage {
  static void Prefix(Thing __instance,ref DamageInfo dinfo,out ConcordHit __state){
   __state=default(ConcordHit);var scope=ElementalConcord.current;
   if(!ElementalConcord.Enabled||scope.source==null)return;
   __state.entered=true;__state.priorDepth=ElementalConcord.damageDepth++;
   if(__state.priorDepth!=0||scope.target!=__instance||dinfo.Instigator!=scope.source||dinfo.Amount<=0||!ElementalConcord.Active(scope.source)||!ElementalConcord.Enemy(scope.source,scope.target))return;
   var hit=scope.source.Map.GetComponent<MapComponent_ElementalConcord>().Prepare(scope.source,scope.target);hit.entered=true;hit.priorDepth=__state.priorDepth;__state=hit;
   if(hit.fanned)dinfo.SetAmount(dinfo.Amount*ConcordRules.FannedHit);
  }
  static void Postfix(DamageWorker.DamageResult __result,ConcordHit __state){if(__state.source!=null&&__result!=null&&__result.totalDamageDealt>0&&__state.source.Spawned)__state.source.Map.GetComponent<MapComponent_ElementalConcord>().Complete(__state);}
  static Exception Finalizer(Exception __exception,ConcordHit __state){if(__state.entered)ElementalConcord.damageDepth=__state.priorDepth;return __exception;}
 }
 public sealed class MapComponent_ElementalConcord:MapComponent {
  List<ConcordPrimer> primers=new List<ConcordPrimer>();List<ConcordMember> members=new List<ConcordMember>();List<ConcordPending> pending=new List<ConcordPending>();
  struct Link {public Pawn a,b;public Color color;public int tick;}
  readonly Link[] links=new Link[ConcordRules.VisualLimit];int cursor;
  readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
  public MapComponent_ElementalConcord(Map map):base(map){}
  int Now{get{return Find.TickManager.TicksGame;}}
  public override void ExposeData(){Scribe_Collections.Look(ref primers,"EH_concordPrimers",LookMode.Deep);Scribe_Collections.Look(ref members,"EH_concordMembers",LookMode.Deep);Scribe_Collections.Look(ref pending,"EH_concordPending",LookMode.Deep);if(Scribe.mode==LoadSaveMode.PostLoadInit){if(primers==null)primers=new List<ConcordPrimer>();if(members==null)members=new List<ConcordMember>();if(pending==null)pending=new List<ConcordPending>();if(primers.Count>ConcordRules.PrimerLimit)primers.RemoveRange(0,primers.Count-ConcordRules.PrimerLimit);if(members.Count>ConcordRules.MemberLimit)members.RemoveRange(ConcordRules.MemberLimit,members.Count-ConcordRules.MemberLimit);if(pending.Count>ConcordRules.PendingLimit)pending.RemoveRange(ConcordRules.PendingLimit,pending.Count-ConcordRules.PendingLimit);}}
  bool Present(Pawn p){return p!=null&&p.Spawned&&p.Map==map&&!p.Dead;}
  void Prune(){int now=Now;primers.RemoveAll(p=>p==null||p.until<=now||!Present(p.source)||!Present(p.target)||p.target.Downed);members.RemoveAll(m=>m==null||m.pawn==null||m.pawn.Destroyed||m.readyAt<=now);}
  public int ReadyAt(Pawn p){foreach(var m in members)if(m.pawn==p)return m.readyAt;return 0;}
  bool Ready(Pawn p){return ReadyAt(p)<=Now;}
  bool Room(Pawn a,Pawn b){int needed=0;bool first=false,second=false;foreach(var m in members){first|=m.pawn==a;second|=m.pawn==b;}if(!first)needed++;if(!second)needed++;return members.Count+needed<=ConcordRules.MemberLimit;}
  bool PairReady(Pawn a,Pawn b,Pawn target){return ElementalConcord.Allied(a,b)&&a.Map==map&&ElementalConcord.Enemy(b,target)&&a.Position.DistanceToSquared(b.Position)<=ConcordRules.Radius*ConcordRules.Radius&&LightningCombat.ClearLine(a.Position,b.Position,map)&&LightningCombat.ClearLine(a.Position,target.Position,map)&&LightningCombat.ClearLine(b.Position,target.Position,map)&&Ready(a)&&Ready(b)&&Room(a,b)&&pending.Count<ConcordRules.PendingLimit;}
  public ConcordHit Prepare(Pawn source,Pawn target){
   var result=new ConcordHit();if(!ElementalConcord.Enabled||!ElementalConcord.Active(source)||source.Map!=map||!ElementalConcord.Enemy(source,target))return result;
   result.source=source;result.target=target;Prune();
   foreach(var primer in primers)if(primer.target==target){if(primer.source!=source&&PairReady(source,primer.source,target)){result.partner=primer.source;result.fanned=ConcordRules.Pair(ElementalTeaching.Element(source),ElementalTeaching.Element(primer.source),"Flame","Wind");}break;}
   return result;
  }
  void SetReady(Pawn pawn){foreach(var m in members)if(m.pawn==pawn){m.readyAt=Now+ConcordRules.Cooldown;return;}members.Add(new ConcordMember{pawn=pawn,readyAt=Now+ConcordRules.Cooldown});}
  void Remember(Pawn source,Pawn target){if(!Present(target)||target.Downed)return;foreach(var p in primers)if(p.target==target){p.source=source;p.until=Now+ConcordRules.Window;return;}if(primers.Count>=ConcordRules.PrimerLimit)primers.RemoveAt(0);primers.Add(new ConcordPrimer{source=source,target=target,until=Now+ConcordRules.Window});}
  public void Complete(ConcordHit hit){
   if(hit.source==null||hit.target==null||!ElementalConcord.Enabled||!ElementalConcord.Active(hit.source)||hit.source.Map!=map)return;
   var a=hit.source;var b=hit.partner;
   // The target may have died to this confirmed hit; the participating allies still synchronize.
   if(b!=null&&ElementalConcord.Allied(a,b)&&Ready(a)&&Ready(b)&&Room(a,b)&&pending.Count<ConcordRules.PendingLimit){
    SetReady(a);SetReady(b);pending.Add(new ConcordPending{first=a,second=b,target=hit.target,firstElement=ElementalTeaching.Element(a),secondElement=ElementalTeaching.Element(b),tick=Now,solar=CelestialAscendant.Active(a)||CelestialAscendant.Active(b)||EclipseSky.Empowered(a)||EclipseSky.Empowered(b)});
   }
   Remember(a,hit.target);
  }
  static void Timed(Pawn pawn,string name){var def=DefDatabase<HediffDef>.GetNamed(name);var h=pawn.health.hediffSet.GetFirstHediffOfDef(def);if(h==null)h=pawn.health.AddHediff(def);var timer=h.TryGetComp<HediffComp_Disappears>();if(timer!=null)timer.ticksToDisappear=ConcordRules.Duration;}
  static void Buff(Pawn pawn,bool solar){
   var weak=DefDatabase<HediffDef>.GetNamed("EH_ElementalConcord");var strong=DefDatabase<HediffDef>.GetNamed("EH_SolarConcord");
   if(solar){var old=pawn.health.hediffSet.GetFirstHediffOfDef(weak);if(old!=null)pawn.health.RemoveHediff(old);Timed(pawn,strong.defName);}
   else Timed(pawn,pawn.health.hediffSet.HasHediff(strong)?strong.defName:weak.defName);
  }
  static void Heal(Pawn pawn){float remaining=ConcordRules.Recovery;var injuries=new List<Hediff_Injury>();foreach(var h in pawn.health.hediffSet.hediffs){var i=h as Hediff_Injury;if(i!=null&&!i.IsPermanent())injuries.Add(i);}injuries.Sort((a,b)=>b.Severity.CompareTo(a.Severity));foreach(var i in injuries){float amount=Math.Min(remaining,i.Severity);if(amount<=0)continue;i.Heal(amount);remaining-=amount;if(remaining<=0)break;}}
  static void Discharge(Pawn source,Pawn target){
   if(!ElementalConcord.Enemy(source,target)||target.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed(ElectricalShock.DefName)))return;
   var resistance=HediffComp_LightningPassives.On(target);float factor=resistance==null?1:Mathf.Clamp01(resistance.Props.resistanceFactor);int duration=Mathf.RoundToInt(60*factor);if(duration<=0)return;
   var shock=target.health.AddHediff(DefDatabase<HediffDef>.GetNamed(ElectricalShock.DefName)).TryGetComp<HediffComp_ElectricalShock>();shock.stunnedUntil=Find.TickManager.TicksGame+duration;shock.recoveryUntil=shock.stunnedUntil+ElectricalShock.RecoveryTicks;target.stances.stunner.StunFor(duration,source);
  }
  void Resolve(ConcordPending p){
   if(p==null||!ElementalConcord.Allied(p.first,p.second)||p.first.Map!=map||Now-p.tick>60)return;
   Buff(p.first,p.solar);Buff(p.second,p.solar);
   bool earth=p.firstElement=="Earth"||p.secondElement=="Earth",water=p.firstElement=="Water"||p.secondElement=="Water";
   if(earth){Timed(p.first,"EH_ConcordWard");Timed(p.second,"EH_ConcordWard");}if(water){Heal(p.first);Heal(p.second);}
   if(ConcordRules.Pair(p.firstElement,p.secondElement,"Water","Lightning"))Discharge(p.firstElement=="Lightning"?p.first:p.second,p.target);
   var color=p.solar?new Color(1,.85f,.35f):water?new Color(.35f,.85f,1):earth?new Color(1,.78f,.35f):ConcordRules.Pair(p.firstElement,p.secondElement,"Flame","Wind")?new Color(1,.55f,.25f):new Color(.78f,.75f,1);
   links[cursor]=new Link{a=p.first,b=p.second,color=color,tick=Now};cursor=(cursor+1)%links.Length;
  }
  public override void MapComponentTick(){if(pending.Count>0){var queue=pending.ToArray();pending.Clear();if(ElementalConcord.Enabled)foreach(var p in queue)Resolve(p);}if(Now%120==0)Prune();}
  public override void MapComponentDraw(){
   if(ElementalHeartsMod.Settings.effectOpacity<=0)return;
   var view=Find.CameraDriver.CurrentViewRect.ExpandedBy(2);int now=Now;
   for(int i=0;i<links.Length;i++){var link=links[i];int age=now-link.tick;if(link.a==null||age<0||age>=36||!Present(link.a)||!Present(link.b))continue;var a=link.a.DrawPos;var b=link.b.DrawPos;if(!view.Contains(a.ToIntVec3())&&!view.Contains(b.ToIntVec3())||a.ToIntVec3().Fogged(map)||b.ToIntVec3().Fogged(map))continue;
    var color=link.color;color.a=.45f*(1-age/36f)*ElementalHeartsMod.Settings.effectOpacity;ConcordVisuals.Draw(a,b,color,age,block);
   }
  }
 }
 // Eight cached ribbon phases and one ring: three draw calls per visible pair,
 // rather than a separate draw for each line/ring segment. No per-frame arrays.
 static class ConcordVisuals {
  static readonly Mesh[] ribbons=new Mesh[8];static Mesh ring;static Material material;
  static Mesh Ribbon(int phase){const int segments=16;var vertices=new Vector3[(segments+1)*2];var triangles=new int[segments*6];for(int i=0;i<=segments;i++){float t=i/(float)segments,z=Mathf.Sin(t*Mathf.PI*2+phase*Mathf.PI/4)*Mathf.Sin(t*Mathf.PI)*.16f;vertices[i*2]=new Vector3(t-.5f,0,z-.023f);vertices[i*2+1]=new Vector3(t-.5f,0,z+.023f);if(i<segments){int k=i*2,j=i*6;triangles[j]=k;triangles[j+1]=k+1;triangles[j+2]=k+2;triangles[j+3]=k+1;triangles[j+4]=k+3;triangles[j+5]=k+2;}}var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
  static Mesh Ring(){const int segments=32;var vertices=new Vector3[(segments+1)*2];var triangles=new int[segments*6];for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;vertices[i*2]=new Vector3(Mathf.Cos(a)*.47f,0,Mathf.Sin(a)*.47f);vertices[i*2+1]=new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f);if(i<segments){int k=i*2,j=i*6;triangles[j]=k;triangles[j+1]=k+2;triangles[j+2]=k+1;triangles[j+3]=k+1;triangles[j+4]=k+2;triangles[j+5]=k+3;}}var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
  public static void Draw(Vector3 a,Vector3 b,Color color,int age,MaterialPropertyBlock block){
   if(material==null)material=MaterialPool.MatFrom(new MaterialRequest(BaseContent.WhiteTex,ShaderDatabase.Transparent));if(ring==null){ring=Ring();for(int i=0;i<ribbons.Length;i++)ribbons[i]=Ribbon(i);}
   block.SetColor(ShaderPropertyIDs.Color,color);a.y=b.y=AltitudeLayer.MoteOverhead.AltitudeFor();var delta=b-a;
   if(delta.sqrMagnitude>.0001f)Graphics.DrawMesh(ribbons[(age/3)%8],Matrix4x4.TRS((a+b)*.5f,Quaternion.AngleAxis(delta.AngleFlat()-90,Vector3.up),new Vector3(delta.magnitude,1,1)),material,0,null,0,block);
   float size=1.1f+age*.024f;a.y=b.y=AltitudeLayer.MoteLow.AltitudeFor();Graphics.DrawMesh(ring,Matrix4x4.TRS(a,Quaternion.identity,new Vector3(size,1,size)),material,0,null,0,block);Graphics.DrawMesh(ring,Matrix4x4.TRS(b,Quaternion.identity,new Vector3(size,1,size)),material,0,null,0,block);
  }
 }
}
