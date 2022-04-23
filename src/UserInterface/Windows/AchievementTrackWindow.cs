using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.Services.Factories.AchievementControl;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    public class AchievementTrackWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementControlProvider achievementControlProvider;
        private readonly Achievement achievement;
        private readonly Texture2D texture;

        public AchievementTrackWindow(
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
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 7 * 74, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 7 * 74, 600 - 30));

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

        protected override void OnShown(EventArgs e) => base.OnShown(e);

        public class AchievementControlProvider : IAchievementControlProvider
        {
            private readonly Dictionary<Type, AchievementControlFactory> mapping = new Dictionary<Type, AchievementControlFactory>();

            public AchievementControlProvider(IAchievementService achievementService, IItemDetailWindowFactory itemDetailWindowFactory)
            {
                this.mapping.Add(typeof(StringDescription), new AchievementTextControlFactory());
                this.mapping.Add(typeof(CollectionDescription), new AchievementCollectionControlFactory(achievementService, itemDetailWindowFactory));
                this.mapping.Add(typeof(ObjectivesDescription), new AchievementObjectiveControlFactory(achievementService, itemDetailWindowFactory));
            }

            public Control GetAchievementControl(Achievement achievement, AchievementTableEntryDescription description, Point size)
                => this.mapping.TryGetValue(description.GetType(), out var factory) ? factory.Create(achievement, description, size) : null;
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
