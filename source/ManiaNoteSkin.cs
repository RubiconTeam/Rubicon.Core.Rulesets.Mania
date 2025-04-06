using System.Linq;
using Godot.Collections;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A resource that holds important information for Mania-related graphics.
/// </summary>
[GlobalClass] public partial class ManiaNoteSkin : Resource
{
	public const string GdManiaSkinModule = "GDManiaSkinModule";
	
	/// <summary>
	/// The SpriteFrames resource to grab note textures from.
	/// </summary>
	[Export] public SpriteFrames Notes;

	/// <summary>
	/// The SpriteFrames resource to grab lane textures from.
	/// </summary>
	[Export] public SpriteFrames Lanes;

	/// <summary>
	/// The SpriteFrames resource to grab hold and tail textures from.
	/// </summary>
	[Export] public SpriteFrames Holds;
	
	/// <summary>
	/// The SpriteFrames resource to grab splash textures from.
	/// </summary>
	[Export] public SpriteFrames Splashes;

	/// <summary>
	/// The SpriteFrames resource to grab hold covers from.
	/// </summary>
	[Export] public SpriteFrames HoldCovers;
	
	/// <summary>
	/// Specifies direction names for each lane count.
	/// </summary>
	[Export] public Dictionary<int, string[]> Directions = new() { { 4, [ "left", "down", "up", "right" ] } };

	/// <summary>
	/// The width of each lane.
	/// </summary>
	[ExportGroup("Graphics"), Export] public float LaneSize = 160f;
	
	/// <summary>
	/// The scale used when generating notes and lanes.
	/// </summary>
	[Export] public Vector2 Scale = Vector2.One;
	
	/// <summary>
	/// The filtering used when generating notes and lanes.
	/// </summary>
	[Export] public CanvasItem.TextureFilterEnum Filter = CanvasItem.TextureFilterEnum.Linear;

	/// <summary>
	/// Enabling this will cause confirm animaions on lanes to play every step.
	/// </summary>
	[ExportGroup("Extras"), Export] public bool StrobeHold = false;

	/// <summary>
	/// Setting this to true will put hold tails behind the receptors.
	/// </summary>
	[Export] public bool HoldsBehindLanes = false;

	/// <summary>
	/// Setting this to true will put hold tails behind the notes.
	/// </summary>
	[Export] public bool HoldsBehindNotes = false;

	/// <summary>
	/// Whether to enable tiling on hold graphics. Hold textures in <see cref="Holds"/> should NOT be an <see cref="AtlasTexture"/>/part of a sprite sheet!
	/// </summary>
	[Export] public bool UseTiledHold = false;

	/// <summary>
	/// Instantiates this packed scene alongside this note skin when it gets loaded.
	/// </summary>
	[ExportGroup("Scripting"), Export] public PackedScene ModuleScene;

	/// <summary>
	/// Instantiates this script as a node alongside this note skin when it gets loaded.
	/// </summary>
	[Export] public Script ModuleScript;

	/// <summary>
	/// Returns a module for use with the note skin. The scene takes priority over the script.
	/// </summary>
	/// <returns>A Node</returns>
	public Node InstantiateModule()
	{
		if (ModuleScene != null && ModuleScene.CanInstantiate())
		{
			Node scene = ModuleScene.Instantiate();
			return scene;
		}

		if (ModuleScript is CSharpScript cSharpModule)
			return cSharpModule.New().As<Node>();

		if (ModuleScript is GDScript gdScriptModule)
			return gdScriptModule.New().As<Node>();
		
		return null;
	}

	/// <summary>
	/// Gets a direction name based on lane count and lane number.
	/// </summary>
	/// <param name="lane">The lane index</param>
	/// <param name="laneCount">The amount of lanes.</param>
	/// <returns>A direction name if found (Ex: "left"), otherwise an empty name.</returns>
	public string GetDirection(int lane, int laneCount = 4)
	{
		string[] directions = GetDirections(laneCount);
		if (lane < directions.Length && lane >= 0)
			return directions[lane];

		return "";
	}
	
	/// <summary>
	/// Gets an array of directions based on the lane count provided.
	/// </summary>
	/// <param name="laneCount">The amount of lanes.</param>
	/// <returns>An array of direction names. (Ex: ["left", "down", "up", "right"])</returns>
	public string[] GetDirections(int laneCount = 4)
	{
		return CollectionExtensions.GetValueOrDefault(Directions, laneCount);
	}

	/// <summary>
	/// Gets the amount of splashes in the <see cref="Splashes"/>.
	/// </summary>
	/// <param name="direction">The direction to search for</param>
	/// <returns>The amount of lane splashes, if any.</returns>
	public int GetSplashCountForDirection(string direction)
	{
		if (Splashes == null)
			return 0;
		
		return Splashes.GetAnimationNames().Count(x => x.StartsWith($"{direction}LaneSplash"));
	}
}
