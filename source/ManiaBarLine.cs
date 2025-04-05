using Rubicon.Core.Chart;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// The bar line class for Mania gameplay.
/// </summary>
[GlobalClass] public partial class ManiaBarLine : BarLine
{
    /// <summary>
    /// The note skin associated with this bar line.
    /// </summary>
    [Export] public ManiaNoteSkin NoteSkin;

    public override NoteController CreateNoteController() => new ManiaNoteController();

    public void ChangeNoteSkin(ManiaNoteSkin noteSkin, bool updatePositions = false)
    {
        NoteSkin = noteSkin;
        for (int c = 0; c < Controllers.Length; c++)
        {
            if (Controllers[c] is not ManiaNoteController controller)
                continue;
            
            controller.ChangeNoteSkin(noteSkin);
            if (!updatePositions)
                continue;
            
            controller.Position = new Vector2(c * NoteSkin.LaneSize - ((Controllers.Length - 1) * NoteSkin.LaneSize / 2f), 0);
        }
    }

    /// <inheritdoc/>
    public override void OnNoteHit(NoteResult result)
    {
        EmitSignalNoteHit(Name, result);
    }

    /// <summary>
    /// Sets all the note managers' direction angle to the one provided
    /// </summary>
    /// <param name="radians">The angle, in radians</param>
    public void SetDirectionAngle(float radians)
    {
        foreach (NoteController noteManager in Controllers)
            if (noteManager is ManiaNoteController maniaNoteManager)
                maniaNoteManager.DirectionAngle = radians;
    }
}