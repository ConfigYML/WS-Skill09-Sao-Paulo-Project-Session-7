using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml;

namespace Session_7_Dennis_Hilfinger;

public partial class AddEditCharityPage : ContentPage, IQueryAttributable
{
    bool IsEditPage = false;
    DispatcherTimer timer = new DispatcherTimer();
    Charity? charityToEdit;
    string CurrentLogoImage;
    public AddEditCharityPage()
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
        IsEditPage = (query["PageType"].ToString() == "Edit");
        query.TryGetValue("CharityToEdit", out object charityObj);
        if (charityObj != null)
        {
            charityToEdit = (Charity)charityObj;
        }

        if (IsEditPage && charityToEdit != null)
        {
            HeadingLabel.Text = "Edit charity";
        }
        else
        {
            HeadingLabel.Text = "Add charity";
        }
        FillData();
    }

    private async void FillData()
    {
        using (var db = new MarathonDB())
        {
            if (IsEditPage)
            {
                NameEntry.Text = charityToEdit.CharityName;
                DescriptionEntry.Text = charityToEdit.CharityDescription;

                //Approach to solve this: save all Charity Images (even MauiImages from app) in FileSystem.AppDataDirectory
                var filepath = Path.Combine(FileSystem.AppDataDirectory, charityToEdit.CharityLogo);
                CurrentLogoImg.Source = ImageSource.FromFile(filepath);
            }
        }
    }

    private async void SaveData(object sender, EventArgs e)
    {
        using (var db = new MarathonDB())
        {
            if (IsEditPage)
            {
                var charity = db.Charities.FirstOrDefault(ch => ch.CharityId == charityToEdit.CharityId);

                var charityName = NameEntry.Text;
                if (string.IsNullOrEmpty(charityName))
                {
                    await DisplayAlert("Info", "Charity Name can not be empty.", "Ok");
                    return;
                }
                charity.CharityName = charityName;

                var charityDescription = DescriptionEntry.Text;
                if (string.IsNullOrEmpty(charityDescription))
                {
                    await DisplayAlert("Info", "Charity Description can not be empty.", "Ok");
                    return;
                }
                charity.CharityDescription = charityDescription;

                var newLogoPath = LogoFileEntry.Text;
                if (!string.IsNullOrEmpty(newLogoPath))
                {
                    try
                    {
                        if (Path.Exists(newLogoPath))
                        {
                            var filename = Path.GetFileName(newLogoPath);
                            var destPath = Path.Combine(FileSystem.AppDataDirectory, filename);
                            if (Path.Exists(destPath)) {
                                await DisplayAlert("Info", "An image with the same name already exists. Please rename the file and try again.", "Ok");
                            } else
                            {
                                File.Copy(newLogoPath, destPath);
                                charity.CharityLogo = filename;
                                CurrentLogoImg.Source = ImageSource.FromFile(destPath);
                            }
                        } else
                        {
                            await DisplayAlert("Error", "File path for new logo does not exist", "Ok");
                        }
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Something went wrong when setting new logo.", "Ok");
                    }
                }

                db.Update(charity);
                db.SaveChanges();
                await DisplayAlert("Success", "Charity updated successfully.", "OK");
            }
            else
            {
                
                var charity = new Charity();

                var charityName = NameEntry.Text;
                if (string.IsNullOrEmpty(charityName))
                {
                    await DisplayAlert("Info", "Charity Name can not be empty.", "Ok");
                    return;
                }
                charity.CharityName = charityName;

                var charityDescription = DescriptionEntry.Text;
                if (string.IsNullOrEmpty(charityDescription))
                {
                    await DisplayAlert("Info", "Charity Description can not be empty.", "Ok");
                    return;
                }
                charity.CharityDescription = charityDescription;

                var newLogoPath = LogoFileEntry.Text;
                if (string.IsNullOrEmpty(newLogoPath))
                {
                    await DisplayAlert("Info", "Charity Logo needs to be selected.", "Ok");
                    return;
                }

                try
                {
                    if (Path.Exists(newLogoPath))
                    {
                        var filename = Path.GetFileName(newLogoPath);
                        var destPath = Path.Combine(FileSystem.AppDataDirectory, filename);
                        if (Path.Exists(destPath))
                        {
                            await DisplayAlert("Info", "An image with the same name already exists. Please rename the file and try again.", "Ok");
                        }
                        else
                        {
                            File.Copy(newLogoPath, destPath);
                            charity.CharityLogo = filename;
                            CurrentLogoImg.Source = ImageSource.FromFile(destPath);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "File path for logo does not exist", "Ok");
                        return;
                    }
                }
                catch
                {
                    await DisplayAlert("Error", "Something went wrong when setting logo.", "Ok");
                    return;
                }

                db.Charities.Add(charity);
                db.SaveChanges();
                await DisplayAlert("Success", "Charity created successfully.", "OK");
                Cancel(null, EventArgs.Empty);
            }
        }
    }


    private async void ChooseLogo(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync(PickOptions.Images);
        if (file != null)
        {
            LogoFileEntry.Text = file.FullPath;
        }
        
    }
    private void Cancel(object sender, EventArgs e)
    {
        Navigation.RemovePage(this);
    }
}