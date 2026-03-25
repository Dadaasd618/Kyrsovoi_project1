using System;
using System.Windows;
using System.Windows.Input;
using PersonnelManagement.Data;

namespace PersonnelManagement.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _auth = new AuthService();

        public LoginWindow()
        {
            InitializeComponent();
            TbError.Text = "";
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TbError.Text = "";

                var login = (TbLogin.Text ?? "").Trim();
                var pass = PbPassword.Password ?? "";

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
                {
                    TbError.Text = "Введите логин и пароль.";
                    return;
                }

                var user = _auth.Login(login, pass);
                if (user == null)
                {
                    TbError.Text = "Неверный логин или пароль.";
                    return;
                }

                var main = new MainWindow(user);
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                TbError.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}