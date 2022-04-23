using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    public class AchievementTrackWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly Achievement achievement;
        private readonly AchievementService achievementService;
        private readonly AchievementControlProvider achievementControlProvider;
        private readonly Texture2D texture;

        public AchievementTrackWindow(ContentsManager contentsManager, Achievement achievement, AchievementService achievementService, AchievementControlProvider achievementControlProvider)
        {
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.achievementService = achievementService;
            this.achievementControlProvider = achievementControlProvider;
            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            this.Title = this.achievement.Name;
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 7 * 74, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 7 * 74, 600 - 30));


            var control = this.achievementControlProvider.GetAchievementControl(this.achievement, this.achievementService.Achievements.FirstOrDefault(x => x.Id == this.achievement.Id).Description, this.ContentRegion.Size);

            if (control is null)
            {
                return;
            }

            control.Parent = this;

        }

        protected override void OnShown(EventArgs e) => base.OnShown(e);

        public class AchievementControlProvider
        {
            private readonly Dictionary<Type, AchievementControlFactory> mapping = new Dictionary<Type, AchievementControlFactory>();

            public AchievementControlProvider(AchievementService achievementService, ItemDetailWindowFactory itemDetailWindowFactory)
            {
                this.mapping.Add(typeof(StringDescription), new AchievementTextControlFactory(achievementService));
                this.mapping.Add(typeof(CollectionDescription), new AchievementCollectionControlFactory(achievementService, itemDetailWindowFactory));
                this.mapping.Add(typeof(ObjectivesDescription), new AchievementObjectiveControlFactory(achievementService, itemDetailWindowFactory));
            }

            public Control GetAchievementControl(Achievement achievement, AchievementTableEntryDescription description, Point size)
            {
                if (this.mapping.TryGetValue(description.GetType(), out var factory))
                {
                    return factory.Create(achievement, description, size);
                }

                return null;
            }
        }

        public interface IControlFactory<T, TDescription>
            where T : IAchievementControl
        {
            T Create(Achievement achievement, TDescription description, Point size);
        }

        public abstract class AchievementControlFactory
        {
            public abstract Control Create(Achievement achievement, object description, Point size);
        }

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

        public class AchievementTextControlFactory : AchievementControlFactory<AchievementTextControl, StringDescription>
        {
            private readonly AchievementService achievementService;

            public AchievementTextControlFactory(AchievementService achievementService)
            {
                this.achievementService = achievementService;
            }

            protected override AchievementTextControl CreateInternal(Achievement achievement, StringDescription description)
                => new AchievementTextControl(achievement, description);
        }

        public class AchievementCollectionControlFactory : AchievementControlFactory<AchievementCollectionControl, CollectionDescription>
        {
            private readonly AchievementService achievementService;
            private readonly ItemDetailWindowFactory itemDetailWindowFactory;

            public AchievementCollectionControlFactory(AchievementService achievementService, ItemDetailWindowFactory itemDetailWindowFactory)
            {
                this.achievementService = achievementService;
                this.itemDetailWindowFactory = itemDetailWindowFactory;
            }

            protected override AchievementCollectionControl CreateInternal(Achievement achievement, CollectionDescription description)
                => new AchievementCollectionControl(achievement, this.itemDetailWindowFactory, description, this.achievementService);
        }

        public class AchievementObjectiveControlFactory : AchievementControlFactory<AchievementObjectivesControl, ObjectivesDescription>
        {
            private readonly AchievementService achievementService;
            private readonly ItemDetailWindowFactory itemDetailWindowFactory;

            public AchievementObjectiveControlFactory(AchievementService achievementService, ItemDetailWindowFactory itemDetailWindowFactory)
            {
                this.achievementService = achievementService;
                this.itemDetailWindowFactory = itemDetailWindowFactory;
            }

            protected override AchievementObjectivesControl CreateInternal(Achievement achievement, ObjectivesDescription description)
                => new AchievementObjectivesControl(achievement, this.itemDetailWindowFactory, description, this.achievementService);
        }

        public interface IAchievementControl
        {
            void BuildControl();

            Point Size { get; set; }
        }

        public class AchievementTextControl : FlowPanel, IAchievementControl
        {
            private readonly Achievement achievement;
            private readonly StringDescription description;

            public AchievementTextControl(Achievement achievement, StringDescription description)
            {
                this.achievement = achievement;
                this.description = description;

                this.FlowDirection = ControlFlowDirection.TopToBottom;
            }

            public void BuildControl()
            {
                if (!string.IsNullOrEmpty(this.description.GameText))
                {
                    new Label()
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
                    new Label()
                    {
                        Parent = this,
                        Width = this.ContentRegion.Width,
                        Text = this.description.GameHint,
                        TextColor = Microsoft.Xna.Framework.Color.LightGray,
                        AutoSizeHeight = true,
                        WrapText = true,
                    };
                }
            }
        }

        public class AchievementCollectionControl : FlowPanel, IAchievementControl
        {
            private readonly Achievement achievement;
            private readonly ItemDetailWindowFactory itemDetailWindowFactory;
            private readonly CollectionDescription description;
            private readonly AchievementService achievementService;
            private readonly CollectionAchievementTable achievementDetails;

            public AchievementCollectionControl(Achievement achievement, ItemDetailWindowFactory itemDetailWindowFactory, CollectionDescription description, AchievementService achievementService)
            {
                this.achievement = achievement;
                this.itemDetailWindowFactory = itemDetailWindowFactory;
                this.description = description;
                this.achievementService = achievementService;
                this.achievementDetails = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);

                this.FlowDirection = ControlFlowDirection.SingleTopToBottom;

            }

            public void BuildControl()
            {
                if (!string.IsNullOrEmpty(this.description.GameText))
                {
                    new Label()
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
                    new Label()
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
                    HeightSizingMode = SizingMode.AutoSize,
                };

                Task.Run(async () =>
                {
                    try
                    {
                        var counter = 0;
                        foreach (var item in this.description.EntryList)
                        {
                            var tint = !this.achievementService.HasFinishedAchievementBit(this.achievement.Id, item.Id);
                            var texture = this.achievementService.GetImage(item.ImageUrl);

                            var image = new Image()
                            {
                                Parent = panel,
                                Width = 64,
                                Height = 64,
                                Texture = texture,
                            };

                            if (tint)
                            {
                                image.Tint = Microsoft.Xna.Framework.Color.Gray;
                            }

                            var index = counter;
                            image.Click += (s, eventArgs) =>
                            {
                                var itemWindow = this.itemDetailWindowFactory.Create(item.DisplayName, this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index]);
                                itemWindow.Parent = GameService.Graphics.SpriteScreen;
                                itemWindow.Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2);
                                itemWindow.ToggleWindow();
                            };
                            counter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                });
            }
        }

        public class AchievementObjectivesControl : FlowPanel, IAchievementControl
        {
            private readonly Achievement achievement;
            private readonly ItemDetailWindowFactory itemDetailWindowFactory;
            private readonly ObjectivesDescription description;
            private readonly AchievementService achievementService;
            private readonly CollectionAchievementTable achievementDetails;

            public AchievementObjectivesControl(Achievement achievement, ItemDetailWindowFactory itemDetailWindowFactory, ObjectivesDescription description, AchievementService achievementService)
            {
                this.achievement = achievement;
                this.itemDetailWindowFactory = itemDetailWindowFactory;
                this.description = description;
                this.achievementService = achievementService;
                this.achievementDetails = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);

                this.FlowDirection = ControlFlowDirection.LeftToRight;
            }

            public void BuildControl()
            {
                if (!string.IsNullOrEmpty(this.description.GameText))
                {
                    new Label()
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
                    new Label()
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
                    HeightSizingMode = SizingMode.AutoSize,
                    ControlPadding = new Vector2(10f),
                };

                Task.Run(async () =>
                {
                    try
                    {
                        for (var i = 0; i < this.description.EntryList.Count; i++)
                        {
                            var label = new Label()
                            {
                                Parent = panel,
                                Width = 64,
                                Height = 64,
                                Text = (i + 1).ToString(),
                                Font = Content.DefaultFont18,
                                VerticalAlignment = VerticalAlignment.Middle,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                BackgroundColor = Microsoft.Xna.Framework.Color.DarkGray,
                            };

                            var index = i;
                            label.Click += (s, eventArgs) =>
                            {
                                var itemWindow = this.itemDetailWindowFactory.Create(this.description.EntryList[index].DisplayName, this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index]);
                                itemWindow.Parent = GameService.Graphics.SpriteScreen;
                                itemWindow.Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2);
                                itemWindow.ToggleWindow();
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                });
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

}
