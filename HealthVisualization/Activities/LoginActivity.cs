using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Firebase.Database;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace HealthVisualization.Activities
{
    [Activity(Label = "Login")]
    public class LoginActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            Button loginButton = FindViewById<Button>(Resource.Id.buttonLogin);
            loginButton.Click += LoginButton_Click;
        }

        private async void LoginButton_Click(object sender, System.EventArgs e)
        {
            var emailEditText = FindViewById<EditText>(Resource.Id.editTextEmail);
            var passwordEditText = FindViewById<EditText>(Resource.Id.editTextPassword);

            string email = emailEditText.Text;
            string password = passwordEditText.Text;

            // Conecta com o banco de dados Realtime Database do Firebase
            string firebaseUrl = "https://ifpr-alerts-default-rtdb.firebaseio.com";
            var firebase = new FirebaseClient(firebaseUrl);

            // Consulta para verificar se o usuário existe
            var user = (await firebase
                .Child("usuarios")
                .OnceAsync<Usuario>())
                .Select(item => new Usuario
                {
                    Email = item.Object.Email,
                    Senha = item.Object.Senha,
                    Nome = item.Object.Nome
                }).FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                if (user.Senha == password)
                {
                    Toast.MakeText(this, "Login efetuado com sucesso!", ToastLength.Short).Show();
                    // Redireciona para a MainActivity ou outra activity após o login bem-sucedido
                    var intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                }
                else
                {
                    Toast.MakeText(this, "Senha incorreta. Digite novamente!", ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "Usuário não encontrado!", ToastLength.Short).Show();
            }
        }

        public class Usuario
        {
            public string Email { get; set; }
            public string Senha { get; set; }
            public string Nome { get; set; }
        }
    }
}