using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.UI.Xaml;

namespace Session_7_Dennis_Hilfinger;

public partial class BmrIndexPage : ContentPage
{
    private bool isMale = true;
    DispatcherTimer timer = new DispatcherTimer();
	public BmrIndexPage()
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

    private async void Calculate(object? sender, EventArgs e)
    {
        if (double.TryParse(WeightEntry.Text, out double weight) &&
            double.TryParse(HeightEntry.Text, out double height) &&
            double.TryParse(AgeEntry.Text, out double age) &&
            height > 0 &&
            height <= 300 &&
            weight > 0 &&
            weight <= 500 &&
            age >= 0 &&
            age <= 150)
        {
            double bmr;
            if (isMale)
            {
                bmr = 66 + (13.7 * weight) + (5 * height) - (6.8 * age);
            } else
            {
                bmr = 655 + (9.6 * weight) + (1.8 * height) - (4.7 * age);
            }
            Sedentiary.Text = (bmr * 1.2).ToString("F2");
            LightlyActive.Text = (bmr * 1.375).ToString("F2");
            ModeratelyActive.Text = (bmr * 1.55).ToString("F2");
            VeryActive.Text = (bmr * 1.725).ToString("F2");
            ExtremelyActive.Text = (bmr * 1.9).ToString("F2");
            BMR_Label.Text = bmr.ToString("F2") + " kcal";
        }
        else
        {
            await DisplayAlert("Error", "Please enter valid numeric values for weight (1kg - 500kg), height (1cm - 300cm), and age (0 years - 150 years).", "Ok");
        }
    }

    private async void Cancel(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void MaleIconTapped(object? sender, EventArgs e)
    {
        MaleBorder.StrokeThickness = 2;
        FemaleBorder.StrokeThickness = 0;
        isMale = true;
    }

    private void FemaleIconTapped(object? sender, EventArgs e)
    {
        MaleBorder.StrokeThickness = 0;
        FemaleBorder.StrokeThickness = 2;
        isMale = false;
    }

    private async void MoreInfo(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Feature not implemented yet.", "Ok");
    }
}