namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A module for <see cref="ManiaNoteSkin"/>. Attaches itself to a <see cref="ManiaNoteController"/>.
/// </summary>
[GlobalClass] public abstract partial class CsManiaSkinModule : Node
{
    /// <summary>
    /// The parent controller.
    /// </summary>
    public ManiaNoteController Controller;

    public override void _Ready()
    {
        base._Ready();

        Controller = GetParent<ManiaNoteController>();

        Controller.NoteSpawned += OnNoteSpawned;
        Controller.NoteHit += OnNoteHit;
        Controller.PressedPlayed += OnPressedAnimation;
        Controller.NeutralPlayed += OnNeutralAnimation;
    }

    /// <summary>
    /// Invoked when a note spawns.
    /// </summary>
    /// <param name="note">The note spawned</param>
    public abstract void OnNoteSpawned(Note note);
    
    /// <summary>
    /// Invoked when a note is hit. (A miss also counts.)
    /// </summary>
    /// <param name="result">The resulting hit.</param>
    public abstract void OnNoteHit(NoteResult result);

    /// <summary>
    /// Invoked when the pressed animation is played.
    /// </summary>
    public abstract void OnPressedAnimation();

    /// <summary>
    /// Invoked when the neutral animation is played.
    /// </summary>
    public abstract void OnNeutralAnimation();
}