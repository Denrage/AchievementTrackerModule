using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public abstract class AchievementControlFactory<T, TDescription> : AchievementControlFactory, IControlFactory<T, TDescription>
        where T : Control, IAchievementControl
    {
        protected abstract T CreateInternal(Achievement achievement, TDescription description);

        public override Control Create(Achievement achievement, object description, Point size)
            => this.Create(achievement, (TDescription)description, size);

        public T Create(Achievement achievement, TDescription description, Point size)
        {
            var control = this.CreateInternal(achievement, description);
            control.Size = size;
            control.BuildControl();

            return control;
        }
    }
}
