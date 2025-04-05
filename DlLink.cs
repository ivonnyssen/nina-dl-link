﻿using IgorVonNyssen.NINA.DlLink.Properties;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Image.ImageData;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Settings = IgorVonNyssen.NINA.DlLink.Properties.Settings;

namespace IgorVonNyssen.NINA.DlLink {

    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options
    /// The base class "PluginBase" will populate all the necessary Manifest Meta Data out of the AssemblyInfo attributes. Please fill these accoringly
    ///
    /// An instance of this class will be created and set as datacontext on the plugin options tab in N.I.N.A. to be able to configure global plugin settings
    /// The user interface for the settings will be defined by a DataTemplate with the key having the naming convention "DlLink_Options" where DlLink corresponds to the AssemblyTitle - In this template example it is found in the Options.xaml
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class DlLink : PluginBase, INotifyPropertyChanged {
        private readonly IPluginOptionsAccessor pluginSettings;
        private readonly IProfileService profileService;

        [ImportingConstructor]
        public DlLink(IProfileService profileService, IOptionsVM options) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            // This helper class can be used to store plugin settings that are dependent on the current profile
            this.pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            this.profileService = profileService;
            // React on a changed profile
            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }

        public override Task Teardown() {
            // Make sure to unregister an event when the object is no longer in use. Otherwise garbage collection will be prevented.
            profileService.ProfileChanged -= ProfileService_ProfileChanged;

            return base.Teardown();
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            // Raise the event that this profile specific value has been changed due to the profile switch
            RaisePropertyChanged(nameof(DLServerAddress));
            RaisePropertyChanged(nameof(DLUserName));
            RaisePropertyChanged(nameof(DLPassword));
        }

        public string DLServerAddress {
            get {
                return pluginSettings.GetValueString(nameof(DLServerAddress), string.Empty);
            }
            set {
                pluginSettings.SetValueString(nameof(DLServerAddress), value);
                RaisePropertyChanged();
            }
        }

        public string DLUserName {
            get {
                return pluginSettings.GetValueString(nameof(DLUserName), string.Empty);
            }
            set {
                pluginSettings.SetValueString(nameof(DLUserName), value);
                RaisePropertyChanged();
            }
        }

        public string DLPassword {
            get {
                return pluginSettings.GetValueString(nameof(DLPassword), string.Empty);
            }
            set {
                pluginSettings.SetValueString(nameof(DLPassword), value);
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}