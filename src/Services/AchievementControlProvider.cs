﻿using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.Services.Factories.AchievementControl;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    public class AchievementControlProvider : IAchievementControlProvider
    {
        private readonly Dictionary<Type, AchievementControlFactory> mapping = new Dictionary<Type, AchievementControlFactory>();

        public AchievementControlProvider(IAchievementService achievementService, IItemDetailWindowFactory itemDetailWindowFactory, ContentsManager contentsManager)
        {
            this.mapping.Add(typeof(StringDescription), new AchievementTextControlFactory());
            this.mapping.Add(typeof(CollectionDescription), new AchievementCollectionControlFactory(achievementService, itemDetailWindowFactory, contentsManager));
            this.mapping.Add(typeof(ObjectivesDescription), new AchievementObjectiveControlFactory(achievementService, itemDetailWindowFactory, contentsManager));
        }

        public Control GetAchievementControl(Achievement achievement, AchievementTableEntryDescription description, Point size)
            => this.mapping.TryGetValue(description.GetType(), out var factory) ? factory.Create(achievement, description, size) : null;
    }
}