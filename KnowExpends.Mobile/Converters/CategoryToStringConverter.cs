using System.Globalization;
using KnowExpends.Mobile.Enums;

namespace KnowExpends.Mobile.Converters
{
    public class CategoryToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExpenseCategory category)
            {
                return ToDisplayName(category);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not used in this app
            return null;
        }

        public static string ToDisplayName(ExpenseCategory category)
        {
            return category switch
            {
                ExpenseCategory.PassagemBus => "Passagem-Bus",
                ExpenseCategory.UBER => "UBER",
                ExpenseCategory.Higiene => "Higiene",
                ExpenseCategory.ContasParents => "Contas parents",
                _ => category.ToString()
            };
        }
    }
}


