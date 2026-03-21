using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#if LVGLSHARP_FORMS
using DrawingColor = LVGLSharp.Drawing.Color;
#else
using DrawingColor = System.Drawing.Color;
#endif

namespace MusicWinFromsDemo;

public sealed class frmMusicDemo : Form
{
    private readonly record struct TrackInfo(
        string Title,
        string Artist,
        string Album,
        int DurationSeconds,
        string CoverFileName,
        byte AccentR,
        byte AccentG,
        byte AccentB,
        string Description);

    private static readonly IReadOnlyList<TrackInfo> s_tracks =
    [
        new("Midnight Circuit", "LVGLSharp Sessions", "Neon Avenue", 222, "cover_1.png", 255, 115, 64, "Warm synth lines, long roads, and a very clean control-layer UI."),
        new("Glass Harbor", "Northern Signals", "After Hours", 256, "cover_2.png", 91, 173, 255, "A calmer groove with plenty of space for seek, switch, and pause interactions."),
        new("Daybreak Relay", "Static Bloom", "First Light", 207, "cover_3.png", 104, 214, 168, "Bright, fast, and useful for checking track switching and progress updates."),
        new("Velvet Transfer", "Tape Motion", "City Loop", 244, "cover_2.png", 244, 189, 86, "A slower bridge track that keeps the playlist from feeling synthetic."),
        new("Blue Hour Frame", "Mono District", "Late Window", 198, "cover_1.png", 176, 148, 255, "A short closer for verifying wraparound behavior and replay state.")
    ];

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _headerLayout = new();
    private readonly FlowLayoutPanel _headerTextLayout = new();
    private readonly Label _headerKickerLabel = new();
    private readonly Label _headerTitleLabel = new();
    private readonly Label _modeLabel = new();

    private readonly TableLayoutPanel _contentLayout = new();
    private readonly TableLayoutPanel _playerCardLayout = new();
    private readonly Label _trackPillLabel = new();
    private readonly FlowLayoutPanel _metaLayout = new();
    private readonly Label _titleLabel = new();
    private readonly Label _artistLabel = new();
    private readonly Label _albumLabel = new();
    private readonly Label _descriptionLabel = new();
    private readonly FlowLayoutPanel _coverHostLayout = new();
    private readonly PictureBox _coverPictureBox = new();
    private readonly TableLayoutPanel _progressLayout = new();
    private readonly TrackBar _progressTrackBar = new();
    private readonly TableLayoutPanel _timeLayout = new();
    private readonly Label _elapsedLabel = new();
    private readonly Label _durationLabel = new();
    private readonly FlowLayoutPanel _transportLayout = new();
    private readonly Button _shuffleButton = new();
    private readonly Button _previousButton = new();
    private readonly Button _playPauseButton = new();
    private readonly Button _nextButton = new();
    private readonly Button _repeatButton = new();
    private readonly FlowLayoutPanel _footerLayout = new();
    private readonly Label _statusLabel = new();
    private readonly Label _hintLabel = new();

    private readonly TableLayoutPanel _queueCardLayout = new();
    private readonly Label _queueTitleLabel = new();
    private readonly Label _queueHintLabel = new();
    private readonly FlowLayoutPanel _playlistLayout = new();
    private readonly Label _queueFooterLabel = new();

    private readonly List<Button> _trackButtons = [];
    private readonly Random _random = new();

    private CancellationTokenSource? _playbackLoopCts;
    private bool _shuffleEnabled;
    private bool _repeatEnabled;
    private bool _isPlaying;
    private bool _suppressProgressChange;
    private int _currentTrackIndex;
    private int _currentSecond;

