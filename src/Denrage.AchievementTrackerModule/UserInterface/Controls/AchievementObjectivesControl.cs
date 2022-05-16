using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementObjectivesControl : AchievementListControl<ObjectivesDescription, TableDescriptionEntry>
    {
        public AchievementObjectivesControl(
            IItemDetailWindowManager itemDetailWindowManager,
            IAchievementService achievementService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            ObjectivesDescription description)
            : base(itemDetailWindowManager, achievementService, contentsManager, achievement, description)
        {
        }

        protected override void ColorControl(Control control, bool achievementBitFinished)
        => control.BackgroundColor = achievementBitFinished
                ? Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50)
                : Microsoft.Xna.Framework.Color.Transparent;

        protected override Control CreateEntryControl(int index, TableDescriptionEntry entry, Container parent) => new Label()
        {
            Parent = parent,
            Width = 32,
            Height = 32,
            Text = (index + 1).ToString(),
            Font = Content.DefaultFont18,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center,
            ZIndex = 1,
        };

        protected override string GetDisplayName(TableDescriptionEntry entry) => entry.DisplayName;

        protected override IEnumerable<TableDescriptionEntry> GetEntries(ObjectivesDescription description) => description.EntryList;
    }
}
