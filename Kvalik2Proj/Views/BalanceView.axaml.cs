using Avalonia.Controls;
using System.Text.RegularExpressions;

namespace Kvalik2Proj.Views;

public partial class BalanceView : UserControl
{
    private bool _formatting;

    public BalanceView()
    {
        InitializeComponent();
        CardNumberBox.TextChanged += OnCardNumberTextChanged;
    }

    private void OnCardNumberTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatting) return;
        _formatting = true;

        var box = CardNumberBox;
        var text = box.Text ?? "";
        var caretPos = box.CaretIndex;

        var digitsBefore = Regex.Replace(text.Substring(0, System.Math.Min(caretPos, text.Length)), @"\D", "").Length;

        var digits = Regex.Replace(text, @"\D", "");
        if (digits.Length > 16)
            digits = digits.Substring(0, 16);

        var formatted = "";
        for (int i = 0; i < digits.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted += " ";
            formatted += digits[i];
        }

        box.Text = formatted;

        int newCaret = 0;
        int count = 0;
        for (int i = 0; i < formatted.Length && count < digitsBefore; i++)
        {
            newCaret = i + 1;
            if (formatted[i] != ' ')
                count++;
        }
        box.CaretIndex = newCaret;

        _formatting = false;
    }
}
