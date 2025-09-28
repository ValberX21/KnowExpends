using System.Collections.ObjectModel;
using KnowExpends.Mobile.Enums;
using KnowExpends.Mobile.Models;
using KnowExpends.Mobile.Services;

namespace KnowExpends.Mobile
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<ExpenseGroup> ExpenseGroups { get; set; }
        private DatabaseService _databaseService;
        private List<Expense> _allExpenses;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            ExpenseGroups = new ObservableCollection<ExpenseGroup>();
            _allExpenses = new List<Expense>();
            BindingContext = this;
            InitializeCategoryFilter();
            LoadExpenses();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadExpenses();
        }

        private void InitializeCategoryFilter()
        {
            var categories = Enum.GetValues<ExpenseCategory>().ToList();
            CategoryFilterPicker.Items.Add("Todas as categorias");
            foreach (var category in categories)
            {
                CategoryFilterPicker.Items.Add(category.ToString());
            }
            CategoryFilterPicker.SelectedIndex = 0;
            CategoryFilterPicker.SelectedIndexChanged += OnCategoryFilterChanged;
        }

        private async Task LoadExpenses()
        {
            try
            {
                _allExpenses = await _databaseService.GetExpensesAsync();
                FilterAndGroupExpenses();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar gastos: {ex.Message}", "OK");
            }
        }

        private void FilterAndGroupExpenses()
        {
            var filteredExpenses = _allExpenses;
            
            // Apply category filter if selected
            if (CategoryFilterPicker.SelectedIndex > 0)
            {
                var selectedCategory = (ExpenseCategory)(CategoryFilterPicker.SelectedIndex - 1);
                filteredExpenses = _allExpenses.Where(e => e.Category == selectedCategory).ToList();
            }
            
            // Group expenses by date
            var groupedExpenses = filteredExpenses
                .GroupBy(e => e.CreatedDate.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new ExpenseGroup(
                    g.Key.ToString("dd/MM/yyyy"), 
                    g.Key))
                .ToList();

            ExpenseGroups.Clear();
            foreach (var group in groupedExpenses)
            {
                // Add expenses to each group
                var groupExpenses = filteredExpenses
                    .Where(e => e.CreatedDate.Date == group.SortDate)
                    .OrderByDescending(e => e.CreatedDate)
                    .ToList();
                
                foreach (var expense in groupExpenses)
                {
                    group.Add(expense);
                }
                
                ExpenseGroups.Add(group);
            }
        }

        private void OnCategoryFilterChanged(object sender, EventArgs e)
        {
            FilterAndGroupExpenses();
        }

        private void OnClearFilterClicked(object sender, EventArgs e)
        {
            CategoryFilterPicker.SelectedIndex = 0;
            FilterAndGroupExpenses();
        }

        private async void OnAddButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddExpensePage());
        }

        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Expense expense)
            {
                await Navigation.PushAsync(new AddExpensePage(expense));
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Expense expense)
            {
                bool confirm = await DisplayAlert("Confirmar Exclusão", 
                    $"Deseja realmente excluir o gasto '{expense.Title}'?", 
                    "Sim", "Não");
                
                if (confirm)
                {
                    try
                    {
                        await _databaseService.DeleteExpenseAsync(expense);
                        await LoadExpenses(); // Refresh the list
                        await DisplayAlert("Sucesso", "Gasto excluído com sucesso!", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Erro", $"Erro ao excluir gasto: {ex.Message}", "OK");
                    }
                }
            }
        }

        private async void OnClipboardButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new MonthReportPage(_allExpenses));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao abrir relatório: {ex.Message}", "OK");
            }
        }
    }
}
