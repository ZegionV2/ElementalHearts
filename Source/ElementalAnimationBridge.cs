using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 // Optional, cached reflection keeps the base mod loadable without Melee Animation.
 public static class ElementalAnimationBridge {
  static bool initialized,failed;
  static Type idleType;
  static MethodInfo animator,dodge,swing,clear,seek,shouldAnimate;
  static PropertyInfo current,destroyed,duration;
  static FieldInfo snapshots,root,timeScale;
  static MemberInfo matrix;
  static void Init(){if(initialized)return;initialized=true;
   Type renderer=AccessTools.TypeByName("AM.AnimRenderer");idleType=AccessTools.TypeByName("AM.Idle.IdleControllerComp");
   if(renderer==null||idleType==null)return;
   animator=AccessTools.Method(renderer,"TryGetAnimator",new[]{typeof(Pawn)});
   shouldAnimate=AccessTools.Method(idleType,"ShouldBeActive");dodge=AccessTools.Method(idleType,"OnMeleeDodge");swing=AccessTools.Method(idleType,"NotifyPawnDidMeleeAttack");clear=AccessTools.Method(idleType,"ClearAnimation");
   current=AccessTools.Property(idleType,"CurrentAnimation");destroyed=AccessTools.Property(renderer,"IsDestroyed");
   snapshots=AccessTools.Field(renderer,"snapshots");root=AccessTools.Field(renderer,"RootTransform");timeScale=AccessTools.Field(renderer,"TimeScale");seek=AccessTools.Method(renderer,"Seek");duration=AccessTools.Property(renderer,"Duration");
  }
  static void Error(Exception e){if(!failed)Log.Warning("[Elemental Hearts] Melee Animation bridge disabled; using fallback: "+e.GetBaseException().Message);failed=true;}
  public static bool Controlled(Pawn p){return CelestialAscendant.Busy(p)||CycloneAirbound.Active(p)||(SovereignCyclone.Busy(p)&&!SovereignCyclone.Impacting(p))||WaterTechnique.Busy(p)||TempestDragon.Busy(p)||AzureReversal.Busy(p)||(ThunderstormBlitz.Busy(p)&&!ThunderstormBlitz.Impacting(p))||ExternallyControlled(p);}
  public static bool ExternallyControlled(Pawn p){Init();if(failed||animator==null||p==null)return false;try{return animator.Invoke(null,new object[]{p})!=null;}catch(Exception e){Error(e);return false;}}
  static object Idle(Pawn p){if(idleType==null||p==null)return null;foreach(var c in p.AllComps)if(idleType.IsInstanceOfType(c))return c;return null;}
  public static void Deflect(Pawn p,Pawn attacker){Init();if(failed||dodge==null)return;try{var c=Idle(p);if(c!=null)dodge.Invoke(c,new object[]{attacker});}catch(Exception e){Error(e);}}
  public static object StartSwing(Pawn p,Pawn target){Init();if(failed||swing==null||snapshots==null||root==null||Controlled(p))return null;
   try{var c=Idle(p);if(c==null||shouldAnimate==null||!(bool)shouldAnimate.Invoke(c,new object[]{null,false}))return null;var before=current.GetValue(c,null);swing.Invoke(c,new object[]{target,null});var a=current.GetValue(c,null);if(a==null||ReferenceEquals(a,before))return null;timeScale.SetValue(a,0f);return a;}catch(Exception e){Error(e);return null;}}
  public static void StopSwing(Pawn p,object expected){if(expected==null)return;try{var c=Idle(p);if(c!=null&&ReferenceEquals(current.GetValue(c,null),expected))clear.Invoke(c,null);}catch(Exception e){Error(e);}}
    public static bool Advance(object animation,int ticks){if(animation==null||failed)return false;try{float end=(float)duration.GetValue(animation,null);float at=Math.Min(ticks/60f,Math.Max(0f,end-.001f));seek.Invoke(animation,new object[]{(float?)at,0f,null,false});return ticks/60f<end;}catch(Exception e){Error(e);return false;}}  static object Read(MemberInfo m,object o){var f=m as FieldInfo;return f!=null?f.GetValue(o):((PropertyInfo)m).GetValue(o,null);}
  public static bool SwingAlive(Pawn p,object animation){if(animation==null||failed)return false;try{var c=Idle(p);return c!=null&&ReferenceEquals(current.GetValue(c,null),animation)&&!(bool)destroyed.GetValue(animation,null);}catch(Exception e){Error(e);return false;}}
  public static bool Crossed(Pawn p,Pawn target,object animation,ref float previous,ref bool sampled){
   if(!SwingAlive(p,animation))return false;
   try{
    var array=(Array)snapshots.GetValue(animation);if(array==null)return false;
    // Find the weapon snapshot by its associated part name; no dependence on array ordering.
    foreach(object snapshot in array){if(snapshot==null)continue;var t=snapshot.GetType();var partMember=(MemberInfo)AccessTools.Field(t,"Part")??AccessTools.Property(t,"Part");if(partMember==null)continue;
     object part=Read(partMember,snapshot);if(part==null)continue;var nameMember=(MemberInfo)AccessTools.Field(part.GetType(),"Name")??AccessTools.Property(part.GetType(),"Name");if(nameMember==null||(string)Read(nameMember,part)!="ItemA")continue;
     if(matrix==null)matrix=(MemberInfo)AccessTools.Property(t,"WorldMatrixPreserveFlip")??AccessTools.Field(t,"WorldMatrixPreserveFlip");if(matrix==null)return false;
     Matrix4x4 world=(Matrix4x4)root.GetValue(animation)*(Matrix4x4)Read(matrix,snapshot);
     Vector3 blade=world.MultiplyVector(Vector3.right),toward=target.DrawPos-p.DrawPos;
     float delta=Mathf.DeltaAngle(Mathf.Atan2(blade.z,blade.x)*Mathf.Rad2Deg,Mathf.Atan2(toward.z,toward.x)*Mathf.Rad2Deg);
     bool hit=WaterDefenseRules.Crossed(previous,delta,sampled);previous=delta;sampled=true;return hit;
    }
   }catch(Exception e){Error(e);}return false;
  }
 }
}
