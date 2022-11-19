using ExtremumSearch.Infrastructure;
using ExtremumSearch.Models;
using ExtremumSearch.ViewModels.Base;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Sprache.Calc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ExtremumSearch.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    #region Datas
    public ObservableCollection<string> MyItems { get; set; }

    private MethodType _selectedMethod = MethodType.GoldenSection;
    public MethodType SelectedMethod
    {
        get { return _selectedMethod; }
        set
        {
            Enum.TryParse(value.ToString(), out MethodType myStatus);
            _selectedMethod = myStatus;
        }
    }

    private ExtremeType _selectedExtremum = ExtremeType.min;
    public ExtremeType SelectedExtremum
    {
        get { return _selectedExtremum; }
        set
        {
            if (_selectedExtremum == value)
                return;

            _selectedExtremum = value;
            OnPropertyChanged("Priority");
            OnPropertyChanged("IsLowPriority");
        }
    }
    public bool IsMin
    {
        get { return SelectedExtremum == ExtremeType.min; }
        set { SelectedExtremum = value ? ExtremeType.min : SelectedExtremum; }
    }
    public bool IsMax
    {
        get { return SelectedExtremum == ExtremeType.max; }
        set { SelectedExtremum = value ? ExtremeType.max : SelectedExtremum; }
    }

    private ChartValues<Point> solutionFunction = new();
    private ChartValues<Point> valuesFunction = new();
    private ChartValues<Point> axisX = new();

    public DataTable Grid { get; set; } = new();
    public SeriesCollection? Chart { get; set; } = new();

    public string InputEquation { get; set; } = "x*x+Sin(x)";
    public double InputIntervalA { get; set; } = -1.0;
    public double InputIntervalB { get; set; } = 0.0;
    public double InputError { get; set; } = 0.005;
    public double OutputX { get; private set; }
    public double OutputY { get; private set; }

    #endregion

    #region Command
    public ICommand _SolCommand;
    public ICommand SolCommand => _SolCommand ??= new LambdaCommand(OnSolCommandExecute, CanSolCommandExecute);

    private bool CanSolCommandExecute(object p) => true;
    private void OnSolCommandExecute(object p)
    {
        solutionFunction.Clear();
        valuesFunction.Clear();
        axisX.Clear();

        Solve();
        ChartOutput();

        OnPropertyChanged("OutputX");
        OnPropertyChanged("OutputY");
        OnPropertyChanged("Grid");
        OnPropertyChanged("Chart");
    }
    #endregion

    private void Solve()
    {
        Grid = new DataTable();
        solutionFunction.Clear();
        Point solution;

        switch (_selectedMethod)
        {
            case MethodType.GoldenSection:
                // Solve.
                List<ItemGoldenSection> dataGoldenSection = Search.GoldenSection(new(InputEquation.ToString()),
                    out solution, _selectedExtremum, InputIntervalA, InputIntervalB, InputError);

                // Grid add data.
                Grid.Columns.Add("n");
                Grid.Columns.Add("eps");
                Grid.Columns.Add("a");
                Grid.Columns.Add("b");
                Grid.Columns.Add("x1");
                Grid.Columns.Add("x2");
                Grid.Columns.Add("f(x1)");
                Grid.Columns.Add("f(x2)");
                for (int i = 0; i < dataGoldenSection.Count; i++)
                    Grid.Rows.Add(new object[]
                    {
                        dataGoldenSection[i].n,
                        dataGoldenSection[i].eps,
                        dataGoldenSection[i].a,
                        dataGoldenSection[i].b,
                        dataGoldenSection[i].x1,
                        dataGoldenSection[i].x2,
                        dataGoldenSection[i].f1,
                        dataGoldenSection[i].f2,
                    });
                break;
            case MethodType.Newton:
                // Solve.
                List<ItemNewton> dataNewton = Search.Newton(new(InputEquation.ToString()),
                    out solution, InputIntervalA, InputIntervalB, InputError);

                // Grid add data.
                Grid.Columns.Add("n");
                Grid.Columns.Add("x");
                Grid.Columns.Add("f");
                Grid.Columns.Add("df");
                Grid.Columns.Add("d2f");
                for (int i = 0; i < dataNewton.Count; i++)
                    Grid.Rows.Add(new object[]
                    {
                        dataNewton[i].n,
                        dataNewton[i].x,
                        dataNewton[i].f,
                        dataNewton[i].df,
                        dataNewton[i].d2f
                    });
                break;
            case MethodType.BrokenLine:
                // Solve.
                List<ItemBrokenLine> dataBrokenLine = Search.BrokenLine(new(InputEquation.ToString()),
                    out solution, InputIntervalA, InputIntervalB, InputError);

                // Grid add data.
                Grid.Columns.Add("x*");
                Grid.Columns.Add("p*");
                Grid.Columns.Add("eps");
                Grid.Columns.Add("x'");
                Grid.Columns.Add("x''");
                Grid.Columns.Add("p");
                for (int i = 0; i < dataBrokenLine.Count; i++)
                    Grid.Rows.Add(new object[]
                    {
                        dataBrokenLine[i].xs,
                        dataBrokenLine[i].ps,
                        dataBrokenLine[i].eps,
                        dataBrokenLine[i].xh,
                        dataBrokenLine[i].xhh,
                        dataBrokenLine[i].ph
                    });
                break;
            default:
                break;
        }

        solutionFunction.Add(solution);
        OutputX = solutionFunction[0].X;
        OutputY = solutionFunction[0].Y;

    }
    private void ChartOutput()
    {
        // Axis points.
        axisX.Add(new Point() { X = InputIntervalA, Y = 0 });
        axisX.Add(new Point() { X = InputIntervalB, Y = 0 });

        // Function points.
        XtensibleCalculator calc = new();
        for (double i = InputIntervalA; i <= InputIntervalB; i += (InputIntervalB - InputIntervalA) / 10)
        {
            Point point = new()
            {
                X = Math.Round(i, 4),
                Y = Math.Round(calc.ParseExpression(InputEquation, x => i).Compile()(), 4)
            };
            valuesFunction.Add(point);
        }

        // Chart.
        Chart = new SeriesCollection
        {
            new LineSeries // Function.
            {
                Configuration = new CartesianMapper<Point>()
                                                .X(point => point.X)
                                                .Y(point => point.Y),
                Name = "Function",
                Values = valuesFunction,
                PointGeometry = null,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Blue
            },
            new LineSeries // Solution.
            {
                Configuration = new CartesianMapper<Point>()
                                                .X(point => point.X)
                                                .Y(point => point.Y),
                Name = "Solution",
                Values = solutionFunction,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red
            },
            new LineSeries // Axis.
            {
                Configuration = new CartesianMapper<Point>(){ }
                                                .X(point => point.X)
                                                .Y(point => point.Y),
                Name = "X",
                Values = axisX,
                PointGeometry = null,
                Stroke = Brushes.Black,
                Fill = Brushes.Transparent
            }
        };

        //DataContext = this;
    }

    public MainWindowViewModel()
    {
        MyItems = new ObservableCollection<string>
        {
            "GoldenSection",
            //"Tanget",
            //"BrokenLine",
            "Newton"
        };
    }
}