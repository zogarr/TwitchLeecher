﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadsViewVM : ViewModelBase, INavigationState
    {
        #region Fields

        private ITwitchService twitchService;
        private IDialogService dialogService;
        private INavigationService navigationService;
        private IEventAggregator eventAggregator;

        private ICommand retryDownloadCommand;
        private ICommand cancelDownloadCommand;
        private ICommand removeDownloadCommand;
        private ICommand viewCommand;
        private ICommand showLogCommand;

        private object commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadsViewVM(
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator)
        {
            this.twitchService = twitchService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.eventAggregator = eventAggregator;

            this.twitchService.PropertyChanged += TwitchService_PropertyChanged;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        double INavigationState.ScrollPosition { get; set; }

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get
            {
                return this.twitchService.Downloads;
            }
        }

        public ICommand RetryDownloadCommand
        {
            get
            {
                if (this.retryDownloadCommand == null)
                {
                    this.retryDownloadCommand = new DelegateCommand<string>(this.RetryDownload);
                }

                return this.retryDownloadCommand;
            }
        }

        public ICommand CancelDownloadCommand
        {
            get
            {
                if (this.cancelDownloadCommand == null)
                {
                    this.cancelDownloadCommand = new DelegateCommand<string>(this.CancelDownload);
                }

                return this.cancelDownloadCommand;
            }
        }

        public ICommand RemoveDownloadCommand
        {
            get
            {
                if (this.removeDownloadCommand == null)
                {
                    this.removeDownloadCommand = new DelegateCommand<string>(this.RemoveDownload);
                }

                return this.removeDownloadCommand;
            }
        }

        public ICommand ViewCommand
        {
            get
            {
                if (this.viewCommand == null)
                {
                    this.viewCommand = new DelegateCommand<string>(this.ViewVideo);
                }

                return this.viewCommand;
            }
        }

        public ICommand ShowLogCommand
        {
            get
            {
                if (this.showLogCommand == null)
                {
                    this.showLogCommand = new DelegateCommand<string>(this.ShowLog);
                }

                return this.showLogCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void RetryDownload(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        this.twitchService.Retry(id);
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void CancelDownload(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        this.twitchService.Cancel(id);
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void RemoveDownload(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        this.twitchService.Remove(id);
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ViewVideo(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideoDownload download = this.Downloads.Where(d => d.Id == id).FirstOrDefault();

                        if (download != null)
                        {
                            string folder = download.DownloadParams.Folder;

                            if (Directory.Exists(folder))
                            {
                                Process.Start(folder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowLog(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideoDownload download = this.Downloads.Where(d => d.Id == id).FirstOrDefault();

                        if (download != null)
                        {
                            this.navigationService.ShowLog(download);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.FirePropertyChanged(e.PropertyName);
        }

        #endregion EventHandlers
    }
}