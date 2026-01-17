using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Windows.Services.Maps;

namespace Session_7_Dennis_Hilfinger;

public partial class UserManagementPage : ContentPage, IQueryAttributable
{
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    DispatcherTimer timer = new DispatcherTimer();
    User user;
	public UserManagementPage()
	{
		InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += timerTick;
        timer.Start();
        this.BindingContext = this;
        SortingPicker.Items.Add("First Name");
        SortingPicker.Items.Add("Last Name");
        SortingPicker.Items.Add("Email");
        RolePicker.Items.Add("All roles");
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
        user = (User) query["User"];
        FillSelection();
        Refresh(null, EventArgs.Empty);
    }

    private void FillSelection()
    {
        using(var db = new MarathonDB())
        {
            var roles = db.Roles;
            RolePicker.Items.Clear();
            RolePicker.Items.Add("All roles");
            RolePicker.SelectedIndex = RolePicker.Items.IndexOf("All roles");
            foreach(var role in roles)
            {
                RolePicker.Items.Add(role.RoleName);
            }
        }
    }

    private async void Refresh(object? sender, EventArgs e)
    {
        using(var db = new MarathonDB())
        {
            var users = db.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId != null);

            if (RolePicker.SelectedItem != null)
            {
                var roleType = RolePicker.SelectedItem.ToString();
                if (roleType != "All roles")
                {
                    users = users.Where(u => u.Role.RoleName == roleType);
                }
            }

            if (SortingPicker.SelectedItem != null)
            {
                var sorting = SortingPicker.SelectedItem.ToString();
                switch (sorting) {
                    case "First Name":
                        users = users.OrderBy(u => u.FirstName);
                        break;
                    case "Last Name":
                        users = users.OrderBy(u => u.LastName);
                        break;
                    case "Email":
                        users = users.OrderBy(u => u.Email);
                        break;
                    default:
                        await DisplayAlert("Error", "Error occurred during sorting process", "Ok");
                        break;
                }
            }


            var searchText = SearchEntry.Text;
            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                users = users.Where(u => u.FirstName.ToLower().Contains(searchText) || 
                                         u.LastName.ToLower().Contains(searchText) || 
                                         u.Email.ToLower().Contains(searchText));
            }
            var fastList = await users.ToListAsync();

            TotalUserCountLabel.Text = fastList.Count.ToString();
            Users.Clear();

            if (fastList.Count == 0)
            {
                await DisplayAlert("Info", "No users with the selected criteria were found! Please change your inputs and try again.", "Ok");
            } else
            {
                foreach (var u in fastList)
                {
                    Users.Add(u);
                }
            }
        }
    }

    private async void AddNewUser(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
        {
            { "PageType", "Add" }
        };
        await Shell.Current.GoToAsync("AddEditUserPage", data);
    }

    private async void EditUser(object? sender, EventArgs e)
    {
        Button btn = sender as Button;
        User userToEdit = btn.CommandParameter as User;
        if (userToEdit != null)
        {
            ShellNavigationQueryParameters data = new ShellNavigationQueryParameters()
            {
                { "PageType", "Edit" },
                { "UserToEdit", userToEdit }
            };
            await Shell.Current.GoToAsync("AddEditUserPage", data);
        }
    }
}