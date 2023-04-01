using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class SubPageInformationWindowManager : ISubPageInformationWindowManager, IDisposable
    {
        private readonly Dictionary<SubPageInformation, WindowBase2> subPageWindows = new Dictionary<SubPageInformation, WindowBase2>();
        private readonly GraphicsService graphicsService;
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly Func<IFormattedLabelHtmlService> getFormattedLabelHtmlSerice;
        private readonly IExternalImageService externalImageService;
        private IFormattedLabelHtmlService formattedLabelHtmlService;

        public SubPageInformationWindowManager(GraphicsService graphicsService, ContentsManager contentsManager, IAchievementService achievementService, Func<IFormattedLabelHtmlService> getFormattedLabelHtmlSerice, IExternalImageService externalImageService)
        {
            this.graphicsService = graphicsService;
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.getFormattedLabelHtmlSerice = getFormattedLabelHtmlSerice;
            this.externalImageService = externalImageService;
        }

        public void Create(SubPageInformation subPageInformation)
        {
            if (this.subPageWindows.TryGetValue(subPageInformation, out var window))
            {
                window.BringWindowToFront();
            }
            else
            {
                if (this.formattedLabelHtmlService == null)
                {
                    this.formattedLabelHtmlService = this.getFormattedLabelHtmlSerice();
                }

                window = new SubPageInformationWindow(this.contentsManager, this.achievementService, this.formattedLabelHtmlService, subPageInformation, this.externalImageService)
                {
                    Parent = this.graphicsService.SpriteScreen,
                };

                window.Hidden += (s, e) =>
                {
                    _ = this.subPageWindows.Remove(subPageInformation);
                    window.Dispose();
                };

                this.subPageWindows[subPageInformation] = window;

                window.Show();
            }
        }

        public void CloseWindows()
        {
            foreach (var item in this.subPageWindows)
            {
                item.Value.Dispose();
            }

            this.subPageWindows.Clear();
        }

        public void Dispose() 
            => this.CloseWindows();
    }
}
