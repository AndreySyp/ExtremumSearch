using System.Windows;
using System.Windows.Input;

namespace ExtremumSearch;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void TextBox_OnlyNumber(object sender, TextCompositionEventArgs e)
    {
        if (!int.TryParse(e.Text, out _) && !(e.Text == ".") && !(e.Text == "-"))
            e.Handled = true;
    }
}