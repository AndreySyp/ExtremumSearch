using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace ExtremumSearch.Models;

internal class Search
{
    private class IncludedValue
    {
        public double? x1 { set; get; }
        public double x2 { set; get; }
        public double p { set; get; }

        public static void Last(List<IncludedValue> list, out double x, out double p)
        {
            int ind = 0;
            double pmin = list[0].p;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].p < pmin)
                {
                    ind = i;
                    pmin = list[i].p;
                }
            }

            if (list[ind].x1 != null)
            {
                x = (double)list[ind].x1;
                list[ind].x1 = null;
            }
            else
            {
                x = list[ind].x2;
                list.RemoveAt(ind);
            }
            p = pmin;
        }
    }
    private static bool Min(double a, double b)
    {
        return a <= b;
    }
    private static bool Max(double a, double b)
    {
        return a >= b;
    }
    private delegate bool Delegate(double a, double b);

    public static List<ItemGoldenSection> GoldenSection(Function function, out Point solution, ExtremeType et,
                                                        double a, double b, double eps = 0.005, int round = 4)
    {
        double x1 = a + (3 - Math.Sqrt(5)) / 2 * (b - a),
               x2 = a + (Math.Sqrt(5) - 1) / 2 * (b - a);
        Delegate extremum = et == ExtremeType.min ? (new(Min)) : (new(Max));
        List<ItemGoldenSection> list = new();

        for (int i = 0; b - a > eps; i++)
        {
            list.Add(new()
            {
                n = i,
                eps = Math.Round(b - a, round),
                f1 = Math.Round(function.f(x1), round),
                f2 = Math.Round(function.f(x2), round),
                x2 = Math.Round(x2, round),
                x1 = Math.Round(x1, round),
                a = Math.Round(a, round),
                b = Math.Round(b, round)
            });

            if (extremum(list[i].f1, list[i].f2))
            {
                b = x2;
                x2 = x1;
                x1 = a + b - x2;
            }
            else
            {
                a = x1;
                x1 = x2;
                x2 = a + b - x1;
            }
        }

        solution = extremum(list[^1].f1, list[^1].f2)
            ? (new() { X = list[^1].x1, Y = list[^1].f1 })
            : (new() { X = list[^1].x2, Y = list[^1].f2 });

        return list;
    }
    public static List<ItemNewton> Newton(Function function, out Point solution, 
                                          double a, double b, double eps = 0.005, int round = 4)
    {
        double xn, x;
        List<ItemNewton> list = new();
        x = function.f(a) * function.d3f(a) > 0 ? a : b;

        for (int i = 0; Math.Abs(function.df(x)) > eps; i++)
        {
            list.Add(new()
            {
                n = i,
                x = Math.Round(x, round),
                f = Math.Round(function.f(x), round),
                df = Math.Round(function.df(x), round),
                d2f = Math.Round(function.d2f(x), round),
            });
            xn = x - list[^1].df / list[^1].d2f;
            x = xn;
        }
        list.Add(new()
        {
            n = list[^1].n+1,
            x = Math.Round(x, round),
            f = Math.Round(function.f(x), round),
            df = Math.Round(function.df(x), round),
            d2f = Math.Round(function.d2f(x), round),
        });

        solution = new() { X = list[^1].x, Y = list[^1].f };
        return list;
    }
    public static List<ItemBrokenLine> BrokenLine(Function function, out Point solution,
                                  double a, double b, double eps = 0.005, int round = 4)
    {
        a = -1; b = 0;
        List<ItemBrokenLine> datas = new();
        List<IncludedValue> ff = new();
        double L = function.f(a);
        for (double x = a; x < b; x+= eps)
            L = Math.Max(L, function.f(x));

        datas.Add(new()
        {
            xs = Math.Round((function.f(a) - function.f(b) + L * (a + b)) / (2 * L), round),
            ps = Math.Round((function.f(a) + function.f(b) + L * (a - b)) / 2, round),
        });

        double delta = 1 / (2 * L) * (function.f(datas[^1].xs) - datas[^1].ps);
        ff.Add(new IncludedValue
        {
            x1 = datas[^1].xs - delta,
            x2 = datas[^1].xs + delta,
            p = (function.f(datas[^1].xs) + datas[^1].ps) / 2
        });

        datas[^1].eps = Math.Round(datas[^1].xs - datas[^1].ps, round);
        datas[^1].xh = Math.Round((double)ff[^1].x1, round);
        datas[^1].xhh = Math.Round(ff[^1].x2, round);
        datas[^1].ph = Math.Round(ff[^1].p, round);

        for (int i = 0; datas[^1].eps >= eps; i++)
        {
            IncludedValue.Last(ff, out double x, out double p);
            datas.Add(new()
            {
                xs = Math.Round(x, round),
                ps = Math.Round(p, round)
            });

            delta = 1 / (2 * L) * (function.f(datas[^1].xs) - datas[^1].ps);
            ff.Add(new IncludedValue
            {
                x1 = datas[^1].xs - delta,
                x2 = datas[^1].xs + delta,
                p = (function.f(datas[^1].xs) + datas[^1].ps) / 2
            });

            datas[^1].eps = Math.Round(function.f(datas[^1].xs) - datas[^1].ps, round);
            datas[^1].xh = Math.Round((double)ff[^1].x1, round);
            datas[^1].xhh = Math.Round(ff[^1].x2, round);
            datas[^1].ph = Math.Round(ff[^1].p, round);
        }

        solution = new()
        {
            X = datas[^1].xs,
            Y = datas[^1].ps,
        };
        return datas;
    }
}