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
        private GlowButton trackButton;

        public AchievementListItem(AchievementTableEntry achievement, IAchievementTrackerService achievementTrackerService, IAchievementService achievementService, ContentService contentService, ITextureService textureService, string icon)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementService = achievementService;
            this.contentService = contentService;
            this.textureService = textureService;
            this.icon = icon;

            //this.achievementService.PlayerAchievementsLoaded += ()
            //    => this.ColorAchievement();
        }

        protected override void Build(Container buildPanel)
        {
            achievementTrackerService.AchievementUntracked += Tracker_AchievementUntracked;

            buildPanel.Height = 140;
            buildPanel.Width = buildPanel.Width - 10;

            this.button = new DetailsButton()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                Icon = textureService.GetTexture(this.icon),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill
            };

            if (this.achievementService.HasFinishedAchievement(this.achievement.Id))
            {
                BuildCompleteButton(this.button);
            }
            else
            {
                buildPanel.Click += BuildPanel_Click;
                BuildInCompleteButton(this.button);
            }
        }

        public void BuildCompleteButton(DetailsButton button)
        {
            if (button == null)
                return;

            button.BackgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50);
            button.IconDetails = "Complete";
        }

        public void BuildInCompleteButton(DetailsButton button)
        {
            if (button == null)
                return;

            button.ShowToggleButton = true;
            this.trackButton = BuildTrackingButton(button);
        }

        public GlowButton BuildTrackingButton(DetailsButton parent)
        {
            return new GlowButton
            {
                ActiveIcon = textureService.GetRefTexture("track_enabled.png"),
                Icon = textureService.GetRefTexture("track_disabled.png"),
                BasicTooltipText = "Track achievement",
                Parent = parent,
                ToggleGlow = true,
                Checked = achievementTrackerService.IsBeingTracked(this.achievement.Id)
            };
        }

        private void Tracker_AchievementUntracked(int achievementId)
        {
            if (this.achievement.Id == achievementId)
                this.trackButton.Checked = false;
        }

        private void BuildPanel_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (this.achievementTrackerService.IsBeingTracked(this.achievement.Id))
            {
                this.achievementTrackerService.RemoveAchievement(this.achievement.Id);
                this.trackButton.Checked = false;
            }
            else
            {
                var trackSuccess = this.achievementTrackerService.TrackAchievement(this.achievement.Id);

                if (trackSuccess)
                {
                    this.trackButton.Checked = true;
                }
                else
                {
                    this.trackButton.Checked = false;

                    // TODO: Localize
                    ScreenNotification.ShowNotification("You can have a maximum of 15 achievements tracked concurrently.\n Untrack one to add a new one.");
                }
            }
        }
    }
}
