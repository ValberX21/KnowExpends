using KnowExpends.Mobile.Enums;
using KnowExpends.Mobile.Models;
using KnowExpends.Mobile.Services;

namespace KnowExpends.Mobile
{
    public partial class AddExpensePage : ContentPage
    {
        private DatabaseService _databaseService;
        private Expense _expenseToEdit;

        public AddExpensePage(Expense expenseToEdit = null)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _expenseToEdit = expenseToEdit;
            InitializeCategoryPicker();
            LoadExpenseData();
        }

        private void InitializeCategoryPicker()
        {
            var categories = Enum.GetValues<ExpenseCategory>();
            foreach (var category in categories)
            {
                CategoryPicker.Items.Add(category.ToString());
            }
            
            // Set a default selection to make the picker more responsive
            if (CategoryPicker.Items.Count > 0)
            {
                CategoryPicker.SelectedIndex = 0;
            }
        }

        private void LoadExpenseData()
        {
            if (_expenseToEdit != null)
            {
                TitleEntry.Text = _expenseToEdit.Title;
                PriceEntry.Text = _expenseToEdit.Price.ToString();
                CategoryPicker.SelectedIndex = (int)_expenseToEdit.Category;
                DescriptionEditor.Text = _expenseToEdit.Description;
                
                // Update page title and button text for edit mode
                Title = "Editar Gasto";
                SaveButton.Text = "Atualizar Gasto";
            }
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Erro", "Por favor, digite um título para o gasto.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out decimal price))
            {
                await DisplayAlert("Erro", "Por favor, digite um valor válido para o gasto.", "OK");
                return;
            }

            if (CategoryPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione uma categoria para o gasto.", "OK");
                return;
            }

            try
            {
                Expense expense;
                
                if (_expenseToEdit != null)
                {
                    // Edit mode - update existing expense
                    expense = _expenseToEdit;
                    expense.Title = TitleEntry.Text.Trim();
                    expense.Price = price;
                    expense.Category = (ExpenseCategory)CategoryPicker.SelectedIndex;
                    expense.Description = DescriptionEditor.Text?.Trim() ?? string.Empty;
                    
                    await _databaseService.UpdateExpenseAsync(expense);
                    await DisplayAlert("Sucesso", $"Gasto '{expense.Title}' atualizado com sucesso!", "OK");
                }
                else
                {
                    // Add mode - create new expense
                    expense = new Expense
                    {
                        Title = TitleEntry.Text.Trim(),
                        Price = price,
                        Category = (ExpenseCategory)CategoryPicker.SelectedIndex,
                        Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                        CreatedDate = DateTime.Now
                    };
                    
                    await _databaseService.SaveExpenseAsync(expense);
                    await DisplayAlert("Sucesso", $"Gasto '{expense.Title}' salvo com sucesso!", "OK");
                }
                
                // Navigate back to main page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar gasto: {ex.Message}", "OK");
            }
        }
    }
}
