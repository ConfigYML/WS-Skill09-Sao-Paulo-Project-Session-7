using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Printing;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;
using Windows.System;
using Windows.UI.WebUI;

namespace Session_7_Dennis_Hilfinger;

public partial class AddEditUserPage : ContentPage, IQueryAttributable
{
    bool IsEditPage = false;
    DispatcherTimer timer = new DispatcherTimer();
    User? userToEdit;
    Entry emailEntry;
    public AddEditUserPage()
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
        IsEditPage = (query["PageType"].ToString() == "Edit");
        query.TryGetValue("UserToEdit", out object userObj);
        if (userObj != null)
        {
            userToEdit = (User) userObj;
        }
        
        if (IsEditPage && userToEdit != null)
        {
            HeadingLabel.Text = "Edit user";

            Label emailLabel = new Label();
            emailLabel.Text = userToEdit.Email;
            DataLayout.Children.Insert(0, emailLabel);

        } else
        {
            HeadingLabel.Text = "Add user";

            emailEntry = new Entry();
            emailEntry.Placeholder = "Email";
            DataLayout.Children.Insert(0, emailEntry);
        }
        FillData();
    }

    private void FillData()
    {
        using (var db = new MarathonDB())
        {
            var roles = db.Roles;
            foreach (var role in roles)
            {
                RolePicker.Items.Add(role.RoleName);
            }
            if (IsEditPage)
            {
                FirstnameEntry.Text = userToEdit.FirstName;
                LastnameEntry.Text = userToEdit.LastName;
                var roleIndex = RolePicker.Items
                    .IndexOf(
                        RolePicker.Items.FirstOrDefault(r => r.ToString() == userToEdit.Role.RoleName)
                    );
                RolePicker.SelectedIndex = roleIndex;
            }
        }
    }

    private async void SaveData(object sender, EventArgs e)
    {
        using (var db = new MarathonDB())
        {
            if (IsEditPage)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == userToEdit.Email);
                if (String.IsNullOrEmpty(FirstnameEntry.Text) || 
                    String.IsNullOrEmpty(LastnameEntry.Text))
                {
                    await DisplayAlert("Info", "First Name and Last Name fields can not be empty when saving edited user", "Ok");
                    return;
                }
                user.FirstName = FirstnameEntry.Text.ToString();
                user.LastName = LastnameEntry.Text.ToString();

                var selectedRole = db.Roles.FirstOrDefault(r => r.RoleName == RolePicker.SelectedItem.ToString());
                user.Role = selectedRole;
                user.RoleId = selectedRole.RoleId;

                var password = PasswordEntry.Text;
                var passwordAgain = PasswordAgainEntry.Text;

                if (!String.IsNullOrEmpty(password) ||
                !String.IsNullOrEmpty(passwordAgain))
                {
                    if (String.IsNullOrEmpty(password) ||
                        String.IsNullOrEmpty(passwordAgain))
                    {
                        await DisplayAlert("Info", "Please enter a value for both password fields to change your password.", "OK");
                        return;
                    }
                    if (passwordAgain == password)
                    {
                        if (CheckPasswordRequirements())
                        {
                            user.Password = password;
                        } else
                        {
                            return;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Info", "Passwords do not match.", "OK");
                        return;
                    }
                }

                db.Update(user);
                db.SaveChanges();
                await DisplayAlert("Success", "Profile updated successfully.", "OK");
            } else
            {
                var user = new User();

                var email = emailEntry.Text;
                if (db.Users.Any(u => u.Email == email))
                {
                    await DisplayAlert("Info", "This email is already registered as a user. Please choose a different one.", "Ok");
                    return;
                }
                if (String.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Info", "Email field can not be empty", "Ok");
                    return;
                }
                user.Email = email;

                if (String.IsNullOrEmpty(FirstnameEntry.Text) || 
                    String.IsNullOrEmpty(LastnameEntry.Text))
                {
                    await DisplayAlert("Info", "First Name and Last Name fields can not be empty when creating new user", "Ok");
                    return;
                }
                user.FirstName = FirstnameEntry.Text.ToString();
                user.LastName = LastnameEntry.Text.ToString();

                if (RolePicker.SelectedItem == null)
                {
                    await DisplayAlert("Info", "A role needs to be selected for the new user", "Ok");
                    return;
                }
                var selectedRole = db.Roles.FirstOrDefault(r => r.RoleName == RolePicker.SelectedItem.ToString());
                user.Role = selectedRole;
                user.RoleId = selectedRole.RoleId;

                var password = PasswordEntry.Text;
                var passwordAgain = PasswordAgainEntry.Text;

                if (!String.IsNullOrEmpty(password) ||
                !String.IsNullOrEmpty(passwordAgain))
                {
                    if (String.IsNullOrEmpty(password) ||
                        String.IsNullOrEmpty(passwordAgain))
                    {
                        await DisplayAlert("Info", "Please enter a value for both password fields to change your password.", "OK");
                        return;
                    }
                    if (passwordAgain == password)
                    {
                        if (CheckPasswordRequirements())
                        {
                            user.Password = password;
                        } else
                        {
                            return;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Info", "Passwords do not match.", "OK");
                        return;
                    }
                } else
                {
                    await DisplayAlert("Info", "User needs a password to be registered. Thereby please enter one.", "Ok");
                    return;
                }

                db.Users.Add(user);
                db.SaveChanges();
                await DisplayAlert("Success", "User created successfully.", "OK");
                Cancel(null, EventArgs.Empty);
            }
        }
    }

    private bool CheckPasswordRequirements()
    {
        string password = PasswordEntry.Text.ToString();
        if (password.Length < 6)
        {
            DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
            return false;
        }
        if (!password.Any(char.IsUpper))
        {
            DisplayAlert("Error", "Password must contain at least one uppercase letter.", "OK");
            return false;
        }
        if (!password.Any(char.IsLower))
        {
            DisplayAlert("Error", "Password must contain at least one lowercase letter.", "OK");
            return false;
        }
        if (!password.Any(char.IsDigit))
        {
            DisplayAlert("Error", "Password must contain at least one digit.", "OK");
            return false;
        }
        var specialCharacters = "!@#$%^";
        if (!password.Any(ch => specialCharacters.Contains(ch)))
        {
            DisplayAlert("Error", "Password must contain at least one special character.", "OK");
            return false;
        }
        return true;
    }

    private void Cancel(object sender, EventArgs e)
    {
        Navigation.RemovePage(this);
    }
}