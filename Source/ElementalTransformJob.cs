using System.Collections.Generic;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public sealed class JobDriver_ElementalTransform:JobDriver {
  HediffComp_ElementalTransform Comp{get{return HediffComp_ElementalTransform.On(pawn);}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>{var c=Comp;if(c!=null)c.Cancel();});
   AddFailCondition(()=>Comp==null||!Comp.Ready);
   var hold=new Toil();hold.defaultCompleteMode=ToilCompleteMode.Never;hold.handlingFacing=true;
   hold.initAction=()=>{var c=Comp;if(c!=null)c.Begin();};
   hold.tickAction=()=>{
    pawn.pather.StopDead();pawn.Rotation=Rot4.South;
    if(!HediffComp_ElementalTransform.Transforming(pawn))ReadyForNextToil();
   };
   yield return hold;
  }
 }
}
