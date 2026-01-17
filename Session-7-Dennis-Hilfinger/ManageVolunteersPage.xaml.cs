using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace Session_7_Dennis_Hilfinger;

public partial class ManageVolunteersPage : ContentPage
{
	public ObservableCollection<Volunteer> Volunteers { get; set; } = new ObservableCollection<Volunteer>();
    DispatcherTimer timer = new DispatcherTimer();
    public ManageVolunteersPage()
	{
		InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
        this.BindingContext = this;

        SortingPicker.Items.Add("First Name");
        SortingPicker.Items.Add("Last Name");
        SortingPicker.Items.Add("Country");
        SortingPicker.Items.Add("Gender");
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData(null, EventArgs.Empty);
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

    private async void LoadData(object? sender, EventArgs e)
    {

        Volunteers.Clear();
        using (var db = new MarathonDB())
        {
            var fastList = await db.Volunteers
                .Include(v => v.CountryCodeNavigation)
                .Include(v => v.GenderNavigation)
                .ToListAsync();

            Volunteers.Clear();
            if (SortingPicker.SelectedIndex >= 0)
            {
                var sorting = SortingPicker.SelectedItem.ToString();
                switch (sorting)
                {
                    case "First Name":
                        var sortedByFirstName = fastList.OrderBy(v => v.FirstName).ToList();
                        foreach (var volunteer in sortedByFirstName)
                        {
                            Volunteers.Add(volunteer);
                        }
                        break;
                    case "Last Name":
                        var sortedByLastName = fastList.OrderBy(v => v.LastName).ToList();
                        foreach (var volunteer in sortedByLastName)
                        {
                            Volunteers.Add(volunteer);
                        }
                        break;
                    case "Country":
                        var sortedByCountry = fastList.OrderBy(v => v.CountryCodeNavigation.CountryName).ToList();
                        foreach (var volunteer in sortedByCountry)
                        {
                            Volunteers.Add(volunteer);
                        }
                        break;
                    case "Gender":
                        var sortedByGender = fastList.OrderBy(v => v.GenderNavigation.Gender1).ToList();
                        foreach (var volunteer in sortedByGender)
                        {
                            Volunteers.Add(volunteer);
                        }
                        break;
                }
            } else
            {
                foreach (var volunteer in fastList)
                {
                    Volunteers.Add(volunteer);
                }
            }
            
        }
        
    }

    private void OpenImportPage(object? sender, EventArgs e)
    {
        AppShell.Current.GoToAsync("ImportVolunteersPage");
    }
}