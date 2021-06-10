using System;
using NightOwl.macOS;
using NightOwl.Schedule;
using NightOwl.Schedule.ScheduledItems;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#if UNITY_EDITOR_WIN
using AutoDarkTheme.Windows;
#elif UNITY_EDITOR_OSX

#endif

namespace NightOwl
{
    [InitializeOnLoad]
    public static class NightOwlTheme
    {
#if UNITY_EDITOR_WIN
        private static RegistryMonitor registryMonitor;
#elif UNITY_EDITOR_OSX
        private static AppearanceMonitor appearanceMonitor;
#endif
        private static ScheduleTimer timer;


        static NightOwlTheme()
        {
            UserPreferences.PreferencesChanged += (sender, args) => MonitorThemeChanges();

            MonitorThemeChanges();
        }

        private static void MonitorThemeChanges()
        {
#if UNITY_EDITOR_WIN
            registryMonitor?.Stop();
#elif UNITY_EDITOR_OSX
            appearanceMonitor?.Stop();
#endif

            timer?.Stop();

            if (UserPreferences.IsEnabled)
            {
                if (UserPreferences.Mode == UserPreferences.AutoThemeMode.System)
                {
#if UNITY_EDITOR_WIN
                    // Windows: Watch system theme changes in registry
                    registryMonitor = new RegistryMonitor(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                    registryMonitor.RegChanged += (sender, args) => SetEditorThemeFromSystemTheme();
                    registryMonitor.Start();
#elif UNITY_EDITOR_OSX
                    // macOS
                    appearanceMonitor = new AppearanceMonitor();
                    appearanceMonitor.AppearanceChanged += (sender, args) =>
                    {
                        SetEditorThemeFromSystemTheme();
                    };
                    appearanceMonitor.Start();                    
#endif
                    // Set current system theme on start/when enabled
                    SetEditorThemeFromSystemTheme();
                }
                else if (UserPreferences.Mode == UserPreferences.AutoThemeMode.Time)
                {
                    var now = DateTime.Now.TimeOfDay;

                    // Check current time and set theme
                    if (UserPreferences.LightThemeTime < UserPreferences.DarkThemeTime)
                    {
                        if (now >= UserPreferences.LightThemeTime && now < UserPreferences.DarkThemeTime)
                        {
                            EditorThemeChanger.SetLightTheme();
                        }
                        else
                        {
                            EditorThemeChanger.SetDarkTheme();
                        }
                    }
                    else
                    {
                        if (now < UserPreferences.LightThemeTime && now >= UserPreferences.DarkThemeTime)
                        {
                            EditorThemeChanger.SetDarkTheme();
                        }
                        else
                        {
                            EditorThemeChanger.SetLightTheme();
                        }
                    }

                    // Schedule theme changes
                    var lightThemeSchedule = new ScheduledTime(EventTimeBase.Daily, UserPreferences.LightThemeTime);
                    var darkThemeSchedule = new ScheduledTime(EventTimeBase.Daily, UserPreferences.DarkThemeTime);
                    timer = new ScheduleTimer();
                    timer.AddJob(lightThemeSchedule, new Action(EditorThemeChanger.SetLightTheme));
                    timer.AddJob(darkThemeSchedule, new Action(EditorThemeChanger.SetDarkTheme));
                    timer.Start();
                }
            }
        }

        private static void SetEditorThemeFromSystemTheme()
        {
#if UNITY_EDITOR_WIN
            var appsUseLightTheme = registryMonitor.ReadDword("AppsUseLightTheme");

            if (appsUseLightTheme == 1)
            {
                EditorThemeChanger.SetLightTheme();
            }
            else if (appsUseLightTheme == 0)
            {
                EditorThemeChanger.SetDarkTheme();
            }
#elif UNITY_EDITOR_OSX
            var isDarkAppearance = AppearanceMonitor.IsDarkAppearance();

            if (isDarkAppearance)
            {
                EditorThemeChanger.SetDarkTheme();
            }
            else
            {
                EditorThemeChanger.SetLightTheme();
            }
#endif
        }
    }
}
