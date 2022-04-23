using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule
{
    public interface IItemDetailWindowFactory
    {
        ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item);
    }

    public class ItemDetailWindowFactory : IItemDetailWindowFactory
    {
        private readonly ContentsManager contentsManager;
        private readonly AchievementService achievementService;
        private readonly AchievementTableEntryProvider achievementTableEntryProvider;

        public ItemDetailWindowFactory(ContentsManager contentsManager, AchievementService achievementService, AchievementTableEntryProvider achievementTableEntryProvider)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementTableEntryProvider = achievementTableEntryProvider;
        }

        public ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item)
            => new ItemDetailWindow(this.contentsManager, this.achievementService, name, columns, item, this.achievementTableEntryProvider);
    }

    public class ItemDetailWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly AchievementService achievementService;
        private readonly string name;
        private readonly string[] columns;
        private readonly List<CollectionAchievementTableEntry> item;
        private readonly AchievementTableEntryProvider achievementTableEntryProvider;
        private readonly Texture2D texture;

        public ItemDetailWindow(ContentsManager contentsManager, AchievementService achievementService, string name, string[] columns, List<CollectionAchievementTableEntry> item, AchievementTableEntryProvider achievementTableEntryProvider)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.name = name;
            this.columns = columns;
            this.item = item;
            this.achievementTableEntryProvider = achievementTableEntryProvider;
            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();

        }

        private void BuildWindow()
        {
            this.Title = this.name;
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 400), new Microsoft.Xna.Framework.Rectangle(0, 30, 300, 400 - 30));

            var panel = new FlowPanel()
            {
                Parent = this,
                Size = this.ContentRegion.Size,
                FlowDirection = ControlFlowDirection.TopToBottom,
            };

            for (var i = 0; i < this.item.Count; i++)
            {
                var innerPannel = new Panel()
                {
                    Parent = panel,
                    Width = panel.ContentRegion.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                };

                var label = new Label()
                {
                    Parent = innerPannel,
                    Width = (int)System.Math.Floor(0.3 * innerPannel.ContentRegion.Width),
                    Text = this.columns[i],
                };

                var control = this.achievementTableEntryProvider.GetTableEntryControl(this.item[i]);

                if (control != null)
                {
                    control.Parent = innerPannel;
                    control.Width = innerPannel.Width - label.Width;
                    control.Location = new Microsoft.Xna.Framework.Point(label.Width, 0);
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   this.texture,
                                   bounds);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }

    public class AchievementTableEntryProvider
    {
        private readonly Dictionary<Type, AchievementTableEntryFactory> mapping = new Dictionary<Type, AchievementTableEntryFactory>();

        public AchievementTableEntryProvider(AchievementService achievementService)
        {
            this.mapping.Add(typeof(CollectionAchievementTableNumberEntry), new AchievementTableNumberEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableCoinEntry), new AchievementTableCoinEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableItemEntry), new AchievementTableItemEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableLinkEntry), new AchievementTableLinkEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableMapEntry), new AchievementTableMapEntryFactory(achievementService));
            this.mapping.Add(typeof(CollectionAchievementTableStringEntry), new AchievementTableStringEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableEmptyEntry), new AchievementTableEmptyEntryFactory());
        }

        public Control GetTableEntryControl(CollectionAchievementTableEntry entry)
        {
            if (this.mapping.TryGetValue(entry.GetType(), out var factory))
            {
                return factory.Create(entry);
            }

            return null;
        }
    }

    public interface ITableEntryFactory<TEntry>
        where TEntry : CollectionAchievementTableEntry
    {
        Control Create(TEntry entry);
    }

    public abstract class AchievementTableEntryFactory
    {
        public abstract Control Create(object entry);
    }

    public abstract class AchievementTableEntryFactory<TEntry> : AchievementTableEntryFactory, ITableEntryFactory<TEntry>
        where TEntry : CollectionAchievementTableEntry
    {
        protected abstract Control CreateInternal(TEntry entry);

        public override Control Create(object entry)
            => this.Create((TEntry)entry);

        public Control Create(TEntry entry)
            => this.CreateInternal(entry);
    }

    public class AchievementTableNumberEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableNumberEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableNumberEntry entry)
            => new Label()
            {
                Text = entry.Number.ToString(),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }

    public class AchievementTableCoinEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableCoinEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableCoinEntry entry)
            => new Label()
            {
                Text = entry.ItemId + ": " + entry.Type.ToString(),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }

    public class AchievementTableItemEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableItemEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableItemEntry entry)
            => new Label()
            {
                Text = entry.Name,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }

    public class AchievementTableLinkEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableLinkEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableLinkEntry entry)
            => new Label()
            {
                Text = entry.Text,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }

    public class AchievementTableMapEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableMapEntry>
    {
        private readonly AchievementService achievementService;

        public AchievementTableMapEntryFactory(AchievementService achievementService)
        {
            this.achievementService = achievementService;
        }

        protected override Control CreateInternal(CollectionAchievementTableMapEntry entry)
            => new Image()
            {
                Texture = this.achievementService.GetDirectImageLink(entry.ImageLink),
                Width = 120,
                Height = 120,
            };
    }

    public class AchievementTableStringEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableStringEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableStringEntry entry)
            => new Label()
            {
                Text = entry.Text,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }

    public class AchievementTableEmptyEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableEmptyEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableEmptyEntry entry)
            => new Label()
            {
                Text = "EMPTY",
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
