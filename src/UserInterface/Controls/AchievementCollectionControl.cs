using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementCollectionControl : FlowPanel, IAchievementControl
    {
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly IAchievementService achievementService;
        private readonly ContentsManager contentsManager;
        private readonly AchievementTableEntry achievement;
        private readonly CollectionDescription description;
        private readonly CollectionAchievementTable achievementDetails;

        public AchievementCollectionControl(
            IItemDetailWindowFactory itemDetailWindowFactory,
            IAchievementService achievementService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            CollectionDescription description)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.achievementService = achievementService;
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.description = description;

            this.achievementDetails = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.ControlPadding = new Vector2(7f);
        }

        public void BuildControl()
        {
            if (!string.IsNullOrEmpty(this.description.GameText))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameText,
                    AutoSizeHeight = true,
                    Width = this.ContentRegion.Width,
                    WrapText = true,
                };
            }

            if (!string.IsNullOrEmpty(this.description.GameHint))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameHint,
                    TextColor = Microsoft.Xna.Framework.Color.LightGray,
                    Width = this.ContentRegion.Width,
                    AutoSizeHeight = true,
                    WrapText = true,
                };
            }

            var panel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = this.ContentRegion.Width,
                ControlPadding = new Vector2(7f),
                HeightSizingMode = SizingMode.AutoSize,
            };

            _ = Task.Run(() =>
            {
                var counter = 0;
                var finishedAchievement = this.achievementService.HasFinishedAchievement(this.achievement.Id);

                foreach (var item in this.description.EntryList)
                {
                    var tint = !(finishedAchievement || this.achievementService.HasFinishedAchievementBit(this.achievement.Id, counter));
                    var texture = this.achievementService.GetImage(item.ImageUrl);

                    var imagePanel = new Panel()
                    {
                        Parent = panel,
                        BackgroundTexture = this.contentsManager.GetTexture("collection_item_background.png"),
                        Width = 71,
                        Height = 71,
                    };

                    var image = new Image()
                    {
                        Parent = imagePanel,
                        Width = 64,
                        Height = 64,
                        Texture = texture,
                        ZIndex = 1,
                    };

                    image.Location = new Point((imagePanel.Width - image.Width) / 2, (imagePanel.Height - image.Height) / 2);

                    if (tint)
                    {
                        image.Tint = Microsoft.Xna.Framework.Color.FromNonPremultiplied(255, 255, 255, 50);
                    }

                    var index = counter;
                    image.Click += (s, eventArgs) =>
                    {
                        var itemWindow = this.itemDetailWindowFactory.Create(item.DisplayName, this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index], this.achievementDetails.Link);
                        itemWindow.Parent = GameService.Graphics.SpriteScreen;
                        itemWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                        itemWindow.ToggleWindow();
                    };
                    counter++;
                }
            });
        }
    }
}
