using System.Collections.Generic;
using System.Linq;
using Rubicon.Core;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using PukiTools.GodotSharp;
using Array = System.Array;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A bar line class for Mania gameplay. Also referred to as a "strum" by some.
/// </summary>
[GlobalClass] public partial class ManiaNoteController : NoteController
{
	/// <summary>
	/// The direction of this note manager.
	/// </summary>
	[Export] public string Direction = "";
	
	/// <inheritdoc/>
	[Export] public override float ScrollSpeed
	{
		get => base.ScrollSpeed;
		set
		{
			base.ScrollSpeed = value;
			for (int i = 0; i < HitObjects.Length; i++)
			{
				Note hitObject = HitObjects[i];
				if (hitObject is not ManiaNote maniaNote)
					continue;
				
				maniaNote.AdjustInitialTailSize();
			}
		}
	}

	/// <summary>
	/// The note that is currently being held.
	/// </summary>
	[Export] public NoteData NoteHeld;
	
	/// <summary>
	/// The angle the notes come from in radians.
	/// </summary>
	[Export] public float DirectionAngle = Mathf.Pi / 2f;

	/// <summary>
	/// The note skin for this manager. Please change via <see cref="ChangeNoteSkin"/>!
	/// </summary>
	[Export] public ManiaNoteSkin NoteSkin;

	/// <summary>
	/// The lane graphic for this manager.
	/// </summary>
	[Export] public AnimatedSprite2D LaneObject;

	/// <summary>
	/// The currently running note skin module, if there is any.
	/// </summary>
	[Export] public Node NoteSkinModule;
	
	/// <summary>
	/// Emitted when the pressed animation is played.
	/// </summary>
	[Signal] public delegate void PressedPlayedEventHandler();
	
	/// <summary>
	/// Emitted when the neutral animation is played.
	/// </summary>
	[Signal] public delegate void NeutralPlayedEventHandler();
	
	private List<AnimatedSprite2D> _splashSprites = new();
	private int _splashCount = 0;
	private AnimatedSprite2D _holdCover;
	private int _lastStep = -1;

	public override void Setup()
	{
		Action = $"play_mania_{ParentBarLine.Controllers.Length}k_{Lane}";
		
		_holdCover = new AnimatedSprite2D();
		_holdCover.Name = "Hold Cover";
		_holdCover.ZIndex = 1;
		_holdCover.Visible = false;
		_holdCover.AnimationFinished += OnHoldCoverAnimationFinished;
		AddChild(_holdCover);
	}

	public override void _Process(double delta)
	{
		if (NoteHeld != null && NoteHeld.MsTime + NoteHeld.MsLength < Conductor.Time * 1000f)
			ProcessQueue.Add(GetResult(noteIndex: HoldingIndex, distance: 0f, holding: false));

		int curStep = Mathf.FloorToInt(Conductor.CurrentStep);
		if (NoteSkin.StrobeHold && HoldingIndex != -1 && curStep != _lastStep)
			LaneObject.Frame = 0;
		
		for (int i = 0; i < _splashSprites.Count; i++)
			if (!_splashSprites[i].IsPlaying())
				_splashSprites[i].Modulate = Colors.Transparent;

		string holdCoverAnim = $"{Direction}LaneCoverHold";
		if (HoldingIndex < 0 && _holdCover.Visible && _holdCover.Animation == holdCoverAnim)
			_holdCover.Visible = false;
		
		if (_holdCover.IsVisible() && _holdCover.Animation == holdCoverAnim)
			_holdCover.Rotation = DirectionAngle;
		
		base._Process(delta);
		
		_lastStep = curStep;
	}

	protected override NoteResult GetResult(int noteIndex, float distance, bool holding)
	{
		NoteResult result = base.GetResult(noteIndex, distance, holding);
		result.Direction = Direction;
		return result;
	}

	/// <summary>
	/// Changes the note skin for this manager. Does not change the notes on-screen automatically!
	/// </summary>
	/// <param name="noteSkin">The note skin</param>
	public void ChangeNoteSkin(ManiaNoteSkin noteSkin)
	{
		if (NoteSkin == noteSkin)
			return;
		
		NoteSkinModule?.QueueFree();
		
		NoteSkin = noteSkin;
		Direction = noteSkin.GetDirection(Lane, ParentBarLine.Chart.Lanes);
		_splashCount = NoteSkin.GetSplashCountForDirection(Direction);

		NoteSkinModule?.QueueFree();
		if (NoteSkin.Module != null && NoteSkin.Module.CanInstantiate())
		{
			NoteSkinModule = NoteSkin.Module.Instantiate();
			NoteSkinModule.Name = "Note Skin Module";
			AddChild(NoteSkinModule);
		}
		
		if (noteSkin.HoldCovers != null)
			_holdCover.SpriteFrames = noteSkin.HoldCovers;

		if (LaneObject == null)
		{
			LaneObject = new AnimatedSprite2D();
			LaneObject.Name = "Lane Graphic";
			LaneObject.AnimationFinished += OnAnimationFinish;
			AddChild(LaneObject);
			MoveChild(LaneObject, 0);
		}
		
		LaneObject.Scale = Vector2.One * NoteSkin.Scale;
		LaneObject.SpriteFrames = NoteSkin.Lanes;
		LaneObject.TextureFilter = NoteSkin.Filter;
		LaneObject.Play($"{Direction}LaneNeutral", 1f, true);
	}

	protected override void AssignData(Note note, NoteData noteData)
	{
		if (note is not ManiaNote maniaNote)
			return;
		
		maniaNote.Assign(noteData, this);
	}

	/// <inheritdoc/>
	protected override void OnNoteHit(NoteResult result)
	{
		if (result.Rating != Judgment.Miss)
		{
			if (result.Hit != Hit.Hold)
			{
				if (NoteHeld == null || NoteHeld != null && (Autoplay || !Autoplay && Input.IsActionPressed($"play_mania_{ParentBarLine.Controllers.Length}k_{Lane}")))
					LaneObject.Animation = $"{Direction}LaneConfirm";
				
				NoteHeld = null;
				HoldingIndex = -1;
				LaneObject.Play();

				bool greatOrAbove = result.Rating <= Judgment.Great;
				switch (result.Hit)
				{
					case Hit.Tap:
						if (greatOrAbove)
							GenerateTapSplash();
						
						if (HitObjects[result.Index] != null)
							RemoveChild(HitObjects[result.Index]);
						break;
					case Hit.Tail:
						if (HitObjects[result.Index] != null)
							RemoveChild(HitObjects[result.Index]);
						
						if (NoteSkin.HoldCovers == null || !greatOrAbove)
							break;
							
						_holdCover.Rotation = 0f;
						_holdCover.Play($"{Direction}LaneCoverEnd");
						break;
				}
				
				HitObjects[result.Index]?.PrepareRecycle();
			}
			else
			{
				NoteHeld = result.Note;
				HoldingIndex = result.Index;
				LaneObject.Animation = $"{Direction}LaneConfirm";
				
				if (!NoteSkin.StrobeHold)
					LaneObject.Pause();
				else
					LaneObject.Play();

				if (NoteSkin.HoldCovers != null)
				{
					_holdCover.Visible = true;
					_holdCover.Rotation = 0f;
					_holdCover.Play($"{Direction}LaneCoverStart");
				}
				
				HitObjects[result.Index]?.SetZIndex(NoteSkin.HoldsBehindLanes ? LaneObject.ZIndex - 1 : LaneObject.ZIndex);
			}	
		}
		else
		{
			if (result.Note == NoteHeld)
			{
				if (HitObjects[result.Index] is ManiaNote maniaNote)
					maniaNote.UnsetHold();
			
				NoteHeld = null;
				if (NoteSkin.HoldCovers != null)
					_holdCover.Visible = false;
			}

			if (result.Note.MsLength <= 0f)
			{
				if (HitObjects[result.Index] != null)
					RemoveChild(HitObjects[result.Index]);
				
				HitObjects[result.Index]?.PrepareRecycle();
			}
		}
		
		base.OnNoteHit(result);
	}

	protected override void PressedEvent()
	{
		NoteData[] notes = Notes;
		if (NoteHitIndex >= notes.Length)
		{
			if (LaneObject.Animation != $"{Direction}LanePress")
			{
				LaneObject.Play($"{Direction}LanePress");
				EmitSignalPressedPlayed();
			}
			
			return;
		}

		float songPos = Conductor.Time * 1000f;
		float hitTime = GetCurrentNoteDistance(true);
		while (notes[NoteHitIndex].MsTime - songPos <= -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle())
		{
			// Miss every note thats too late first
			ProcessQueue.Add(GetResult(noteIndex: NoteHitIndex, distance: -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() - 1f, holding: false));
			NoteHitIndex++;
		}
			
		if (Mathf.Abs(hitTime) <= ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle()) // Literally any other rating
		{
			ProcessQueue.Add(GetResult(noteIndex: NoteHitIndex, distance: hitTime, holding: notes[NoteHitIndex].MsLength > 0));
			NoteHitIndex++;
		}
		else
		{
			if (UserSettings.Rubicon.Mania.GhostTapping)
				InvokeGhostTap();
				
			if (LaneObject.Animation != $"{Direction}LanePress")
			{
				LaneObject.Play($"{Direction}LanePress");
				EmitSignalPressedPlayed();
			}
		}
	}

	protected override void ReleasedEvent()
	{
		if (NoteHeld != null)
		{
			float length = NoteHeld.MsTime + NoteHeld.MsLength - (Conductor.Time * 1000f);
			bool holding = length <= ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle();
			ProcessQueue.Add(GetResult(noteIndex: HoldingIndex, distance: length, holding: !holding));
		}

		if (LaneObject.Animation != $"{Direction}LaneNeutral")
		{
			LaneObject.Play($"{Direction}LaneNeutral", 1f, true);
			EmitSignalNeutralPlayed();;
		}
	}

	private void GenerateTapSplash()
	{
		if (_splashCount == 0)
			return;
		
		string anim = Direction + "LaneSplash" + GD.RandRange(0, _splashCount - 1);
		AnimatedSprite2D splash = _splashSprites.FirstOrDefault(x => !x.IsPlaying());
		if (splash != null)
		{
			splash.SpriteFrames = NoteSkin.Splashes;
			splash.Frame = 0;
			splash.Scale = NoteSkin.Scale;
			splash.TextureFilter = NoteSkin.Filter;
			splash.Modulate = Colors.White;
			splash.Play(anim);
			return;
		}
		
		splash = new AnimatedSprite2D();
		splash.Name = "Tap Splash " + _splashSprites.Count;
		splash.SpriteFrames = NoteSkin.Splashes;
		splash.Scale = NoteSkin.Scale;
		splash.TextureFilter = NoteSkin.Filter;
		splash.Modulate = Colors.White;
		_splashSprites.Add(splash);
		AddChild(splash);
		splash.Play(anim);
	}

	/// <summary>
	/// Mainly for when the autoplay finishes hitting a note.
	/// </summary>
	private void OnAnimationFinish()
	{
		if (!Autoplay || LaneObject.Animation != $"{Direction}LaneConfirm")
			return;

		if (LaneObject.Animation != $"{Direction}LaneNeutral")
		{
			LaneObject.Play($"{Direction}LaneNeutral", 1f, true);
			EmitSignalNeutralPlayed();;
		}
	}

	private void OnHoldCoverAnimationFinished()
	{
		if (NoteSkin.HoldCovers == null)
			return;
		
		bool isStartAnim = _holdCover.Animation == $"{Direction}LaneCoverStart";
		if (isStartAnim)
		{
			if (HoldingIndex != -1)
				_holdCover.Play($"{Direction}LaneCoverHold");
			else
				_holdCover.Play($"{Direction}LaneCoverEnd");
		}
		
		bool isEndAnim = _holdCover.Animation == $"{Direction}LaneCoverEnd";
		if (!isEndAnim)
			return;

		_holdCover.Visible = false;
	}
}
