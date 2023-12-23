using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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

        private User currentUser;
        private List<Book> books;

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
            string sql = "SELECT id, username, password, isadmin FROM users WHERE username = @username AND password = @password";

            using (var conn = new NpgsqlConnection(databaseParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("Authentication successful!");

                            currentUser = new User()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Password = reader["password"].ToString(),
                                isAdmin = Convert.ToBoolean(reader["isadmin"])
                            };

                            books = GetBooks();
                            UpdateBooksList(books);
                            ShowBooksWindow();
                        }
                        else
                        {
                            Console.WriteLine("Authentication failed. Invalid username or password.");
                        }
                    }
                }
            }
        }

        public List<Book> GetBooks()
        {
            List<Book> books = new List<Book>();

            using (var conn = new NpgsqlConnection(databaseParams))
            {
                conn.Open();

                var query = "SELECT name, author, publisher, genre, releaseDate, pagesCount, totalPrice, costPrice, isSequel FROM booksData";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Book book = new Book
                            {
                                name = reader["name"].ToString(),
                                author = reader["author"].ToString(),
                                publisher = reader["publisher"].ToString(),
                                genre = reader["genre"].ToString(),
                                releaseDate = Convert.ToDateTime(reader["releasedate"]),
                                pagesCount = Convert.ToInt32(reader["pagescount"]),
                                totalPrice = (double)Convert.ToDecimal(reader["totalprice"]),
                                costPrice = (double)Convert.ToDecimal(reader["costprice"]),
                                isSequel = Convert.ToBoolean(reader["issequel"])
                            };
                            books.Add(book);
                        }
                    }
                }
            }

            return books;
        }

        public List<Book> Search(string data)
        {
            string searchData = data.ToLower();

            var result = books.FindAll(book =>
                book.genre.ToLower().Contains(searchData) ||
                book.author.ToLower().Contains(searchData) ||
                book.name.ToLower().Contains(searchData)
            );

            return result;
        }

        public void UpdateBooksList(List<Book> updateBooks)
        {
            booksList.Children.Clear();
            foreach (var book in updateBooks)
            {
                var cardExpander = new Wpf.Ui.Controls.CardExpander();
                cardExpander.Header = book.name;
                cardExpander.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                cardExpander.Margin = new Thickness(0, 6, 0, 0);

                var grid = new Grid();

                var stackPanel = new StackPanel();

                string[] labelsContent = { $"Автор: {book.author}", $"Издатель: {book.publisher}", $"Количество страниц: {book.pagesCount}", $"Жанр: {book.genre}", $"Год издания: {book.releaseDate.Year}", $"Себестоимость: {book.costPrice}р.", $"Цена для продажи: {book.totalPrice}р.", $"Сиквел: {(book.isSequel ? "Да" : "Нет")}" };

                foreach (var content in labelsContent)
                {
                    var label = new Label();
                    label.Content = content;
                    label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    label.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    label.Margin = new System.Windows.Thickness(0, 0, 0, 4);
                    stackPanel.Children.Add(label);
                }

                grid.Children.Add(stackPanel);
                cardExpander.Content = grid;
                booksList.Children.Add(cardExpander);
            }
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text.Length > 3 && PasswordInput.Text.Length > 3)
                Login(UsernameInput.Text, PasswordInput.Password);
        }

        private void SearchBtnClick(object sender, RoutedEventArgs e)
        {
            if (SearchInput.Text.Length >= 3)
                UpdateBooksList(Search(SearchInput.Text));
        }

        private void CatalogBtnClick(object sender, RoutedEventArgs e)
        {
            UpdateBooksList(books);
        }

        private void NewBooksClick(object sender, RoutedEventArgs e)
        {
            List<Book> newBooks = new List<Book>();

            foreach (var book in books)
            {
                if (book.releaseDate.Year >= 1920)
                    newBooks.Add(book);
            }

            UpdateBooksList(newBooks);
        }

        Wpf.Ui.Controls.Button previousSender;
        private void RandomBook(object sender, RoutedEventArgs e)
        {
            if (previousSender != (Wpf.Ui.Controls.Button)sender)
            {
                previousSender = (Wpf.Ui.Controls.Button)sender;
                Random random = new Random();
                List<Book> randomBooks = new List<Book>();

                foreach (var book in books)
                {
                    bool a = random.Next(0, 2) == 1;

                    if (a)
                    {
                        randomBooks.Add(book);
                    }
                }

                UpdateBooksList(randomBooks);
            }
        }
    }
}
