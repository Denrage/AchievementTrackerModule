using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{

    public class AchievementCollectionControl : AchievementListControl<CollectionDescription, CollectionDescriptionEntry>
    {
        private readonly IExternalImageService externalImageService;

        public AchievementCollectionControl(
            IItemDetailWindowManager itemDetailWindowManager,
            IAchievementService achievementService,
            IFormattedLabelHtmlService formattedLabelHtmlService,
            IExternalImageService externalImageService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            CollectionDescription description)
            : base(itemDetailWindowManager, achievementService, formattedLabelHtmlService, contentsManager, achievement, description)
        {
            this.externalImageService = externalImageService;
        }

        protected override void ColorControl(Control control, bool achievementBitFinished)
        {
            if (control is ImageSpinner image)
            {
                image.Tint = achievementBitFinished
                           ? Microsoft.Xna.Framework.Color.White
                           : Microsoft.Xna.Framework.Color.FromNonPremultiplied(255, 255, 255, 50);
            }
        }

        protected override Control CreateEntryControl(int index, CollectionDescriptionEntry entry, Container parent)
        => new ImageSpinner(this.externalImageService.GetImageFromIndirectLink(entry.ImageUrl))
           {
               Parent = parent,
               Width = 32,
               Height = 32,
               ZIndex = 1,
           };
        

        protected override string GetDisplayName(CollectionDescriptionEntry entry) => entry?.DisplayName ?? string.Empty;

        protected override IEnumerable<CollectionDescriptionEntry> GetEntries(CollectionDescription description) => description?.EntryList ?? System.Array.Empty<CollectionDescriptionEntry>().ToList();
    }
}
