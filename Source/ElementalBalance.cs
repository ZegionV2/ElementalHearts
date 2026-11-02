using HarmonyLib;
using Verse;
namespace ElementalHearts {
 public static class ElementalBalance {
  public const float DamageMultiplier=.8f;
  public static bool Affected(string damage,bool meleeWeapon,bool rangedWeapon){
   if(damage=="EH_AwakeningShockwave"||damage=="EH_WaterDamage"||damage=="EH_Lightning"||damage=="EH_Flame"||damage=="EH_AzureFlameDamage"||damage=="EH_ThunderstormDamage")return true;
   if(rangedWeapon)return false;
   return meleeWeapon||damage=="Cut"||damage=="Stab"||damage=="Blunt"||damage=="EH_Lightning"||damage=="EH_Flame"||damage=="EH_AzureFlameDamage"||damage=="EH_ThunderstormDamage";
  }
 }
 // Adjust final hit packets once: chains, weapon-derived skills and ordinary melee share the same modest reduction.
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class ElementalDamageBalance {
  public static void Prefix(ref DamageInfo dinfo){
   Pawn source=dinfo.Instigator as Pawn;if(source==null||dinfo.Def==null||dinfo.Amount<=0)return;
   if(!ElementalTeaching.Trained(source)&&!ElementalHeartUtility.HasHeart(source))return;
   var weapon=dinfo.Weapon;
   if(ElementalBalance.Affected(dinfo.Def.defName,weapon!=null&&weapon.IsMeleeWeapon,weapon!=null&&weapon.IsRangedWeapon))dinfo.SetAmount(dinfo.Amount*ElementalBalance.DamageMultiplier);
  }
 }
}
