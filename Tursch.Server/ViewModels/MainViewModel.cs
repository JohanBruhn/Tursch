using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Server.ViewModels
{
    internal class MainViewModel
    {
        public ServerViewModel ServerViewModel { get; }

        public MainViewModel(ServerViewModel serverViewModel)
        {
            ServerViewModel = serverViewModel;
        }
    }
}
