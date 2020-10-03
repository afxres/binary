namespace Mikodev.Binary.Benchmarks.IntegrationTests.Models
{
    public class Type01
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int[] List { get; set; }

        public Type02 Item { get; set; }
    }
}
