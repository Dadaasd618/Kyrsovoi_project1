using Microsoft.Win32;
using PersonnelManagement.Data;
using PersonnelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonnelManagement.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _user;
        private readonly EmployeeRepository _empRepo = new EmployeeRepository();
        private readonly TimeEntryRepository _timeRepo = new TimeEntryRepository();
        private readonly ReportRepository _reportRepo = new ReportRepository();
        private readonly LookupRepository _lookupRepo = new LookupRepository();
        private readonly StatusRepository _statusRepo = new StatusRepository();

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;

            TbHeader.Text = $"Вы вошли: {_user.FullName} ({_user.Login})";

            RpStart.SelectedDate = DateTime.Today.AddMonths(-1);
            RpEnd.SelectedDate = DateTime.Today;

            LoadEmployeesForStatus();
            LoadAll();
        }

        private void LoadAll()
        {
            LvEmployees.ItemsSource = _empRepo.GetAll();
            DpStart.SelectedDate = DateTime.Today.AddDays(-30);
            DpEnd.SelectedDate = DateTime.Today;
            LoadTimeByPeriod();
            LoadStatuses();
        }

        private void LoadEmployeesForStatus()
        {
            var employees = _lookupRepo.GetEmployees();
            CbEmployeeForStatus.ItemsSource = employees;
            if (employees.Any()) CbEmployeeForStatus.SelectedIndex = 0;
        }

        private void LoadTimeByPeriod()
        {
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null) return;
            var data = _timeRepo.GetByPeriod(DpStart.SelectedDate.Value, DpEnd.SelectedDate.Value);
            LvTime.ItemsSource = data;
        }

        private void LoadStatuses()
        {
            var statuses = _statusRepo.GetActiveStatuses(DateTime.Today);
            var displayList = statuses.Select(s => new
            {
                s.EmployeeName,
                s.StatusType,
                s.StartDate,
                s.EndDate,
                DaysCount = (s.EndDate - s.StartDate).Days + 1,
                s.Comment
            }).ToList();
            LvStatuses.ItemsSource = displayList;
        }

        // Сотрудники
        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadAll();
        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var win = new EmployeeEditWindow(null) { Owner = this };
            if (win.ShowDialog() == true)
            {
                _empRepo.Insert(win.ResultEmployee);
                LvEmployees.ItemsSource = _empRepo.GetAll();
            }
        }
        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = LvEmployees.SelectedItem as Employee;
            if (emp == null) return;
            var win = new EmployeeEditWindow(emp) { Owner = this };
            if (win.ShowDialog() == true)
            {
                _empRepo.Update(win.ResultEmployee);
                LvEmployees.ItemsSource = _empRepo.GetAll();
            }
        }
        private void DeactivateEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = LvEmployees.SelectedItem as Employee;
            if (emp == null) return;
            if (MessageBox.Show($"Сделать сотрудника {emp.FullName} неактивным?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _empRepo.SoftDeactivate(emp.EmployeeId);
                LvEmployees.ItemsSource = _empRepo.GetAll();
            }
        }

        // Рабочие часы
        private void AddTimeEntry_Click(object sender, RoutedEventArgs e)
        {
            var win = new TimeEntryAddWindow() { Owner = this };
            if (win.ShowDialog() == true)
            {
                _timeRepo.Insert(win.ResultTimeEntry);
                LoadTimeByPeriod();
            }
        }
        private void LoadLast30_Click(object sender, RoutedEventArgs e)
        {
            DpStart.SelectedDate = DateTime.Today.AddDays(-30);
            DpEnd.SelectedDate = DateTime.Today;
            LoadTimeByPeriod();
        }
        private void LoadTimeByPeriod_Click(object sender, RoutedEventArgs e) => LoadTimeByPeriod();

        // Отчёты
        private void BuildReport_Click(object sender, RoutedEventArgs e)
        {
            if (RpStart.SelectedDate == null || RpEnd.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты периода.");
                return;
            }
            var start = RpStart.SelectedDate.Value;
            var end = RpEnd.SelectedDate.Value;

            string selected = (CbReportType.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selected == "Отработанные часы")
            {
                var data = _reportRepo.GetHoursReport(start, end);
                LvReport.ItemsSource = data;
                ReportGridView.Columns.Clear();
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Код", DisplayMemberBinding = new System.Windows.Data.Binding("EmployeeId"), Width = 60 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "ФИО", DisplayMemberBinding = new System.Windows.Data.Binding("ФИО"), Width = 200 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Отдел", DisplayMemberBinding = new System.Windows.Data.Binding("Отдел"), Width = 150 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Часов", DisplayMemberBinding = new System.Windows.Data.Binding("ОтработаноЧасов"), Width = 80 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Ставка (руб/час)", DisplayMemberBinding = new System.Windows.Data.Binding("Ставка"), Width = 100 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Сумма (руб)", DisplayMemberBinding = new System.Windows.Data.Binding("Сумма"), Width = 100 });
            }
            else if (selected == "Отсутствия (отпуск/больничный)")
            {
                var data = _reportRepo.GetLeaveReport(start, end);
                LvReport.ItemsSource = data;
                ReportGridView.Columns.Clear();
                ReportGridView.Columns.Add(new GridViewColumn { Header = "ФИО", DisplayMemberBinding = new System.Windows.Data.Binding("ФИО"), Width = 200 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Тип", DisplayMemberBinding = new System.Windows.Data.Binding("Тип"), Width = 120 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "С", DisplayMemberBinding = new System.Windows.Data.Binding("С"), Width = 100 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "По", DisplayMemberBinding = new System.Windows.Data.Binding("По"), Width = 100 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Дней", DisplayMemberBinding = new System.Windows.Data.Binding("Дней"), Width = 70 });
                ReportGridView.Columns.Add(new GridViewColumn { Header = "Комментарий", DisplayMemberBinding = new System.Windows.Data.Binding("Комментарий"), Width = 200 });
            }
            else if (selected == "Сводка по сотруднику")
            {
                var employees = _lookupRepo.GetEmployees();
                if (!employees.Any())
                {
                    MessageBox.Show("Нет активных сотрудников.");
                    return;
                }

                // Создаём окно с белым фоном и чёрным текстом, переопределяя все стили
                var selectWin = new Window
                {
                    Title = "Выбор сотрудника",
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = System.Windows.Media.Brushes.White,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                // Добавляем ресурсы, чтобы переопределить стили для всех элементов внутри
                selectWin.Resources.Add(typeof(TextBlock), new Style(typeof(TextBlock))
                {
                    Setters = { new Setter(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Black) }
                });
                selectWin.Resources.Add(typeof(ComboBox), new Style(typeof(ComboBox))
                {
                    Setters =
            {
                new Setter(ComboBox.ForegroundProperty, System.Windows.Media.Brushes.Black),
                new Setter(ComboBox.BackgroundProperty, System.Windows.Media.Brushes.White)
            }
                });
                selectWin.Resources.Add(typeof(Button), new Style(typeof(Button))
                {
                    Setters =
            {
                new Setter(Button.ForegroundProperty, System.Windows.Media.Brushes.Black),
                new Setter(Button.BackgroundProperty, System.Windows.Media.Brushes.LightGray)
            }
                });

                var panel = new StackPanel { Margin = new Thickness(10) };
                panel.Children.Add(new TextBlock { Text = "Выберите сотрудника:", Margin = new Thickness(0, 0, 0, 10) });
                var combo = new ComboBox { Name = "CbEmp", ItemsSource = employees, DisplayMemberPath = "Name", SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 20) };
                panel.Children.Add(combo);
                var btn = new Button { Content = "ОК", Width = 80, HorizontalAlignment = HorizontalAlignment.Center };
                btn.Click += (s, ev) => selectWin.DialogResult = true;
                panel.Children.Add(btn);

                selectWin.Content = panel;

                if (selectWin.ShowDialog() == true)
                {
                    var selectedEmp = combo.SelectedItem as LookupItem;
                    if (selectedEmp != null)
                    {
                        var summary = _reportRepo.GetEmployeeSummary(selectedEmp.Id, start, start);
                        LvReport.ItemsSource = new List<EmployeeTimeSummary> { summary };
                        ReportGridView.Columns.Clear();
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "ФИО", DisplayMemberBinding = new System.Windows.Data.Binding("ФИО"), Width = 200 });
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "Часов за неделю", DisplayMemberBinding = new System.Windows.Data.Binding("ЧасовЗаНеделю"), Width = 120 });
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "Часов за месяц", DisplayMemberBinding = new System.Windows.Data.Binding("ЧасовЗаМесяц"), Width = 120 });
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "Ставка (руб/час)", DisplayMemberBinding = new System.Windows.Data.Binding("Ставка"), Width = 100 });
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "Зарплата за неделю", DisplayMemberBinding = new System.Windows.Data.Binding("ЗарплатаЗаНеделю"), Width = 120 });
                        ReportGridView.Columns.Add(new GridViewColumn { Header = "Зарплата за месяц", DisplayMemberBinding = new System.Windows.Data.Binding("ЗарплатаЗаМесяц"), Width = 120 });
                    }
                }
            }
        }

        // Экспорт в HTML для всех отчётов
        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            string selected = (CbReportType.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selected == "Отработанные часы")
                ExportHoursReportToHtml();
            else if (selected == "Отсутствия (отпуск/больничный)")
                ExportLeaveReportToHtml();
            else if (selected == "Сводка по сотруднику")
                ExportEmployeeSummaryToHtml();
        }

        private void ExportHoursReportToHtml()
        {
            var data = LvReport.ItemsSource as List<HoursReportRow>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта. Сначала сформируйте отчёт.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "HTML файлы (*.html)|*.html",
                FileName = $"Отчет_Отработанные_часы_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };
            if (sfd.ShowDialog() != true) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'><title>Отчёт по отработанным часам</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Times New Roman', Times, serif; margin: 2cm; }");
            sb.AppendLine(".header { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine(".title { font-size: 20pt; font-weight: bold; margin: 20px 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".footer { margin-top: 30px; text-align: right; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'><div class='title'>ОТЧЕТ</div>");
            sb.AppendLine("<div>по отработанным часам</div></div>");

            sb.AppendLine($"<div class='info'>");
            sb.AppendLine($"<p><strong>Период:</strong> {RpStart.SelectedDate:dd.MM.yyyy} – {RpEnd.SelectedDate:dd.MM.yyyy}</p>");
            sb.AppendLine($"<p><strong>Дата формирования:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Пользователь:</strong> {_user.FullName}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine("<th>Код</th><th>ФИО</th><th>Отдел</th><th>Часов</th><th>Ставка (руб/час)</th><th>Сумма (руб)</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var row in data)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.EmployeeId}</td>");
                sb.AppendLine($"<td>{row.ФИО}</td>");
                sb.AppendLine($"<td>{row.Отдел}</td>");
                sb.AppendLine($"<td>{row.ОтработаноЧасов:F2}</td>");
                sb.AppendLine($"<td>{row.Ставка:F2}</td>");
                sb.AppendLine($"<td>{row.Сумма:F2}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<div class='footer'><p>_________________ / _________________ /</p><p>(подпись)      (расшифровка)</p></div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Отчёт сохранён в {sfd.FileName}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportLeaveReportToHtml()
        {
            var data = LvReport.ItemsSource as List<LeaveReportRow>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта. Сначала сформируйте отчёт.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "HTML файлы (*.html)|*.html",
                FileName = $"Отчет_Отсутствия_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };
            if (sfd.ShowDialog() != true) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'><title>Отчёт по отсутствиям</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Times New Roman', Times, serif; margin: 2cm; }");
            sb.AppendLine(".header { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine(".title { font-size: 20pt; font-weight: bold; margin: 20px 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".footer { margin-top: 30px; text-align: right; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'><div class='title'>ОТЧЕТ</div>");
            sb.AppendLine("<div>по отсутствиям (отпуск, больничный, командировка)</div></div>");

            sb.AppendLine($"<div class='info'>");
            sb.AppendLine($"<p><strong>Период:</strong> {RpStart.SelectedDate:dd.MM.yyyy} – {RpEnd.SelectedDate:dd.MM.yyyy}</p>");
            sb.AppendLine($"<p><strong>Дата формирования:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Пользователь:</strong> {_user.FullName}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine("<th>ФИО</th><th>Тип</th><th>С</th><th>По</th><th>Дней</th><th>Комментарий</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var row in data)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.ФИО}</td>");
                sb.AppendLine($"<td>{row.Тип}</td>");
                sb.AppendLine($"<td>{row.С:dd.MM.yyyy}</td>");
                sb.AppendLine($"<td>{row.По:dd.MM.yyyy}</td>");
                sb.AppendLine($"<td>{row.Дней}</td>");
                sb.AppendLine($"<td>{row.Комментарий}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<div class='footer'><p>_________________ / _________________ /</p><p>(подпись)      (расшифровка)</p></div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Отчёт сохранён в {sfd.FileName}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportEmployeeSummaryToHtml()
        {
            var summary = LvReport.ItemsSource as List<EmployeeTimeSummary>;
            if (summary == null || summary.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта. Сначала сформируйте сводку по сотруднику.");
                return;
            }

            var data = summary[0];
            string position = GetEmployeePosition(data.ФИО);

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "HTML файлы (*.html)|*.html",
                FileName = $"Отчет_{data.ФИО}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };
            if (sfd.ShowDialog() != true) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'><title>Отчёт о проделанной работе</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Times New Roman', Times, serif; margin: 2cm; }");
            sb.AppendLine(".header { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine(".title { font-size: 20pt; font-weight: bold; margin: 20px 0; }");
            sb.AppendLine(".info { margin-bottom: 20px; line-height: 1.6; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".footer { margin-top: 30px; text-align: right; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'><div class='title'>ОТЧЕТ</div>");
            sb.AppendLine("<div>о проделанной работе за день сотрудника на дистанционной работе</div></div>");

            sb.AppendLine("<div class='info'>");
            sb.AppendLine($"<p><strong>Сотрудник:</strong> {data.ФИО}</p>");
            sb.AppendLine($"<p><strong>Должность:</strong> {position}</p>");
            sb.AppendLine($"<p><strong>Период составления отчета:</strong> {RpStart.SelectedDate:dd.MM.yyyy} – {RpEnd.SelectedDate:dd.MM.yyyy}</p>");
            sb.AppendLine($"<p><strong>Дата составления отчета:</strong> {DateTime.Now:dd.MM.yyyy}</p>");
            sb.AppendLine($"<p><strong>Место</strong> ___________________</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table><thead>");
            sb.AppendLine("<tr><th>Номер задания</th><th>Задание</th><th>Результат</th><th>Время работы</th></tr>");
            sb.AppendLine("</thead><tbody>");
            for (int i = 1; i <= 5; i++)
            {
                sb.AppendLine("<tr><td></td><td></td><td></td><td></td></tr>");
            }
            sb.AppendLine("</tbody></table>");

            sb.AppendLine("<div class='footer'><p>_________________ / _________________ /</p><p>(подпись)      (расшифровка)</p></div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Отчёт сохранён в {sfd.FileName}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetEmployeePosition(string fullName)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT r.Название 
FROM dbo.Сотрудники e
JOIN dbo.Должности r ON r.КодДолжности = e.КодДолжности
WHERE (e.Фамилия + ' ' + e.Имя + ' ' + ISNULL(e.Отчество,'')) = @name", con))
                {
                    cmd.Parameters.AddWithValue("@name", fullName);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Не указана";
                }
            }
        }

        // Управление отсутствиями
        private void AddStatus_Click(object sender, RoutedEventArgs e)
        {
            if (CbEmployeeForStatus.SelectedItem == null || DpStatusStart.SelectedDate == null || DpStatusEnd.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var empId = (CbEmployeeForStatus.SelectedItem as LookupItem).Id;
            var type = (CbStatusType.SelectedItem as ComboBoxItem)?.Content.ToString();
            var start = DpStatusStart.SelectedDate.Value;
            var end = DpStatusEnd.SelectedDate.Value;
            if (end < start)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.");
                return;
            }

            _statusRepo.AddStatus(new EmployeeStatus
            {
                EmployeeId = empId,
                StatusType = type,
                StartDate = start,
                EndDate = end,
                Comment = ""
            });

            LoadStatuses();
            MessageBox.Show("Статус добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshStatuses_Click(object sender, RoutedEventArgs e) => LoadStatuses();

        // Помощь
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Система управления персоналом\n\n" +
                "Версия 1.0\n" +
                "Автор: Путников Дмитрий Васильевич\n\n" +
                "Руководство:\n" +
                "• Сотрудники – добавление, редактирование, деактивация.\n" +
                "• Рабочие часы – учёт отработанного времени с выбором периода.\n" +
                "• Отчёты – выбор типа отчёта и периода, экспорт в HTML.\n" +
                "• Управление отсутствиями – добавление отпусков, больничных, командировок.\n\n" +
                "Для выхода нажмите ✕.",
                "Помощь",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Оконные команды
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}