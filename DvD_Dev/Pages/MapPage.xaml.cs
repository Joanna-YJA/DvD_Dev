using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DJI.WindowsSDK;
using Windows.UI.Xaml.Media.Imaging;
using DJIVideoParser;
using System.Threading.Tasks;
using MUXC = Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using System.Threading;
using Windows.Devices.Geolocation;
using DJI.WindowsSDK.Components;
using DJI.WindowsSDK.Mission.Waypoint;
using Windows.Services.Maps;
using static DvD_Dev.SpatialMath;
using static DvD_Dev.FootprintCalculator;
using DvD_Dev;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DvD_Dev
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        MapController mapController;

        public MapPage()
        {
            this.InitializeComponent();
            mapController = new MapController(Map);
        }

        //When map is loaded(navigated to), set the starting location of the map
        override
        protected void OnNavigatedTo(NavigationEventArgs e)
        {
            mapController.RenewMap();
        }

        private void Map_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, MapInputEventArgs args)
        {
            BasicGeoposition tappedPos = args.Location.Position;
            mapController.HandleMapTap(tappedPos);
        }

    }
}
