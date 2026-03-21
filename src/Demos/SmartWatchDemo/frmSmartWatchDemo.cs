using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

#if WINDOWS
using DrawingColor = System.Drawing.Color;
#else
using DrawingColor = LVGLSharp.Drawing.Color;
#endif

using RenderColor = SixLabors.ImageSharp.Color;
using RenderEllipse = SixLabors.ImageSharp.Drawing.EllipsePolygon;
using RenderFont = SixLabors.Fonts.Font;
using RenderFontStyle = SixLabors.Fonts.FontStyle;
using RenderImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
using RenderImageExtensions = SixLabors.ImageSharp.ImageExtensions;
using RenderPixel = SixLabors.ImageSharp.PixelFormats.Rgba32;
using RenderPointF = SixLabors.ImageSharp.PointF;
using SixHorizontalAlignment = SixLabors.Fonts.HorizontalAlignment;
using SixRichTextOptions = SixLabors.ImageSharp.Drawing.Processing.RichTextOptions;
using SixVerticalAlignment = SixLabors.Fonts.VerticalAlignment;

namespace SmartWatchDemo;

public sealed class frmSmartWatchDemo : Form
{
    private const int SwipeArmThreshold = 14;

    private enum WatchPage
    {
        Quick,
        Apps,
        Home,
        Activity,
        Notifications,
        HeartRate,
        Weather,
        Sleep,
    }

