using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.Foundation.Metadata;
using Windows.Services.Maps;
using Windows.System;

namespace Session_7_Dennis_Hilfinger;

public partial class ManageCharitiesPage : ContentPage
{
    public ObservableCollection<CharityDTO> Charities { get; set; } = new ObservableCollection<CharityDTO>();
    DispatcherTimer timer = new DispatcherTimer();
	public ManageCharitiesPage()
	{
		InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
        this.BindingContext = this;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
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

    private async void LoadData()
    {
        Charities.Clear();
        using(var db = new MarathonDB())
        {
            var fastList = await db.Charities.ToListAsync();
            foreach(var charity in fastList)
            {
                var charityDTO = new CharityDTO()
                {
                    CharityObj = charity,
                    LogoFilePath = Path.Combine(FileSystem.AppDataDirectory, charity.CharityLogo)
                };
                Charities.Add(charityDTO);
            }
        }
    }

    private async void EditCharity(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        CharityDTO charityToEdit = btn.CommandParameter as CharityDTO;
        if (charityToEdit != null)
        {
            ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
            {
                { "PageType", "Edit" },
                { "CharityToEdit", charityToEdit.CharityObj }
            };
            await Shell.Current.GoToAsync("AddEditCharityPage", data);
        }
    }

    private async void AddNewCharity(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
        {
            { "PageType", "Add" }
        };
        await Shell.Current.GoToAsync("AddEditCharityPage", data);
    }

    public class CharityDTO
    {
        public Charity CharityObj {  get; set; }
        public string LogoFilePath { get; set; }
        public ImageSource ImageSource { 
        get
            {
                if(string.IsNullOrEmpty(LogoFilePath))
                {
                    return null;
                }
                return ImageSource.FromStream(() => File.OpenRead(LogoFilePath));
            }
        }
    }
}