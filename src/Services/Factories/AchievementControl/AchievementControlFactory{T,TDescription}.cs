using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public abstract class AchievementControlFactory<T, TDescription> : AchievementControlFactory, IControlFactory<T, TDescription>
        where T : Panel, IAchievementControl
    {
        protected abstract T CreateInternal(AchievementTableEntry achievement, TDescription description);

        public override Control Create(AchievementTableEntry achievement, object description, Point size)
            => this.Create(achievement, (TDescription)description, size);

        public T Create(AchievementTableEntry achievement, TDescription description, Point size)
        {
            var control = this.CreateInternal(achievement, description);
            control.Width = size.X;
            control.HeightSizingMode = SizingMode.AutoSize;
            control.BuildControl();

            return control;
        }
    }
}
