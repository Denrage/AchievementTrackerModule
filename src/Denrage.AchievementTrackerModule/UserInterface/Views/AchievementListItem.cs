using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Services;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementListItem : View
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly IAchievementService achievementService;
        private readonly ContentService contentService;
        private readonly ITextureService textureService;
        private readonly string icon;
        private readonly AchievementTableEntry achievement;
        private DetailsButton button;

        public AchievementListItem(AchievementTableEntry achievement, IAchievementTrackerService achievementTrackerService, IAchievementService achievementService, ContentService contentService, ITextureService textureService, string icon)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementService = achievementService;
            this.contentService = contentService;
            this.textureService = textureService;
            this.icon = icon;

            this.achievementService.PlayerAchievementsLoaded += ()
                => this.ColorAchievement();
        }

        protected override void Build(Container buildPanel)
        {
            buildPanel.Click += (s, e) =>
            {
                if(!this.achievementTrackerService.TrackAchievement(this.achievement.Id))
                {
                    // TODO: Localize
                    ScreenNotification.ShowNotification("You can have a maximum of 15 achievements tracked concurrently.\n Untrack one to add a new one.");
                }
            };

            this.button = new DetailsButton()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                ShowToggleButton = true,
                Icon = textureService.GetTexture(this.icon),
            };

            this.ColorAchievement();
        }

        public void ColorAchievement()
        {
            if (this.button != null)
            {
                if (this.achievementService.HasFinishedAchievement(this.achievement.Id))
                {
                    this.button.BackgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50);
                }
            }
        }
    }
}
