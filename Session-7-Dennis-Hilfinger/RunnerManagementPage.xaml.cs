using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Storage.Pickers;
using Windows.Web.UI;
using WinRT.Interop;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Session_7_Dennis_Hilfinger;

public partial class RunnerManagementPage : ContentPage, IQueryAttributable
{
    DispatcherTimer timer = new DispatcherTimer();
    User user;
    public ObservableCollection<DataDTO> CurrentRunners { get; set; } = new ObservableCollection<DataDTO>();
    public RunnerManagementPage()
    {
        InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
        this.BindingContext = this;
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
        Refresh(null, EventArgs.Empty);
    }

    private void FillSelections()
    {
        using (var db = new MarathonDB())
        {

            var statuses = db.RegistrationStatuses.ToList();
            StatusPicker.Items.Clear();
            StatusPicker.Items.Add("");
            foreach (var stat in statuses)
            {
                if (!string.IsNullOrEmpty(stat.RegistrationStatus1))
                {
                    StatusPicker.Items.Add(stat.RegistrationStatus1);
                }
            }
            var eventTypes = db.EventTypes.ToList();

            EventTypePicker.Items.Clear();
            EventTypePicker.Items.Add("");
            foreach (var et in eventTypes)
            {
                if (!string.IsNullOrEmpty(et.EventTypeName))
                {
                    EventTypePicker.Items.Add(et.EventTypeName);
                }
            }

            SortingPicker.Items.Clear();
            SortingPicker.Items.Add("");
            SortingPicker.Items.Add("First Name");
            SortingPicker.Items.Add("Last Name");
            SortingPicker.Items.Add("Email");
            SortingPicker.Items.Add("Status");

        }
    }

    private async void Refresh(object? sender, EventArgs e)
    {
        using (var db = new MarathonDB())
        {
            var runners = db.Runners
            .Include(r => r.Registrations)
            .ThenInclude(r => r.RegistrationEvents)
            .Where(r => r.Registrations.Count > 0)
            .Where(r => r.Registrations.Any(reg => reg.RegistrationEvents.Count > 0));

            if (StatusPicker.SelectedItem != null)
            {
                if (StatusPicker.SelectedItem.ToString() != "")
                {
                    var statusId = db.RegistrationStatuses.FirstOrDefault(st => st.RegistrationStatus1 == StatusPicker.SelectedItem.ToString()).RegistrationStatusId;
                    runners = runners.Where(r => r.Registrations.First().RegistrationStatusId == statusId);
                }
            }

            if (EventTypePicker.SelectedItem != null)
            {
                if (EventTypePicker.SelectedItem.ToString() != "")
                {
                    var events = db.Events
                        .Include(e => e.EventType)
                        .Where(e => e.EventType.EventTypeName == EventTypePicker.SelectedItem.ToString());
                    runners = runners.Where(r => events.Any(e => e.EventId == r.Registrations.First().RegistrationEvents.First().EventId));
                }
            }

            var query = runners.Select(r =>
            new {
                Runner = r,
                User = db.Users.FirstOrDefault(u => u.Email == r.Email),
                status = db.RegistrationStatuses.FirstOrDefault(st => st.RegistrationStatusId == r.Registrations.First().RegistrationStatusId)
            })
                .Where(r => r.status != null);

            if (SortingPicker.SelectedItem != null)
            {
                if (SortingPicker.SelectedItem.ToString() != "")
                {
                    switch (SortingPicker.SelectedItem.ToString())
                    {
                        case "First Name":
                            query = query.OrderBy(r => r.User.FirstName);
                            break;
                        case "Last Name":
                            query = query.OrderBy(r => r.User.LastName);
                            break;
                        case "Email":
                            query = query.OrderBy(r => r.Runner.Email);
                            break;
                        case "Status":
                            query = query.OrderBy(r => r.status.RegistrationStatus1);
                            break;
                    }
                }
            }

            CurrentRunners.Clear();
            var results = await query.ToListAsync();
            List<DataDTO> fastList = new List<DataDTO>();

            foreach (var res in results)
            {
                fastList.Add(new DataDTO
                {
                    FirstName = res.User?.FirstName,
                    LastName = res.User?.LastName,
                    runner = res.Runner,
                    status = res?.status
                }); ;
            }

            foreach (var data in fastList)
            {
                CurrentRunners.Add(data);
            }

            var totalRunners = CurrentRunners.Count();
            TotalRunnersLabel.Text = totalRunners.ToString();

            if (totalRunners == 0)
            {
                await DisplayAlert("No runners found", "No runners were found using these criteria", "Ok");
            }

        }

    }

