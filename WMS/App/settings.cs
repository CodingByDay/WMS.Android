﻿using Plugin.Settings;
using Plugin.Settings.Abstractions;
using TrendNET.WMS.Core.Data;

namespace WMS.App
{
    public class Settings
    {
        public static int linkNo;
        public static string linkKey;

        private static ISettings AppSettings =>
            CrossSettings.Current;


        public static NameValueObject? openOrderSerial = null;


        public static bool login
        {
            get => AppSettings.GetValueOrDefault(nameof(login), false);
            set => AppSettings.AddOrUpdateValue(nameof(login), value);
        }

        public static Spinner locations { get; set; }

        public static string lastWarehouse
        {
            get => AppSettings.GetValueOrDefault(nameof(lastWarehouse), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(lastWarehouse), value);
        }

        public static string lastLocation
        {
            get => AppSettings.GetValueOrDefault(nameof(lastLocation), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(lastLocation), value);
        }

        public static string lastRegistration
        {
            get => AppSettings.GetValueOrDefault(nameof(lastRegistration), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(lastRegistration), value);
        }

        public static bool restart
        {
            get => AppSettings.GetValueOrDefault(nameof(restart), false);
            set => AppSettings.AddOrUpdateValue(nameof(restart), value);
        }

        public static string ID
        {
            get => AppSettings.GetValueOrDefault(nameof(ID), "");
            set => AppSettings.AddOrUpdateValue(nameof(ID), value);
        }

        public static string device
        {
            get => AppSettings.GetValueOrDefault(nameof(device), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(device), value);
        }

        public static bool tablet
        {
            get => AppSettings.GetValueOrDefault(nameof(tablet), false);
            set => AppSettings.AddOrUpdateValue(nameof(tablet), value);
        }

        public static string RootURL
        {
            get => AppSettings.GetValueOrDefault(nameof(RootURL), "");
            set => AppSettings.AddOrUpdateValue(nameof(RootURL), value);
        }

        public static string versionAPI
        {
            get => AppSettings.GetValueOrDefault(nameof(versionAPI), "");
            set => AppSettings.AddOrUpdateValue(nameof(versionAPI), value);
        }
    }
}