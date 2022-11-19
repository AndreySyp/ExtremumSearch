using Sprache.Calc;

namespace ExtremumSearch.Models
{
    internal class Function
    {
        private readonly string function;
        public Function(string function)
        {
            this.function = function;
        }
        public double f(double number)
        {
            XtensibleCalculator qwse = new();
            return qwse.ParseExpression(function, x => number).Compile()();
        }
        public double df(double x, double h = 0.01)
        {
            return (f(x + h) - f(x - h)) / (2 * h);
        }
        public double d2f(double x, double h = 0.01)
        {
            return (f(x + h) - 2 * f(x) + f(x - h)) / (h * h);
        }
        public double d3f(double x, double h = 0.01)
        {
            return (f(x + h) - 3 * f(x) + 3 * f(x - h) - f(x - 2 * h)) / (h * h * h);
        }
    }
}