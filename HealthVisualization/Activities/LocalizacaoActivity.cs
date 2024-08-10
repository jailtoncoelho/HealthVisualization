using Android.App;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Locations;
using Android.Widget;
using AndroidX.AppCompat.App;
using Android.Runtime;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Content;
using Java.Security;
using Android.Content.PM;
using Android.Media;

namespace HealthVisualization.Activities
{
    [Activity(Label = "LocalizacaoActivity", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LocalizacaoActivity : AppCompatActivity, IOnMapReadyCallback, Android.Locations.ILocationListener
    {
        private GoogleMap _map;
        private LocationManager _locationManager;
        private string _locationProvider;
        private TextView _locationTextView;
        const int RequestLocationId = 1;

        private LatLng _startLocation;
        private List<LatLng> _route;
        private int _currentIndex;
        private Handler _handler;
        private Action _updatePositionAction;
        private Marker _currentMarker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_localizacao);

            _locationTextView = FindViewById<TextView>(Resource.Id.locationTextView);

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.AccessFineLocation }, RequestLocationId);
            }
            else
            {
                InitializeMap();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            // Permissão concedida
                            InitializeMap();
                        }
                        else
                        {
                            // Permissão negada, você pode mostrar uma mensagem ou fazer outra ação
                            Toast.MakeText(this, "Permissão de localização foi recusada.", ToastLength.Short).Show(); // OK? - TODO: Definir uma nova mensagem toast
                        }
                    }
                    break;
            }
        }

        private void InitializeMap()
        {
            var mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            _startLocation = new LatLng(-23.550520, -46.633308); // Início do percurso (São Paulo, Brasil) // TODO: Definir um novo ponto de início da rota
            _route = GenerateRoute(_startLocation, 5000, 50); // Gera uma rota de 5km com 50 pontos // TODO: Definir uma nova rota

            _handler = new Handler();
            _updatePositionAction = new Action(UpdatePosition);
        }

        private void UpdatePosition()
        {
            if (_route == null || _route.Count == 0)
                return;

            if (_currentIndex >= _route.Count)
            {
                _currentIndex = 0; // Reinicia o percurso
            }

            LatLng newLocation = _route[_currentIndex];
            _currentIndex++;

            Location location = new Location("fake_provider")
            {
                Latitude = newLocation.Latitude,
                Longitude = newLocation.Longitude,
                Accuracy = 1
            };

            OnLocationChanged(location);

            // Agenda a próxima atualização
            _handler.PostDelayed(_updatePositionAction, 3000); // OK? - TODO: Definir um novo intervalo de tempo de atualização da rota
        }

        private List<LatLng> GenerateRoute(LatLng startLocation, double totalDistanceMeters, int points)
        {
            List<LatLng> route = new List<LatLng>();
            double distancePerPoint = totalDistanceMeters / points;

            Random random = new Random();
            double lat = startLocation.Latitude;
            double lng = startLocation.Longitude;

            for (int i = 0; i < points; i++)
            {
                // Gera pequenos deslocamentos aleatórios
                double deltaLat = (random.NextDouble() - 0.5) * 0.001;
                double deltaLng = (random.NextDouble() - 0.5) * 0.001;

                lat += deltaLat;
                lng += deltaLng;

                route.Add(new LatLng(lat, lng));
            }

            return route;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            _map.MyLocationEnabled = true;

            _locationManager = (LocationManager)GetSystemService(LocationService);
            _locationProvider = LocationManager.GpsProvider;

            if (_locationManager.IsProviderEnabled(_locationProvider))
            {
                StartLocationUpdates();
            }
            else
            {
                Toast.MakeText(this, "GPS não esta habilitado", ToastLength.Short).Show(); // OK? - TODO: Definir uma nova mensagem toast
            }
        }

        private void StartLocationUpdates()
        {
            //_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);

            // Inicia a atualização das posições
            _handler.PostDelayed(_updatePositionAction, 3000); // OK? - TODO: Definir um novo intervalo de tempo de atualização da rota
        }

        public void OnLocationChanged(Location location)
        {
            if (location != null && _map != null)
            {
                LatLng userLocation = new LatLng(location.Latitude, location.Longitude);
                _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(userLocation, 15));

                // Remove o marcador atual, se houver
                _currentMarker?.Remove();

                // Adiciona um novo marcador e armazena a referência
                _currentMarker = _map.AddMarker(new MarkerOptions()
                    .SetPosition(userLocation)
                    .SetTitle("You are here")
                    .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue)));

                // Atualiza o texto com a latitude e longitude atual
                _locationTextView.Text = $"A localização está nas coordenadas: Latitude: {location.Latitude}, Longitude: {location.Longitude}"; // OK? - TODO: Definir um novo texto do locationTextView
            }
        }

        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }
    }
}