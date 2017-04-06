﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Win.PInvoke;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Win.IO;

namespace RepoZ.Api.Win
{
	public partial class Form1 : Form
	{
		private Timer _timer;
		private WindowFinder _finder;
		private WindowsRepositoryReader _reader;
		private DefaultRepositoryMonitor _monitor;
		private BindingList<Repository> _dataSource = new BindingList<Repository>();
		private WindowsExplorerHandler _explorerHandler;

		public Form1()
		{
			InitializeComponent();

			dataGridView1.AutoGenerateColumns = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_reader = new WindowsRepositoryReader();
			_finder = new WindowFinder(new IPathFinder[] { new WindowsExplorerPathFinder() });
			_explorerHandler = new WindowsExplorerHandler(_reader);

			_timer = new Timer() { Interval = 1000, Enabled = true };
			_timer.Tick += _timer_Tick;
			_timer.Start();

			_monitor = new DefaultRepositoryMonitor(new DefaultDriveEnumerator(), _reader, new DefaultRepositoryObserverFactory(_reader), new DefaultPathCrawlerFactory());
			_monitor.OnChangeDetected = (repo) => notifyRepoChange(repo);
			_monitor.Observe();

			dataGridView1.DataSource = _dataSource;
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			var windowPath = _finder.GetPathOfCurrentWindow();

			var repo = _reader.ReadRepository(windowPath?.Path);

			lblFound.Text = windowPath?.Path ?? "n/a";
			lblPath.Text = repo.Path;
			lblRepository.Text = repo.Name;
			lblGitBranch.Text = repo.CurrentBranch;

			string repoBranch = repo?.CurrentBranch ?? "";
			//_finder.SetW(windowPath.Handle, repoBranch);

			_explorerHandler.Pulse();
		}

		private void notifyRepoChange(Repository repo)
		{
			Action act = () =>
			{

				var rems = _dataSource.Where(r => r.Path == repo.Path).ToArray();

				for (int i = 0; i < rems.Length; i++)
					_dataSource.Remove(rems[i]);

				_dataSource.Add(repo);
			};

			this.BeginInvoke(act);
		}

		private void label4_Click(object sender, EventArgs e)
		{

		}
	}
}
