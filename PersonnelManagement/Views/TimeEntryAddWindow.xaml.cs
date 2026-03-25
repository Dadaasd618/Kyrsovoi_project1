using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using PersonnelManagement.Data;
using PersonnelManagement.Models;

namespace PersonnelManagement.Views
{
    public partial class TimeEntryAddWindow : Window
    {
        public TimeEntry ResultTimeEntry { get; private set; }

        private readonly LookupRepository _lookups = new LookupRepository();

        public TimeEntryAddWindow()
        {
            InitializeComponent();

            CbEmployee.ItemsSource = _lookups.GetEmployees();
            CbShift.ItemsSource = _lookups.GetShiftTypes();

            DpDate.SelectedDate = DateTime.Today;
            if (CbEmployee.Items.Count > 0) CbEmployee.SelectedIndex = 0;
            if (CbShift.Items.Count > 0) CbShift.SelectedIndex = 0;
        }

        private static int GetIntId(object obj)
        {
            if (obj == null) return 0;

            var t = obj.GetType();
            string[] props = { "Id", "EmployeeId", "ShiftTypeId" };

            foreach (var p in props)
            {
                var pi = t.GetProperty(p);
                if (pi != null && pi.PropertyType == typeof(int))
                    return (int)pi.GetValue(obj);
            }
            return 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // ВАЖНО: проверяем SelectedItem
            if (CbEmployee.SelectedItem == null || CbShift.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника и тип смены.");
                return;
            }

            int employeeId = GetIntId(CbEmployee.SelectedItem);
            int shiftTypeId = GetIntId(CbShift.SelectedItem);

            if (employeeId == 0 || shiftTypeId == 0)
            {
                MessageBox.Show("Не удалось определить ID сотрудника/типа смены. Проверьте модели (Id/EmployeeId/ShiftTypeId).");
                return;
            }

            if (DpDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату.");
                return;
            }

            if (!TimeSpan.TryParseExact(TbStart.Text, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
            {
                MessageBox.Show("StartTime в формате HH:mm (пример: 09:00).");
                return;
            }

            if (!TimeSpan.TryParseExact(TbEnd.Text, @"hh\:mm", CultureInfo.InvariantCulture, out var end))
            {
                MessageBox.Show("EndTime в формате HH:mm (пример: 18:00).");
                return;
            }

            if (!int.TryParse(TbBreak.Text, out var br) || br < 0 || br > 300)
            {
                MessageBox.Show("Перерыв должен быть 0..300 минут.");
                return;
            }

            ResultTimeEntry = new TimeEntry
            {
                EmployeeId = employeeId,
                ShiftTypeId = shiftTypeId,
                WorkDate = DpDate.SelectedDate.Value.Date,
                StartTime = start,
                EndTime = end,
                BreakMinutes = br,
                Comment = (TbComment.Text ?? "").Trim()
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}