using System.Collections.ObjectModel;

namespace KnowExpends.Mobile.Models
{
    public class ExpenseGroup : ObservableCollection<Expense>
    {
        public string Date { get; set; }
        public DateTime SortDate { get; set; }

        public ExpenseGroup(string date, DateTime sortDate)
        {
            Date = date;
            SortDate = sortDate;
        }
    }
}

