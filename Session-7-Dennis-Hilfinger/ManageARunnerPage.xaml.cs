using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.UI.Xaml;
using System.Text;
using Windows.System;

namespace Session_7_Dennis_Hilfinger;

public partial class ManageARunnerPage : ContentPage, IQueryAttributable
{
    DispatcherTimer timer = new DispatcherTimer();
    User user;
    int runnerId;
    byte userStatusId;
    public ManageARunnerPage()
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
        runnerId = (int)query["RunnerId"];
        userStatusId = (byte) query["StatusId"];
        FillData();
    }

    private void FillData()
    {
        using(var db = new MarathonDB())
        {
            var runner = db.Runners
                .Include(r => r.CountryCodeNavigation)
                .Include(r => r.Registrations)
                .ThenInclude(r => r.RegistrationEvents)
                .ThenInclude(r => r.Event)
                .ThenInclude(e => e.EventType)
                .FirstOrDefault(r => r.RunnerId == runnerId);
            var user = db.Users.FirstOrDefault(u => u.Email == runner.Email);
            var charity = db.Charities.FirstOrDefault(c => c.CharityId == runner.Registrations.First().CharityId);
            EmailLabel.Text = runner.Email;

            FirstnameLabel.Text = user.FirstName;
            LastnameLabel.Text = user.LastName;

            GenderLabel.Text = runner.Gender;
            DOBLabel.Text = runner.DateOfBirth.ToString();
            CountryLabel.Text = runner.CountryCodeNavigation.CountryName;
            CharityLabel.Text = charity.CharityName;
            TargetRaiseLabel.Text = runner.Registrations.First().SponsorshipTarget.ToString();
            RacekitLabel.Text = $"Option {runner.Registrations.First().RaceKitOptionId}";
            foreach(var item in runner.Registrations.First().RegistrationEvents)
            {
                RaceEventsLabel.Text = RaceEventsLabel.Text + $"{item.Event.EventType.EventTypeName}\n";
            }

            var statuses = db.RegistrationStatuses
                .OrderBy(st => st.RegistrationStatusId)
                .ToList();
            bool isTicked = true;
            for (int i = 0; i < statuses.Count(); i++)
            {
                HorizontalStackLayout hor = new HorizontalStackLayout();

                hor.Spacing = 10;
                hor.HorizontalOptions = LayoutOptions.End;

                Label nameLabel = new Label();
                nameLabel.Text = statuses[i].RegistrationStatus1;
                nameLabel.VerticalOptions = LayoutOptions.Center;
                nameLabel.FontSize = 22;
                hor.Children.Add(nameLabel);

                Image icon = new Image();
                icon.Source = isTicked ? ImageSource.FromFile("tick_icon.png") : ImageSource.FromFile("cross_icon.png");
                icon.WidthRequest = 80;
                icon.HeightRequest = 80;
                hor.Children.Add(icon);

                if (statuses[i].RegistrationStatusId == userStatusId)
                {
                    isTicked = false;
                }

                RegistrationStatusLayout.Children.Insert(i, hor);
            }
        }
    }

    private async void EditProfile(object sender, EventArgs e)
    {
        using (var db = new MarathonDB())
        {
            var userToEdit = db.Users
                .Include(u => u.Runners)
                .FirstOrDefault(u => u.Email == db.Runners.FirstOrDefault(r => r.RunnerId == runnerId).Email); 
            ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
            {
                { "User", userToEdit },
                { "FromManageRunner", true },
                { "CoordinatorUser", user },
                { "RunnerId", runnerId },
                { "StatusId", userStatusId }
            };
            await Shell.Current.GoToAsync("EditProfilePage", data);
        }
            
    }

    private async void PreviewCertificate(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
        {
            { "RunnerId", runnerId }
        };
        await Shell.Current.GoToAsync("CertificatePreviewPage", data);
    }
}