    private enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
    }

    private readonly record struct AppItem(string Title, string Subtitle, byte R, byte G, byte B);
    private readonly record struct NotificationItem(string Title, string Time, string Body);

    private static readonly AppItem[] s_appItems =
    [
        new("Sports", "Move", 237, 46, 95),
        new("Activity", "Rings", 255, 136, 28),
        new("Heart", "BPM", 238, 74, 106),
        new("Weather", "Today", 54, 196, 163),
        new("Sleep", "Night", 80, 110, 255),
        new("SpO2", "Blood", 87, 176, 255),
        new("Stress", "Status", 164, 96, 255),
        new("Music", "Player", 119, 74, 255),
        new("Settings", "Prefs", 76, 88, 108),
    ];

    private static readonly NotificationItem[] s_notificationItems =
    [
        new("Phone", "09:18", "Missed call from Alex"),
        new("Calendar", "08:45", "Design review starts in 15 min"),
        new("Fitness", "08:20", "You are 1,358 steps away from target"),
        new("Music", "07:58", "Resume playlist on connected phone"),
    ];

    private static readonly string[] s_weatherConditions = ["Cloudy", "Sunny", "Rain", "Windy"];
    private static readonly string[] s_weatherIcons = ["Cloud", "Sun", "Rain", "Wind"];
    private static readonly string[] s_sleepStageTitles = ["Deep", "Light", "REM"];

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly FlowLayoutPanel _headerLayout = new();
    private readonly Label _titleLabel = new();
    private readonly Label _subtitleLabel = new();
    private readonly FlowLayoutPanel _stageLayout = new();
    private readonly FlowLayoutPanel _stageTopLayout = new();
    private readonly FlowLayoutPanel _stageMiddleLayout = new();
    private readonly FlowLayoutPanel _stageBottomLayout = new();
    private readonly Button _quickNavButton = new();
    private readonly Button _appsNavButton = new();
    private readonly Button _activityNavButton = new();
    private readonly Button _notificationsNavButton = new();
    private readonly Button _homeNavButton = new();
    private readonly Panel _watchShellPanel = new();
    private readonly Panel _watchViewportPanel = new();
    private readonly FlowLayoutPanel _footerLayout = new();
    private readonly Label _pageTitleLabel = new();
    private readonly Label _statusLabel = new();

    private readonly Panel _homePage = new();
    private readonly Panel _quickPage = new();
    private readonly Panel _appsPage = new();
    private readonly Panel _activityPage = new();
    private readonly Panel _notificationsPage = new();
    private readonly Panel _heartRatePage = new();
    private readonly Panel _weatherPage = new();
    private readonly Panel _sleepPage = new();

    private readonly Dictionary<WatchPage, Panel> _pageMap = new();
    private readonly Dictionary<WatchPage, Button> _navButtons = new();
    private readonly Dictionary<Button, bool> _quickToggleStates = new();
    private readonly HashSet<WatchPage> _attachedPages = new();
    private readonly Queue<WatchPage> _warmupPages = new();
    private readonly Random _random = new();
    private readonly Queue<string> _generatedWatchfacePaths = new();
    private readonly string _generatedAssetDirectory = Path.Combine(Path.GetTempPath(), "LVGLSharp.Forms.SmartWatchDemo", Guid.NewGuid().ToString("N"));

    private readonly PictureBox _homeDialPictureBox = new();
    private readonly Button _homeHeartButton = new();
    private readonly Button _homeWeatherButton = new();
    private readonly Button _homeSleepButton = new();
    private readonly Label _homeGestureHintLabel = new();
    private readonly Label _homeSummaryLabel = new();

    private readonly Label _quickClockLabel = new();
    private readonly Label _quickBatteryLabel = new();

    private readonly Label _activityStepsValueLabel = new();
    private readonly Label _activityMoveValueLabel = new();
    private readonly Label _activityCaloriesValueLabel = new();
    private readonly Label _activitySummaryLabel = new();
    private readonly Button _activityHeartButton = new();
    private readonly Button _activityWeatherButton = new();
    private readonly Button _activitySleepButton = new();

    private readonly Label _heartHeroValueLabel = new();
    private readonly Label _heartHeroStatusLabel = new();
    private readonly Label _heartRestingValueLabel = new();
    private readonly Label _heartVariabilityValueLabel = new();
    private readonly Panel[] _heartZoneTracks = [new(), new(), new()];
    private readonly Panel[] _heartZoneFills = [new(), new(), new()];
    private readonly Label[] _heartZoneValueLabels = [new(), new(), new()];

    private readonly Label _weatherHeroTempLabel = new();
    private readonly Label _weatherHeroConditionLabel = new();
    private readonly Label _weatherHeroRangeLabel = new();
    private readonly Label[] _weatherForecastTimeLabels = [new(), new(), new()];
    private readonly Label[] _weatherForecastTempLabels = [new(), new(), new()];
    private readonly Label _weatherHumidityLabel = new();
    private readonly Label _weatherWindLabel = new();
    private readonly Label _weatherUvLabel = new();

    private readonly Label _sleepHeroDurationLabel = new();
    private readonly Label _sleepHeroScoreLabel = new();
    private readonly Panel[] _sleepStageTracks = [new(), new(), new()];
    private readonly Panel[] _sleepStageFills = [new(), new(), new()];
    private readonly Label[] _sleepStageValueLabels = [new(), new(), new()];
    private readonly Label _sleepBedtimeLabel = new();
    private readonly Label _sleepWakeLabel = new();

    private int _heartRate = 80;
    private int _batteryPercent = 93;
    private int _stepCount = 1358;
    private int _activeCalories = 478;
    private int _movePercent = 61;
    private int _exerciseMinutes = 42;
    private int _standHours = 9;
    private int _sleepMinutes = 428;
    private int _sleepScore = 92;
    private int _weatherTempC = 26;
    private int _weatherConditionIndex;
    private int _watchfaceRenderSerial;
    private int _lastRenderedWatchfaceSecond = -1;
    private Size _lastRenderedWatchfaceSize = Size.Empty;
    private WatchPage _currentPage = WatchPage.Home;
    private WatchPage _currentTopLevelPage = WatchPage.Home;
    private WatchPage _detailReturnPage = WatchPage.Home;

    private bool _swipeTracking;
    private bool _swipeDragging;
    private Point _swipeStart;
    private WatchPage _swipeSourcePage;
    private WatchPage _swipeTargetPage;
    private SwipeDirection _swipeDirection;
    private long _ignoreClicksUntilTick;
    private bool _layoutApplying;

    private long _lastDynamicUpdateStamp = -1;
    private long _lastWarmupTick = -1;

    public frmSmartWatchDemo()
    {
        Directory.CreateDirectory(_generatedAssetDirectory);
        InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TryDeleteGeneratedAssets();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        _watchShellPanel.SuspendLayout();
        _watchViewportPanel.SuspendLayout();

        Text = "SmartWatchDemo";
        ClientSize = new Size(720, 780);
        MinimumSize = new Size(680, 720);
        BackColor = Rgb(9, 14, 22);

        _titleLabel.Text = "Smart Watch Demo";
        _titleLabel.Location = new Point(0, 0);
        _titleLabel.Size = new Size(320, 34);
        _titleLabel.ForeColor = Rgb(244, 247, 252);
        _titleLabel.Font = new Font("Segoe UI", 24F);

        _subtitleLabel.Text = "Vivid clock home + live swipe tiles + heart / weather / sleep cards";
        _subtitleLabel.Location = new Point(0, 38);
        _subtitleLabel.Size = new Size(560, 22);
        _subtitleLabel.ForeColor = Rgb(135, 157, 184);
        _subtitleLabel.Font = new Font("Segoe UI", 11F);

        ConfigureNavButton(_quickNavButton, "Quick", (_, _) => ShowTopLevelPage(WatchPage.Quick, "Top tile opened"));
        ConfigureNavButton(_appsNavButton, "Apps", (_, _) => ShowTopLevelPage(WatchPage.Apps, "Left tile opened"));
        ConfigureNavButton(_activityNavButton, "Activity", (_, _) => ShowTopLevelPage(WatchPage.Activity, "Right tile opened"));
        ConfigureNavButton(_notificationsNavButton, "Notify", (_, _) => ShowTopLevelPage(WatchPage.Notifications, "Bottom tile opened"));
        ConfigureNavButton(_homeNavButton, "Home", (_, _) => ReturnHome());

        _quickNavButton.Size = new Size(120, 40);
        _appsNavButton.Size = new Size(100, 40);
        _activityNavButton.Size = new Size(100, 40);
        _notificationsNavButton.Size = new Size(120, 40);
        _homeNavButton.Size = new Size(120, 40);

        _watchShellPanel.Margin = new Padding(0);
        _watchShellPanel.BackColor = Rgb(18, 24, 36);
        _watchShellPanel.EnableLvglInputEvents = false;

        _watchViewportPanel.BackColor = Rgb(0, 0, 0);

        _homeNavButton.Location = new Point(0, 10);
        _pageTitleLabel.Size = new Size(220, 26);
        _pageTitleLabel.ForeColor = Rgb(238, 241, 246);
        _pageTitleLabel.Font = new Font("Segoe UI", 16F);

        _pageTitleLabel.Location = new Point(140, 8);
        _statusLabel.Size = new Size(540, 20);
        _statusLabel.ForeColor = Rgb(123, 145, 170);
        _statusLabel.Font = new Font("Segoe UI", 10F);
        _statusLabel.Location = new Point(140, 36);

        InitializeHomePage();
        InitializeQuickPage();
        InitializeAppsPage();
        InitializeActivityPage();
        InitializeNotificationsPage();
        InitializeHeartRatePage();
        InitializeWeatherPage();
        InitializeSleepPage();

        _pageMap.Add(WatchPage.Home, _homePage);
        _pageMap.Add(WatchPage.Quick, _quickPage);
        _pageMap.Add(WatchPage.Apps, _appsPage);
        _pageMap.Add(WatchPage.Activity, _activityPage);
        _pageMap.Add(WatchPage.Notifications, _notificationsPage);
        _pageMap.Add(WatchPage.HeartRate, _heartRatePage);
        _pageMap.Add(WatchPage.Weather, _weatherPage);
        _pageMap.Add(WatchPage.Sleep, _sleepPage);

        _navButtons.Add(WatchPage.Quick, _quickNavButton);
        _navButtons.Add(WatchPage.Apps, _appsNavButton);
        _navButtons.Add(WatchPage.Home, _homeNavButton);
        _navButtons.Add(WatchPage.Activity, _activityNavButton);
        _navButtons.Add(WatchPage.Notifications, _notificationsNavButton);

        foreach ((WatchPage pageKey, Panel page) in _pageMap)
        {
            page.Visible = false;

            if (pageKey == WatchPage.Home)
            {
                _watchViewportPanel.Controls.Add(page);
                _attachedPages.Add(pageKey);
            }
        }

        AttachSwipeHandlersRecursive(_watchViewportPanel);
        Controls.Add(_titleLabel);
        Controls.Add(_subtitleLabel);
        Controls.Add(_quickNavButton);
        Controls.Add(_appsNavButton);
        Controls.Add(_activityNavButton);
        Controls.Add(_notificationsNavButton);
        Controls.Add(_watchShellPanel);
        Controls.Add(_watchViewportPanel);
        Controls.Add(_homeNavButton);
        Controls.Add(_pageTitleLabel);
        Controls.Add(_statusLabel);

        Load += frmSmartWatchDemo_Load;
        SizeChanged += frmSmartWatchDemo_SizeChanged;

        _watchViewportPanel.ResumeLayout(false);
        _watchShellPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void InitializeHomePage()
    {
        _homePage.SuspendLayout();
        _homePage.BackColor = Rgb(0, 0, 0);
        _homePage.Padding = new Padding(16);

        _homeDialPictureBox.BackColor = Rgb(0, 0, 0);
        _homeDialPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

        ConfigureActionButton(_homeHeartButton, Rgb(244, 79, 112), (_, _) => OpenDetailPage(WatchPage.HeartRate, WatchPage.Home, "Heart rate card opened"));
        ConfigureActionButton(_homeWeatherButton, Rgb(62, 194, 170), (_, _) => OpenDetailPage(WatchPage.Weather, WatchPage.Home, "Weather card opened"));
        ConfigureActionButton(_homeSleepButton, Rgb(95, 118, 255), (_, _) => OpenDetailPage(WatchPage.Sleep, WatchPage.Home, "Sleep card opened"));

        _homeGestureHintLabel.ForeColor = Rgb(163, 176, 194);
        _homeGestureHintLabel.Font = new Font("Segoe UI", 9F);
        _homeGestureHintLabel.TextAlign = ContentAlignment.MiddleCenter;
        _homeGestureHintLabel.Text = "Drag left / right / up / down on the watch face";

        _homeSummaryLabel.ForeColor = Rgb(123, 145, 170);
        _homeSummaryLabel.Font = new Font("Segoe UI", 10F);
        _homeSummaryLabel.TextAlign = ContentAlignment.MiddleCenter;

        _homePage.Controls.Add(_homeDialPictureBox);
        _homePage.Controls.Add(_homeHeartButton);
        _homePage.Controls.Add(_homeWeatherButton);
        _homePage.Controls.Add(_homeSleepButton);
        _homePage.Controls.Add(_homeGestureHintLabel);
        _homePage.Controls.Add(_homeSummaryLabel);
        _homePage.ResumeLayout(false);
    }

    private void InitializeQuickPage()
    {
        _quickPage.SuspendLayout();
        _quickPage.BackColor = Rgb(0, 0, 0);
        _quickPage.Controls.Add(CreatePageTitle("Quick Center", "Status bar + quick toggles"));

        _quickClockLabel.Location = new Point(28, 78);
        _quickClockLabel.Size = new Size(160, 44);
        _quickClockLabel.ForeColor = Rgb(245, 247, 252);
        _quickClockLabel.Font = new Font("Segoe UI", 28F);

        _quickBatteryLabel.Location = new Point(228, 86);
        _quickBatteryLabel.Size = new Size(120, 30);
        _quickBatteryLabel.TextAlign = ContentAlignment.MiddleRight;
        _quickBatteryLabel.ForeColor = Rgb(111, 196, 255);
        _quickBatteryLabel.Font = new Font("Segoe UI", 14F);

        _quickPage.Controls.Add(_quickClockLabel);
        _quickPage.Controls.Add(_quickBatteryLabel);

        (string Title, bool Toggled)[] quickButtons =
        [
            ("Wrist", true),
            ("BT Link", true),
            ("Bright", true),
            ("DND", false),
            ("Phone", false),
            ("Setting", false),
        ];

        for (int i = 0; i < quickButtons.Length; i++)
        {
            int column = i % 3;
            int row = i / 3;
            Button button = new()
            {
                Location = new Point(26 + (column * 112), 152 + (row * 96)),
                Size = new Size(96, 76),
                Text = quickButtons[i].Title,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Rgb(244, 247, 252),
                UseVisualStyleBackColor = false,
            };

            _quickToggleStates.Add(button, quickButtons[i].Toggled);
            button.Click += quickToggleButton_Click;
            _quickPage.Controls.Add(button);
        }

        _quickPage.ResumeLayout(false);
    }

    private void InitializeAppsPage()
    {
        _appsPage.SuspendLayout();
        _appsPage.BackColor = Rgb(0, 0, 0);
        _appsPage.Controls.Add(CreatePageTitle("App Grid", "Swipe-left launcher with direct health cards"));

        for (int i = 0; i < s_appItems.Length; i++)
        {
            AppItem item = s_appItems[i];
            int column = i % 3;
            int row = i / 3;

            Button button = new()
            {
                Location = new Point(22 + (column * 114), 96 + (row * 88)),
                Size = new Size(100, 68),
                Text = $"{item.Title}\n{item.Subtitle}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Rgb(255, 255, 255),
                BackColor = Rgb(item.R, item.G, item.B),
                UseVisualStyleBackColor = false,
            };

            button.Click += (_, _) => LaunchApp(item.Title);
            _appsPage.Controls.Add(button);
        }

        _appsPage.ResumeLayout(false);
    }

    private void InitializeActivityPage()
    {
        _activityPage.SuspendLayout();
        _activityPage.BackColor = Rgb(0, 0, 0);
        _activityPage.Controls.Add(CreatePageTitle("Daily Rings", "Swipe-right tile with live health shortcuts"));

        Panel heroCard = CreateCardPanel(new Point(20, 88), new Size(344, 92), Rgb(19, 27, 40));

        Label stepsTitle = CreateCardCaption("Steps", new Point(16, 14), new Size(80, 18), 12F);
        _activityStepsValueLabel.Location = new Point(16, 34);
        _activityStepsValueLabel.Size = new Size(126, 34);
        _activityStepsValueLabel.ForeColor = Rgb(255, 255, 255);
        _activityStepsValueLabel.Font = new Font("Segoe UI", 24F);

        Label moveTitle = CreateCardCaption("Move", new Point(166, 14), new Size(70, 18), 12F);
        _activityMoveValueLabel.Location = new Point(166, 34);
        _activityMoveValueLabel.Size = new Size(70, 28);
        _activityMoveValueLabel.ForeColor = Rgb(86, 224, 165);
        _activityMoveValueLabel.Font = new Font("Segoe UI", 20F);

        Label caloriesTitle = CreateCardCaption("Calories", new Point(250, 14), new Size(78, 18), 12F);
        _activityCaloriesValueLabel.Location = new Point(250, 34);
        _activityCaloriesValueLabel.Size = new Size(78, 28);
        _activityCaloriesValueLabel.ForeColor = Rgb(255, 184, 90);
        _activityCaloriesValueLabel.Font = new Font("Segoe UI", 20F);

        _activitySummaryLabel.Location = new Point(16, 68);
        _activitySummaryLabel.Size = new Size(312, 18);
        _activitySummaryLabel.ForeColor = Rgb(135, 157, 184);
        _activitySummaryLabel.Font = new Font("Segoe UI", 9F);

        heroCard.Controls.Add(stepsTitle);
        heroCard.Controls.Add(_activityStepsValueLabel);
        heroCard.Controls.Add(moveTitle);
        heroCard.Controls.Add(_activityMoveValueLabel);
        heroCard.Controls.Add(caloriesTitle);
        heroCard.Controls.Add(_activityCaloriesValueLabel);
        heroCard.Controls.Add(_activitySummaryLabel);

        ConfigureFeatureButton(_activityHeartButton, "Heart", "Live BPM", Rgb(242, 72, 113), (_, _) => OpenDetailPage(WatchPage.HeartRate, WatchPage.Activity, "Heart rate card opened"));
        ConfigureFeatureButton(_activityWeatherButton, "Weather", "Today", Rgb(62, 194, 170), (_, _) => OpenDetailPage(WatchPage.Weather, WatchPage.Activity, "Weather card opened"));
        ConfigureFeatureButton(_activitySleepButton, "Sleep", "Last night", Rgb(95, 118, 255), (_, _) => OpenDetailPage(WatchPage.Sleep, WatchPage.Activity, "Sleep card opened"));

        _activityHeartButton.Location = new Point(20, 194);
        _activityWeatherButton.Location = new Point(136, 194);
        _activitySleepButton.Location = new Point(252, 194);

        Panel bottomCard = CreateCardPanel(new Point(20, 292), new Size(344, 64), Rgb(17, 24, 36));

        Label bottomTitle = CreateCardCaption("Coach", new Point(16, 12), new Size(80, 18), 12F);
        Label bottomText = new()
        {
            Location = new Point(16, 32),
            Size = new Size(312, 20),
            Text = "Move ring closes fastest after a 10 min walk right now.",
            ForeColor = Rgb(179, 190, 206),
            Font = new Font("Segoe UI", 9F),
        };

        bottomCard.Controls.Add(bottomTitle);
        bottomCard.Controls.Add(bottomText);

        _activityPage.Controls.Add(heroCard);
        _activityPage.Controls.Add(_activityHeartButton);
        _activityPage.Controls.Add(_activityWeatherButton);
        _activityPage.Controls.Add(_activitySleepButton);
        _activityPage.Controls.Add(bottomCard);
        _activityPage.ResumeLayout(false);
    }

    private void InitializeNotificationsPage()
    {
        _notificationsPage.SuspendLayout();
        _notificationsPage.BackColor = Rgb(0, 0, 0);
        _notificationsPage.Controls.Add(CreatePageTitle("Notifications", "Swipe-down stack with compact cards"));

        for (int i = 0; i < s_notificationItems.Length; i++)
        {
            NotificationItem item = s_notificationItems[i];
            Panel card = CreateCardPanel(new Point(20, 88 + (i * 70)), new Size(344, 58), Rgb(17, 24, 36));

            Label title = new()
            {
                Location = new Point(14, 10),
                Size = new Size(180, 18),
                Text = item.Title,
                ForeColor = Rgb(241, 245, 250),
                Font = new Font("Segoe UI", 11F),
            };

            Label time = new()
            {
                Location = new Point(252, 10),
                Size = new Size(76, 18),
                Text = item.Time,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Rgb(128, 147, 171),
                Font = new Font("Segoe UI", 9F),
            };

            Label body = new()
            {
                Location = new Point(14, 30),
                Size = new Size(314, 18),
                Text = item.Body,
                ForeColor = Rgb(171, 184, 202),
                Font = new Font("Segoe UI", 9F),
            };

            card.Controls.Add(title);
            card.Controls.Add(time);
            card.Controls.Add(body);
            _notificationsPage.Controls.Add(card);
        }

        _notificationsPage.ResumeLayout(false);
    }

    private void InitializeHeartRatePage()
    {
        _heartRatePage.SuspendLayout();
        _heartRatePage.BackColor = Rgb(0, 0, 0);
        _heartRatePage.EnableLvglInputEvents = false;
        CreateDetailHeader(_heartRatePage, "Heart Rate", "Live tracking + quick zones");

        Panel heroCard = CreateCardPanel(new Point(20, 84), new Size(344, 92), Rgb(22, 28, 39));
        _heartHeroValueLabel.Location = new Point(18, 14);
        _heartHeroValueLabel.Size = new Size(122, 48);
        _heartHeroValueLabel.ForeColor = Rgb(255, 255, 255);
        _heartHeroValueLabel.Font = new Font("Segoe UI", 40F);

        Label bpmLabel = new()
        {
            Location = new Point(144, 28),
            Size = new Size(60, 22),
            Text = "BPM",
            ForeColor = Rgb(255, 120, 146),
            Font = new Font("Segoe UI", 14F),
        };

        _heartHeroStatusLabel.Location = new Point(18, 62);
        _heartHeroStatusLabel.Size = new Size(180, 18);
        _heartHeroStatusLabel.ForeColor = Rgb(171, 184, 202);
        _heartHeroStatusLabel.Font = new Font("Segoe UI", 9F);

        heroCard.Controls.Add(_heartHeroValueLabel);
        heroCard.Controls.Add(bpmLabel);
        heroCard.Controls.Add(_heartHeroStatusLabel);

        Panel restingCard = CreateCardPanel(new Point(20, 188), new Size(168, 62), Rgb(19, 27, 40));
        restingCard.Controls.Add(CreateCardCaption("Resting", new Point(14, 12), new Size(100, 18), 12F));
        _heartRestingValueLabel.Location = new Point(14, 30);
        _heartRestingValueLabel.Size = new Size(120, 20);
        _heartRestingValueLabel.ForeColor = Rgb(255, 255, 255);
        _heartRestingValueLabel.Font = new Font("Segoe UI", 12F);
        restingCard.Controls.Add(_heartRestingValueLabel);

        Panel variabilityCard = CreateCardPanel(new Point(196, 188), new Size(168, 62), Rgb(19, 27, 40));
        variabilityCard.Controls.Add(CreateCardCaption("Variability", new Point(14, 12), new Size(100, 18), 12F));
        _heartVariabilityValueLabel.Location = new Point(14, 30);
        _heartVariabilityValueLabel.Size = new Size(120, 20);
        _heartVariabilityValueLabel.ForeColor = Rgb(255, 255, 255);
        _heartVariabilityValueLabel.Font = new Font("Segoe UI", 12F);
        variabilityCard.Controls.Add(_heartVariabilityValueLabel);

        string[] heartZones = ["Calm", "Fat burn", "Peak"];
        DrawingColor[] zoneColors = [Rgb(86, 224, 165), Rgb(255, 174, 92), Rgb(242, 72, 113)];

        for (int i = 0; i < _heartZoneTracks.Length; i++)
        {
            Panel row = CreateCardPanel(new Point(20, 262 + (i * 30)), new Size(344, 24), Rgb(14, 19, 29));

            Label nameLabel = new()
            {
                Location = new Point(10, 4),
                Size = new Size(74, 16),
                Text = heartZones[i],
                ForeColor = Rgb(201, 211, 224),
                Font = new Font("Segoe UI", 9F),
            };

            Panel track = _heartZoneTracks[i];
            track.EnableLvglInputEvents = false;
            track.Location = new Point(92, 6);
            track.Size = new Size(186, 12);
            track.BackColor = Rgb(37, 48, 66);

            Panel fill = _heartZoneFills[i];
            fill.EnableLvglInputEvents = false;
            fill.BackColor = zoneColors[i];
            track.Controls.Add(fill);

            Label valueLabel = _heartZoneValueLabels[i];
            valueLabel.Location = new Point(288, 4);
            valueLabel.Size = new Size(44, 16);
            valueLabel.TextAlign = ContentAlignment.MiddleRight;
            valueLabel.ForeColor = Rgb(255, 255, 255);
            valueLabel.Font = new Font("Segoe UI", 9F);

            row.Controls.Add(nameLabel);
            row.Controls.Add(track);
            row.Controls.Add(valueLabel);
            _heartRatePage.Controls.Add(row);
        }

        _heartRatePage.Controls.Add(heroCard);
        _heartRatePage.Controls.Add(restingCard);
        _heartRatePage.Controls.Add(variabilityCard);
        _heartRatePage.ResumeLayout(false);
    }

    private void InitializeWeatherPage()
    {
        _weatherPage.SuspendLayout();
        _weatherPage.BackColor = Rgb(0, 0, 0);
        _weatherPage.EnableLvglInputEvents = false;
        CreateDetailHeader(_weatherPage, "Weather", "Daily forecast + quick glance");

        Panel heroCard = CreateCardPanel(new Point(20, 84), new Size(344, 96), Rgb(20, 33, 45));

        _weatherHeroTempLabel.Location = new Point(16, 10);
        _weatherHeroTempLabel.Size = new Size(120, 46);
        _weatherHeroTempLabel.ForeColor = Rgb(255, 255, 255);
        _weatherHeroTempLabel.Font = new Font("Segoe UI", 34F);

        _weatherHeroConditionLabel.Location = new Point(148, 18);
        _weatherHeroConditionLabel.Size = new Size(160, 22);
        _weatherHeroConditionLabel.ForeColor = Rgb(166, 223, 214);
        _weatherHeroConditionLabel.Font = new Font("Segoe UI", 16F);

        _weatherHeroRangeLabel.Location = new Point(148, 44);
        _weatherHeroRangeLabel.Size = new Size(160, 18);
        _weatherHeroRangeLabel.ForeColor = Rgb(171, 184, 202);
        _weatherHeroRangeLabel.Font = new Font("Segoe UI", 10F);

        heroCard.Controls.Add(_weatherHeroTempLabel);
        heroCard.Controls.Add(_weatherHeroConditionLabel);
        heroCard.Controls.Add(_weatherHeroRangeLabel);

        for (int i = 0; i < _weatherForecastTimeLabels.Length; i++)
        {
            Panel forecastCard = CreateCardPanel(new Point(20 + (i * 116), 196), new Size(108, 76), Rgb(18, 26, 38));

            Label timeLabel = _weatherForecastTimeLabels[i];
            timeLabel.Location = new Point(14, 12);
            timeLabel.Size = new Size(76, 18);
            timeLabel.ForeColor = Rgb(166, 179, 196);
            timeLabel.Font = new Font("Segoe UI", 10F);

            Label tempLabel = _weatherForecastTempLabels[i];
            tempLabel.Location = new Point(14, 34);
            tempLabel.Size = new Size(76, 24);
            tempLabel.ForeColor = Rgb(255, 255, 255);
            tempLabel.Font = new Font("Segoe UI", 16F);

            forecastCard.Controls.Add(timeLabel);
            forecastCard.Controls.Add(tempLabel);
            _weatherPage.Controls.Add(forecastCard);
        }

        Panel metricsCard = CreateCardPanel(new Point(20, 286), new Size(344, 54), Rgb(17, 24, 36));
        _weatherHumidityLabel.Location = new Point(18, 16);
        _weatherHumidityLabel.Size = new Size(94, 20);
        _weatherHumidityLabel.ForeColor = Rgb(255, 255, 255);
        _weatherHumidityLabel.Font = new Font("Segoe UI", 10F);

        _weatherWindLabel.Location = new Point(126, 16);
        _weatherWindLabel.Size = new Size(96, 20);
        _weatherWindLabel.ForeColor = Rgb(255, 255, 255);
        _weatherWindLabel.Font = new Font("Segoe UI", 10F);

        _weatherUvLabel.Location = new Point(238, 16);
        _weatherUvLabel.Size = new Size(88, 20);
        _weatherUvLabel.ForeColor = Rgb(255, 255, 255);
        _weatherUvLabel.Font = new Font("Segoe UI", 10F);

        metricsCard.Controls.Add(_weatherHumidityLabel);
        metricsCard.Controls.Add(_weatherWindLabel);
        metricsCard.Controls.Add(_weatherUvLabel);

        _weatherPage.Controls.Add(heroCard);
        _weatherPage.Controls.Add(metricsCard);
        _weatherPage.ResumeLayout(false);
    }

    private void InitializeSleepPage()
    {
        _sleepPage.SuspendLayout();
        _sleepPage.BackColor = Rgb(0, 0, 0);
        _sleepPage.EnableLvglInputEvents = false;
        CreateDetailHeader(_sleepPage, "Sleep", "Last night + stage balance");

        Panel heroCard = CreateCardPanel(new Point(20, 84), new Size(344, 84), Rgb(20, 27, 43));
        _sleepHeroDurationLabel.Location = new Point(16, 12);
        _sleepHeroDurationLabel.Size = new Size(170, 34);
        _sleepHeroDurationLabel.ForeColor = Rgb(255, 255, 255);
        _sleepHeroDurationLabel.Font = new Font("Segoe UI", 24F);

        _sleepHeroScoreLabel.Location = new Point(16, 46);
        _sleepHeroScoreLabel.Size = new Size(160, 18);
        _sleepHeroScoreLabel.ForeColor = Rgb(170, 187, 255);
        _sleepHeroScoreLabel.Font = new Font("Segoe UI", 10F);

        heroCard.Controls.Add(_sleepHeroDurationLabel);
        heroCard.Controls.Add(_sleepHeroScoreLabel);

        Panel scheduleCard = CreateCardPanel(new Point(20, 178), new Size(344, 48), Rgb(17, 24, 36));
        _sleepBedtimeLabel.Location = new Point(16, 14);
        _sleepBedtimeLabel.Size = new Size(140, 20);
        _sleepBedtimeLabel.ForeColor = Rgb(255, 255, 255);
        _sleepBedtimeLabel.Font = new Font("Segoe UI", 10F);

        _sleepWakeLabel.Location = new Point(188, 14);
        _sleepWakeLabel.Size = new Size(140, 20);
        _sleepWakeLabel.ForeColor = Rgb(255, 255, 255);
        _sleepWakeLabel.Font = new Font("Segoe UI", 10F);

        scheduleCard.Controls.Add(_sleepBedtimeLabel);
        scheduleCard.Controls.Add(_sleepWakeLabel);

        for (int i = 0; i < _sleepStageTracks.Length; i++)
        {
            Panel row = CreateCardPanel(new Point(20, 238 + (i * 30)), new Size(344, 24), Rgb(14, 19, 29));

            Label titleLabel = new()
            {
                Location = new Point(10, 4),
                Size = new Size(74, 16),
                Text = s_sleepStageTitles[i],
                ForeColor = Rgb(201, 211, 224),
                Font = new Font("Segoe UI", 9F),
            };

            Panel track = _sleepStageTracks[i];
            track.EnableLvglInputEvents = false;
            track.Location = new Point(92, 6);
            track.Size = new Size(186, 12);
            track.BackColor = Rgb(37, 48, 66);

            Panel fill = _sleepStageFills[i];
            fill.EnableLvglInputEvents = false;
            fill.BackColor = i switch
            {
                0 => Rgb(95, 118, 255),
                1 => Rgb(143, 164, 255),
                _ => Rgb(188, 132, 255),
            };

            track.Controls.Add(fill);

            Label valueLabel = _sleepStageValueLabels[i];
            valueLabel.Location = new Point(288, 4);
            valueLabel.Size = new Size(44, 16);
            valueLabel.TextAlign = ContentAlignment.MiddleRight;
            valueLabel.ForeColor = Rgb(255, 255, 255);
            valueLabel.Font = new Font("Segoe UI", 9F);

            row.Controls.Add(titleLabel);
            row.Controls.Add(track);
            row.Controls.Add(valueLabel);
            _sleepPage.Controls.Add(row);
        }

        _sleepPage.Controls.Add(heroCard);
        _sleepPage.Controls.Add(scheduleCard);
        _sleepPage.ResumeLayout(false);
    }

    private void frmSmartWatchDemo_Load(object? sender, EventArgs e)
    {
        ShowTopLevelPage(WatchPage.Home, "Vivid clock watchface ready");
        ApplyWindowLayout();
        UpdateDynamicContent();
        _lastDynamicUpdateStamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
        InitializeWarmupQueue();
    }

    private void frmSmartWatchDemo_SizeChanged(object? sender, EventArgs e)
    {
        ApplyWindowLayout();
    }

    private static void ConfigureStageRow(FlowLayoutPanel panel)
    {
        panel.Margin = Padding.Empty;
        panel.BackColor = RgbStatic(9, 14, 22);
    }

    private void ConfigureNavButton(Button button, string text, EventHandler clickHandler)
    {
        button.Text = text;
        button.Margin = new Padding(0);
        button.Font = new Font("Segoe UI", 11F);
        button.ForeColor = Rgb(236, 240, 245);
        button.BackColor = Rgb(24, 33, 47);
        button.UseVisualStyleBackColor = false;
        button.Click += (_, e) =>
        {
            if (IsTapSuppressed())
            {
                return;
            }

            clickHandler(button, e);
        };
    }

    private void ConfigureActionButton(Button button, DrawingColor backColor, EventHandler clickHandler)
    {
        button.ForeColor = Rgb(255, 255, 255);
        button.BackColor = backColor;
        button.Font = new Font("Segoe UI", 10F);
        button.UseVisualStyleBackColor = false;
        button.Click += (_, e) =>
        {
            if (IsTapSuppressed())
            {
                return;
            }

            clickHandler(button, e);
        };
    }

    private void ConfigureFeatureButton(Button button, string title, string subtitle, DrawingColor backColor, EventHandler clickHandler)
    {
        button.Size = new Size(112, 86);
        button.Text = $"{title}\n{subtitle}";
        button.ForeColor = Rgb(255, 255, 255);
        button.BackColor = backColor;
        button.Font = new Font("Segoe UI", 10F);
        button.UseVisualStyleBackColor = false;
        button.Click += (_, e) =>
        {
            if (IsTapSuppressed())
            {
                return;
            }

            clickHandler(button, e);
        };
    }

    private Panel CreateCardPanel(Point location, Size size, DrawingColor backColor)
    {
        return new Panel
        {
            EnableLvglInputEvents = false,
            Location = location,
            Size = size,
            BackColor = backColor,
        };
    }

    private Label CreateCardCaption(string text, Point location, Size size, float fontSize)
    {
        return new Label
        {
            Location = location,
            Size = size,
            Text = text,
            ForeColor = Rgb(205, 216, 230),
            Font = new Font("Segoe UI", fontSize),
        };
    }

    private Label CreatePageTitle(string title, string subtitle)
    {
        return new Label
        {
            Location = new Point(20, 18),
            Size = new Size(340, 50),
            Text = $"{title}\n{subtitle}",
            ForeColor = Rgb(231, 235, 242),
            Font = new Font("Segoe UI", 14F),
        };
    }

    private void CreateDetailHeader(Panel page, string title, string subtitle)
    {
        Button backButton = new()
        {
            Location = new Point(20, 18),
            Size = new Size(48, 28),
            Text = "<",
            Font = new Font("Segoe UI", 12F),
            ForeColor = Rgb(244, 247, 252),
            BackColor = Rgb(24, 33, 47),
            UseVisualStyleBackColor = false,
        };

        backButton.Click += (_, _) =>
        {
            if (IsTapSuppressed())
            {
                return;
            }

            ReturnFromDetail();
        };

        Label titleLabel = new()
        {
            Location = new Point(82, 16),
            Size = new Size(180, 22),
            Text = title,
            ForeColor = Rgb(244, 247, 252),
            Font = new Font("Segoe UI", 16F),
        };

        Label subtitleLabel = new()
        {
            Location = new Point(82, 40),
            Size = new Size(220, 18),
            Text = subtitle,
            ForeColor = Rgb(135, 157, 184),
            Font = new Font("Segoe UI", 9F),
        };

        page.Controls.Add(backButton);
        page.Controls.Add(titleLabel);
        page.Controls.Add(subtitleLabel);
    }

    private void UpdateDynamicContent()
    {
        DateTime now = DateTime.Now;
        _heartRate = Math.Clamp(_heartRate + _random.Next(-2, 3), 74, 104);
        _stepCount = Math.Min(12000, _stepCount + _random.Next(4, 22));
        _activeCalories = Math.Min(920, _activeCalories + _random.Next(1, 5));
        _movePercent = Math.Min(100, _movePercent + _random.Next(0, 2));
        _weatherConditionIndex = (now.Minute / 7) % s_weatherConditions.Length;
        _weatherTempC = 24 + ((now.Minute / 10) % 5);

        if (now.Second % 17 == 0 && _exerciseMinutes < 60)
        {
            _exerciseMinutes++;
        }

        if (now.Minute % 30 == 0 && now.Second == 0 && _batteryPercent > 18)
        {
            _batteryPercent--;
        }

        _homeHeartButton.Text = $"Heart\n{_heartRate} bpm";
        _homeWeatherButton.Text = $"Weather\n{_weatherTempC}° {s_weatherConditions[_weatherConditionIndex]}";
        _homeSleepButton.Text = $"Sleep\n{_sleepMinutes / 60}h {_sleepMinutes % 60:00}m";
        _homeSummaryLabel.Text = $"Steps {_stepCount:N0}  |  Calories {_activeCalories}  |  Battery {_batteryPercent}%";

        _quickClockLabel.Text = now.ToString("HH:mm");
        _quickBatteryLabel.Text = $"Battery {_batteryPercent}%";
        UpdateQuickButtons();

        _activityStepsValueLabel.Text = $"{_stepCount:N0}";
        _activityMoveValueLabel.Text = $"{_movePercent}%";
        _activityCaloriesValueLabel.Text = $"{_activeCalories}";
        _activitySummaryLabel.Text = $"Workout {_exerciseMinutes}m  |  Stand {_standHours}/12  |  Temp {_weatherTempC}°";
        _activityHeartButton.Text = $"Heart\n{_heartRate} bpm";
        _activityWeatherButton.Text = $"Weather\n{_weatherTempC}° {s_weatherConditions[_weatherConditionIndex]}";
        _activitySleepButton.Text = $"Sleep\nScore {_sleepScore}";

        _heartHeroValueLabel.Text = _heartRate.ToString();
        _heartHeroStatusLabel.Text = _heartRate switch
        {
            < 82 => "Relaxed rhythm",
            < 94 => "Active but stable",
            _ => "Recovery recommended",
        };
        _heartRestingValueLabel.Text = "Resting 61 bpm";
        _heartVariabilityValueLabel.Text = "HRV 48 ms";

        int calmPercent = Math.Clamp(100 - _movePercent, 18, 72);
        int burnPercent = Math.Clamp(_movePercent + 10, 20, 84);
        int peakPercent = Math.Clamp((_heartRate - 70) * 2, 6, 58);
        UpdateHorizontalFill(_heartZoneTracks[0], _heartZoneFills[0], calmPercent);
        UpdateHorizontalFill(_heartZoneTracks[1], _heartZoneFills[1], burnPercent);
        UpdateHorizontalFill(_heartZoneTracks[2], _heartZoneFills[2], peakPercent);
        _heartZoneValueLabels[0].Text = $"{calmPercent}%";
        _heartZoneValueLabels[1].Text = $"{burnPercent}%";
        _heartZoneValueLabels[2].Text = $"{peakPercent}%";

        _weatherHeroTempLabel.Text = $"{_weatherTempC}°";
        _weatherHeroConditionLabel.Text = s_weatherConditions[_weatherConditionIndex];
        _weatherHeroRangeLabel.Text = $"Feels like {_weatherTempC + 1}°  |  L 20°  H {Math.Min(31, _weatherTempC + 3)}°";
        for (int i = 0; i < _weatherForecastTimeLabels.Length; i++)
        {
            int hour = (now.Hour + (i * 3) + 1) % 24;
            _weatherForecastTimeLabels[i].Text = $"{hour:00}:00";
            _weatherForecastTempLabels[i].Text = $"{_weatherTempC + i - 1}°";
        }

        _weatherHumidityLabel.Text = "Humidity 61%";
        _weatherWindLabel.Text = "Wind 4.8 m/s";
        _weatherUvLabel.Text = "UV Index 3";

        _sleepHeroDurationLabel.Text = $"{_sleepMinutes / 60}h {_sleepMinutes % 60:00}m";
        _sleepHeroScoreLabel.Text = $"Sleep score {_sleepScore}  |  Recovery good";
        _sleepBedtimeLabel.Text = "Bedtime 23:18";
        _sleepWakeLabel.Text = "Wake 06:26";

        int deepPercent = 26;
        int lightPercent = 48;
        int remPercent = 26;
        UpdateHorizontalFill(_sleepStageTracks[0], _sleepStageFills[0], deepPercent);
        UpdateHorizontalFill(_sleepStageTracks[1], _sleepStageFills[1], lightPercent);
        UpdateHorizontalFill(_sleepStageTracks[2], _sleepStageFills[2], remPercent);
        _sleepStageValueLabels[0].Text = "1h 52m";
        _sleepStageValueLabels[1].Text = "3h 26m";
        _sleepStageValueLabels[2].Text = "1h 50m";

        RenderHomeWatchface(now, force: false);
    }

    private void UpdateQuickButtons()
    {
        foreach ((Button button, bool enabled) in _quickToggleStates)
        {
            button.BackColor = enabled ? Rgb(48, 110, 211) : Rgb(28, 39, 56);
        }
    }

    private void UpdateHorizontalFill(Panel track, Panel fill, int percent)
    {
        int width = Math.Max(12, (track.Width * Math.Clamp(percent, 0, 100)) / 100);
        fill.Location = new Point(0, 0);
        fill.Size = new Size(width, track.Height);
    }

    private void ApplyWindowLayout()
    {
        if (_layoutApplying || ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        _layoutApplying = true;

        try
        {
            const int padding = 18;
            const int headerHeight = 76;
            const int footerHeight = 72;
            const int navRowHeight = 64;
            const int sectionGap = 10;

            _quickNavButton.Size = new Size(120, 40);
            _appsNavButton.Size = new Size(100, 40);
            _activityNavButton.Size = new Size(100, 40);
            _notificationsNavButton.Size = new Size(120, 40);
            _homeNavButton.Size = new Size(120, 40);

            int contentLeft = padding;
            int contentWidth = Math.Max(0, ClientSize.Width - (padding * 2));
            int headerTop = padding;
            int topRowTop = headerTop + headerHeight + sectionGap;
            int footerTop = Math.Max(headerTop + headerHeight + navRowHeight + 220, ClientSize.Height - padding - footerHeight);
            int bottomRowTop = Math.Max(topRowTop + navRowHeight + 140, footerTop - sectionGap - navRowHeight);
            int middleTop = topRowTop + navRowHeight;
            int middleHeight = Math.Max(312, bottomRowTop - middleTop);

            _titleLabel.Location = new Point(contentLeft, headerTop);
            _titleLabel.Size = new Size(contentWidth, 34);
            _subtitleLabel.Location = new Point(contentLeft, headerTop + 38);
            _subtitleLabel.Size = new Size(contentWidth, 22);

            _quickNavButton.Location = new Point(
                contentLeft + Math.Max(0, (contentWidth - _quickNavButton.Width) / 2),
                topRowTop + 12);

            _notificationsNavButton.Location = new Point(
                contentLeft + Math.Max(0, (contentWidth - _notificationsNavButton.Width) / 2),
                bottomRowTop + 12);

            _homeNavButton.Location = new Point(contentLeft, footerTop + 10);
            int footerTextLeft = _homeNavButton.Right + 20;
            _pageTitleLabel.Size = new Size(Math.Min(240, Math.Max(140, contentWidth - (footerTextLeft - contentLeft) - 20)), 26);
            _statusLabel.Size = new Size(Math.Max(200, contentWidth - (footerTextLeft - contentLeft) - 20), 20);
            _pageTitleLabel.Location = new Point(footerTextLeft, footerTop + 8);
            _statusLabel.Location = new Point(footerTextLeft, footerTop + 36);

            ApplyWatchViewportLayout(contentLeft, contentWidth, middleTop, middleHeight);
        }
        finally
        {
            _layoutApplying = false;
        }
    }

    private void ApplyWatchViewportLayout(int contentLeft, int contentWidth, int middleTop, int middleHeight)
    {
        if (contentWidth <= 0 || middleHeight <= 0)
        {
            return;
        }

        int shellSize = Math.Min(420, Math.Min(contentWidth - 236, middleHeight - 24));
        shellSize = Math.Max(320, shellSize);
        shellSize = Math.Min(shellSize, Math.Min(contentWidth - 140, middleHeight));
        if (shellSize <= 0)
        {
            return;
        }

        _watchShellPanel.Size = new Size(shellSize, shellSize);
        _watchViewportPanel.Size = new Size(Math.Max(1, shellSize - 28), Math.Max(1, shellSize - 28));

        int shellTop = middleTop + Math.Max(0, (middleHeight - shellSize) / 2);
        int shellLeft = contentLeft + ((contentWidth - shellSize) / 2);
        int leftButtonX = Math.Max(contentLeft, shellLeft - _appsNavButton.Width - 22);
        int rightButtonX = Math.Min(contentLeft + contentWidth - _activityNavButton.Width, shellLeft + shellSize + 22);
        int sideButtonY = shellTop + Math.Max(0, (shellSize - _appsNavButton.Height) / 2);

        _watchShellPanel.Location = new Point(shellLeft, shellTop);
        _watchViewportPanel.Location = new Point(shellLeft + 14, shellTop + 14);
        _appsNavButton.Location = new Point(leftButtonX, sideButtonY);
        _activityNavButton.Location = new Point(rightButtonX, sideButtonY);

        Size viewportSize = _watchViewportPanel.ClientSize.Width > 0 || _watchViewportPanel.ClientSize.Height > 0
            ? _watchViewportPanel.ClientSize
            : _watchViewportPanel.Size;
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        foreach (Panel page in _pageMap.Values)
        {
            page.Size = viewportSize;
        }

        LayoutHomePage();
        ResetVisiblePages();
        RenderHomeWatchface(DateTime.Now, force: true);
    }

    private void LayoutHomePage()
    {
        Size viewportSize = _homePage.Size;
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            viewportSize = _watchViewportPanel.ClientSize.Width > 0 || _watchViewportPanel.ClientSize.Height > 0
                ? _watchViewportPanel.ClientSize
                : _watchViewportPanel.Size;
        }

        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        int dialSize = Math.Clamp(Math.Min(viewportSize.Width - 40, viewportSize.Height - 138), 224, 308);
        int dialX = (viewportSize.Width - dialSize) / 2;
        int dialY = 18;
        int buttonWidth = (viewportSize.Width - 48) / 3;
        int buttonY = dialY + dialSize + 10;

        _homeDialPictureBox.Location = new Point(dialX, dialY);
        _homeDialPictureBox.Size = new Size(dialSize, dialSize);

        _homeHeartButton.Location = new Point(12, buttonY);
        _homeHeartButton.Size = new Size(buttonWidth, 58);

        _homeWeatherButton.Location = new Point(18 + buttonWidth, buttonY);
        _homeWeatherButton.Size = new Size(buttonWidth, 58);

        _homeSleepButton.Location = new Point(24 + (buttonWidth * 2), buttonY);
        _homeSleepButton.Size = new Size(buttonWidth, 58);

        _homeGestureHintLabel.Location = new Point(20, buttonY + 62);
        _homeGestureHintLabel.Size = new Size(viewportSize.Width - 40, 18);

        _homeSummaryLabel.Location = new Point(20, buttonY + 82);
        _homeSummaryLabel.Size = new Size(viewportSize.Width - 40, 18);
    }

    private void ShowTopLevelPage(WatchPage page, string status)
    {
        if (!IsTopLevelPage(page))
        {
            throw new InvalidOperationException($"Page {page} is not a top-level tile.");
        }

        EnsurePageAttached(page);
        _swipeTracking = false;
        _swipeDragging = false;
        _currentPage = page;
        _currentTopLevelPage = page;
        _detailReturnPage = page;
        UpdatePageChrome(status);
        ResetVisiblePages();
    }

    private void OpenDetailPage(WatchPage page, WatchPage returnPage, string status)
    {
        EnsurePageAttached(page);
        _swipeTracking = false;
        _swipeDragging = false;
        _currentPage = page;
        _detailReturnPage = returnPage;
        _currentTopLevelPage = returnPage;
        UpdatePageChrome(status);
        ResetVisiblePages();
    }

    private void ReturnFromDetail()
    {
        ShowTopLevelPage(_detailReturnPage, $"Returned to {GetPageTitle(_detailReturnPage)}");
    }

    private void ReturnHome()
    {
        ShowTopLevelPage(WatchPage.Home, "Returned to watchface");
    }

    private void EnsurePageAttached(WatchPage page)
    {
        if (_attachedPages.Contains(page))
        {
            return;
        }

        if (!_pageMap.TryGetValue(page, out Panel? panel))
        {
            return;
        }

        _watchViewportPanel.Controls.Add(panel);
        _attachedPages.Add(page);
    }

    private void InitializeWarmupQueue()
    {
        _warmupPages.Clear();
        _lastWarmupTick = Environment.TickCount64;

        WatchPage[] warmupOrder =
        [
            WatchPage.Quick,
            WatchPage.Apps,
            WatchPage.Activity,
            WatchPage.Notifications,
            WatchPage.HeartRate,
            WatchPage.Weather,
            WatchPage.Sleep,
        ];

        foreach (WatchPage page in warmupOrder)
        {
            if (!_attachedPages.Contains(page))
            {
                _warmupPages.Enqueue(page);
            }
        }
    }

    private void WarmPendingPages()
    {
        while (_warmupPages.Count > 0 && _attachedPages.Contains(_warmupPages.Peek()))
        {
            _warmupPages.Dequeue();
        }

        if (_warmupPages.Count == 0 || _swipeDragging)
        {
            return;
        }

        WatchPage nextPage = _warmupPages.Peek();
        int intervalMs = IsTopLevelPage(nextPage) ? 150 : 450;
        long now = Environment.TickCount64;
        if (_lastWarmupTick >= 0 && now - _lastWarmupTick < intervalMs)
        {
            return;
        }

        _warmupPages.Dequeue();
        _lastWarmupTick = now;
        EnsurePageAttached(nextPage);
    }

    private void ResetVisiblePages()
    {
        foreach ((WatchPage page, Panel panel) in _pageMap)
        {
            panel.Location = Point.Empty;
            panel.Visible = page == _currentPage;
        }

        if (_pageMap.TryGetValue(_currentPage, out Panel? currentPanel))
        {
            currentPanel.BringToFront();
        }
    }

    private void UpdatePageChrome(string status)
    {
        _pageTitleLabel.Text = GetPageTitle(_currentPage);
        _statusLabel.Text = status;

        foreach ((WatchPage page, Button button) in _navButtons)
        {
            button.BackColor = page == _currentTopLevelPage ? Rgb(55, 105, 188) : Rgb(24, 33, 47);
        }
    }

    private static string GetPageTitle(WatchPage page)
    {
        return page switch
        {
            WatchPage.Quick => "Quick Center",
            WatchPage.Apps => "App Grid",
            WatchPage.Home => "Vivid Clock",
            WatchPage.Activity => "Daily Rings",
            WatchPage.Notifications => "Notifications",
            WatchPage.HeartRate => "Heart Rate",
            WatchPage.Weather => "Weather",
            WatchPage.Sleep => "Sleep",
            _ => "Watch",
        };
    }

    private void LaunchApp(string title)
    {
        if (IsTapSuppressed())
        {
            return;
        }

        switch (title)
        {
            case "Heart":
                OpenDetailPage(WatchPage.HeartRate, WatchPage.Apps, "Heart rate card opened");
                break;
            case "Weather":
                OpenDetailPage(WatchPage.Weather, WatchPage.Apps, "Weather card opened");
                break;
            case "Sleep":
                OpenDetailPage(WatchPage.Sleep, WatchPage.Apps, "Sleep card opened");
                break;
            case "Activity":
                ShowTopLevelPage(WatchPage.Activity, "Activity tile opened");
                break;
            case "Settings":
                ShowTopLevelPage(WatchPage.Quick, "Settings routed to quick center");
                break;
            case "Music":
                ShowTopLevelPage(WatchPage.Home, "Music shortcut routed to watchface");
                break;
            default:
                ShowTopLevelPage(WatchPage.Apps, $"{title} shortcut pressed");
                break;
        }
    }

    private void ToggleQuickButton(Button button)
    {
        bool current = _quickToggleStates[button];
        _quickToggleStates[button] = !current;
        UpdateQuickButtons();
        ShowTopLevelPage(WatchPage.Quick, $"{button.Text} switched {(_quickToggleStates[button] ? "on" : "off")}");
    }

    private void quickToggleButton_Click(object? sender, EventArgs e)
    {
        if (IsTapSuppressed())
        {
            return;
        }

        if (sender is Button button && _quickToggleStates.ContainsKey(button))
        {
            ToggleQuickButton(button);
        }
    }

    private void AttachSwipeHandlersRecursive(Control control)
    {
        control.MouseDown += (sender, e) =>
        {
            if (TryGetMousePosition(e, out int x, out int y))
            {
                SwipeSurface_MouseDown(sender, x, y);
            }
        };

        control.MouseMove += (sender, e) =>
        {
            if (TryGetMousePosition(e, out int x, out int y))
            {
                SwipeSurface_MouseMove(sender, x, y);
            }
        };

        control.MouseUp += (sender, e) =>
        {
            if (TryGetMousePosition(e, out int x, out int y))
            {
                SwipeSurface_MouseUp(sender, x, y);
            }
        };

        foreach (Control child in control.Controls)
        {
            AttachSwipeHandlersRecursive(child);
        }
    }

    private void SwipeSurface_MouseDown(object? sender, int x, int y)
    {
        if (!IsTopLevelPage(_currentPage) || IsTapSuppressed())
        {
            return;
        }

        _swipeTracking = true;
        _swipeDragging = false;
        _swipeStart = new Point(x, y);
        _swipeSourcePage = _currentPage;
        _swipeTargetPage = _currentPage;
        _swipeDirection = SwipeDirection.None;
    }

    private void SwipeSurface_MouseMove(object? sender, int x, int y)
    {
        if (!_swipeTracking || !IsTopLevelPage(_currentPage))
        {
            return;
        }

        int dx = x - _swipeStart.X;
        int dy = y - _swipeStart.Y;

        if (!_swipeDragging)
        {
            if (Math.Abs(dx) < SwipeArmThreshold && Math.Abs(dy) < SwipeArmThreshold)
            {
                return;
            }

            _swipeDirection = ResolveSwipeDirection(dx, dy);
            WatchPage? target = ResolveSwipeTarget(_swipeSourcePage, _swipeDirection);
            if (target is null)
            {
                _swipeTracking = false;
                return;
            }

            EnsurePageAttached(target.Value);
            _swipeDragging = true;
            _swipeTargetPage = target.Value;
            _pageMap[_swipeSourcePage].Visible = true;
            _pageMap[_swipeTargetPage].Visible = true;
            _pageMap[_swipeTargetPage].BringToFront();
        }

        ApplySwipeOffset(dx, dy);
    }

    private void SwipeSurface_MouseUp(object? sender, int x, int y)
    {
        if (!_swipeTracking)
        {
            return;
        }

        _swipeTracking = false;

        if (!_swipeDragging)
        {
            return;
        }

        int dx = x - _swipeStart.X;
        int dy = y - _swipeStart.Y;
        int distance = _swipeDirection switch
        {
            SwipeDirection.Left => -dx,
            SwipeDirection.Right => dx,
            SwipeDirection.Up => -dy,
            SwipeDirection.Down => dy,
            _ => 0,
        };

        bool commit = distance >= GetSwipeCommitThreshold();
        _swipeDragging = false;

        if (commit)
        {
            _ignoreClicksUntilTick = Environment.TickCount64 + 180;
            ShowTopLevelPage(_swipeTargetPage, $"{GetPageTitle(_swipeTargetPage)} opened by swipe");
        }
        else
        {
            ShowTopLevelPage(_swipeSourcePage, $"{GetPageTitle(_swipeSourcePage)} kept in place");
        }
    }

    private static bool TryGetMousePosition(EventArgs e, out int x, out int y)
    {
#if WINDOWS
        if (e is System.Windows.Forms.MouseEventArgs mouseEvent)
        {
            x = mouseEvent.X;
            y = mouseEvent.Y;
            return true;
        }
#else
        if (e is MouseEventArgs mouseEvent)
        {
            x = mouseEvent.X;
            y = mouseEvent.Y;
            return true;
        }
#endif

        x = 0;
        y = 0;
        return false;
    }

    private void ApplySwipeOffset(int dx, int dy)
    {
        Size viewportSize = _watchViewportPanel.ClientSize;
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        Panel sourcePanel = _pageMap[_swipeSourcePage];
        Panel targetPanel = _pageMap[_swipeTargetPage];

        switch (_swipeDirection)
        {
            case SwipeDirection.Left:
            {
                int offset = Math.Clamp(dx, -viewportSize.Width, 0);
                sourcePanel.Location = new Point(offset, 0);
                targetPanel.Location = new Point(-viewportSize.Width - offset, 0);
                break;
            }
            case SwipeDirection.Right:
            {
                int offset = Math.Clamp(dx, 0, viewportSize.Width);
                sourcePanel.Location = new Point(offset, 0);
                targetPanel.Location = new Point(viewportSize.Width - offset, 0);
                break;
            }
            case SwipeDirection.Up:
            {
                int offset = Math.Clamp(dy, -viewportSize.Height, 0);
                sourcePanel.Location = new Point(0, offset);
                targetPanel.Location = new Point(0, -viewportSize.Height - offset);
                break;
            }
            case SwipeDirection.Down:
            {
                int offset = Math.Clamp(dy, 0, viewportSize.Height);
                sourcePanel.Location = new Point(0, offset);
                targetPanel.Location = new Point(0, viewportSize.Height - offset);
                break;
            }
        }
    }

    private int GetSwipeCommitThreshold()
    {
        Size viewportSize = _watchViewportPanel.ClientSize;
        int threshold = Math.Min(viewportSize.Width, viewportSize.Height) / 4;
        return Math.Max(70, threshold);
    }

    private static SwipeDirection ResolveSwipeDirection(int dx, int dy)
    {
        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            return dx < 0 ? SwipeDirection.Left : SwipeDirection.Right;
        }

        return dy < 0 ? SwipeDirection.Up : SwipeDirection.Down;
    }

    private static WatchPage? ResolveSwipeTarget(WatchPage page, SwipeDirection direction)
    {
        return (page, direction) switch
        {
            (WatchPage.Home, SwipeDirection.Left) => WatchPage.Apps,
            (WatchPage.Home, SwipeDirection.Right) => WatchPage.Activity,
            (WatchPage.Home, SwipeDirection.Up) => WatchPage.Quick,
            (WatchPage.Home, SwipeDirection.Down) => WatchPage.Notifications,
            (WatchPage.Apps, SwipeDirection.Right) => WatchPage.Home,
            (WatchPage.Activity, SwipeDirection.Left) => WatchPage.Home,
            (WatchPage.Quick, SwipeDirection.Down) => WatchPage.Home,
            (WatchPage.Notifications, SwipeDirection.Up) => WatchPage.Home,
            _ => null,
        };
    }

    private static bool IsTopLevelPage(WatchPage page)
    {
        return page is WatchPage.Home or WatchPage.Quick or WatchPage.Apps or WatchPage.Activity or WatchPage.Notifications;
    }

    private bool IsTapSuppressed()
    {
        return Environment.TickCount64 < _ignoreClicksUntilTick;
    }

    private void RenderHomeWatchface(DateTime now, bool force)
    {
        if (_currentPage != WatchPage.Home)
        {
            return;
        }

        int renderSize = Math.Min(_homeDialPictureBox.Width, _homeDialPictureBox.Height);
        if (renderSize <= 0)
        {
            LayoutHomePage();
            renderSize = Math.Min(_homeDialPictureBox.Width, _homeDialPictureBox.Height);
        }

        if (renderSize <= 0)
        {
            return;
        }

        Size renderTargetSize = new(renderSize, renderSize);

        if (!force &&
            _lastRenderedWatchfaceSecond == now.Second &&
            _lastRenderedWatchfaceSize == renderTargetSize)
        {
            return;
        }

        string path = BuildWatchfaceImage(now, renderSize);
        _homeDialPictureBox.Load(path);
        _lastRenderedWatchfaceSecond = now.Second;
        _lastRenderedWatchfaceSize = renderTargetSize;
    }

    private string BuildWatchfaceImage(DateTime now, int size)
    {
        string path = Path.Combine(_generatedAssetDirectory, $"watchface-{_watchfaceRenderSerial++:D5}.png");

        using RenderImage image = new(size, size, new RenderPixel(0, 0, 0, 0));
        image.Mutate(context =>
        {
            float center = size / 2f;
            float outerRadius = (size / 2f) - 10f;
            float innerRadius = outerRadius - 26f;
            float coreRadius = innerRadius - 22f;

            context.Clear(RenderColor.Transparent);
            context.Fill(RenderColor.ParseHex("#04070C"), new RenderEllipse(center, center, outerRadius));
            context.Fill(RenderColor.FromRgba(16, 23, 36, 255), new RenderEllipse(center, center, outerRadius - 4f));

            RenderColor[] palette =
            [
                RenderColor.ParseHex("#F54B82"),
                RenderColor.ParseHex("#FF8B42"),
                RenderColor.ParseHex("#F5D147"),
                RenderColor.ParseHex("#5EDFAE"),
                RenderColor.ParseHex("#44C6E8"),
                RenderColor.ParseHex("#7E83FF"),
                RenderColor.ParseHex("#C06BFF"),
            ];

            for (int i = 0; i < 48; i++)
            {
                float angle = (-90f + (i * 7.5f)) * (MathF.PI / 180f);
                float radius = innerRadius + 8f;
                float x = center + (MathF.Cos(angle) * radius);
                float y = center + (MathF.Sin(angle) * radius);
                float dotRadius = i % 4 == 0 ? 5.6f : 4.2f;
                RenderColor color = palette[i % palette.Length];
                context.Fill(color, new RenderEllipse(x, y, dotRadius));
            }

            context.Fill(RenderColor.ParseHex("#0A101B"), new RenderEllipse(center, center, coreRadius));
            context.Draw(RenderColor.ParseHex("#1E2A3D"), 3f, new RenderEllipse(center, center, coreRadius));

            DrawAnalogHand(context, center, center, coreRadius * 0.50f, ((now.Hour % 12) + (now.Minute / 60f)) * 30f, 5.2f, RenderColor.ParseHex("#F8FAFD"));
            DrawAnalogHand(context, center, center, coreRadius * 0.74f, (now.Minute + (now.Second / 60f)) * 6f, 3.6f, RenderColor.ParseHex("#DEE6F5"));
            DrawAnalogHand(context, center, center, coreRadius * 0.85f, now.Second * 6f, 2.1f, RenderColor.ParseHex("#FF5B7D"));

            context.Fill(RenderColor.ParseHex("#F8FAFD"), new RenderEllipse(center, center, 6f));
            context.Fill(RenderColor.ParseHex("#FF5B7D"), new RenderEllipse(center, center, 3f));

            float dateCenterX = center;
            float dateCenterY = center + (coreRadius * 0.38f);
            context.Fill(RenderColor.ParseHex("#121A28"), new RenderEllipse(dateCenterX, dateCenterY, 28f));
            context.Draw(RenderColor.ParseHex("#24324A"), 2f, new RenderEllipse(dateCenterX, dateCenterY, 28f));

            context.Fill(RenderColor.ParseHex("#F5C257"), new RenderEllipse(center - (coreRadius * 0.58f), center - (coreRadius * 0.46f), 14f));
            context.Fill(RenderColor.ParseHex("#5EDFAE"), new RenderEllipse(center + (coreRadius * 0.58f), center - (coreRadius * 0.46f), 14f));
            context.Fill(RenderColor.ParseHex("#FF5B7D"), new RenderEllipse(center - (coreRadius * 0.56f), center + (coreRadius * 0.18f), 13f));
            context.Fill(RenderColor.ParseHex("#44C6E8"), new RenderEllipse(center + (coreRadius * 0.56f), center + (coreRadius * 0.18f), 13f));

            context.Fill(RenderColor.ParseHex("#FF5B7D"), new RenderEllipse(center - (coreRadius * 0.50f), center + (coreRadius * 0.68f), 19f));
            context.Fill(RenderColor.ParseHex("#5EDFAE"), new RenderEllipse(center, center + (coreRadius * 0.82f), 18f));
            context.Fill(RenderColor.ParseHex("#7E83FF"), new RenderEllipse(center + (coreRadius * 0.50f), center + (coreRadius * 0.68f), 19f));
        });

        RenderImageExtensions.SaveAsPng(image, path);
        _generatedWatchfacePaths.Enqueue(path);

        while (_generatedWatchfacePaths.Count > 12)
        {
            string oldPath = _generatedWatchfacePaths.Dequeue();
            TryDeleteFile(oldPath);
        }

        return path;
    }

    private static void DrawAnalogHand(IImageProcessingContext context, float centerX, float centerY, float length, float degrees, float thickness, RenderColor color)
    {
        float radians = (degrees - 90f) * (MathF.PI / 180f);
        float endX = centerX + (MathF.Cos(radians) * length);
        float endY = centerY + (MathF.Sin(radians) * length);
        context.DrawLine(color, thickness, new RenderPointF(centerX, centerY), new RenderPointF(endX, endY));
    }

    private static void DrawShortcutOrbit(IImageProcessingContext context, float centerX, float centerY, float radius, RenderColor fill, string caption, RenderFont font)
    {
        context.Fill(fill, new RenderEllipse(centerX, centerY, radius));
        DrawCenteredText(context, caption, font, RenderColor.ParseHex("#F9FBFE"), centerX, centerY);
    }

    private static void DrawCenteredText(IImageProcessingContext context, string text, RenderFont font, RenderColor color, float centerX, float centerY)
    {
        context.DrawText(
            new SixRichTextOptions(font)
            {
                Origin = new RenderPointF(centerX, centerY),
                HorizontalAlignment = SixHorizontalAlignment.Center,
                VerticalAlignment = SixVerticalAlignment.Center,
            },
            text,
            color);
    }

    private void TryDeleteGeneratedAssets()
    {
        foreach (string path in _generatedWatchfacePaths)
        {
            TryDeleteFile(path);
        }

        try
        {
            if (Directory.Exists(_generatedAssetDirectory))
            {
                Directory.Delete(_generatedAssetDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore best-effort temp cleanup failures.
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore best-effort temp cleanup failures.
        }
    }

    private static DrawingColor Rgb(byte r, byte g, byte b)
    {
#if WINDOWS
        return DrawingColor.FromArgb(r, g, b);
#else
        return new DrawingColor(r, g, b);
#endif
    }

    private static DrawingColor RgbStatic(byte r, byte g, byte b)
    {
#if WINDOWS
        return DrawingColor.FromArgb(r, g, b);
#else
        return new DrawingColor(r, g, b);
#endif
    }

    protected override void OnMessageLoopIteration()
    {
        base.OnMessageLoopIteration();

        WarmPendingPages();

        long secondStamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
        if (secondStamp == _lastDynamicUpdateStamp)
        {
            return;
        }

        _lastDynamicUpdateStamp = secondStamp;
        UpdateDynamicContent();
    }
}
