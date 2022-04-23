using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementObjectivesControl : FlowPanel, IAchievementControl
    {
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly IAchievementService achievementService;
        private readonly ObjectivesDescription description;
        private readonly CollectionAchievementTable achievementDetails;

        public AchievementObjectivesControl(
            IItemDetailWindowFactory itemDetailWindowFactory,
            IAchievementService achievementService,
            Achievement achievement,
            ObjectivesDescription description)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.achievementService = achievementService;

            this.description = description;
            this.achievementDetails = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);
            this.FlowDirection = ControlFlowDirection.LeftToRight;
        }

        public void BuildControl()
        {
            if (!string.IsNullOrEmpty(this.description.GameText))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameText,
                    AutoSizeHeight = true,
                    Width = this.ContentRegion.Width,
                    WrapText = true,
                };
            }

            if (!string.IsNullOrEmpty(this.description.GameHint))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameHint,
                    TextColor = Microsoft.Xna.Framework.Color.LightGray,
                    Width = this.ContentRegion.Width,
                    AutoSizeHeight = true,
                    WrapText = true,
                };
            }

            var panel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = this.ContentRegion.Width,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(10f),
            };

            _ = Task.Run(() =>
            {
                for (var i = 0; i < this.description.EntryList.Count; i++)
                {
                    var label = new Label()
                    {
                        Parent = panel,
                        Width = 64,
                        Height = 64,
                        Text = (i + 1).ToString(),
                        Font = Content.DefaultFont18,
                        VerticalAlignment = VerticalAlignment.Middle,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BackgroundColor = Microsoft.Xna.Framework.Color.DarkGray,
                    };

                    var index = i;
                    label.Click += (s, eventArgs) =>
                    {
                        var itemWindow = this.itemDetailWindowFactory.Create(this.description.EntryList[index].DisplayName, this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index]);
                        itemWindow.Parent = GameService.Graphics.SpriteScreen;
                        itemWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                        itemWindow.ToggleWindow();
                    };
                }
            });
        }
    }
}
