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

        public MonthReportPage(List<Expense> allExpenses)
        {
            InitializeComponent();
            CategorySummaries = new ObservableCollection<CategorySummaryWithPercentage>();
            BindingContext = this;
            
            CalculateAndDisplaySummary(allExpenses);
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
                    await Navigation.PushAsync(new CategoryExpensesPage(item.Category));
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao abrir gastos da categoria: {ex.Message}", "OK");
                }
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
