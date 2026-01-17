using Microsoft.UI.Xaml;

namespace Session_7_Dennis_Hilfinger;

public partial class CoordinatorPage : ContentPage, IQueryAttributable
{
    DispatcherTimer timer = new DispatcherTimer();
    User user;
    public CoordinatorPage()
    {
        InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
    }

    private void timerTick(object? sender, object e)
    {
        DateTime targetTime = new DateTime(2026, 9, 5, 6, 0, 0);
        DateTime currentTime = DateTime.Now;
        TimeSpan timeDiff = targetTime - currentTime;

        TimerLabel.Text = string.Format("{0} days {1} hours and {2} minutes until the race starts!",
            timeDiff.Days,
            timeDiff.Hours,
            timeDiff.Minutes);
    }

    private void Logout(object? sender, EventArgs e)
    {
        AppShell.Current.GoToAsync("//MainPage");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        user = (User)query["User"];
    }

    public async void Runners(object? sender, EventArgs e)
    {
        ShellNavigationQueryParameters userData = new ShellNavigationQueryParameters()
        {
            { "User", user }
        };
        await AppShell.Current.GoToAsync("RunnerManagementPage", userData);
    }

    public async void Sponsorship(object? sender, EventArgs e)
    {
        await AppShell.Current.GoToAsync("SponsorshipOverviewPage");
    }

}