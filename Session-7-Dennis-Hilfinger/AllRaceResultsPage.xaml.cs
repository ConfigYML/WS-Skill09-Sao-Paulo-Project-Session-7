using Microsoft.EntityFrameworkCore;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using System.Net.WebSockets;
using Windows.System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Session_7_Dennis_Hilfinger;

public partial class AllRaceResultsPage : ContentPage, IQueryAttributable
{
    DispatcherTimer timer = new DispatcherTimer();
    User user;
    public AllRaceResultsPage()
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
        FillSelections();
    }

    private void FillSelections()
    {
        using (var db = new MarathonDB())
        {
            var marathons = db.Marathons
                .Include(m => m.CountryCodeNavigation)
                .ToList();
            MarathonPicker.Items.Clear();
            foreach (var mar in marathons)
            {
                MarathonPicker.Items.Add($"{mar.YearHeld} - {mar.CountryCodeNavigation.CountryName}");
            }
            
            var eventTypes = db.EventTypes.ToList();
            EventTypePicker.Items.Clear();
            foreach (var et in eventTypes)
            {
                EventTypePicker.Items.Add(et.EventTypeName.ToString());
            }

            var genders = db.Genders.ToList();
            GenderPicker.Items.Clear();
            foreach(var g in genders)
            {
                GenderPicker.Items.Add(g.Gender1);
            }
            GenderPicker.Items.Add("Any");

            AgeCategoryPicker.Items.Clear();
            AgeCategoryPicker.Items.Add("Under 18");
            AgeCategoryPicker.Items.Add("18-29");
            AgeCategoryPicker.Items.Add("30-39");
            AgeCategoryPicker.Items.Add("40-55");
            AgeCategoryPicker.Items.Add("56-70");
            AgeCategoryPicker.Items.Add("Over 70");
            AgeCategoryPicker.Items.Add("All");
        }
    }


    private List<DateTime> GetAgeCategory(string category)
    {
        DateTime dateMin = DateTime.MinValue;
        DateTime dateMax = DateTime.MaxValue;
        switch (category)
        {
            case "Under 18":
                dateMin = DateTime.Now.AddYears(-0);
                dateMax = DateTime.Now.AddYears(-(17 + 1));
                break;
            case "18-29":
                dateMin = DateTime.Now.AddYears(-18);
                dateMax = DateTime.Now.AddYears(-(29 + 1));
                break;
            case "30-39":
                dateMin = DateTime.Now.AddYears(-30);
                dateMax = DateTime.Now.AddYears(-(39 + 1));
                break;
            case "40-55":
                dateMin = DateTime.Now.AddYears(-40);
                dateMax = DateTime.Now.AddYears(-(55 + 1));
                break;
            case "56-70":
                dateMin = DateTime.Now.AddYears(-56);
                dateMax = DateTime.Now.AddYears(-(70 + 1));
                break;
            case "Over 70":
                dateMin = DateTime.Now.AddYears(-70);
                dateMax = DateTime.Now.AddYears(-(200 + 1));
                break;
        }
        return new List<DateTime>() { dateMin, dateMax };
    }

    private void Search(object? sender, EventArgs e)
    {
        SearchBtn.IsEnabled = false;
        try
        {
            using (var db = new MarathonDB())
            {
                var ev = db.Events
                    .Include(e => e.Marathon)
                    .ThenInclude(m => m.CountryCodeNavigation)
                    .ToList();
                if (MarathonPicker.SelectedItem != null)
                {
                    string selectedMarathon = MarathonPicker.SelectedItem.ToString();
                    int yearHeld = int.Parse(selectedMarathon.Split('-')[0].Trim());
                    string Country = selectedMarathon.Split("-")[1].Trim();
                    var events = ev.Where(e => e.Marathon.YearHeld == yearHeld && e.Marathon.CountryCodeNavigation.CountryName == Country);
                    if (EventTypePicker.SelectedItem != null)
                    {
                        string eventTypeStr = EventTypePicker.SelectedItem.ToString();
                        var eventType = db.EventTypes.FirstOrDefault(et => et.EventTypeName == eventTypeStr);
                        var filteredEvent = events.FirstOrDefault(m => m.EventTypeId == eventType.EventTypeId);

                        var results = db.RegistrationEvents
                            .Include(re => re.Registration)
                            .ThenInclude(re => re.Runner)
                            .Where(re => re.EventId == filteredEvent.EventId)
                            .OrderBy(re => re.RaceTime);
                        if (GenderPicker.SelectedItem != null)
                        {
                            var gender = GenderPicker.SelectedItem.ToString();
                            if (gender != null && gender != "Any")
                            {
                                results = results
                                    .Where(re => re.Registration.Runner.Gender == gender)
                                    .OrderBy(re => re.RaceTime);
                            }
                        }
                        if (AgeCategoryPicker.SelectedItem != null)
                        {
                            string ageCategory = AgeCategoryPicker.SelectedItem.ToString();
                            List<DateTime> dates = GetAgeCategory(ageCategory);
                            if (ageCategory != null && ageCategory != "All")
                            {
                                results = results
                                    .Where(re => re.Registration.Runner.DateOfBirth < dates[0]
                                    && re.Registration.Runner.DateOfBirth > dates[1])
                                    .OrderBy(re => re.RaceTime);
                            }
                        }
                        var totalRunners = results.Count();
                        TotalRunnersLabel.Text = totalRunners.ToString();

                        if (totalRunners == 0)
                        {
                            DisplayAlert("No entries found", "No entries were found for this event. Please use another filter", "Ok");
                            SearchBtn.IsEnabled = true;
                            return;
                        }

                        results = results
                                    .Where(re => re.RaceTime != null && re.RaceTime > 0)
                                    .OrderBy(re => re.RaceTime);

                        DisplayData(results);
                        SearchBtn.IsEnabled = true;

                    }
                    else
                    {
                        DisplayAlert("Information", "Please select a race event to show results", "Ok");
                        SearchBtn.IsEnabled = true;
                        return;
                    }
                }
                else
                {
                    DisplayAlert("Information", "Please select a marathon to show results", "Ok");
                    SearchBtn.IsEnabled = true;
                    return;
                }

            }
        } catch
        {
            DisplayAlert("Error", "Error occurred while loading data", "Ok");
            SearchBtn.IsEnabled = true;
            return;
        }

    }

    private void DisplayData(IOrderedQueryable<RegistrationEvent> data)
    {
        ResultsGrid.Children.Clear();
        ResultsGrid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(),
            new RowDefinition()
        };

        Label RankLabel = new Label();
        RankLabel.Text = "Rank";
        RankLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(RankLabel, 0);
        ResultsGrid.Children.Add(RankLabel);

        Label RacetimeLabel = new Label();
        RacetimeLabel.Text = "Race time";
        RacetimeLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(RacetimeLabel, 1);
        ResultsGrid.Children.Add(RacetimeLabel);

        Label RunnerNameLabel = new Label();
        RunnerNameLabel.Text = "Runner name";
        RunnerNameLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(RunnerNameLabel, 2);
        ResultsGrid.Children.Add(RunnerNameLabel);

        Label CountryLabel = new Label();
        CountryLabel.Text = "Country";
        CountryLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(CountryLabel, 3);
        ResultsGrid.Children.Add(CountryLabel);

        var totalRunnersFinished = data.Count();
        TotalFinishedLabel.Text = totalRunnersFinished.ToString();


        if (totalRunnersFinished == 0)
        {
            DisplayAlert("No runners finished", "Not one runner who competed has finished the race", "Ok");
            SearchBtn.IsEnabled = true;
            return;
        }

        var averageTime = data.Average(re => re.RaceTime);
        var averageTimeSpan = TimeSpan.FromSeconds((long)averageTime);
        var averageTimeFormatted = averageTimeSpan.ToString(@"hh\:mm\:ss");
        AverageRacetimeLabel.Text = averageTimeFormatted;

        for (int i = 0; i < data.Count(); i++)
        {
            ResultsGrid.RowDefinitions.Add(new RowDefinition());
            RankLabel = new Label();
            RankLabel.Text = $"{i + 1}";
            Grid.SetColumn(RankLabel, 0);
            Grid.SetRow(RankLabel, i + 1);
            ResultsGrid.Children.Add(RankLabel);

            RacetimeLabel = new Label();
            var raceTimeSpan = TimeSpan.FromSeconds((long)data.ElementAt(i).RaceTime);
            var raceTimeFormatted = raceTimeSpan.ToString(@"hh\:mm\:ss");
            RacetimeLabel.Text = raceTimeFormatted;
            Grid.SetColumn(RacetimeLabel, 1);
            Grid.SetRow(RacetimeLabel, i + 1);
            ResultsGrid.Children.Add(RacetimeLabel);


            var runner = data.ElementAt(i).Registration.Runner;

            using (var db = new MarathonDB())
            {
                var user = db.Users.FirstOrDefault(u => u.Runners.Contains(runner));

                RunnerNameLabel = new Label();
                RunnerNameLabel.Text = $"{user.FirstName} {user.LastName}";
                Grid.SetColumn(RunnerNameLabel, 2);
                Grid.SetRow(RunnerNameLabel, i + 1);
                ResultsGrid.Children.Add(RunnerNameLabel);
            }

            CountryLabel = new Label();
            CountryLabel.Text = runner.CountryCode;
            Grid.SetColumn(CountryLabel, 3);
            Grid.SetRow(CountryLabel, i + 1);
            ResultsGrid.Children.Add(CountryLabel);

        }
    }
}