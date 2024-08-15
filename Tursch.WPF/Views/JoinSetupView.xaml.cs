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
using Tursch.WPF.Views;

namespace Tursch.WPF
{
    /// <summary>
    /// Interaction logic for JoinSetupView.xaml
    /// </summary>
    public partial class JoinSetupView : Page
    {
        public JoinSetupView()
        {
            InitializeComponent();
            textboxServerPortInput.Text = "5000";
        }

        public JoinSetupView(string serverIP, string serverPort, string playerName)
        {
            InitializeComponent();
            textboxServerIPInput.Text = serverIP;
            textboxServerPortInput.Text = serverPort;
            textboxUsernameInput.Text = playerName;
        }

        private void btnJoinLobby_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textboxUsernameInput.Text))
            {
                // Attempt connection through TCP
                // If successful, get LobbyState from server (just list of all connected players) and switch page to JoinLobbyPage with name list
                // Then await updates from server or disconnect button
                // this.NavigationService.Navigate(new HostLobbyView(textboxUsernameInput.Text));

                JoinLobbyView lobbyView = new JoinLobbyView();

                JoinLobbyViewModel lobbyViewModel = App.JoinServer(textboxServerIPInput.Text, textboxServerPortInput.Text, textboxUsernameInput.Text);

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
