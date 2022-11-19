namespace ExtremumSearch.Models
{
    internal enum MethodType
    {
            GoldenSection,
            Tanget,
            BrokenLine,
            Newton
    }
    internal enum ExtremeType
    {
        min,
        max
    }

    internal class ItemGoldenSection
    {
        public int n { set; get; }
        public double eps { set; get; }
        public double a { get; set; }
        public double b { get; set; }
        public double x1 { get; set; }
        public double x2 { get; set; }
        public double f1 { get; set; }
        public double f2 { get; set; }
    }
    internal class ItemNewton
    {
        public int n { set; get; }
        public double x { get; set; }
        public double f { get; set; }
        public double df { get; set; }
        public double d2f { get; set; }
    }
    class ItemBrokenLine
    {
        public double xs { get; set; }
        public double ps { get; set; }
        public double eps { get; set; }
        public double xh { get; set; }
        public double xhh { get; set; }
        public double ph { get; set; }
    }
}
