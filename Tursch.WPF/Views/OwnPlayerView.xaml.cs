using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using Tursch.Domain.Models;
using Tursch.WPF.ViewModels;

namespace Tursch.WPF.Views
{
    /// <summary>
    /// Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class OwnPlayerView : UserControl
    {
        public OwnPlayerView()
        {
            InitializeComponent();
            // gridPlayer.Children.Cast<ListView>().ToList().Find(x => x.Name.Equals("listViewHand")).SelectedItems
        }

        private void ListViewHand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OwnPlayerViewModel viewModel = (OwnPlayerViewModel)DataContext;
            viewModel.SelectedCards = listViewHand.SelectedItems.Cast<CardViewModel>().ToList();
        }
    }
}
