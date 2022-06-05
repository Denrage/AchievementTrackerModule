using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using System.Threading.Tasks;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableCoinEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableCoinEntry>
    {
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly AsyncTexture2D copperTexture;
        private readonly AsyncTexture2D silverTexture;
        private readonly AsyncTexture2D goldTexture;

        public AchievementTableCoinEntryFactory(Gw2ApiManager gw2ApiManager, ContentsManager contentsManager)
        {
            this.gw2ApiManager = gw2ApiManager;

            // TODO: Maybe get these from the gw2dat
            this.copperTexture = contentsManager.GetTexture("Copper_coin.png");
            this.silverTexture = contentsManager.GetTexture("Silver_coin.png");
            this.goldTexture = contentsManager.GetTexture("Gold_coin.png");
        }

        // TODO: Get prices from TradingPost
        protected override Control CreateInternal(CollectionAchievementTableCoinEntry entry)
        {
            var outerPanel = new Panel()
            {
                HeightSizingMode = SizingMode.AutoSize,
            };

            var result = new Label()
            {
                Text = (entry?.ItemId.ToString() ?? string.Empty) + ": " + (entry?.Type.ToString() ?? string.Empty),
                AutoSizeHeight = true,
                WrapText = true,
            };

            _ = Task.Run(async () =>
            {
                var price = await this.gw2ApiManager.Gw2ApiClient.V2.Commerce.Prices.GetAsync(entry.ItemId);
                var sellPrice = this.ConvertIntoCoinParts(price.Sells.UnitPrice);
                var buyPrice = this.ConvertIntoCoinParts(price.Buys.UnitPrice);
                var formattedLabel = new AchievementTrackerModule.UserInterface.Controls.FormattedLabel.FormattedLabelBuilder();

                if (entry.Type == CollectionAchievementTableCoinEntry.TradingPostType.Sell)
                {
                    _ = formattedLabel
                        .CreatePart($"{sellPrice.Gold}", x => x.SetSuffixImage(this.goldTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)))
                        .CreatePart($" {sellPrice.Silver}", x => x.SetSuffixImage(this.silverTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)))
                        .CreatePart($" {sellPrice.Copper}", x => x.SetSuffixImage(this.copperTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)));
                }
                else
                {
                    _ = formattedLabel
                        .CreatePart($"{buyPrice.Gold}", x => x.SetSuffixImage(this.goldTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)))
                        .CreatePart($" {buyPrice.Silver}", x => x.SetSuffixImage(this.silverTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)))
                        .CreatePart($" {buyPrice.Copper}", x => x.SetSuffixImage(this.copperTexture).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(15, 15)));
                }

                _ = formattedLabel.AutoSizeHeight().SetVerticalAlignment(VerticalAlignment.Middle);

                var label = formattedLabel.Build();
                outerPanel.Resized += (s, e) => label.Width = e.CurrentSize.X;
                label.Parent = outerPanel;
                label.Width = outerPanel.Width;
            });

            return outerPanel;
        }

        private (int Gold, int Silver, int Copper) ConvertIntoCoinParts(int copper)
        {
            var copperCoins = copper % 100;
            var silver = copper / 100 % 100;
            var gold = copper / 100 / 100;

            return (gold, silver, copperCoins);
        }
    }
}
