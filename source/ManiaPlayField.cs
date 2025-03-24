using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Events;
using Rubicon.Core.Meta;
using PukiTools.GodotSharp;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A <see cref="PlayField"/> class with Mania-related gameplay incorporated. Also the main mode for Rubicon Engine.
/// </summary>
[GlobalClass] public partial class ManiaPlayField : PlayField
{
    /// <summary>
    /// The max score this instance can get.
    /// </summary>
    [Export] public int MaxScore = 1000000;

    [Export] public ManiaNoteSkin NoteSkin;

    /// <summary>
    /// Readies this PlayField for Mania gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public override void Setup(SongMeta meta, RubiChart chart, int targetIndex, EventMeta events)
    {
        string noteSkinName = meta.NoteSkin;
        string noteSkinPath = PathUtility.GetResourcePath($"res://resources/ui/styles/{noteSkinName}/Mania");
        if (string.IsNullOrWhiteSpace(noteSkinPath))
        {
            string defaultSkin = ProjectSettings.GetSetting("rubicon/rulesets/mania/default_note_skin").AsString();
            PrintUtility.PrintError("ManiaPlayField", $"Mania Note Skin Path: {noteSkinName} does not exist. Defaulting to {defaultSkin}");
            noteSkinPath = PathUtility.GetResourcePath($"res://resources/ui/styles/{defaultSkin}/Mania");
        }

        NoteSkin = ResourceLoader.LoadThreadedGet(noteSkinPath) as ManiaNoteSkin;
        ManiaNoteFactory maniaFactory = new ManiaNoteFactory();
        maniaFactory.NoteSkin = NoteSkin;
        Factory = maniaFactory;
        
        base.Setup(meta, chart, targetIndex, events);
        
        Name = "Mania PlayField";
        for (int i = 0; i < BarLines.Length; i++)
            BarLines[i].MoveToFront();
    }
    
    /// <inheritdoc/>
    public override void UpdateOptions()
    {
        //BarLineContainer.
        //BarLineContainer.Position = new Vector2(0f, UserSettings.DownScroll ? -120f : 120f);

        for (int i = 0; i < BarLines.Length; i++)
        {
            //barLine.AnchorLeft = barLine.AnchorRight = ((index * 0.5f) - (Chart.Charts.Length - 1) * 0.5f / 2f) + 0.5f;
            if (BarLines[i] is not ManiaBarLine maniaBarLine)
                continue;
                
            maniaBarLine.SetDirectionAngle(!UserSettings.Rubicon.Mania.DownScroll ? Mathf.Pi / 2f : -Mathf.Pi / 2f);
            maniaBarLine.AnchorTop = BarLines[i].AnchorBottom = UserSettings.Rubicon.Mania.DownScroll ? 1f : 0f;
            maniaBarLine.OffsetTop = BarLines[i].OffsetBottom = UserSettings.Rubicon.Mania.DownScroll ? -140f : 140f;

            if (UserSettings.Rubicon.Mania.CenterBarLine)
            {
                maniaBarLine.AnchorLeft = maniaBarLine.AnchorRight = 0.5f;
                maniaBarLine.Visible = TargetIndex == i;
                
                continue;
            }
                
            maniaBarLine.AnchorLeft = maniaBarLine.AnchorRight = ((i * 0.5f) - (Chart.Charts.Length - 1) * 0.5f / 2f) + 0.5f;
            //BarLines[i].SetAnchorsPreset(barLinePreset, true);
        }
    }

    /// <inheritdoc />
    public override void UpdateStatistics()
    {
        float noteValue = (float)MaxScore / ScoreTracker.NoteCount * 0.35f;
        int hitNotes = ScoreTracker.PerfectHits + ScoreTracker.GreatHits + ScoreTracker.GoodHits + ScoreTracker.OkayHits + ScoreTracker.BadHits + ScoreTracker.Misses;
        
        // Score
        if (ScoreTracker.PerfectHits == ScoreTracker.NoteCount) ScoreTracker.Score = MaxScore;
        else
        {
            float baseNoteValue = noteValue;
            float baseScore = (baseNoteValue * ScoreTracker.PerfectHits) + (baseNoteValue * (ScoreTracker.GreatHits * 0.9375f)) + (baseNoteValue * (ScoreTracker.GoodHits * 0.625f)) + (baseNoteValue * (ScoreTracker.OkayHits * 0.3125f)) + (baseNoteValue * (ScoreTracker.BadHits * 0.15625f));
            float bonusScore = Mathf.Sqrt(((float)ScoreTracker.HighestCombo / ScoreTracker.MaxCombo) * 100f) * MaxScore * 0.065f; 
            ScoreTracker.Score = (int)Math.Floor(baseScore + bonusScore);
        }
        
        // Accuracy
        ScoreTracker.Accuracy = ScoreTracker.PerfectHits == ScoreTracker.NoteCount
            ? 100f
            : (ScoreTracker.PerfectHits + (ScoreTracker.GreatHits * 0.95f) + (ScoreTracker.GoodHits * 0.65f) +
               (ScoreTracker.OkayHits * 0.3f) + (ScoreTracker.BadHits * 0.15f)) / hitNotes * 100f;

        // Rank
        float maxBaseScore = noteValue * hitNotes;
        float maxBonusScore = Mathf.Sqrt((float)ScoreTracker.NotesHit / ScoreTracker.MaxCombo * 100f) * MaxScore * 0.065f;
        int maxScore = Mathf.FloorToInt(maxBaseScore + maxBonusScore);
        if (ScoreTracker.Score >= maxScore)
            ScoreTracker.Rank = ScoreRank.P;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.975f))
            ScoreTracker.Rank = ScoreRank.Sss;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.95f))
            ScoreTracker.Rank = ScoreRank.Ss;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.9f))
            ScoreTracker.Rank = ScoreRank.S;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.8f))
            ScoreTracker.Rank = ScoreRank.A;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.7f))
            ScoreTracker.Rank = ScoreRank.B;
        else if (ScoreTracker.Score >= Mathf.FloorToInt(maxScore * 0.6f))
            ScoreTracker.Rank = ScoreRank.C;
        else
            ScoreTracker.Rank = ScoreRank.D;
        
        // Clear Rank
        if (ScoreTracker.Misses + ScoreTracker.BadHits + ScoreTracker.OkayHits > 0)
            ScoreTracker.Clear = ClearRank.Clear;
        else if (ScoreTracker.GoodHits > 0)
            ScoreTracker.Clear = ClearRank.FullCombo;
        else if (ScoreTracker.GreatHits > 0)
            ScoreTracker.Clear = ClearRank.GreatFullCombo;
        else
            ScoreTracker.Clear = ClearRank.Perfect;
    }

    public override void UpdateHealth(Judgment hit)
    {
        int healthAddition = 0;
        switch (hit)
        {
            case Judgment.Perfect:
                healthAddition = 3;
                break;
            case Judgment.Great:
                healthAddition = 2;
                break;
            case Judgment.Good:
                healthAddition = 1;
                break;
            case Judgment.Okay:
                healthAddition = -1;
                break;
            case Judgment.Bad:
                healthAddition = -2;
                break;
            case Judgment.Miss:
                healthAddition = -5 - (int)ScoreTracker.MissStreak * 4;
                break;
        }

        int predictedHealth = Health + healthAddition;
        if (predictedHealth > MaxHealth)
            healthAddition -= predictedHealth % MaxHealth;

        Health += healthAddition;
        if (Health < 0)
            Health = 0;
    }

    /// <inheritdoc />
    public override bool GetFailCondition() => Health <= 0;

    public override BarLine CreateBarLine(IndividualChart chart, int index)
    {
        ManiaBarLine barLine = new ManiaBarLine();
        barLine.Setup(chart, NoteSkin, Chart.ScrollSpeed);
        barLine.Name = "Mania Bar Line " + index;
        
        return barLine;
    }
}