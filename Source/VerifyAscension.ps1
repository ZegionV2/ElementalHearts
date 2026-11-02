$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Get-ChildItem C:/Games/RimWorld/RimWorldWin64_Data/Managed -Filter *.dll | ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
$game=[Reflection.Assembly]::LoadFrom('C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll')
$own=[Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
[xml]$bases=Get-Content "$root/Defs/ElementalHeartBases.xml"
$hbase=$bases.SelectSingleNode("Defs/HediffDef[@Name='EH_ElementalHeartHediffBase']")
if($hbase.ParentName -or $hbase.hediffClass -ne 'HediffWithComps' -or $hbase.defaultInstallPart -or $hbase.spawnThingOnRemoved){throw 'Ascension still inherits organ behavior'}
$ibase=$bases.SelectSingleNode("Defs/ThingDef[@Name='EH_ElementalHeartItemBase']")
if($ibase.ParentName -ne 'MechSerumBase' -or $ibase.comps.li.useJob -ne 'UseItem'){throw 'Self-injection setup missing'}
$rbase=$bases.SelectSingleNode("Defs/RecipeDef[@Name='EH_InstallElementalHeartBase']")
if($rbase.targetsBodyPart -ne 'false' -or $rbase.anesthetize -ne 'false' -or $rbase.recipeUsers.li -notcontains 'Human'){throw 'Administration still requires organ surgery or has no patient users'}
foreach($e in @('Lightning','Flame','Wind','Water')){
 [xml]$defs=Get-Content "$root/Defs/${e}Heart.xml"
 $h=$defs.SelectSingleNode("Defs/HediffDef[defName='EH_${e}Heart']")
 $item=$defs.SelectSingleNode("Defs/ThingDef[defName='EH_${e}Heart']")
 if($h.label -ne "$e Ascendant" -or $h.spawnThingOnRemoved -or $item.comps.li.hediff -ne $h.defName){throw 'Serum/condition mapping mismatch'}
 if(!(Test-Path "$root/Textures/$($item.graphicData.texPath).png")){throw 'Serum icon missing'}
 $moods=@($defs.SelectNodes('//baseMoodEffect'));if($moods.Count -lt 1 -or $moods[0].InnerText -ne '30'){throw 'Ordinary ascendant mood regression'}
 if($e -in @('Flame','Lightning','Water','Wind')){if($moods.Count -ne 2 -or $moods[1].InnerText -ne '50'){throw 'Evolved mood must replace ordinary mood with +50'}}elseif($moods.Count -ne 1){throw 'Unexpected extra mood stage'}
}
# Exercise real game objects through C# (PowerShell cannot bind HediffDef's case-only member names).
Add-Type -ReferencedAssemblies @("C:/Games/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll","C:/Games/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll","$root/Assemblies/ElementalHearts.dll") -TypeDefinition @'
using System;
using System.Collections.Generic;
using Verse;
using ElementalHearts;
public static class AscensionMigrationChecks {
 public static void Run(){
  var def=new HediffDef{tags=new List<string>{"EH_ElementalHeart"}};
  var h=(HediffWithComps)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(HediffWithComps));h.def=def;typeof(Hediff).GetField("part",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(h,new BodyPartRecord());h.loadID=12345;var abilities=new HediffComp_WindAbilities{flowStacks=3,nextAutoCheck=900};h.comps=new List<HediffComp>{abilities};
  if(!ElementalAscension.Normalize(h)||h.Part!=null||h.loadID!=12345)throw new Exception("Migration failed or replaced saved identity");
  if(!object.ReferenceEquals(h.comps[0],abilities)||abilities.flowStacks!=3||abilities.nextAutoCheck!=900)throw new Exception("Ability state reset during migration");
  if(ElementalAscension.Normalize(h))throw new Exception("Non-idempotent migration");
  var other=(HediffWithComps)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(HediffWithComps));other.def=new HediffDef();typeof(Hediff).GetField("part",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(other,new BodyPartRecord());
  if(ElementalAscension.Normalize(other)||other.Part==null)throw new Exception("Unrelated condition altered");
  var pawn=(Pawn)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Pawn));pawn.def=(ThingDef)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ThingDef));pawn.def.race=new RaceProperties{intelligence=Intelligence.Humanlike};
  pawn.health=new Pawn_HealthTracker(pawn);h.pawn=pawn;pawn.health.hediffSet.hediffs.Add(h);
  if(ElementalAscension.CanReceive(pawn).Accepted)throw new Exception("Duplicate serum allowed");
  if(new Recipe_ObsoleteElementalExtraction().AvailableOnNow(pawn))throw new Exception("Legacy extraction still available");
 }
}
'@
[AscensionMigrationChecks]::Run()
[xml]$visors=Get-Content "$root/Defs/ElementalVisors.xml"
foreach($v in $visors.Defs.ThingDef){if($v.apparel.drawData.defaultData.offset -ne '(0, 0, 0.07)'){throw 'Visor lift mismatch'}}
Write-Output 'PASS: four serum mappings, whole-body base, direct use and administration definitions, +30 ordinary / +50 evolved mood stages, actual-object migration/idempotence/unrelated-condition preservation, duplicate-dose rejection, disabled extraction and visor lift.'
Write-Output 'LIMIT: live injection, existing save migration and visual positioning require in-game verification.'

