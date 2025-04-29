using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;

namespace Rubicon.Core.Rulesets.Mania;

public partial class ManiaScoreManager : ScoreManager
{
    /// <summary>
    /// The max score this instance can get.
    /// </summary>
    [Export] public int MaxScore = 1000000;
    
    /// <summary>
    /// How many notes there is in the chart.
    /// </summary>
    [Export] public int NoteCount = 0;

    /// <summary>
    /// The highest combo that can be achieved in a song.
    /// </summary>
    [Export] public int MaxCombo = 0;

    /// <summary>
    /// The amount of notes hit, counting the start of a hold note.
    /// Only takes account of notes that count towards score.
    /// </summary>
    [Export] public int NotesHit = 0;

    /// <summary>
    /// The amount of notes hit, counting the start and end of a hold note.
    /// Only takes account of notes that count towards score.
    /// </summary>
    [Export] public int TailsHit = 0;

    public override void Initialize(RubiChart chart, StringName target)
    {
        base.Initialize(chart, target);
        
        NoteData[] notes = Chart.GetNotes();
        MaxCombo += notes.Length;
        NoteCount += MaxCombo + GetHoldNoteCount(notes);
    }

    public override void JudgeNoteResult(NoteResult result)
    {
        Judgment rating = result.Rating;
        if (result.Note.CountsTowardScore) 
        {
            if (result.Hit == Hit.Tap || result.Hit == Hit.Hold) // Tap note or initial tap of hold note
            {
                NotesHit++;
                        
                switch (rating)
                {
                    case Judgment.Perfect:
                        PerfectHits++;
                        Combo++;
                        break;
                    case Judgment.Great:
                        GreatHits++;
                        Combo++;
                        break;
                    case Judgment.Good:
                        GoodHits++;
                        Combo++;
                        break;
                    case Judgment.Okay:
                        OkayHits++;
                        ComboBreaks++;
                        Combo = 0;
                        break;
                    case Judgment.Bad:
                        BadHits++;
                        ComboBreaks++;
                        Combo = 0;
                        break;
                    case Judgment.Miss:
                        Misses++;
                        ComboBreaks++;
                        if (result.Note.MeasureLength <= 0)
                            break;

                        Misses++;
                        ComboBreaks++;
                        break;
                }
            }
            else // Hold note end
            {
                TailsHit++;
                        
                switch (rating)
                {
                    case Judgment.Perfect:
                        PerfectHits++;
                        break;
                    case Judgment.Miss:
                        ComboBreaks++;
                        break;
                }
            }   
        }

        if (rating == Judgment.Miss)
        {
            MissStreak++;
            Combo = 0;
            ComboBreaks++;
        }
        else if (rating != Judgment.None)
            MissStreak = 0;
                
        if (Combo > HighestCombo)
            HighestCombo = Combo; 
        
        float noteValue = (float)MaxScore / NoteCount * 0.35f;
        int hitNotes = PerfectHits + GreatHits + GoodHits + OkayHits + BadHits + Misses;
        
        // Score
        if (PerfectHits == NoteCount) Score = MaxScore;
        else
        {
            float baseNoteValue = noteValue;
            float baseScore = (baseNoteValue * PerfectHits) + (baseNoteValue * (GreatHits * 0.9375f)) + (baseNoteValue * (GoodHits * 0.625f)) + (baseNoteValue * (OkayHits * 0.3125f)) + (baseNoteValue * (BadHits * 0.15625f));
            float bonusScore = Mathf.Sqrt(((float)HighestCombo / MaxCombo) * 100f) * MaxScore * 0.065f; 
            Score = (int)Math.Floor(baseScore + bonusScore);
        }
        
        // Accuracy
        Accuracy = PerfectHits == NoteCount
            ? 100f
            : (PerfectHits + (GreatHits * 0.95f) + (GoodHits * 0.65f) +
               (OkayHits * 0.3f) + (BadHits * 0.15f)) / hitNotes * 100f;

        // Rank
        float maxBaseScore = noteValue * hitNotes;
        float maxBonusScore = Mathf.Sqrt((float)NotesHit / MaxCombo * 100f) * MaxScore * 0.065f;
        int maxScore = Mathf.FloorToInt(maxBaseScore + maxBonusScore);
        if (Score >= maxScore)
            Rank = ScoreRank.P;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.975f))
            Rank = ScoreRank.Sss;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.95f))
            Rank = ScoreRank.Ss;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.9f))
            Rank = ScoreRank.S;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.8f))
            Rank = ScoreRank.A;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.7f))
            Rank = ScoreRank.B;
        else if (Score >= Mathf.FloorToInt(maxScore * 0.6f))
            Rank = ScoreRank.C;
        else
            Rank = ScoreRank.D;
        
        // Clear Rank
        if (Misses + BadHits + OkayHits > 0)
            Clear = ClearRank.Clear;
        else if (GoodHits > 0)
            Clear = ClearRank.FullCombo;
        else if (GreatHits > 0)
            Clear = ClearRank.GreatFullCombo;
        else
            Clear = ClearRank.Perfect;
        
        if (result.Rating != Judgment.None && result.Hit != Hit.Tail)
            EmitSignalStatisticsUpdated(Combo, result.Rating, result.Distance);
    }
    
    private int GetHoldNoteCount(NoteData[] notes)
    {
        return notes.Count(x => x.MeasureLength > 0f);
    }
}