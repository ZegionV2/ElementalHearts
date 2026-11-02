param([string]$RimWorldDir='C:/Games/RimWorld')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed="$RimWorldDir/RimWorldWin64_Data/Managed"
Get-ChildItem $managed -Filter *.dll|ForEach-Object {try{[void][Reflection.Assembly]::LoadFrom($_.FullName)}catch{}}
[void][Reflection.Assembly]::LoadFrom("$RimWorldDir/Mods/2009463077/1.6/Assemblies/0Harmony.dll")
[void][Reflection.Assembly]::LoadFrom("$root/Assemblies/ElementalHearts.dll")
$checks=@'
using System;
using System.Collections.Generic;
using ElementalHearts;
using Verse;
public static class CombatFlowChecks {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static string Run(){
  for(int bits=0;bits<64;bits++){
   bool attack=(bits&1)!=0,approach=(bits&2)!=0,wait=(bits&4)!=0,forced=(bits&8)!=0,queue=(bits&16)!=0,recent=(bits&32)!=0;
   bool result=ElementalCombatFlow.ContinuePursuit(attack,approach,wait,forced,queue,recent);
   if(queue)Check(!result,"Queued orders overridden");
   else if(attack||approach)Check(result,"Live combat order lost");
   else Check(result==(wait&&!forced&&recent),"Hold/move/expired engagement not respected");
  }
  for(int bits=0;bits<8;bits++)for(int queue=0;queue<4;queue++){
   bool manual=(bits&1)!=0,enabled=(bits&2)!=0,multi=(bits&4)!=0;
   Check(ElementalCombatFlow.CarryCombo(manual,enabled,multi,queue)==(!manual&&enabled&&multi&&queue<2),"Combo intent protection");
  }
  Check(ElementalCombatFlow.ThinkDelay(true)==12&&ElementalCombatFlow.ThinkDelay(false)==60,"Decision cadence");
  IntVec3 start=new IntVec3(0,0,0);
  var open=WindNavigation.Tree(start,(a,b)=>true);
  Check(open.Count==113,"Search must remain within six-tile radius");
  foreach(var cell in open.Keys){Check(cell.DistanceToSquared(start)<=36,"Unbounded search");var path=WindNavigation.Path(open,start,cell);Check(path!=null,"Reachable cell missing");IntVec3 previous=start;foreach(var step in path){Check(previous.DistanceToSquared(step)<=2,"Nonadjacent move");previous=step;}Check(previous==cell,"Wrong destination");}
  var blocked=WindNavigation.Tree(start,(a,b)=>false);
  Check(blocked.Count==1&&WindNavigation.Path(blocked,start,new IntVec3(1,0,0))==null,"Blocked route found");
  // Solid vertical wall except for a gate: a route must pass through the gate, not through the wall.
  Func<IntVec3,bool> floor=c=>c.x!=1||c.z==2;
  var walls=WindNavigation.Tree(start,(a,b)=>floor(b)&&(a.x==b.x||a.z==b.z||(floor(new IntVec3(a.x,0,b.z))&&floor(new IntVec3(b.x,0,a.z)))));
  var detour=WindNavigation.Path(walls,start,new IntVec3(2,0,0));Check(detour!=null,"Gate route lost");bool gate=false;
  foreach(var step in detour){Check(floor(step),"Wall crossed");if(step.x==1&&step.z==2)gate=true;}Check(gate,"Corner cut through wall");
  Check(WindNavigation.Path(open,start,new IntVec3(7,0,0))==null,"Out-of-range destination");
  return "PASS: 64 combat-order cases, 32 combo-intent cases, active/idle cadence, bounded 113-cell navigation, adjacent steps, blocked routes, gate/corner checks, and out-of-range rejection.";
 }
}
'@
# Compile the shipping helper sources against this test host's framework. RimWorld's Mono
# places Queue<T> in mscorlib, whereas Windows .NET Framework places it in System.dll.
$helpers=(Get-Content "$PSScriptRoot/WindNavigation.cs" -Raw)+(Get-Content "$PSScriptRoot/ElementalCombatFlow.cs" -Raw)
$helpers=[regex]::Replace($helpers,'(?m)^using [^;]+;\r?\n','')
$checks="using System.Runtime.CompilerServices; using RimWorld; using Verse.AI;`n"+$checks+"`n"+$helpers
Add-Type -IgnoreWarnings -ReferencedAssemblies @("$managed/Assembly-CSharp.dll","$managed/UnityEngine.CoreModule.dll","$managed/netstandard.dll","$root/Assemblies/ElementalHearts.dll") -TypeDefinition $checks -WarningAction SilentlyContinue
[CombatFlowChecks]::Run()
Write-Output 'LIMIT: no live-game frame-time profiling or combat/save-reload test.'



