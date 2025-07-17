using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Meta;
using PukiTools.GodotSharp;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A <see cref="PlayField"/> class with Mania-related gameplay incorporated. Also the main mode for Rubicon Engine.
/// </summary>
[GlobalClass] public partial class ManiaPlayField : PlayField
{
    [Export] public ManiaNoteSkin NoteSkin;

    [Export] public Node NoteSkinModule;

    /// <summary>
    /// Readies this PlayField for Mania gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public override void Setup(RuleSet ruleSetData, SongMeta meta, RubiChart chart, int targetIndex)
    {
        string noteSkinName = meta.NoteSkin;
        string noteSkinPath = RubiconCore.NoteSkins.Skins[noteSkinName].Rulesets[ruleSetData.UniqueId].Path;
        NoteSkin = ResourceLoader.Load<ManiaNoteSkin>(noteSkinPath);
        base.Setup(ruleSetData, meta, chart, targetIndex);

        Name = "Mania PlayField";
        for (int i = 0; i < BarLines.Length; i++)
            BarLines[i].MoveToFront();
        
        UpdateOptions();
        UserSettings.SettingsChanged += UpdateOptions;
    }
    
    public void UpdateOptions()
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
        
        GlobalHud?.FlipVertical(UserSettings.Rubicon.Mania.DownScroll);
        BarLines[TargetIndex].LocalHud?.FlipVertical(UserSettings.Rubicon.Mania.DownScroll);
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
                healthAddition = -5 - (int)(ScoreManager.MissStreak * 2.5f);
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
    public override bool HasFailed() => Health <= 0;

    public override NoteFactory CreateNoteFactory()
    {
        ManiaNoteFactory maniaFactory = new ManiaNoteFactory();
        maniaFactory.NoteSkin = NoteSkin;
        return maniaFactory;
    }

    public override BarLine CreateBarLine() => new ManiaBarLine();

    public override ScoreManager CreateScoreManager() => new ManiaScoreManager();

    public override void AfterBarLineSetup(BarLine barLine)
    {
        if (barLine is not ManiaBarLine maniaBarLine) 
            return;
        
        maniaBarLine.ChangeNoteSkin(NoteSkin, true);
    }
}