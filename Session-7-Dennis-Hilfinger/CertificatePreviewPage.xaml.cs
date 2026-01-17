using Microsoft.EntityFrameworkCore;
using Windows.System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Session_7_Dennis_Hilfinger;

public partial class CertificatePreviewPage : ContentPage, IQueryAttributable
{
    int runnerId;
    public CertificatePreviewPage()
    {
        InitializeComponent();
    }

    private void Logout(object? sender, EventArgs e)
    {
        AppShell.Current.GoToAsync("//MainPage");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        runnerId = (int)query["RunnerId"];
        FillData();
    }
    
    private void FillData()
    {
        using(var db = new MarathonDB())
        {
            var eventTypes = db.EventTypes.ToList();
            foreach(var type in eventTypes)
            {
                EventTypePicker.Items.Add(type.EventTypeName.ToString());
            }
        }
    }

    private void LoadCertificate(object sender, EventArgs e)
    {
        using(var db = new MarathonDB())
        {
            var eventType = db.EventTypes.FirstOrDefault(et => et.EventTypeName == EventTypePicker.SelectedItem.ToString());
            var registrationEvents = db.RegistrationEvents
                .Include(re => re.Event)
                .Include(re => re.Registration)
                .ThenInclude(re => re.Charity)
                .Where(re => re.Registration.Runner.RunnerId == runnerId && 
                             re.Event.Marathon.MarathonId == 4);

            var searchedEvent = registrationEvents.FirstOrDefault(re => re.Event.EventTypeId == eventType.EventTypeId);
            if (searchedEvent != null)
            {
                // Show certificate
                var user = db.Users.FirstOrDefault(u => u.Email == db.Runners.FirstOrDefault(r => r.RunnerId == runnerId).Email);
                NameLabel.Text = string.Format("Congratulations {0} {1} for running in the {2}",
                    user.FirstName,
                    user.LastName,
                    eventType.EventTypeName);

                var raceTimeSpan = TimeSpan.FromSeconds((long)searchedEvent.RaceTime);
                var raceTimeFormatted = raceTimeSpan.ToString(@"hh\:mm\:ss");
                var rank = registrationEvents
                    .OrderBy(re => re.RaceTime)
                    .ToList()
                    .FindIndex(re => re.RegistrationEventId == searchedEvent.RegistrationEventId)
                    + 1;
                TimeLabel.Text = string.Format("You ran a time of {0} and secured place #{1} with that!",
                    raceTimeFormatted,
                    rank);

                var raisedAmount = db.Sponsorships
                    .Where(sp => sp.RegistrationId == searchedEvent.Registration.RegistrationId)
                    .Sum(sp => sp.Amount);
                CharityLabel.Text = string.Format("You also raised ${0} for {1}!",
                    raisedAmount,
                    searchedEvent.Registration.Charity.CharityName);

                CertificateLayout.IsVisible = true;

            } else
            {
                CertificateLayout.IsVisible = false;
                DisplayAlert("Info", "No event record for this type of marathon found in your profile", "Ok");
            }
        }
    }
}