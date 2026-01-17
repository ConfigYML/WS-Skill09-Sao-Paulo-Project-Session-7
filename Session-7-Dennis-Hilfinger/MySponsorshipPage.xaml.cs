using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Windows.System;

namespace Session_7_Dennis_Hilfinger;

public partial class MySponsorshipPage : ContentPage, IQueryAttributable
{
    DispatcherTimer timer = new DispatcherTimer();
    User user;
	public MySponsorshipPage()
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
        FillData();
    }

    private void FillData()
    {
        using(var db = new MarathonDB())
        {
            var registration = db.Registrations
                .Include(r => r.Charity)
                .FirstOrDefault(r => r.RunnerId == user.Runners.First().RunnerId);
            CharityImg.Source = ImageSource.FromFile(registration.Charity.CharityLogo);
            CharityText.Text = registration.Charity.CharityDescription;

            var reg = db.Registrations.FirstOrDefault(r => r.RunnerId == user.Runners.First().RunnerId);

            var sponsorships = db.Sponsorships
                .Where(s => s.RegistrationId == reg.RegistrationId)
                .ToList();

            for (int i = 0; i < sponsorships.Count; i++) 
            {
                SponsorGrid.RowDefinitions.Add(new RowDefinition());
                Label nameLabel = new Label();
                nameLabel.Text = sponsorships[i].SponsorName;
                Grid.SetColumn(nameLabel, 0);
                Grid.SetRow(nameLabel, i + 1);
                SponsorGrid.Children.Add(nameLabel);

                Label amountLabel = new Label();
                amountLabel.Text = $"${sponsorships[i].Amount.ToString()}";
                Grid.SetColumn(amountLabel, 1);
                Grid.SetRow(amountLabel, i + 1);
                SponsorGrid.Children.Add(amountLabel);
            }

            var totalAmount = sponsorships.Sum(s => s.Amount);

            Label sumLabel = new Label();
            sumLabel.Text = $"Total ${totalAmount}";
            Grid.SetColumn(sumLabel, 1);
            Grid.SetRow(sumLabel, sponsorships.Count + 1);
            SponsorGrid.Children.Add(sumLabel);
            /*
            var test = db.Users
                .Include(u => u.Runners)
                .ThenInclude(r => r.Registrations)
                .ThenInclude(r => r.Sponsorships)
                .Where(u => u.Runners.First().Registrations.First().Sponsorships.Count > 3);
            var testing = test.First();
            */
        }
    }

    private void Cancel(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters userData = new ShellNavigationQueryParameters()
        {
            { "User", user }
        };
        AppShell.Current.GoToAsync("RunnerPage", userData);
    }
}