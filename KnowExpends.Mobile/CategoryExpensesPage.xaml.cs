using System.Collections.ObjectModel;
using KnowExpends.Mobile.Enums;
using KnowExpends.Mobile.Models;
using KnowExpends.Mobile.Services;

namespace KnowExpends.Mobile
{
    public partial class CategoryExpensesPage : ContentPage
    {
        public ObservableCollection<ExpenseGroup> ExpenseGroups { get; set; }
        private DatabaseService _databaseService;
        private ExpenseCategory _selectedCategory;
        private List<Expense> _allExpenses;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private bool _isCustomPeriod;

        public CategoryExpensesPage(ExpenseCategory category, DateTime? startDate = null, DateTime? endDate = null, bool isCustomPeriod = false)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _selectedCategory = category;
            _startDate = startDate;
            _endDate = endDate;
            _isCustomPeriod = isCustomPeriod;
            ExpenseGroups = new ObservableCollection<ExpenseGroup>();
            _allExpenses = new List<Expense>();
            BindingContext = this;
            
            LoadExpenses();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadExpenses();
        }

        private async Task LoadExpenses()
        {
            try
            {
                _allExpenses = await _databaseService.GetExpensesAsync();
                FilterAndGroupExpenses();
                UpdateHeader();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar gastos: {ex.Message}", "OK");
            }
        }

        private void FilterAndGroupExpenses()
        {
            // Filter expenses by selected category
            var filteredExpenses = _allExpenses
                .Where(e => e.Category == _selectedCategory)
                .ToList();

            // Apply date filter if provided
            if (_startDate.HasValue && _endDate.HasValue)
            {
                var start = _startDate.Value.Date;
                var end = _endDate.Value.Date.AddDays(1).AddTicks(-1);
                
                filteredExpenses = filteredExpenses
                    .Where(e => e.CreatedDate >= start && e.CreatedDate <= end)
                    .ToList();
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

            // Show/hide empty state
            EmptyStateFrame.IsVisible = !filteredExpenses.Any();
            ExpensesCollectionView.IsVisible = filteredExpenses.Any();
        }

        private void UpdateHeader()
        {
            var categoryExpenses = _allExpenses.Where(e => e.Category == _selectedCategory).ToList();
            
            // Apply date filter if provided
            if (_startDate.HasValue && _endDate.HasValue)
            {
                var start = _startDate.Value.Date;
                var end = _endDate.Value.Date.AddDays(1).AddTicks(-1);
                
                categoryExpenses = categoryExpenses
                    .Where(e => e.CreatedDate >= start && e.CreatedDate <= end)
                    .ToList();
            }
            
            var totalAmount = categoryExpenses.Sum(e => e.Price);
            var totalCount = categoryExpenses.Count;

            CategoryTitleLabel.Text = $"💰 {_selectedCategory}";
            
            if (_startDate.HasValue && _endDate.HasValue)
            {
                CategorySummaryLabel.Text = $"{totalCount} gastos • Total: ${totalAmount:F2} • Período: {_startDate.Value:dd/MM/yyyy} - {_endDate.Value:dd/MM/yyyy}";
            }
            else
            {
                CategorySummaryLabel.Text = $"{totalCount} gastos • Total: ${totalAmount:F2}";
            }
        }
    }
}