    private async void DisplayRunners()
    {
        /*
        ResultsGrid.Children.Clear();
        ResultsGrid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(),
            new RowDefinition()
        };

        Label FirstnameLabel = new Label();
        FirstnameLabel.Text = "First Name";
        FirstnameLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(FirstnameLabel, 0);
        ResultsGrid.Children.Add(FirstnameLabel);

        Label LastnameLabel = new Label();
        LastnameLabel.Text = "Last Name";
        LastnameLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(LastnameLabel, 1);
        ResultsGrid.Children.Add(LastnameLabel);

        Label EmailLabel = new Label();
        EmailLabel.Text = "Email";
        EmailLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(EmailLabel, 2);
        ResultsGrid.Children.Add(EmailLabel);

        Label StatusLabel = new Label();
        StatusLabel.Text = "Status";
        StatusLabel.FontAttributes = FontAttributes.Bold;
        Grid.SetColumn(StatusLabel, 3);
        ResultsGrid.Children.Add(StatusLabel);

        Button EditBtn = new Button();
        EditBtn.Text = "Edit";
        EditBtn.IsVisible = false;
        Grid.SetColumn(EditBtn, 4);
        ResultsGrid.Children.Add(EditBtn);

        var totalRunners = currentRunners.Count();
        TotalRunnersLabel.Text = totalRunners.ToString();


        if (totalRunners == 0)
        {
            await DisplayAlert("No runners found", "No runners were found using these criteria", "Ok");
            return;
        }


        using (var db = new MarathonDB())
        {
            Microsoft.Maui.Controls.Application.Current.Resources.TryGetValue("ButtonGreen", out object style);
            var users = db.Users.ToList();
            var statuses = db.RegistrationStatuses.ToList();
            for (int i = 0; i < currentRunners.Count() - 1; i++)
            {

                ResultsGrid.RowDefinitions.Add(new RowDefinition());
                FirstnameLabel = new Label();
                FirstnameLabel.Text = currentRunners[i].FirstName;
                Grid.SetColumn(FirstnameLabel, 0);
                Grid.SetRow(FirstnameLabel, i + 1);
                ResultsGrid.Children.Add(FirstnameLabel);

                LastnameLabel = new Label();
                LastnameLabel.Text = currentRunners[i].LastName;
                Grid.SetColumn(LastnameLabel, 1);
                Grid.SetRow(LastnameLabel, i + 1);
                ResultsGrid.Children.Add(LastnameLabel);

                EmailLabel = new Label();
                EmailLabel.Text = currentRunners[i].runner.Email;
                Grid.SetColumn(EmailLabel, 2);
                Grid.SetRow(EmailLabel, i + 1);
                ResultsGrid.Children.Add(EmailLabel);

                StatusLabel = new Label();
                StatusLabel.Text = currentRunners[i].status.RegistrationStatus1;
                Grid.SetColumn(StatusLabel, 3);
                Grid.SetRow(StatusLabel, i + 1);
                ResultsGrid.Children.Add(StatusLabel);

                EditBtn = new Button();
                EditBtn.Text = "Edit";
                if (style != null)
                {
                    EditBtn.Style = (Microsoft.Maui.Controls.Style)style;
                }

                int runnerId = currentRunners[i].runner.RunnerId;
                int statusId = currentRunners[i].status.RegistrationStatusId;
                EditBtn.Clicked += (s, e) =>
                {
                    EditRunner(null, EventArgs.Empty, runnerId, statusId);
                };
                Grid.SetColumn(EditBtn, 4);
                Grid.SetRow(EditBtn, i + 1);
                ResultsGrid.Children.Add(EditBtn);
            }
        }*/
    }

    private async void EditRunner(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var data = btn.CommandParameter as DataDTO;
        if (data != null)
        {
            ShellNavigationQueryParameters userData = new ShellNavigationQueryParameters()
            {
                { "User", user },
                { "RunnerId", data.runner.RunnerId },
                { "StatusId", data.status.RegistrationStatusId }
            };
            await Shell.Current.GoToAsync("ManageARunnerPage", userData);
        } else
        {
            await DisplayAlert("Info", "An error occurred. Please try again later.", "Ok");
        }
        
    }

    private async void ExportRunnerDetails(object sender, EventArgs e)
    {
        
        FolderPicker picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        var window = Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView as MauiWinUIWindow;
        var handle = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(picker, handle);
        var folder = await picker.PickSingleFolderAsync();
        var file = await folder.CreateFileAsync("runnerData.csv", Windows.Storage.CreationCollisionOption.ReplaceExisting);
        using (var stream = await file.OpenStreamForWriteAsync() ) {
            StreamWriter st = new StreamWriter(stream);
            foreach (var runner in CurrentRunners)
            {
                string eventStr = "";
                using (var db = new MarathonDB())
                {
                    var events = db.Events.Include(e => e.EventType);
                    foreach (var reg in runner.runner.Registrations)
                    {
                        foreach (var reEv in reg.RegistrationEvents)
                        {
                            var ev = await events.FirstOrDefaultAsync(e => e.EventId == reEv.EventId);
                            eventStr += $"{ev.EventName}, ";
                        }
                    }
                }
                if (eventStr.EndsWith(", "))
                {
                    eventStr = eventStr.Remove(eventStr.LastIndexOf(", "));
                }

                await st.WriteLineAsync(
                    $"{runner.FirstName}, " +
                    $"{runner.LastName}, " +
                    $"{runner.runner.Email}, " +
                    $"{runner.runner.Gender}, " +
                    $"{runner.runner.CountryCode}, " +
                    $"{runner.runner.DateOfBirth}, " +
                    $"{runner.status.RegistrationStatus1}, " +
                    $"\"{eventStr}\"");
            }
            st.Close();
        }
    }
    private async void ShowMailAddressList(object? sender, EventArgs e)
    {
        await DisplayAlert("Info", "Functionality not implemented yet", "Ok");
    }

    public class DataDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Runner runner { get; set; }
        public RegistrationStatus status { get; set; }
    }
}