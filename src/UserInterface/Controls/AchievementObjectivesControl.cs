using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementObjectivesControl : AchievementListControl<ObjectivesDescription, TableDescriptionEntry>
    {
        public AchievementObjectivesControl(
            IItemDetailWindowFactory itemDetailWindowFactory,
            IAchievementService achievementService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            ObjectivesDescription description)
            : base(itemDetailWindowFactory, achievementService, contentsManager, achievement, description)
        {
        }

        protected override void ColorControl(Control control, bool achievementBitFinished)
        => control.BackgroundColor = achievementBitFinished
                ? Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50)
                : Microsoft.Xna.Framework.Color.Transparent;

        protected override Control CreateEntryControl(int index, TableDescriptionEntry entry, Container parent) => new Label()
        {
            Parent = parent,
            Width = 64,
            Height = 64,
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
