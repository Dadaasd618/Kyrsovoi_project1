using System;
using System.Windows;
using System.Windows.Input;
using PersonnelManagement.Data;
using PersonnelManagement.Models;

namespace PersonnelManagement.Views
{
    public partial class EmployeeEditWindow : Window
    {
        public Employee ResultEmployee { get; private set; }

        private readonly Employee _edit;
        private readonly LookupRepository _lookups = new LookupRepository();

        public EmployeeEditWindow(Employee toEdit)
        {
            InitializeComponent();

            // Загружаем справочники
            CbDepartment.ItemsSource = _lookups.GetDepartments();
            CbRole.ItemsSource = _lookups.GetRoles();

            _edit = toEdit;

            if (_edit != null)
            {
                // Заполняем поля
                TbLast.Text = _edit.LastName;
                TbFirst.Text = _edit.FirstName;
                TbMiddle.Text = _edit.MiddleName;
                TbPhone.Text = _edit.Phone;
                TbEmail.Text = _edit.Email;
                CbActive.IsChecked = _edit.IsActive;

                // ВАЖНО: выбираем объект по ID (не через SelectedValuePath)
                SelectComboItemById(CbDepartment, _edit.DepartmentId);
                SelectComboItemById(CbRole, _edit.RoleId);
            }
            else
            {
                CbActive.IsChecked = true;
                if (CbDepartment.Items.Count > 0) CbDepartment.SelectedIndex = 0;
                if (CbRole.Items.Count > 0) CbRole.SelectedIndex = 0;
            }
        }

        // Универсальный выбор элемента по ID — работает при любых названиях свойств
        private static void SelectComboItemById(System.Windows.Controls.ComboBox cb, int id)
        {
            foreach (var item in cb.Items)
            {
                int itemId = GetIntId(item);
                if (itemId == id)
                {
                    cb.SelectedItem = item;
                    return;
                }
            }
        }

        // Берём ID из объекта: ищем свойства Id / DepartmentId / RoleId / EmployeeId / ShiftTypeId
        private static int GetIntId(object obj)
        {
            if (obj == null) return 0;

            var t = obj.GetType();
            string[] props = { "Id", "DepartmentId", "RoleId", "EmployeeId", "ShiftTypeId" };

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
            if (string.IsNullOrWhiteSpace(TbLast.Text) || string.IsNullOrWhiteSpace(TbFirst.Text))
            {
                MessageBox.Show("Заполните Фамилию и Имя.");
                return;
            }

            // ВАЖНО: проверяем SelectedItem (а не SelectedValue)
            if (CbDepartment.SelectedItem == null || CbRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите отдел и должность.");
                return;
            }

            int departmentId = GetIntId(CbDepartment.SelectedItem);
            int roleId = GetIntId(CbRole.SelectedItem);

            if (departmentId == 0 || roleId == 0)
            {
                MessageBox.Show("Не удалось определить ID отдела/должности. Проверьте модели (Id/DepartmentId/RoleId).");
                return;
            }

            ResultEmployee = new Employee
            {
                EmployeeId = _edit != null ? _edit.EmployeeId : 0,
                LastName = TbLast.Text.Trim(),
                FirstName = TbFirst.Text.Trim(),
                MiddleName = (TbMiddle.Text ?? "").Trim(),
                Phone = (TbPhone.Text ?? "").Trim(),
                Email = (TbEmail.Text ?? "").Trim(),
                DepartmentId = departmentId,
                RoleId = roleId,
                IsActive = (CbActive.IsChecked == true)
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