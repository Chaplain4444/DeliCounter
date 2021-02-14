﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using ModernWpf.Controls;
using Slicer.Backend;

namespace Slicer.Controls
{
    /// <summary>
    ///     Interaction logic for ModManagementDrawer.xaml
    /// </summary>
    public partial class ModManagementDrawer : UserControl
    {
        public List<Mod> SelectedMods = new();

        public ModManagementDrawer()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            switch (SelectedMods.Count)
            {
                case 0:
                    UpdateShowNone();
                    break;
                case 1:
                    UpdateShowOne();
                    break;
                default:
                    UpdateShowMultiple();
                    break;
            }
        }

        private void UpdateShowNone()
        {
            // Hide all the text blocks
            TextBlockTitle.Text = "No mod selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;
            TextBlockDependenciesWrapper.Visibility = Visibility.Collapsed;
            TextBlockSourceWrapper.Visibility = Visibility.Collapsed;

            // Enable and make the buttons visible
            ButtonInstall.IsEnabled = false;
            ButtonInstall.Visibility = Visibility.Collapsed;
            ButtonUpdate.IsEnabled = false;
            ButtonUpdate.Visibility = Visibility.Collapsed;
            ButtonUninstall.IsEnabled = false;
            ButtonUninstall.Visibility = Visibility.Collapsed;
        }

        private void UpdateShowMultiple()
        {
            // Hide all the text blocks
            TextBlockTitle.Text = $"{SelectedMods.Count} mods selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;
            TextBlockDependenciesWrapper.Visibility = Visibility.Collapsed;
            TextBlockSourceWrapper.Visibility = Visibility.Collapsed;

            // Enable and make the buttons visible
            ButtonInstall.IsEnabled = true;
            ButtonInstall.Visibility = Visibility.Visible;
            ButtonUpdate.IsEnabled = true;
            ButtonUpdate.Visibility = Visibility.Visible;
            ButtonUninstall.IsEnabled = true;
            ButtonUninstall.Visibility = Visibility.Visible;
        }

        private void UpdateShowOne()
        {
            // Get the selected mod and latest version
            var mod = SelectedMods[0];
            var version = mod.Latest;

            // Update text block visibility
            TextBlockDescriptionWrapper.Visibility = Visibility.Visible;
            TextBlockLatestWrapper.Visibility = Visibility.Visible;
            TextBlockInstalledWrapper.Visibility = Visibility.Visible;
            TextBlockAuthorsWrapper.Visibility = Visibility.Visible;
            TextBlockDependenciesWrapper.Visibility = Visibility.Visible;
            TextBlockSourceWrapper.Visibility = Visibility.Visible;

            // Update the text values
            TextBlockTitle.Text = version.Name;
            TextBlockDescription.Text = version.Description;
            TextBlockAuthors.Text = string.Join(", ", version.Authors);
            TextBlockLatest.Text = version.VersionNumber.ToString();
            TextBlockInstalled.Text = mod.IsInstalled ? mod.Installed.VersionNumber.ToString() : "No";
            TextBlockDependencies.Text = string.Join(", ", version.Dependencies.Select(x => x.Key + " " + x.Value));
            TextBlockSource.Text = version.SourceUrl;
            HyperlinkSource.NavigateUri = new Uri(version.SourceUrl, UriKind.Absolute);

            // Update the action button visibility
            if (mod.IsInstalled)
            {
                ButtonInstall.IsEnabled = false;
                ButtonInstall.Visibility = Visibility.Collapsed;
                ButtonUninstall.IsEnabled = true;
                ButtonUninstall.Visibility = Visibility.Visible;
                ButtonUpdate.IsEnabled = !mod.UpToDate;
                ButtonUpdate.Visibility = mod.UpToDate ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                ButtonInstall.IsEnabled = true;
                ButtonInstall.Visibility = Visibility.Visible;
                ButtonUninstall.IsEnabled = false;
                ButtonUninstall.Visibility = Visibility.Collapsed;
                ButtonUpdate.IsEnabled = false;
                ButtonUpdate.Visibility = Visibility.Collapsed;
            }
        }

        private void HyperlinkSource_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var proc = new ProcessStartInfo("cmd", "/C start " + e.Uri.AbsoluteUri)
            {
                CreateNoWindow = true
            };
            Process.Start(proc);
            e.Handled = true;
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMods.Count == 1)
                ModManagement.InstallMod(SelectedMods[0]);
        }

        private async void ButtonUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMods.Count != 1) return;
            var mod = SelectedMods[0];

            var dependentCount = ModRepository.Instance.Mods.Values.Count(x => x.IsInstalled && x.Installed.Dependencies.ContainsKey(mod.Guid));
            if (dependentCount > 0)
            {
                // Confirm they want to do this
                var alert = new AlertDialogue("Are you sure?", $"You have {dependentCount} mod(s) installed which directly depend on this one, they will also be uninstalled. This operation is cascading.")
                {
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = "Cancel"
                    

                };
                var result = await alert.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
            }

            // Uninstall
            ModManagement.UninstallMod(mod);
        }
    }
}