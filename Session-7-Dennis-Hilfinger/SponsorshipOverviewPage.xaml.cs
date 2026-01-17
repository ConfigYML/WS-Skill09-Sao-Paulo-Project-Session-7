using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace Session_7_Dennis_Hilfinger;

public partial class SponsorshipOverviewPage : ContentPage
{
    DispatcherTimer timer = new DispatcherTimer();
    public ObservableCollection<CharityDTO> Charities { get; set; } = new ObservableCollection<CharityDTO>();
	public SponsorshipOverviewPage()
	{
		InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
        this.BindingContext = this;
        SortingPicker.Items.Add("Charity Name");
        SortingPicker.Items.Add("Total Amount");
        LoadData();
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

    private void SortCharities(object sender, EventArgs e)
    {
        if (SortingPicker.SelectedItem != null)
        {
            if (SortingPicker.SelectedItem.ToString() == "Charity Name")
            {
                var sortedItems = Charities
                    .OrderBy(ch => ch.CharityName)
                    .ToList();
                Charities.Clear();
                foreach(var item in sortedItems)
                {
                    Charities.Add(item);
                }
            } else if (SortingPicker.SelectedItem.ToString() == "Total Amount") {
                var sortedItems =  Charities
                    .OrderBy(ch => ch.SumValue)
                    .ToList();
                Charities.Clear();
                foreach (var item in sortedItems)
                {
                    Charities.Add(item);
                }
            }
        }
    }

    private void LoadData()
    {
        using (var db = new MarathonDB())
        {
            var charities = db.Charities.ToList();
            decimal totalSum = 0;
            foreach (var ch in charities)
            {
                var sum = db.Sponsorships
                    .Where(sp => sp.Registration.CharityId == ch.CharityId)
                    .Sum(sp => sp.Amount);
                totalSum += sum;

                Charities.Add(new CharityDTO
                {
                    Filename = ch.CharityLogo,
                    CharityName = ch.CharityName,
                    SumValue = sum,
                    AmountSum = $"${(int) sum}"
                });
            }
            CharityCountLabel.Text = Charities.Count.ToString();
            TotalSponsorshipsLabel.Text = $"${(int) totalSum}";
        }
    }

    public class CharityDTO
    {
        public string Filename { get; set; }
        public string CharityName { get; set; }
        public decimal SumValue { get; set; }
        public string AmountSum { get; set; }
    }

}