using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Book_Shop
{
    public partial class MainWindow : Window
    {
        private static string databaseName = "BookBase";
        private static string databasePassword = "050512ok";
        private string databaseParams = $"Host=localhost;Username=postgres;Password={databasePassword};Database={databaseName};";
        
        public MainWindow()
        {
            InitializeComponent();
            ShowAuthWindow();
        }

        private void ShowAuthWindow()
        {
            BooksWindow.Visibility = Visibility.Hidden;
            AuthWindow.Visibility = Visibility.Visible;
        }

        private void ShowBooksWindow()
        {
            AuthWindow.Visibility = Visibility.Hidden;
            BooksWindow.Visibility = Visibility.Visible;
        }

        private void Login(string username, string password)
        {
            string sql = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";

            using (var conn = new NpgsqlConnection(databaseParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine("Authentication successful!");
                        ShowBooksWindow();
                    }
                    else
                    {
                        Console.WriteLine("Authentication failed. Invalid username or password.");
                        //При не успешной авторизации
                    }
                }
            }
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text.Length > 3 && PasswordInput.Text.Length > 3)
                Login(UsernameInput.Text, PasswordInput.Password);
        }
    }
}
