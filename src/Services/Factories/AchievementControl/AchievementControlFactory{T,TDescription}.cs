using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public abstract class AchievementControlFactory<T, TDescription> : AchievementControlFactory, IControlFactory<T, TDescription>
        where T : Panel, IAchievementControl
    {
        protected abstract T CreateInternal(AchievementTableEntry achievement, TDescription description);

        public override Control Create(AchievementTableEntry achievement, object description)
            => this.Create(achievement, (TDescription)description);

        public T Create(AchievementTableEntry achievement, TDescription description)
        {
            var control = this.CreateInternal(achievement, description);
            control.HeightSizingMode = SizingMode.AutoSize;
            control.BuildControl();

            return control;
        }
    }
}
