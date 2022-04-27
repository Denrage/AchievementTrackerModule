using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementListItem : View
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly IAchievementService achievementService;
        private readonly ContentService contentService;
        private readonly string icon;
        private readonly Achievement achievement;

        public AchievementListItem(Achievement achievement, IAchievementTrackerService achievementTrackerService, IAchievementService achievementService, ContentService contentService, string icon)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementService = achievementService;
            this.contentService = contentService;
            this.icon = icon;
        }

        protected override void Build(Container buildPanel)
        {
            buildPanel.Click += (s, e) => this.achievementTrackerService.TrackAchievement(this.achievement);

            var button = new DetailsButton()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                ShowToggleButton = true,
                Icon = this.contentService.GetRenderServiceTexture(this.icon),
            };

            if (this.achievementService.HasFinishedAchievement(this.achievement.Id))
            {
                button.BackgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50);
            }
        }
    }
}
