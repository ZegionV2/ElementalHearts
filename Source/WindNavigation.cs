using System;
using System.Collections.Generic;
using Verse;
namespace ElementalHearts {
 public static class WindNavigation {
  // One bounded flood-fill supplies every candidate flank, rather than eight separate searches.
  public static Dictionary<IntVec3,IntVec3> Tree(IntVec3 start,Func<IntVec3,IntVec3,bool> passable){
   return Tree(start,passable,36,113);
  }
  public static Dictionary<IntVec3,IntVec3> Tree(IntVec3 start,Func<IntVec3,IntVec3,bool> passable,int radiusSquared,int limit){
   var queue=new Queue<IntVec3>();var previous=new Dictionary<IntVec3,IntVec3>();queue.Enqueue(start);previous[start]=start;
   while(queue.Count>0){var at=queue.Dequeue();
    for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){
     if(x==0&&z==0)continue;var next=at+new IntVec3(x,0,z);
     if(previous.ContainsKey(next)||next.DistanceToSquared(start)>radiusSquared||!passable(at,next))continue;
     if(previous.Count>=limit)return previous;previous[next]=at;queue.Enqueue(next);
    }
   }
   return previous;
  }
  public static List<IntVec3> Path(Dictionary<IntVec3,IntVec3> tree,IntVec3 start,IntVec3 end){
   return Path(tree,start,end,12);
  }
  public static List<IntVec3> Path(Dictionary<IntVec3,IntVec3> tree,IntVec3 start,IntVec3 end,int maxSteps){
   if(!tree.ContainsKey(end))return null;var result=new List<IntVec3>();
   while(end!=start){if(result.Count>=maxSteps)return null;result.Add(end);end=tree[end];}
   result.Reverse();return result;
  }
 }
}
