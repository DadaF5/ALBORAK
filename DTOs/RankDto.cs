namespace FRAProject.DTOs
{
    public class RankDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string FullRank { get; set; } = string.Empty;

        public int Sequence { get; set; }

        public int RankTypeId { get; set; }
    }
}
