using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    public class AchievementDetailsWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementControlProvider achievementControlProvider;
        private readonly Achievement achievement;
        private readonly Texture2D texture;

        public AchievementDetailsWindow(
            ContentsManager contentsManager,
            Achievement achievement,
            IAchievementService achievementService,
            IAchievementControlProvider achievementControlProvider)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementControlProvider = achievementControlProvider;
            this.achievement = achievement;

            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            this.Title = this.achievement.Name;
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 550, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 550, 600 - 30));

            var control = this.achievementControlProvider.GetAchievementControl(
                this.achievement,
                this.achievementService.Achievements.FirstOrDefault(x => x.Id == this.achievement.Id).Description,
                this.ContentRegion.Size);

            if (control is null)
            {
                return;
            }

            control.Parent = this;

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   this.texture,
                                   bounds);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
