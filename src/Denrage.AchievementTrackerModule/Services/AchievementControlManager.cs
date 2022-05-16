using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementControlManager : IAchievementControlManager, IDisposable
    {
        private readonly Dictionary<int, Control> controls = new Dictionary<int, Control>();
        private readonly IAchievementControlProvider achievementControlProvider;

        public AchievementControlManager(IAchievementControlProvider achievementControlProvider)
        {
            this.achievementControlProvider = achievementControlProvider;
        }

        public void InitializeControl(int achievementId, AchievementTableEntry achievement, AchievementTableEntryDescription description)
        {
            if (!this.controls.ContainsKey(achievementId))
            {
                var control = this.achievementControlProvider.GetAchievementControl(achievement, achievement.Description);

                this.controls[achievementId] = control;
            }
        }

        public bool ControlExists(int achievementId)
            => this.controls.ContainsKey(achievementId);

        public void ChangeParent(int achievementId, Container parent)
        {
            if (this.controls.TryGetValue(achievementId, out var control))
            {
                control.Parent = parent;

                if (parent != null)
                {
                    control.Width = parent.ContentRegion.Width;
                    control.Height = parent.ContentRegion.Height;
                }
            }
        }

        public void RemoveParent(int achievementId)
            => this.ChangeParent(achievementId, null);

        public Control GetControl(int achievementId)
            => this.controls[achievementId];

        public void Dispose()
        {
            foreach (var item in this.controls)
            {
                item.Value.Dispose();
            }

            this.controls.Clear();
        }
    }
}
