using System.Collections.ObjectModel;
using KnowExpends.Mobile.Models;
using KnowExpends.Mobile.Services;
using KnowExpends.Mobile.Enums;

namespace KnowExpends.Mobile
{
    public class CategorySummaryWithPercentage
    {
        public ExpenseCategory Category { get; set; }
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public partial class MonthReportPage : ContentPage
    {
        public ObservableCollection<CategorySummaryWithPercentage> CategorySummaries { get; set; }
        public decimal TotalAmount { get; set; }
        private ExpenseCalculator.MonthlySummary _monthlySummary;
        private readonly List<Expense> _allExpenses;
        private bool _isCustomPeriodView = false;

        public MonthReportPage(List<Expense> allExpenses)
        {
            InitializeComponent();
            CategorySummaries = new ObservableCollection<CategorySummaryWithPercentage>();
            BindingContext = this;
            _allExpenses = allExpenses;

            // Initialize date pickers
            var now = DateTime.Now;
            MonthPicker.Date = new DateTime(now.Year, now.Month, 1);
            StartDatePicker.Date = new DateTime(now.Year, now.Month, 1);
            EndDatePicker.Date = now.Date;

            CalculateAndDisplaySummaryForMonth(MonthPicker.Date);
        }

        private void CalculateAndDisplaySummary(List<Expense> allExpenses)
        {
            try
            {
                _monthlySummary = ExpenseCalculator.CalculateLast30DaysSummary(allExpenses);
                
                // Set total amount for percentage calculations
                TotalAmount = _monthlySummary.TotalAmount;
                
                // Update period label
                PeriodLabel.Text = $"Período: {_monthlySummary.StartDate:dd/MM/yyyy} - {_monthlySummary.EndDate:dd/MM/yyyy}";
                
                // Update total labels
                TotalAmountLabel.Text = $"${_monthlySummary.TotalAmount:F2}";
                TotalExpensesLabel.Text = _monthlySummary.TotalExpenses.ToString();
                
                // Update category summaries with percentages
                CategorySummaries.Clear();
                foreach (var category in _monthlySummary.CategorySummaries)
                {
                    var percentage = _monthlySummary.TotalAmount > 0 
                        ? (category.Total / _monthlySummary.TotalAmount) * 100 
                        : 0;
                    
                    CategorySummaries.Add(new CategorySummaryWithPercentage
                    {
                        Category = category.Category,
                        Total = category.Total,
                        Count = category.Count,
                        Percentage = percentage
                    });
                }
                
                // Show/hide empty state
                EmptyStateFrame.IsVisible = !_monthlySummary.CategorySummaries.Any();
                CategoryCollectionView.IsVisible = _monthlySummary.CategorySummaries.Any();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", $"Erro ao calcular relatório: {ex.Message}", "OK");
            }
        }

        private void CalculateAndDisplaySummaryForMonth(DateTime monthDate)
        {
            try
            {
                _monthlySummary = ExpenseCalculator.CalculateMonthlySummary(_allExpenses, monthDate.Year, monthDate.Month);

                TotalAmount = _monthlySummary.TotalAmount;

                PeriodLabel.Text = $"Período: {_monthlySummary.StartDate:dd/MM/yyyy} - {_monthlySummary.EndDate:dd/MM/yyyy}";

                TotalAmountLabel.Text = $"${_monthlySummary.TotalAmount:F2}";
                TotalExpensesLabel.Text = _monthlySummary.TotalExpenses.ToString();

                CategorySummaries.Clear();
                foreach (var category in _monthlySummary.CategorySummaries)
                {
                    var percentage = _monthlySummary.TotalAmount > 0
                        ? (category.Total / _monthlySummary.TotalAmount) * 100
                        : 0;

                    CategorySummaries.Add(new CategorySummaryWithPercentage
                    {
                        Category = category.Category,
                        Total = category.Total,
                        Count = category.Count,
                        Percentage = percentage
                    });
                }

                EmptyStateFrame.IsVisible = !_monthlySummary.CategorySummaries.Any();
                CategoryCollectionView.IsVisible = _monthlySummary.CategorySummaries.Any();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", $"Erro ao calcular relatório: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnCategoryTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is CategorySummaryWithPercentage item)
            {
                try
                {
                    DateTime? startDate = null;
                    DateTime? endDate = null;
                    bool isCustomPeriod = false;

                    if (_isCustomPeriodView)
                    {
                        startDate = StartDatePicker.Date;
                        endDate = EndDatePicker.Date;
                        isCustomPeriod = true;
                    }
                    else
                    {
                        // For month view, calculate the month boundaries
                        var monthDate = MonthPicker.Date;
                        startDate = new DateTime(monthDate.Year, monthDate.Month, 1);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        isCustomPeriod = false;
                    }

                    await Navigation.PushAsync(new CategoryExpensesPage(item.Category, startDate, endDate, isCustomPeriod));
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao abrir gastos da categoria: {ex.Message}", "OK");
                }
            }
        }

        private void OnMonthSelected(object sender, DateChangedEventArgs e)
        {
            if (_isCustomPeriodView) return;

            // Normalize to first day of month to avoid drifting when selecting the same month on different days
            var normalized = new DateTime(e.NewDate.Year, e.NewDate.Month, 1);
            if (MonthPicker.Date != normalized)
            {
                MonthPicker.Date = normalized;
                return;
            }

            CalculateAndDisplaySummaryForMonth(normalized);
        }

        private void OnCustomPeriodChanged(object sender, DateChangedEventArgs e)
        {
            if (!_isCustomPeriodView) return;

            // Validate date range
            if (StartDatePicker.Date > EndDatePicker.Date)
            {
                DisplayAlert("Erro", "A data inicial não pode ser maior que a data final.", "OK");
                return;
            }

            CalculateAndDisplaySummaryForCustomPeriod(StartDatePicker.Date, EndDatePicker.Date);
        }

        private void OnViewToggleClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            
            if (button == MonthViewButton)
            {
                _isCustomPeriodView = false;
                MonthViewButton.BackgroundColor = Color.FromArgb("#1E90FF"); // DodgerBlue
                MonthViewButton.TextColor = Colors.White;
                CustomPeriodButton.BackgroundColor = Colors.LightGray;
                CustomPeriodButton.TextColor = Colors.Black;
                
                MonthPicker.IsVisible = true;
                CustomPeriodStack.IsVisible = false;
                
                CalculateAndDisplaySummaryForMonth(MonthPicker.Date);
            }
            else if (button == CustomPeriodButton)
            {
                _isCustomPeriodView = true;
                CustomPeriodButton.BackgroundColor = Color.FromArgb("#1E90FF"); // DodgerBlue
                CustomPeriodButton.TextColor = Colors.White;
                MonthViewButton.BackgroundColor = Colors.LightGray;
                MonthViewButton.TextColor = Colors.Black;
                
                MonthPicker.IsVisible = false;
                CustomPeriodStack.IsVisible = true;
                
                CalculateAndDisplaySummaryForCustomPeriod(StartDatePicker.Date, EndDatePicker.Date);
            }
        }

