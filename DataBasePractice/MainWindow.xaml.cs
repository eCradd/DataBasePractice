using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataBasePractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
 
        public partial class MainWindow : Window
        {
            private readonly AppDbContext _context;
            private readonly ObservableCollection<Person> _displayPeople = new();
            private bool _sortByAge = false;

            public MainWindow()
            {
                InitializeComponent();

            //contextulizing the database

                _context = new AppDbContext();
                _context.Database.EnsureCreated();

                // Load data into local tracking context
                _context.People.Load();

                dgPeople.ItemsSource = _displayPeople;

                // Default sorting: Last name ascending, then first name
                ApplyFilterAndSort();

                if (_displayPeople.Count > 0)
                {
                    dgPeople.SelectedIndex = 0;
                }
            }



        //filtering
            private void ApplyFilterAndSort(Person? itemToSelect = null)
            {
                var query = _context.People.Local.AsQueryable();

                //clause for search
                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p =>
                        (p.FirstName != null && p.FirstName.ToLower().Contains(searchText.ToLower())) ||
                        (p.LastName != null && p.LastName.ToLower().Contains(searchText.ToLower())));
                }

                // OrderBy clause for sorting (HELPED WITH AI HERE)
                query = _sortByAge
                    ? query.OrderBy(p => p.Age)
                    : query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

                var currentSelection = itemToSelect ?? dgPeople.SelectedItem as Person;

                _displayPeople.Clear();
                foreach (var person in query)
                {
                    _displayPeople.Add(person);
                }






                // Maintain or update selection
                if (currentSelection != null && _displayPeople.Contains(currentSelection))
                {
                    dgPeople.SelectedItem = currentSelection;
                }
                else if (_displayPeople.Count > 0)
                {
                    dgPeople.SelectedIndex = 0;
                }
            }

            private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            {
                ApplyFilterAndSort();
            }

            private void BtnSortByName_Click(object sender, RoutedEventArgs e)
            {
                _sortByAge = false;
                lblSortStatus.Text = "Sorted by last name, then first name";
                ApplyFilterAndSort();
            }

            private void BtnSortByAge_Click(object sender, RoutedEventArgs e)
            {
                _sortByAge = true;
                lblSortStatus.Text = "Sorted by age";
                ApplyFilterAndSort();
            }

            private void BtnAdd_Click(object sender, RoutedEventArgs e)
            {
                var newPerson = new Person
                {
                    FirstName = "New",
                    LastName = "Person",
                    Age = 20
                };

                _context.People.Add(newPerson);
                ApplyFilterAndSort(itemToSelect: newPerson);

                // Focus FirstName textbox and highlight its contents
                txtFirstName.Focus();
                txtFirstName.SelectAll();
            }

            private void BtnDelete_Click(object sender, RoutedEventArgs e)
            {
                if (dgPeople.SelectedItem is Person selectedPerson)
                {
                    _context.People.Remove(selectedPerson);
                    ApplyFilterAndSort();
                }
            }

            private void BtnSave_Click(object sender, RoutedEventArgs e)
            {
                _context.SaveChanges();
                MessageBox.Show("Changes saved to database.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            protected override void OnClosed(System.EventArgs e)
            {
                _context.Dispose();
                base.OnClosed(e);
            }
        }
    }