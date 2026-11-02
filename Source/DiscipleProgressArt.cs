using System;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class DiscipleProgressArt {
  static readonly Texture2D[] icons=new Texture2D[6];
  public static int Stage(int kills){return Math.Max(0,Math.Min(5,kills/10));}
  public static Texture2D Icon(int kills){int stage=Stage(kills);return icons[stage]??(icons[stage]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/DiscipleProgress/Progress_"+stage.ToString("00")));}
 }
}
