using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Windows.Services.Maps;

namespace Session_7_Dennis_Hilfinger;

public partial class ImportVolunteersPage : ContentPage
{
    DispatcherTimer timer = new DispatcherTimer();
	public ImportVolunteersPage()
	{
		InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
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

    private async void SelectCsv(object? sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select a CSV file"
        });
        if (result != null)
        {
            CsvFileEntry.Text = result.FullPath;
        }
    }

    private async void Import(object? sender, EventArgs e)
    {
        if (CsvFileEntry.Text == null)
        {
            await DisplayAlert("Error", "Please select a CSV file first.", "Ok");
            return;
        }
        var path = CsvFileEntry.Text.ToString();
        if (!Path.Exists(path) || path.EndsWith(".csv") == false)
        {
            await DisplayAlert("Error", "The selected file does not exist or is not a valid CSV file.", "Ok");
            return;
        }

        var lines = await File.ReadAllLinesAsync(path);
        if (lines[0].Split(',').Length != 5) {
            await DisplayAlert("Error", "The CSV file format is incorrect. Expected 5 columns.", "Ok");
            return;
        }
        using(var db = new MarathonDB())
        {
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length != 5)
                {
                    await DisplayAlert("Error", $"Entry in row {i + 1} was not in the correct format.", "Ok");
                    return;
                }

                if (!int.TryParse(values[0], out int id))
                {
                    await DisplayAlert("Error", $"Error while getting id in row {i + 1}. Please check if the entry is a correct number.", "Ok");
                    return;
                }
                string firstName = values[1].Trim().ToString();
                string lastName = values[2].Trim().ToString();
                var country = db.Countries.FirstOrDefault(c => c.CountryCode.ToLower() == values[3].Trim().ToString().ToLower());
                if (country == null)
                {
                    await DisplayAlert("Error", $"Error while getting country in row {i + 1}. Please check if the entry is correct.", "Ok");
                    return;
                }
                var gender = db.Genders.FirstOrDefault(g => g.Gender1.ToLower().StartsWith(values[4].Trim().ToString().ToLower()));
                if (gender == null)
                {
                    await DisplayAlert("Error", $"Error while getting gender in row {i + 1}. Please check if the entry is correct. Gender should be 'M' for males and 'F' for females.", "Ok");
                    return;
                }

                if (db.Volunteers.Any(v => v.VolunteerId == id))
                {
                    var vol = db.Volunteers.FirstOrDefault(v => v.VolunteerId == id);

                    vol.FirstName = firstName;
                    vol.LastName = lastName;
                    vol.CountryCodeNavigation = country;
                    vol.CountryCode = country.CountryCode;
                    vol.GenderNavigation = gender;
                    vol.Gender = gender.Gender1;

                    db.Volunteers.Update(vol);
                } else
                {
                    Volunteer newVol = new Volunteer()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        CountryCodeNavigation = country,
                        CountryCode = country.CountryCode,
                        GenderNavigation = gender,
                        Gender = gender.Gender1
                    };
                    db.Volunteers.Add(newVol);
                }
                db.SaveChanges();   
            }
        }
        


        await DisplayAlert("Import", "Volunteers imported successfully!", "OK");
    }

    private async void Cancel(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}