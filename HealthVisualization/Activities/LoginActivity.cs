using Android.App;
using Android.OS;
using AndroidX.ViewPager.Widget;
using Firebase.Database;
using HealthVisualization.BaseClasses;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Newtonsoft.Json;
using Android.Views;
using Firebase.Database.Query;

namespace HealthVisualization.Activities
{
    [Activity(Label = "Login")]
    public class LoginActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_login);

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
            viewPager.Adapter = new CustomPagerAdapter(SupportFragmentManager);
        }
    }


    public class CustomPagerAdapter : FragmentPagerAdapter
    {
        // TODO: Defina novos nomes para as tabs
        private readonly string[] tabTitles = { "User", "Cadastrar" };

        public CustomPagerAdapter(AndroidX.Fragment.App.FragmentManager fm) : base(fm)
        {

        }

        public override int Count => tabTitles.Length;

        public override AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0:
                    return new TabFragment(Resource.Layout.activity_login_usuario);
                case 1:
                    return new TabFragment(Resource.Layout.activity_cadastro_usuario);
                default:
                    return null;
            }
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(tabTitles[position]);
        }
    }

    public class TabFragment : AndroidX.Fragment.App.Fragment
    {
        private readonly int layoutResourceId;

        public TabFragment(int layoutResourceId)
        {
            this.layoutResourceId = layoutResourceId;
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(layoutResourceId, container, false);

            Button loginButton = view.FindViewById<Button>(Resource.Id.buttonLogin);
            if (loginButton != null)
            {
                loginButton.Click += (s, e) => LoginButton_Click(s, e, view);
            }

            Button cadastrarUsuarioButton = view.FindViewById<Button>(Resource.Id.buttonCadastrar);
            if (cadastrarUsuarioButton != null)
            {
                cadastrarUsuarioButton.Click += (s, e) => CadastraUsuarioAsync(s, e, view);
            }

            return view;
        }


        private async void CadastraUsuarioAsync(object? sender, EventArgs e, View view)
        {
            // TODO: Adicione aqui os novos campos que foram criados
            var nomeUser = view.FindViewById<EditText>(Resource.Id.editTextNome);
            var nomeCpf = view.FindViewById<EditText>(Resource.Id.editTextCpf);
            var emailUser = view.FindViewById<EditText>(Resource.Id.editTextEmail);
            var senhaUser = view.FindViewById<EditText>(Resource.Id.editTextSenha);
            var confSenhaUser = view.FindViewById<EditText>(Resource.Id.editTextConfirmarSenha);


            if (senhaUser?.Text == confSenhaUser?.Text)
            {
                // Crie um objeto com os dados que deseja salvar
                var dados = new
                {
                    Nome = nomeUser?.Text,
                    Cpf = nomeCpf?.Text,
                    Senha = senhaUser?.Text,
                    Email = emailUser?.Text
                };

                try
                {
                    string jsonDados = JsonConvert.SerializeObject(dados);

                    // Busca a URL do Firebase do arquivo strings.xml
                    string firebaseUrl = Resources.GetString(Resource.String.firebase_url);

                    //Conecta com o banco de dados Realitme Database do Firebase
                    FirebaseClient firebase = new FirebaseClient(firebaseUrl);

                    // TODO: Defina uma nova raiz para o banco de dados. Exemplo: pessoas
                    var result = await firebase
                        .Child("pessoas")                      
                        .PostAsync(jsonDados);

                    if (result != null)
                    {
                        // reinicia valores dos campos da tela
                        nomeUser.Text = "";
                        senhaUser.Text = "";
                        emailUser.Text = "";
                        confSenhaUser.Text = "";

                        Toast.MakeText(Activity, "Cadastrado realizado com sucesso!", ToastLength.Short)?.Show();                        
                    }
                    else
                    {
                        Toast.MakeText(Activity, "O cadastro não pôde ser concluído!", ToastLength.Short)?.Show();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }
            }
            else
            {
                Toast.MakeText(Activity, "As senha estão diferentes!", ToastLength.Short)?.Show();
            }
        }

        private async void LoginButton_Click(object? sender, EventArgs e, View view)
        {
            // captura os valores do campos de texto da tela
            var email = view.FindViewById<EditText>(Resource.Id.editTextEmail)?.Text;
            var password = view.FindViewById<EditText>(Resource.Id.editTextPassword)?.Text;

            // Busca a URL do Firebase do arquivo strings.xml
            string firebaseUrl = Resources.GetString(Resource.String.firebase_url);

            //Conecta com o banco de dados Realitme Database do Firebase
            FirebaseClient firebase = new FirebaseClient(firebaseUrl);

            // TODO: Defina uma nova raiz para o banco de dados. Exemplo: pessoas
            var usuario = (await firebase
                .Child("pessoas")
                .OnceAsync<Usuario>()).Select(item => new Usuario
                {
                    Email = item.Object.Email,
                    Senha = item.Object.Senha,
                    Nome = item.Object.Nome
                }).Where(item => item.Email == email).FirstOrDefault();

            if (usuario != null)
            {
                if (usuario.Senha == password)
                {
                    MainActivity._usuario = usuario;

                    Toast.MakeText(Activity, "Usuário logado com sucesso!", ToastLength.Short)?.Show();
                    Activity?.Finish(); // Fecha a Activity pai a partir do Fragment
                }
                else
                {
                    Toast.MakeText(Activity, "Senha incorreta. Digite novamente!", ToastLength.Short)?.Show();
                }
            }
            else
            {
                Toast.MakeText(Activity, "Usuário não encontrado!", ToastLength.Short)?.Show();
            }
        }
    }
}