    public frmMusicDemo()
    {
        InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _playbackLoopCts?.Cancel();
            _playbackLoopCts?.Dispose();
            _playbackLoopCts = null;
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        _rootLayout.SuspendLayout();
        _headerLayout.SuspendLayout();
        _headerTextLayout.SuspendLayout();
        _contentLayout.SuspendLayout();
        _playerCardLayout.SuspendLayout();
        _metaLayout.SuspendLayout();
        _coverHostLayout.SuspendLayout();
        _progressLayout.SuspendLayout();
        _timeLayout.SuspendLayout();
        _transportLayout.SuspendLayout();
        _footerLayout.SuspendLayout();
        _queueCardLayout.SuspendLayout();
        _playlistLayout.SuspendLayout();

        Text = "MusicWinFromsDemo";
        ClientSize = new Size(1240, 820);
        BackColor = Rgb(12, 18, 28);

        Load += frmMusicWinFromsDemo_Load;
        SizeChanged += frmMusicWinFromsDemo_SizeChanged;

        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Padding = new Padding(18);
        _rootLayout.BackColor = Rgb(12, 18, 28);

        _headerLayout.ColumnCount = 2;
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        _headerLayout.RowCount = 1;
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _headerLayout.Dock = DockStyle.Fill;
        _headerLayout.Margin = Padding.Empty;
        _headerLayout.Padding = new Padding(18, 14, 18, 14);
        _headerLayout.BackColor = Rgb(21, 31, 47);

        _headerTextLayout.Dock = DockStyle.Fill;
        _headerTextLayout.Margin = Padding.Empty;

        _headerKickerLabel.Text = "LVGLSharp.WinForms Only";
        _headerKickerLabel.Size = new Size(280, 24);
        _headerKickerLabel.ForeColor = Rgb(132, 160, 196);
        _headerKickerLabel.Font = new Font("Segoe UI", 12F);

        _headerTitleLabel.Text = "MusicWinFromsDemo";
        _headerTitleLabel.Size = new Size(520, 42);
        _headerTitleLabel.ForeColor = Rgb(244, 247, 252);
        _headerTitleLabel.Font = new Font("Segoe UI", 26F);

        _headerTextLayout.Controls.Add(_headerKickerLabel);
        _headerTextLayout.Controls.Add(_headerTitleLabel);

        _modeLabel.Dock = DockStyle.Fill;
        _modeLabel.Text = "Loading playlist";
        _modeLabel.TextAlign = ContentAlignment.MiddleCenter;
        _modeLabel.ForeColor = Rgb(224, 231, 242);
        _modeLabel.Font = new Font("Segoe UI", 13F);

        _headerLayout.Controls.Add(_headerTextLayout, 0, 0);
        _headerLayout.Controls.Add(_modeLabel, 1, 0);

        _contentLayout.ColumnCount = 2;
        _contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63F));
        _contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
        _contentLayout.RowCount = 1;
        _contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _contentLayout.Dock = DockStyle.Fill;
        _contentLayout.Margin = new Padding(0, 18, 0, 0);

        _playerCardLayout.ColumnCount = 1;
        _playerCardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _playerCardLayout.RowCount = 6;
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 392F));
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        _playerCardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _playerCardLayout.Dock = DockStyle.Fill;
        _playerCardLayout.Margin = new Padding(0, 0, 10, 0);
        _playerCardLayout.Padding = new Padding(22);
        _playerCardLayout.BackColor = Rgb(21, 31, 47);

        _trackPillLabel.Text = "NOW PLAYING";
        _trackPillLabel.Size = new Size(180, 26);
        _trackPillLabel.ForeColor = Rgb(147, 174, 204);
        _trackPillLabel.Font = new Font("Segoe UI", 12F);

        _metaLayout.Dock = DockStyle.Fill;
        _metaLayout.Margin = Padding.Empty;

        _titleLabel.Text = "Select a track";
        _titleLabel.Size = new Size(520, 44);
        _titleLabel.ForeColor = Rgb(247, 249, 252);
        _titleLabel.Font = new Font("Segoe UI", 28F);

        _artistLabel.Size = new Size(520, 30);
        _artistLabel.ForeColor = Rgb(197, 208, 224);
        _artistLabel.Font = new Font("Segoe UI", 16F);

        _albumLabel.Size = new Size(520, 24);
        _albumLabel.ForeColor = Rgb(139, 160, 188);
        _albumLabel.Font = new Font("Segoe UI", 12F);

        _descriptionLabel.Size = new Size(560, 44);
        _descriptionLabel.ForeColor = Rgb(167, 182, 202);
        _descriptionLabel.Font = new Font("Segoe UI", 11F);

        _metaLayout.Controls.Add(_titleLabel);
        _metaLayout.Controls.Add(_artistLabel);
        _metaLayout.Controls.Add(_albumLabel);
        _metaLayout.Controls.Add(_descriptionLabel);

        _coverHostLayout.Dock = DockStyle.Fill;
        _coverHostLayout.Margin = Padding.Empty;
        _coverHostLayout.Padding = new Padding(0, 12, 0, 0);
        _coverHostLayout.BackColor = Rgb(16, 24, 36);

        _coverPictureBox.Size = new Size(360, 360);
        _coverPictureBox.Margin = new Padding(12);
        _coverPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

        _coverHostLayout.Controls.Add(_coverPictureBox);

        _progressLayout.ColumnCount = 1;
        _progressLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _progressLayout.RowCount = 2;
        _progressLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        _progressLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _progressLayout.Dock = DockStyle.Fill;
        _progressLayout.Margin = Padding.Empty;

        _progressTrackBar.Minimum = 0;
        _progressTrackBar.Maximum = 1;
        _progressTrackBar.Value = 0;
        _progressTrackBar.Dock = DockStyle.Fill;
        _progressTrackBar.Margin = new Padding(0, 4, 0, 0);
        _progressTrackBar.ValueChanged += progressTrackBar_ValueChanged;

        _timeLayout.ColumnCount = 2;
        _timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _timeLayout.RowCount = 1;
        _timeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _timeLayout.Dock = DockStyle.Fill;
        _timeLayout.Margin = Padding.Empty;

        _elapsedLabel.Dock = DockStyle.Fill;
        _elapsedLabel.Text = "00:00";
        _elapsedLabel.ForeColor = Rgb(200, 210, 224);
        _elapsedLabel.Font = new Font("Segoe UI", 11F);

        _durationLabel.Dock = DockStyle.Fill;
        _durationLabel.Text = "00:00";
        _durationLabel.TextAlign = ContentAlignment.MiddleRight;
        _durationLabel.ForeColor = Rgb(200, 210, 224);
        _durationLabel.Font = new Font("Segoe UI", 11F);

        _timeLayout.Controls.Add(_elapsedLabel, 0, 0);
        _timeLayout.Controls.Add(_durationLabel, 1, 0);

        _progressLayout.Controls.Add(_progressTrackBar, 0, 0);
        _progressLayout.Controls.Add(_timeLayout, 0, 1);

        _transportLayout.Dock = DockStyle.Fill;
        _transportLayout.Margin = Padding.Empty;

        ConfigureTransportButton(_shuffleButton, "Shuffle", shuffleButton_Click);
        ConfigureTransportButton(_previousButton, "Prev", previousButton_Click);
        ConfigureTransportButton(_playPauseButton, "Play", playPauseButton_Click, 120);
        ConfigureTransportButton(_nextButton, "Next", nextButton_Click);
        ConfigureTransportButton(_repeatButton, "Loop", repeatButton_Click);

        _transportLayout.Controls.Add(_shuffleButton);
        _transportLayout.Controls.Add(_previousButton);
        _transportLayout.Controls.Add(_playPauseButton);
        _transportLayout.Controls.Add(_nextButton);
        _transportLayout.Controls.Add(_repeatButton);

        _footerLayout.Dock = DockStyle.Fill;
        _footerLayout.Margin = Padding.Empty;
        _footerLayout.Padding = new Padding(0, 8, 0, 0);

        _statusLabel.Size = new Size(640, 28);
        _statusLabel.ForeColor = Rgb(228, 234, 243);
        _statusLabel.Font = new Font("Segoe UI", 12F);

        _hintLabel.Size = new Size(640, 24);
        _hintLabel.Text = "Try track switch, play/pause, shuffle, loop, and drag the progress bar.";
        _hintLabel.ForeColor = Rgb(143, 165, 191);
        _hintLabel.Font = new Font("Segoe UI", 10F);

        _footerLayout.Controls.Add(_statusLabel);
        _footerLayout.Controls.Add(_hintLabel);

        _playerCardLayout.Controls.Add(_trackPillLabel, 0, 0);
        _playerCardLayout.Controls.Add(_metaLayout, 0, 1);
        _playerCardLayout.Controls.Add(_coverHostLayout, 0, 2);
        _playerCardLayout.Controls.Add(_progressLayout, 0, 3);
        _playerCardLayout.Controls.Add(_transportLayout, 0, 4);
        _playerCardLayout.Controls.Add(_footerLayout, 0, 5);

        _queueCardLayout.ColumnCount = 1;
        _queueCardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _queueCardLayout.RowCount = 4;
        _queueCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _queueCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        _queueCardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _queueCardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _queueCardLayout.Dock = DockStyle.Fill;
        _queueCardLayout.Margin = new Padding(10, 0, 0, 0);
        _queueCardLayout.Padding = new Padding(20);
        _queueCardLayout.BackColor = Rgb(21, 31, 47);

        _queueTitleLabel.Text = "PLAYLIST";
        _queueTitleLabel.Size = new Size(180, 28);
        _queueTitleLabel.ForeColor = Rgb(147, 174, 204);
        _queueTitleLabel.Font = new Font("Segoe UI", 14F);

        _queueHintLabel.Text = "Each entry is a regular WinForms-style button mapped onto the LVGL host.";
        _queueHintLabel.Size = new Size(360, 44);
        _queueHintLabel.ForeColor = Rgb(167, 182, 202);
        _queueHintLabel.Font = new Font("Segoe UI", 11F);

        _playlistLayout.Dock = DockStyle.Fill;
        _playlistLayout.Margin = Padding.Empty;
        _playlistLayout.Padding = new Padding(0, 10, 0, 0);

        _queueFooterLabel.Size = new Size(360, 24);
        _queueFooterLabel.ForeColor = Rgb(133, 153, 179);
        _queueFooterLabel.Font = new Font("Segoe UI", 10F);

        _queueCardLayout.Controls.Add(_queueTitleLabel, 0, 0);
        _queueCardLayout.Controls.Add(_queueHintLabel, 0, 1);
        _queueCardLayout.Controls.Add(_playlistLayout, 0, 2);
        _queueCardLayout.Controls.Add(_queueFooterLabel, 0, 3);

        _contentLayout.Controls.Add(_playerCardLayout, 0, 0);
        _contentLayout.Controls.Add(_queueCardLayout, 1, 0);

        _rootLayout.Controls.Add(_headerLayout, 0, 0);
        _rootLayout.Controls.Add(_contentLayout, 0, 1);

        Controls.Add(_rootLayout);

        _playlistLayout.ResumeLayout(false);
        _queueCardLayout.ResumeLayout(false);
        _footerLayout.ResumeLayout(false);
        _transportLayout.ResumeLayout(false);
        _timeLayout.ResumeLayout(false);
        _progressLayout.ResumeLayout(false);
        _coverHostLayout.ResumeLayout(false);
        _metaLayout.ResumeLayout(false);
        _playerCardLayout.ResumeLayout(false);
        _contentLayout.ResumeLayout(false);
        _headerTextLayout.ResumeLayout(false);
        _headerLayout.ResumeLayout(false);
        _rootLayout.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void frmMusicWinFromsDemo_Load(object? sender, EventArgs e)
    {
        BuildPlaylistButtons();
        SelectTrack(0, startPlayback: true);

        _playbackLoopCts = new CancellationTokenSource();
        _ = PlaybackLoopAsync(_playbackLoopCts.Token);

        ApplyResponsiveLayout();
    }

    private void frmMusicWinFromsDemo_SizeChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ConfigureTransportButton(Button button, string text, EventHandler clickHandler, int width = 96)
    {
        button.Text = text;
        button.Size = new Size(width, 42);
        button.Margin = new Padding(0, 0, 12, 0);
        button.ForeColor = Rgb(245, 247, 252);
        button.BackColor = Rgb(51, 70, 96);
        button.Font = new Font("Segoe UI", 12F);
        button.Click += clickHandler;
    }

    private void BuildPlaylistButtons()
    {
        _playlistLayout.Controls.Clear();
        _trackButtons.Clear();

        for (int i = 0; i < s_tracks.Count; i++)
        {
            int trackIndex = i;
            TrackInfo track = s_tracks[i];
            Button button = new()
            {
                Size = new Size(360, 58),
                Margin = new Padding(0, 0, 0, 12),
                Text = BuildPlaylistButtonText(trackIndex, track, isCurrent: false),
                ForeColor = Rgb(219, 227, 239),
                BackColor = Rgb(36, 48, 68),
                Font = new Font("Segoe UI", 11F),
            };

            button.Click += (_, _) => SelectTrack(trackIndex, startPlayback: true);

            _trackButtons.Add(button);
            _playlistLayout.Controls.Add(button);
        }
    }

    private void playPauseButton_Click(object? sender, EventArgs e)
    {
        _isPlaying = !_isPlaying;
        UpdatePlaybackState();
    }

    private void previousButton_Click(object? sender, EventArgs e)
    {
        int previousIndex = _shuffleEnabled
            ? PickRandomTrackIndex(excluding: _currentTrackIndex)
            : (_currentTrackIndex - 1 + s_tracks.Count) % s_tracks.Count;

        SelectTrack(previousIndex, startPlayback: _isPlaying);
    }

    private void nextButton_Click(object? sender, EventArgs e)
    {
        MoveToNextTrack(startPlayback: _isPlaying);
    }

    private void shuffleButton_Click(object? sender, EventArgs e)
    {
        _shuffleEnabled = !_shuffleEnabled;
        UpdatePlaybackState();
    }

    private void repeatButton_Click(object? sender, EventArgs e)
    {
        _repeatEnabled = !_repeatEnabled;
        UpdatePlaybackState();
    }

    private void progressTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        if (_suppressProgressChange)
        {
            return;
        }

        _currentSecond = Math.Clamp(_progressTrackBar.Value, 0, s_tracks[_currentTrackIndex].DurationSeconds);
        UpdateProgressPresentation();
        UpdateStatusText($"Seeked to {FormatTime(_currentSecond)}");
    }

    private void SelectTrack(int trackIndex, bool startPlayback)
    {
        _currentTrackIndex = Math.Clamp(trackIndex, 0, s_tracks.Count - 1);
        _currentSecond = 0;
        _isPlaying = startPlayback;

        ApplyTrackPresentation();
        UpdatePlaybackState();
    }

    private void MoveToNextTrack(bool startPlayback)
    {
        int nextIndex = _shuffleEnabled
            ? PickRandomTrackIndex(excluding: _currentTrackIndex)
            : (_currentTrackIndex + 1) % s_tracks.Count;

        SelectTrack(nextIndex, startPlayback);
    }

    private int PickRandomTrackIndex(int excluding)
    {
        if (s_tracks.Count <= 1)
        {
            return excluding;
        }

        int nextIndex;
        do
        {
            nextIndex = _random.Next(0, s_tracks.Count);
        }
        while (nextIndex == excluding);

        return nextIndex;
    }

    private async Task PlaybackLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_isPlaying)
            {
                continue;
            }

            TrackInfo track = s_tracks[_currentTrackIndex];
            if (_currentSecond < track.DurationSeconds)
            {
                _currentSecond++;
                UpdateProgressPresentation();
            }

            if (_currentSecond < track.DurationSeconds)
            {
                continue;
            }

            if (_repeatEnabled)
            {
                _currentSecond = 0;
                UpdateProgressPresentation();
                UpdateStatusText($"Replaying {track.Title}");
                continue;
            }

            MoveToNextTrack(startPlayback: true);
        }
    }

    private void ApplyTrackPresentation()
    {
        TrackInfo track = s_tracks[_currentTrackIndex];

        _trackPillLabel.Text = $"NOW PLAYING  |  {_currentTrackIndex + 1:00}/{s_tracks.Count:00}";
        _titleLabel.Text = track.Title;
        _artistLabel.Text = track.Artist;
        _albumLabel.Text = $"{track.Album}  |  {FormatTime(track.DurationSeconds)}";
        _descriptionLabel.Text = track.Description;

        string? coverPath = ResolveCoverPath(track.CoverFileName);
        if (!string.IsNullOrWhiteSpace(coverPath))
        {
            _coverPictureBox.Load(coverPath);
        }
        else
        {
            _coverPictureBox.ImageLocation = null;
            _coverPictureBox.Image = null;
        }

        DrawingColor accentColor = Rgb(track.AccentR, track.AccentG, track.AccentB);

        _trackPillLabel.ForeColor = accentColor;
        _playPauseButton.BackColor = accentColor;
        _playerCardLayout.BackColor = Rgb(21, 31, 47);
        _coverHostLayout.BackColor = Rgb(16, 24, 36);

        UpdateProgressPresentation();
        RefreshPlaylistButtons();
        UpdateQueueFooter();
    }

    private void UpdatePlaybackState()
    {
        _playPauseButton.Text = _isPlaying ? "Pause" : "Play";
        _playPauseButton.ForeColor = Rgb(245, 247, 252);

        _shuffleButton.BackColor = _shuffleEnabled ? Rgb(74, 117, 186) : Rgb(51, 70, 96);
        _repeatButton.BackColor = _repeatEnabled ? Rgb(74, 117, 186) : Rgb(51, 70, 96);

        string stateText = _isPlaying ? "Playing" : "Paused";
        string sequenceText = _shuffleEnabled ? "Shuffle" : "Sequential";
        string repeatText = _repeatEnabled ? "Loop one" : "Loop off";

        _modeLabel.Text = $"{stateText}  |  {sequenceText}  |  {repeatText}";
        UpdateStatusText($"{stateText}: {s_tracks[_currentTrackIndex].Title}");
        UpdateQueueFooter();
    }

    private void UpdateProgressPresentation()
    {
        TrackInfo track = s_tracks[_currentTrackIndex];
        int clampedSecond = Math.Clamp(_currentSecond, 0, track.DurationSeconds);
        _currentSecond = clampedSecond;

        _suppressProgressChange = true;
        _progressTrackBar.Minimum = 0;
        _progressTrackBar.Maximum = Math.Max(1, track.DurationSeconds);
        _progressTrackBar.Value = Math.Min(clampedSecond, _progressTrackBar.Maximum);
        _suppressProgressChange = false;

        _elapsedLabel.Text = FormatTime(clampedSecond);
        _durationLabel.Text = FormatTime(track.DurationSeconds);
    }

    private void RefreshPlaylistButtons()
    {
        for (int i = 0; i < _trackButtons.Count; i++)
        {
            TrackInfo track = s_tracks[i];
            bool isCurrent = i == _currentTrackIndex;
            Button button = _trackButtons[i];

            button.Text = BuildPlaylistButtonText(i, track, isCurrent);
            button.BackColor = isCurrent
                ? Rgb(track.AccentR, track.AccentG, track.AccentB)
                : Rgb(36, 48, 68);
            button.ForeColor = isCurrent
                ? Rgb(255, 255, 255)
                : Rgb(219, 227, 239);
        }
    }

    private void UpdateStatusText(string text)
    {
        TrackInfo track = s_tracks[_currentTrackIndex];
        _statusLabel.Text = $"{text}  |  {FormatTime(_currentSecond)} / {FormatTime(track.DurationSeconds)}";
    }

    private void UpdateQueueFooter()
    {
        string stateText = _isPlaying ? "live" : "idle";
        string orderText = _shuffleEnabled ? "random order" : "fixed order";
        _queueFooterLabel.Text = $"Track {_currentTrackIndex + 1}/{s_tracks.Count}  |  {stateText}  |  {orderText}";
    }

    private void ApplyResponsiveLayout()
    {
        int availableHeroWidth = Math.Max(260, _playerCardLayout.ClientSize.Width - 44);
        int availableHeroHeight = Math.Max(240, _contentLayout.ClientSize.Height - 360);
        int coverSize = Math.Max(220, Math.Min(availableHeroWidth, availableHeroHeight));
        coverSize = Math.Min(coverSize, 420);

        _playerCardLayout.RowStyles[2] = new RowStyle(SizeType.Absolute, coverSize + 24F);
        _coverPictureBox.Size = new Size(coverSize, coverSize);
        _titleLabel.Size = new Size(availableHeroWidth, 44);
        _artistLabel.Size = new Size(availableHeroWidth, 30);
        _albumLabel.Size = new Size(availableHeroWidth, 24);
        _descriptionLabel.Size = new Size(availableHeroWidth, 44);
        _statusLabel.Size = new Size(availableHeroWidth, 28);
        _hintLabel.Size = new Size(availableHeroWidth, 24);

        int playlistWidth = Math.Max(240, _queueCardLayout.ClientSize.Width - 40);
        foreach (Button button in _trackButtons)
        {
            button.Size = new Size(playlistWidth, 58);
        }

        _queueHintLabel.Size = new Size(playlistWidth, 44);
        _queueFooterLabel.Size = new Size(playlistWidth, 24);

        _playerCardLayout.PerformLayout();
        _queueCardLayout.PerformLayout();
        _playlistLayout.PerformLayout();
    }

    private static string BuildPlaylistButtonText(int index, TrackInfo track, bool isCurrent)
    {
        string marker = isCurrent ? "> " : "  ";
        return $"{marker}{index + 1:00}  {track.Title}  |  {FormatTime(track.DurationSeconds)}";
    }

    private static string FormatTime(int totalSeconds)
    {
        TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
    }

    private static string? ResolveCoverPath(string fileName)
    {
        string[] localCandidates =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "music", fileName),
            Path.Combine(AppContext.BaseDirectory, fileName),
        ];

        foreach (string candidate in localCandidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        string currentDirectory = AppContext.BaseDirectory;
        for (int depth = 0; depth < 8; depth++)
        {
            string candidate = Path.Combine(
                currentDirectory,
                "libs",
                "lv_port_linux",
                "lvgl",
                "demos",
                "music",
                "assets",
                "png",
                "480_png",
                fileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            string? parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (string.IsNullOrWhiteSpace(parentDirectory))
            {
                break;
            }

            currentDirectory = parentDirectory;
        }

        return null;
    }

    private static DrawingColor Rgb(byte r, byte g, byte b)
    {
#if LVGLSHARP_FORMS
        return new DrawingColor(r, g, b);
#else
        return DrawingColor.FromArgb(r, g, b);
#endif
    }
}
