using Microsoft.AspNetCore.SignalR.Client;
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
using Tursch.Domain;
using Tursch.WPF.Services;

namespace Tursch.WPF
{
    /// <summary>
    /// Interaction logic for HostLobbyView.xaml
    /// </summary>
    public partial class HostLobbyView : Page
    {
        public HostLobbyView()
        {
            InitializeComponent();
        }

        // Server awaits: Connection(s)/Host starts game/Disconnection(s)/Host closes lobby
        // Host and clients await: lobby update from server, or send command (disconnect/disband lobby)

        private void btnReturnLobby_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HostSetupView());
        }
    }
}