        private void CalculateAndDisplaySummaryForCustomPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                _monthlySummary = ExpenseCalculator.CalculateCustomPeriodSummary(_allExpenses, startDate, endDate);

                TotalAmount = _monthlySummary.TotalAmount;

                PeriodLabel.Text = $"Período: {_monthlySummary.StartDate:dd/MM/yyyy} - {_monthlySummary.EndDate:dd/MM/yyyy}";

                TotalAmountLabel.Text = $"${_monthlySummary.TotalAmount:F2}";
                TotalExpensesLabel.Text = _monthlySummary.TotalExpenses.ToString();

                CategorySummaries.Clear();
                foreach (var category in _monthlySummary.CategorySummaries)
                {
                    var percentage = _monthlySummary.TotalAmount > 0
                        ? (category.Total / _monthlySummary.TotalAmount) * 100
                        : 0;

                    CategorySummaries.Add(new CategorySummaryWithPercentage
                    {
                        Category = category.Category,
                        Total = category.Total,
                        Count = category.Count,
                        Percentage = percentage
                    });
                }

                EmptyStateFrame.IsVisible = !_monthlySummary.CategorySummaries.Any();
                CategoryCollectionView.IsVisible = _monthlySummary.CategorySummaries.Any();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", $"Erro ao calcular relatório: {ex.Message}", "OK");
            }
        }


        //private async void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.CurrentSelection.FirstOrDefault() is CategorySummaryWithPercentage categorySummary)
        //    {
        //        try
        //        {
        //            await Navigation.PushAsync(new CategoryExpensesPage(categorySummary.Category));

        //            // Clear selection to allow clicking the same item again
        //            CategoryCollectionView.SelectedItem = null;
        //        }
        //        catch (Exception ex)
        //        {
        //            await DisplayAlert("Erro", $"Erro ao abrir gastos da categoria: {ex.Message}", "OK");
        //        }
        //    }
        //}
    }
}
