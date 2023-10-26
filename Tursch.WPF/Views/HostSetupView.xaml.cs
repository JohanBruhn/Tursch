using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tursch.WPF.ViewModels;

namespace Tursch.WPF
{
    /// <summary>
    /// Interaction logic for HostSetupView.xaml
    /// </summary>
    public partial class HostSetupView : Page
    {
        public HostSetupView()
        {
            InitializeComponent();
        }

        public HostSetupView(string playerName)
        {
            InitializeComponent();
            textboxUsernameInput.Text = playerName;
        }

        private void btnCreateLobby_Click(object sender, RoutedEventArgs e)
        {
            // Add server settings
            if (!string.IsNullOrWhiteSpace(textboxUsernameInput.Text)) // Check so all fields are filled, ideally wait for successful server start to create lobby (maybe already does?)
            {
                HostLobbyView lobbyView = new HostLobbyView();

                HostLobbyViewModel lobbyViewModel = App.StartServer(textboxServerIPInput.Text, textboxServerPortInput.Text, textboxUsernameInput.Text);

                lobbyView.DataContext = lobbyViewModel;

                this.NavigationService.Navigate(lobbyView);
            }

        }

        private void btnReturnMenu_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MenuPage());
        }

        
    }
}
