﻿using CommunityToolkit.Mvvm.ComponentModel;
using Lively.Common;
using Lively.Common.Services;
using Lively.Grpc.Client;
using Lively.Models;
using Lively.UI.WinUI.Factories;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lively.UI.Shared.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUserSettingsClient userSettings;
        private readonly IDesktopCoreClient desktopCore;
        private readonly IDisplayManagerClient displayManager;
        private readonly IDispatcherService dispatcher;
        private readonly IAppThemeFactory themeFactory;

        public MainViewModel(IUserSettingsClient userSettings,
                             IDesktopCoreClient desktopCore,
                             IDispatcherService dispatcher,
                             IDisplayManagerClient displayManager,
                             IAppThemeFactory themeFactory)
        {
            this.userSettings = userSettings;
            this.desktopCore = desktopCore;
            this.displayManager = displayManager;
            this.themeFactory = themeFactory;
            this.dispatcher = dispatcher;

            _ = UpdateTheme();

            desktopCore.WallpaperChanged += (_, _) =>
            {
                if (userSettings.Settings.ApplicationThemeBackground == Models.Enums.AppThemeBackground.dynamic)
                {
                    _ = dispatcher.TryEnqueue(() =>
                        _ = UpdateTheme()
                    );
                }
            };

            IsUpdatedNotify = userSettings.Settings.IsUpdatedNotify;
        }

        [ObservableProperty]
        private bool isUpdatedNotify;

        [ObservableProperty]
        private string appThemeBackground = string.Empty;

        public async Task UpdateTheme()
        {
            switch (userSettings.Settings.ApplicationThemeBackground)
            {
                case Models.Enums.AppThemeBackground.default_mica:
                case Models.Enums.AppThemeBackground.default_acrylic:
                    {
                        AppThemeBackground = string.Empty;
                    }
                    break;
                case Models.Enums.AppThemeBackground.dynamic:
                    {
                        if (desktopCore.Wallpapers.Any())
                        {
                            var wallpaper = desktopCore.Wallpapers.FirstOrDefault(x => x.Display.Equals(displayManager.PrimaryMonitor));
                            if (wallpaper is null)
                            {
                                AppThemeBackground = string.Empty;
                            }
                            else
                            {
                                var userThemeDir = Path.Combine(wallpaper.LivelyInfoFolderPath, "lively_theme");
                                if (Directory.Exists(userThemeDir))
                                {
                                    var themeFile = string.Empty;
                                    try
                                    {
                                        themeFile = themeFactory.CreateFromDirectory(userThemeDir).File;
                                    }
                                    catch { }
                                    AppThemeBackground = themeFile;
                                }
                                else
                                {
                                    var fileName = new DirectoryInfo(wallpaper.LivelyInfoFolderPath).Name + ".jpg";
                                    var filePath = Path.Combine(Constants.CommonPaths.ThemeCacheDir, fileName);
                                    if (!File.Exists(filePath))
                                    {
                                        await desktopCore.TakeScreenshot(desktopCore.Wallpapers[0].Display.DeviceId, filePath);
                                    }
                                    AppThemeBackground = filePath;
                                }
                            }
                        }
                        else
                        {
                            AppThemeBackground = string.Empty;
                        }
                    }
                    break;
                case Models.Enums.AppThemeBackground.custom:
                    {
                        if (!string.IsNullOrWhiteSpace(userSettings.Settings.ApplicationThemeBackgroundPath))
                        {
                            var themeFile = string.Empty;
                            try
                            {
                                var theme = themeFactory.CreateFromDirectory(userSettings.Settings.ApplicationThemeBackgroundPath);
                                themeFile = theme.Type == ThemeType.picture ? theme.File : string.Empty;
                            }
                            catch { }
                            AppThemeBackground = themeFile;
                        }
                    }
                    break;
                default:
                    {
                        AppThemeBackground = string.Empty;
                    }
                    break;
            }
        }
    }
}
