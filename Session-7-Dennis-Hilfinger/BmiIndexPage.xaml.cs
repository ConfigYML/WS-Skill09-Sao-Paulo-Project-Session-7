using Microsoft.UI.Xaml;

namespace Session_7_Dennis_Hilfinger;

public partial class BmiIndexPage : ContentPage
{
    DispatcherTimer timer = new DispatcherTimer();
	public BmiIndexPage()
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
            height > 0 &&
            height <= 300 &&
            weight > 0 &&
            weight <= 500)
        {
            double bmi = weight / ((height / 100) * (height / 100));
            string category;
            if (bmi < 18.5)
            {
                category = "Underweight";
                BodyImage.Source = ImageSource.FromFile("bmi_underweight_icon.png");
            }
            else if (bmi < 24.9)
            {
                category = "Normal weight";
                BodyImage.Source = ImageSource.FromFile("bmi_healthy_icon.png");
            }
            else if (bmi < 29.9)
            {
                category = "Overweight";
                BodyImage.Source = ImageSource.FromFile("bmi_overweight_icon.png");
            }
            else
            {
                category = "Obesity";
                BodyImage.Source = ImageSource.FromFile("bmi_obese_icon.png");
            }
            CategoryLabel.Text = $"Category: {category} (Value: {bmi:F2})";
        }
        else
        {
            await DisplayAlert("Error", "Please enter valid numeric values for weight (1kg - 500kg) and height (1cm - 300cm).", "Ok");
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
    }

    private void FemaleIconTapped(object? sender, EventArgs e)
    {
        MaleBorder.StrokeThickness = 0;
        FemaleBorder.StrokeThickness = 2;
    }
}