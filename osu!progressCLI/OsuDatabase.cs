using OsuParsers.Database;

namespace osu_progressCLI
{
    internal class OsuDatabase
    {
        private static OsuDatabase instance;

        private OsuDatabase()
        {
            showmeallthescore();
        }

        public static OsuDatabase Instance()
        {
            if (instance == null)
            {
                instance = new OsuDatabase();
            }

            return instance;
        }

        private void showmeallthescore()
        {
            string[] scoredbs = Directory.GetFiles("scoredbs");
            int count = 0;
            foreach (var item in scoredbs)
            {
                ScoresDatabase scores = OsuParsers.Decoders.DatabaseDecoder.DecodeScores(item);
                int newcount = scores.Scores.Count;
                count += newcount;
                Console.WriteLine($"scores found in {item}: {newcount}");
                scores.Scores.ForEach(score =>
                {
                    // List<Score> scores1 = score.Item2.ToList(); 


                });
            }

            Console.WriteLine($"Total Scores: {count}");
        }
    }
}
