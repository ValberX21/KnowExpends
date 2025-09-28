using KnowExpends.Mobile.Models;
using KnowExpends.Mobile.Enums;

namespace KnowExpends.Mobile.Services
{
    public class ExpenseCalculator
    {
        public class CategorySummary
        {
            public ExpenseCategory Category { get; set; }
            public decimal Total { get; set; }
            public int Count { get; set; }
        }

        public class MonthlySummary
        {
            public List<CategorySummary> CategorySummaries { get; set; }
            public decimal TotalAmount { get; set; }
            public int TotalExpenses { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public static MonthlySummary CalculateLast30DaysSummary(List<Expense> allExpenses)
        {
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-30);

            // Filter expenses from last 30 days
            var last30DaysExpenses = allExpenses
                .Where(e => e.CreatedDate.Date >= startDate && e.CreatedDate.Date <= endDate)
                .ToList();

            // Group by category and calculate totals
            var categorySummaries = last30DaysExpenses
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Price),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            return new MonthlySummary
            {
                CategorySummaries = categorySummaries,
                TotalAmount = last30DaysExpenses.Sum(e => e.Price),
                TotalExpenses = last30DaysExpenses.Count,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public static string FormatSummaryText(MonthlySummary summary)
        {
            var text = $"📊 RESUMO DOS ÚLTIMOS 30 DIAS\n";
            text += $"📅 Período: {summary.StartDate:dd/MM/yyyy} - {summary.EndDate:dd/MM/yyyy}\n\n";

            if (summary.CategorySummaries.Any())
            {
                text += "💰 GASTOS POR CATEGORIA:\n";
                foreach (var category in summary.CategorySummaries)
                {
                    var percentage = summary.TotalAmount > 0 
                        ? (category.Total / summary.TotalAmount * 100) 
                        : 0;
                    
                    text += $"• {category.Category}: ${category.Total:F2} ({category.Count} gastos) - {percentage:F1}%\n";
                }
            }
            else
            {
                text += "❌ Nenhum gasto encontrado no período.\n";
            }

            text += $"\n💵 TOTAL GERAL: ${summary.TotalAmount:F2}\n";
            text += $"📈 Total de gastos: {summary.TotalExpenses}";

            return text;
        }
    }
}



