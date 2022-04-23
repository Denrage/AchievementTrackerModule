using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IControlFactory<T, TDescription>
            where T : IAchievementControl
    {
        T Create(Achievement achievement, TDescription description, Point size);
    }
}
