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
            EditBook.Visibility = Visibility.Hidden;
            AuthWindow.Visibility = Visibility.Visible;
        }

        private void ShowBooksWindow()
        {
            EditBook.Visibility = Visibility.Hidden;
            AuthWindow.Visibility = Visibility.Hidden;
            BooksWindow.Visibility = Visibility.Visible;
        }

        private void ShowEditBookWindow(Book book, bool isEdit)
        {
            if(isEdit)
            {
                EditBookLabel.Content = "Изменение книги";
                EditBookAuthor.Text = book.author;
                EditBookGenre.Text = book.genre;
                EditBookName.Text = book.name;
                EditBookPublisher.Text = book.publisher;
                EditBookPagesCount.Text = book.pagesCount.ToString();
                EditBookCostPrice.Text = book.costPrice.ToString();
                EditBookTotalPrice.Text = book.totalPrice.ToString();
                EditBookDate.SelectedDate = book.releaseDate;
                EditBookSiquel.IsChecked = book.isSequel;
            }
            else
            {
                EditBookLabel.Content = "Добавление книги";
                EditBookAuthor.Text = null;
                EditBookGenre.Text = null;
                EditBookName.Text = null;
                EditBookPublisher.Text = null;
                EditBookPagesCount.Text = null;
                EditBookCostPrice.Text = null;
                EditBookTotalPrice.Text = null;
                EditBookDate.SelectedDate = null;
                EditBookSiquel.IsChecked = null;
            }

            BooksWindow.Visibility = Visibility.Hidden;
            AuthWindow.Visibility = Visibility.Hidden;
            EditBook.Visibility = Visibility.Visible;
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
                            currentUser = new User()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Password = reader["password"].ToString(),
                                isAdmin = Convert.ToBoolean(reader["isadmin"])
                            };

                            AddBookButton.Visibility = currentUser.isAdmin ? Visibility.Visible : Visibility.Collapsed;

                            books = GetBooks();
                            UpdateBooksList(books);
                            ShowBooksWindow();
                        }
                        else
                        {
                            MessageBox.Show("Неверное имя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void Register(string username, string password)
        {
            using (var connection = new NpgsqlConnection(databaseParams))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
                    cmd.Parameters.AddWithValue("username", username);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Имя занято.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    cmd.CommandText = "INSERT INTO users (username, password) VALUES (@username, @password)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.Parameters.AddWithValue("isadmin", false);
                    cmd.ExecuteNonQuery();
                    Login(username, password);
                }
            }
        }

        private List<Book> GetBooks()
        {
            List<Book> books = new List<Book>();

            using (var conn = new NpgsqlConnection(databaseParams))
            {
                conn.Open();

                var query = "SELECT name, author, publisher, genre, releaseDate, pagesCount, totalPrice, costPrice, isSequel FROM booksdata";

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

            books = books.OrderBy(b => b.name).ToList();

            return books;
        }

        public void UpdateOrAddBook(string name, string author, string publisher, string genre, DateTime releaseDate, int pagesCount, double totalPrice, double costPrice, bool isSequel)
        {
            using (var connection = new NpgsqlConnection(databaseParams))
            {
                connection.Open();

                using (var command = new NpgsqlCommand())
                {
                    command.Connection = connection;

                    command.CommandText = "SELECT COUNT(*) FROM booksdata WHERE name = @Name AND author = @Author";
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Author", author);

                    int existingCount = Convert.ToInt32(command.ExecuteScalar());

                    if (existingCount > 0)
                    {
                        command.CommandText = @"UPDATE booksdata SET publisher = @Publisher, genre = @Genre, releaseDate = @ReleaseDate, 
                                            pagesCount = @PagesCount, totalPrice = @TotalPrice, costPrice = @CostPrice, isSequel = @IsSequel 
                                            WHERE name = @Name AND author = @Author";
                    }
                    else
                    {
                        command.CommandText = @"INSERT INTO booksdata(name, author, publisher, genre, releaseDate, pagesCount, totalPrice, costPrice, isSequel) 
                                            VALUES (@Name, @Author, @Publisher, @Genre, @ReleaseDate, @PagesCount, @TotalPrice, @CostPrice, @IsSequel)";
                    }

                    command.Parameters.AddWithValue("@Publisher", publisher);
                    command.Parameters.AddWithValue("@Genre", genre);
                    command.Parameters.AddWithValue("@ReleaseDate", releaseDate);
                    command.Parameters.AddWithValue("@PagesCount", pagesCount);
                    command.Parameters.AddWithValue("@TotalPrice", totalPrice);
                    command.Parameters.AddWithValue("@CostPrice", costPrice);
                    command.Parameters.AddWithValue("@IsSequel", isSequel);

                    command.ExecuteNonQuery();
                }
            }

            books = GetBooks();
            UpdateBooksList(books);
            ShowBooksWindow();
        }

        private List<Book> Search(string data)
        {
            string searchData = data.ToLower();

            var result = books.FindAll(book =>
                book.genre.ToLower().Contains(searchData) ||
                book.author.ToLower().Contains(searchData) ||
                book.name.ToLower().Contains(searchData)
            );

            return result;
        }

        private void UpdateBooksList(List<Book> updateBooks)
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

                if (currentUser.isAdmin)
                {
                    var stackPanel2 = new StackPanel();
                    stackPanel2.Orientation = Orientation.Horizontal;
                    Wpf.Ui.Controls.Button edit = new Wpf.Ui.Controls.Button();
                    edit.Content = "Изменить";

                    edit.Margin = new Thickness(0, 0, 6, 0);
                    edit.Click += (sender, e) =>
                    {
                        ShowEditBookWindow(book, true);
                    };
                    stackPanel2.Children.Add(edit);

                    Wpf.Ui.Controls.Button remove = new Wpf.Ui.Controls.Button();
                    remove.Content = "Удалить";
                    remove.Click += (sender, e) =>
                    {
                        DeleteBookByName(book.name);
                        booksList.Children.Remove(cardExpander);
                        books.Remove(book);
                    };

                    stackPanel2.Children.Add(remove);

                    stackPanel.Children.Add(stackPanel2);
                }

                grid.Children.Add(stackPanel);
                cardExpander.Content = grid;
                booksList.Children.Add(cardExpander);
            }
        }

        public void DeleteBookByName(string bookName)
        {
            using (var conn = new NpgsqlConnection(databaseParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM booksdata WHERE name = @bookName";
                    cmd.Parameters.AddWithValue("bookName", bookName);

                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text.Length > 3 && PasswordInput.Text.Length > 3)
                Login(UsernameInput.Text, PasswordInput.Password);
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text.Length > 3 && PasswordInput.Text.Length > 3)
                Register(UsernameInput.Text, PasswordInput.Password);
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

        private void CloseEditWindowClick(object sender, RoutedEventArgs e)
        {
            ShowBooksWindow();
        }

        private void AddOrEditBook(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EditBookName.Text != null && EditBookAuthor.Text != null && EditBookPublisher.Text != null && EditBookGenre.Text != null && DateTime.TryParse(EditBookDate.Text, out DateTime selectedDate) && int.TryParse(EditBookPagesCount.Text, out int pageCount) && double.TryParse(EditBookTotalPrice.Text, out double totalPrice) && double.TryParse(EditBookCostPrice.Text, out double costPrice) && EditBookSiquel.IsChecked != null)
                    UpdateOrAddBook(EditBookName.Text, EditBookAuthor.Text, EditBookPublisher.Text, EditBookGenre.Text, EditBookDate.SelectedDate ?? DateTime.MinValue, Int32.Parse(EditBookPagesCount.Text), Double.Parse(EditBookTotalPrice.Text), Double.Parse(EditBookCostPrice.Text), EditBookSiquel.IsChecked ?? false);
            }
            catch { }
        }

        private void AddBookClick(object sender, RoutedEventArgs e)
        {
            ShowEditBookWindow(null, false);
        }
    }
}
