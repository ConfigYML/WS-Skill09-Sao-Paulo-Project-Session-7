namespace Session_7_Dennis_Hilfinger
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SponsorRunnerPage), typeof(SponsorRunnerPage));
            Routing.RegisterRoute(nameof(SponsorshipConfirmationPage), typeof(SponsorshipConfirmationPage));
            Routing.RegisterRoute(nameof(FindOutMorePage), typeof(FindOutMorePage));
            Routing.RegisterRoute(nameof(CharityListPage), typeof(CharityListPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RunnerPage), typeof(RunnerPage));
            Routing.RegisterRoute(nameof(CoordinatorPage), typeof(CoordinatorPage));
            Routing.RegisterRoute(nameof(AdminPage), typeof(AdminPage));
            Routing.RegisterRoute(nameof(CompetedBeforePage), typeof(CompetedBeforePage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(RegisterEventPage), typeof(RegisterEventPage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
            Routing.RegisterRoute(nameof(EventConfirmationPage), typeof(EventConfirmationPage));
            Routing.RegisterRoute(nameof(MyRaceResultsPage), typeof(MyRaceResultsPage));
            Routing.RegisterRoute(nameof(AllRaceResultsPage), typeof(AllRaceResultsPage));
            Routing.RegisterRoute(nameof(AboutMarathonSkillsPage), typeof(AboutMarathonSkillsPage));
            Routing.RegisterRoute(nameof(InteractiveMapPage), typeof(InteractiveMapPage));
            Routing.RegisterRoute(nameof(MarathonLengthPage), typeof(MarathonLengthPage));
            Routing.RegisterRoute(nameof(MySponsorshipPage), typeof(MySponsorshipPage));
            Routing.RegisterRoute(nameof(RunnerManagementPage), typeof(RunnerManagementPage));
            Routing.RegisterRoute(nameof(ManageARunnerPage), typeof(ManageARunnerPage));
            Routing.RegisterRoute(nameof(CertificatePreviewPage), typeof(CertificatePreviewPage));
            Routing.RegisterRoute(nameof(SponsorshipOverviewPage), typeof(SponsorshipOverviewPage));
            Routing.RegisterRoute(nameof(UserManagementPage), typeof(UserManagementPage));
            Routing.RegisterRoute(nameof(AddEditUserPage), typeof(AddEditUserPage));
            Routing.RegisterRoute(nameof(ManageCharitiesPage), typeof(ManageCharitiesPage));
            Routing.RegisterRoute(nameof(AddEditCharityPage), typeof(AddEditCharityPage));
            Routing.RegisterRoute(nameof(ManageVolunteersPage), typeof(ManageVolunteersPage));
            Routing.RegisterRoute(nameof(ImportVolunteersPage), typeof(ImportVolunteersPage));
            Routing.RegisterRoute(nameof(BmiIndexPage), typeof(BmiIndexPage));
            Routing.RegisterRoute(nameof(BmrIndexPage), typeof(BmrIndexPage));
        }
    }
}
