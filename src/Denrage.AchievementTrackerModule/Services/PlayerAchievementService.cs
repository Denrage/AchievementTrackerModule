using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class PlayerAchievementService
    {
        private Task trackAchievementProgressTask;
        private CancellationTokenSource trackAchievementProgressCancellationTokenSource;
        private bool isLoaded = false;

        public Dictionary<int, List<int>> ManualCompletedAchievements { get; set; } = new Dictionary<int, List<int>>();

        public IEnumerable<AccountAchievement> PlayerAchievements { get; private set; }

        public event Action PlayerAchievementsLoaded;


        public PlayerAchievementService(Logger logger, IGw2WebApiV2Client gw2Client, IGw2ApiPermission gw2ApiPermission, IPersistanceService persistanceService)
        {
            this.logger = logger;
            this.gw2Client = gw2Client;
            this.gw2ApiPermission = gw2ApiPermission;
            this.persistanceService = persistanceService;

            this.persistanceService?.RegisterSaveDelegate(storage => storage.ManualCompletedAchievements = ManualCompletedAchievements);
        }

        public async Task LoadAsync()
        {
            if (!isLoaded)
            {
                isLoaded = true;
                // Do a better distinction. There is no need for persistence for other Party Members
                if (persistanceService != null)
                {
                    ManualCompletedAchievements = persistanceService.Get().ManualCompletedAchievements;
                }
                await LoadPlayerAchievements(default);
            }
        }

        public void ToggleManualCompleteStatus(int achievementId, int bit)
        {
            if (specialSnowflakeCompletedHandling.TryGetValue(achievementId, out var conversionFunc))
            {
                bit = conversionFunc(bit);
            }

            if (PlayerAchievements != null)
            {
                var achievement = PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);
                if (achievement != null)
                {
                    if (achievement.Done)
                    {
                        return;
                    }

                    if (achievement.Bits.Contains(bit))
                    {
                        return;
                    }
                }
            }

            if (!ManualCompletedAchievements.TryGetValue(achievementId, out var achievementBits))
            {
                achievementBits = new List<int>();
                ManualCompletedAchievements[achievementId] = achievementBits;
            }

            if (achievementBits.Contains(bit))
            {
                _ = achievementBits.Remove(bit);
            }
            else
            {
                achievementBits.Add(bit);
            }

            PlayerAchievementsLoaded?.Invoke();
        }

        public bool HasFinishedAchievement(int achievementId)
        {
            if (PlayerAchievements is null)
            {
                return false;
            }

            var achievement = PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);

            return !(achievement is null) && achievement.Done;
        }

        public bool HasFinishedAchievementBit(int achievementId, int positionIndex)
        {
            if (specialSnowflakeCompletedHandling.TryGetValue(achievementId, out var conversionFunc))
            {
                positionIndex = conversionFunc(positionIndex);
            }

            if (ManualCompletedAchievements.TryGetValue(achievementId, out var manualAchievement))
            {
                if (manualAchievement.Contains(positionIndex))
                {
                    return true;
                }
            }

            if (PlayerAchievements is null)
            {
                return false;
            }

            var achievement = PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);
            return !(achievement is null) && (achievement.Bits?.Contains(positionIndex) ?? false);
        }

        public async Task LoadPlayerAchievements(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            if (forceRefresh || PlayerAchievements == null)
            {
                if (gw2ApiPermission.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
                {
                    logger.Info("Refreshing Player Achievements");
                    try
                    {
                        PlayerAchievements = await gw2Client.Account.Achievements.GetAsync(cancellationToken);

                        foreach (var item in PlayerAchievements)
                        {
                            if (ManualCompletedAchievements.TryGetValue(item.Id, out var achievementBits))
                            {
                                if (item.Done)
                                {
                                    _ = ManualCompletedAchievements.Remove(item.Id);
                                }
                                else
                                {
                                    foreach (var bit in item.Bits)
                                    {
                                        if (achievementBits.Contains(bit))
                                        {
                                            _ = achievementBits.Remove(bit);
                                        }
                                    }
                                }
                            }
                        }

                        _ = Task.Run(() => PlayerAchievementsLoaded?.Invoke(), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Exception occured during refresh of player achievements. Skipping this time.");
                    }

                    TrackAchievementProgress();
                }
                else
                {
                    logger.Info("Permissions not granted");
                }
            }
        }

        private void TrackAchievementProgress()
        {
            if (trackAchievementProgressTask != null)
            {
                return;
            }

            trackAchievementProgressCancellationTokenSource = new CancellationTokenSource();
            trackAchievementProgressTask = Task.Run(TrackAchievementProgressMethod);
        }

        private async Task TrackAchievementProgressMethod()
        {
            try
            {
                while (true)
                {
                    trackAchievementProgressCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromMinutes(5), trackAchievementProgressCancellationTokenSource.Token);
                    await LoadPlayerAchievements(true, trackAchievementProgressCancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            { /* NOOP */ }
        }

        private readonly Dictionary<int, Func<int, int>> specialSnowflakeCompletedHandling = new Dictionary<int, Func<int, int>>()
        {
            { 5693, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 6 : index == 4 ? 7 : index == 5 ? 8 : -1  },
            { 5700, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5704, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5703, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 3 : index == 4 ? 4 : index == 5 ? 5 : index == 6 ? 7 : -1  },
            { 5697, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 3 : index == 3 ? 5 : index == 4 ? 6 : -1  },
            { 5688, index => index == 0 ? 3 : index == 1 ? 4 : index == 2 ? 6 : index == 3 ? 7 : index == 4 ? 8 : -1  },
            { 5709, index => index == 0 ? 0 : index == 1 ? 2 : index == 2 ? 4 : index == 3 ? 6 : index == 4 ? 7 : -1  },
            { 5698, index => index == 0 ? 0 : index == 1 ? 3 : index == 2 ? 4 : index == 3 ? 6 : -1  },
            { 5691, index => index == 0 ? 4 : index == 1 ? 5 : index == 2 ? 6 : index == 3 ? 7 : -1  },
            { 5708, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 5 : index == 4 ? 8 : -1  },
        };
        private readonly Logger logger;
        private readonly IGw2WebApiV2Client gw2Client;
        private readonly IGw2ApiPermission gw2ApiPermission;
        private readonly IPersistanceService persistanceService;

        public void Dispose()
            => trackAchievementProgressCancellationTokenSource.Cancel();
    }
}
