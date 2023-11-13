using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Microsoft.Xna.Framework.Content;
using System;

namespace Denrage.AchievementTrackerModule.Services
{
    public class BlishTabNavigationService : IBlishTabNavigationService, IDisposable
    {
        private readonly Func<AchievementTrackerView> achievementOverviewView;
        private readonly OverlayService overlayService;
        private readonly ContentsManager contentsManager;
        private WindowTab blishhudOverlayTab;
        private bool disposedValue;

        public BlishTabNavigationService(OverlayService overlayService, ContentsManager contentsManager, AchievementTrackerView achievementTrackerView)
        {
            this.achievementOverviewView = () => achievementTrackerView;
            this.overlayService = overlayService;
            this.contentsManager = contentsManager;
        }

        public void NavigateToAchievementTab()
        {
            if (!overlayService.BlishHudWindow.Visible)
            {
                overlayService.BlishHudWindow.Show();
            }

            overlayService.BlishHudWindow.Navigate(achievementOverviewView());
        }

        public void Initialize()
        {
            this.blishhudOverlayTab = GameService.Overlay.BlishHudWindow.AddTab(
            "Achievement Tracker",
                this.contentsManager.GetTexture("achievement_icon.png"),
                achievementOverviewView);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    overlayService.BlishHudWindow.RemoveTab(blishhudOverlayTab);
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
