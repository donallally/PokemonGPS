﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GPSPokemon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // rootPage = MainPage.Current;
        public static List<Fence> listOfFences = new List<Fence>();
        public static List<Pokemon> listOfPokemon = new List<Pokemon>();
        double myX;
        double myY;

        Geolocator Location;
        public MainPage()
        {
            this.InitializeComponent();
            //loadFences();

        }

        public static async Task LoadLocalData()
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync("Assets\\GPScoords.txt");
            var result = await FileIO.ReadTextAsync(file);
            var coordsList = JsonArray.Parse(result);
            loadFences(coordsList);

            var file1 = await Package.Current.InstalledLocation.GetFileAsync("Assets\\pokemon.txt");
            var result1 = await FileIO.ReadTextAsync(file1);
            var pokemonList = JsonArray.Parse(result1);
            loadPokemon(pokemonList);
        }

        async private void Initialize()
        {
            this.Initialize();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await LoadLocalData();
            //initilise
            Location = new Geolocator();
            //add 2 event handlers
            Location.PositionChanged += Location_PositionChanged;
            //status changed
            Location.StatusChanged += Location_StatusChanged;
            GridViewDogs.ItemsSource = listOfPokemon;

        }

        async void Location_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            //add a text block to the page , put status value in here
            //gps runs in the back ground so we need to use the
            //dispatcher which is async
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () =>
                {
                    StatusValue.Text = args.Status.ToString();
                });
        }

        async void Location_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            //similar StatusChanged, use the dispatcher
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () =>
                {
                    //gives us a position that we can use
                    Geoposition position = args.Position;
                    LatitudeValue.Text = position.Coordinate.Latitude.ToString();
                    LongitudeValue.Text = position.Coordinate.Longitude.ToString();
                    AccuracyValue.Text = position.Coordinate.Accuracy.ToString();
                    myX = position.Coordinate.Longitude;
                    myY = position.Coordinate.Latitude;
                });

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //takes the app stuff out of memory when exited
            Location.PositionChanged -= Location_PositionChanged;
            Location.StatusChanged -= Location_StatusChanged;
            base.OnNavigatingFrom(e);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Location == null)
            {
                Location = new Geolocator();
            }

            Geoposition position = await Location.GetGeopositionAsync();//this gets our newest position

            myX = position.Coordinate.Longitude;
            myY = position.Coordinate.Latitude;

            //find and set the types of pokemon here

            LatitudeValue.Text = position.Coordinate.Latitude.ToString();
            LongitudeValue.Text = position.Coordinate.Longitude.ToString();
            AccuracyValue.Text = position.Coordinate.Accuracy.ToString();

        }

        private static void loadFences(JsonArray coordsList)
        {
            foreach (var item in coordsList)
            {
                var oneFence = item.GetObject();
                Fence nFence = new Fence();

                foreach (var key in oneFence.Keys)
                {
                    IJsonValue value;
                    if (!oneFence.TryGetValue(key, out value))
                        continue;

                    switch (key)
                    {
                        case "Name":
                            nFence.id = value.GetString();
                            break;
                        case "pointX":
                            nFence.pointX = value.GetString();
                            break;
                        case "pointY":
                            nFence.pointY = value.GetString();
                            break;
                        case "Type1":
                            nFence.type1 = value.GetString();
                            break;
                        case "Type2":
                            nFence.type2 = value.GetString();
                            break;
                    } // end switch
                } // end foreach
                listOfFences.Add(nFence);
            } // end foreach
        }

        private static void loadPokemon(JsonArray pokemonList)
        {

            foreach (var item in pokemonList)
            {
                var onePokemon = item.GetObject();
                Pokemon nPokemon = new Pokemon();

                foreach (var key in onePokemon.Keys)
                {
                    IJsonValue value;
                    if (!onePokemon.TryGetValue(key, out value))
                        continue;

                    switch (key)
                    {
                        case "Number":
                            nPokemon.number = value.GetString();
                            break;
                        case "Name":
                            nPokemon.name = value.GetString();
                            break;
                        case "Type1":
                            nPokemon.type1 = value.GetString();
                            break;
                        case "Type2":
                            nPokemon.type2 = value.GetString();
                            break;
                        case "Image":
                            nPokemon.image = value.GetString();
                            break;
                    } // end switch
                } // end foreach
                listOfPokemon.Add(nPokemon);
            } // end foreach

        }

        private void checkLocation()
        {
            foreach (Fence fence in listOfFences) // Loop through List with foreach.
            {
                insideFence(fence.pointX, fence.pointY);
            }
        }

        private Boolean insideFence(string pointX, string pointY)
        {
            double centerX = Convert.ToDouble(pointX);
            double centerY = Convert.ToDouble(pointY);
            double radius = 50; //50 meters


            //inside  =   (x - center_x)^2 + (y - center_y)^2 <  radius^2
            if (Math.Pow((myX - centerX), 2) + Math.Pow((myY - centerY), 2) < Math.Pow(radius, 2))
            {
                return true;
            }
            //outside =   (x - center_x)^2 + (y - center_y)^2 >  radius^2
            else if (Math.Pow((myX - centerX), 2) + Math.Pow((myY - centerY), 2) > Math.Pow(radius, 2))
            {
                return false;
            }           
            //if it isnt outside or inside then it is on the edge of the circle
            //onCircle =   (x - center_x)^2 + (y - center_y)^2 == radius^2
            else
            {
                return true;
            }
        }

    }
}