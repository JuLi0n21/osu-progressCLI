using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_progressCLI
{
    public class OsuDbsExposer
    {
        private static OsuParsers.Database.OsuDatabase Osudb { get; set; } = null;
        private static OsuParsers.Database.ScoresDatabase Scoredb { get; set; } = null;
        private static OsuParsers.Database.CollectionDatabase Collectiondb { get; set; } = null;
        private static OsuParsers.Database.PresenceDatabase Presencedb { get; set; } = null;

        static OsuDbsExposer()
        {
            AllParseDBs();
        }

        static void AllParseDBs()
        {
            string filepath = $"{Credentials.Instance.GetConfig().osufolder}";
            string file = "/osu!.db";
            if (File.Exists(filepath + file))
            {
                using (
                    FileStream fileStream = new FileStream(
                        filepath + file,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        bufferSize: 4096,
                        useAsync: true
                    )
                )
                {
                    Console.WriteLine($"Parsing {file}");
                    Osudb = OsuParsers.Decoders.DatabaseDecoder.DecodeOsu(
                        $"{Credentials.Instance.GetConfig().osufolder}{file}"
                    );
                    Console.WriteLine($"Parsed {file}");
                }
            }

            file = "/collection.db";
            if (File.Exists(filepath + file))
            {
                using (
                    FileStream fileStream = new FileStream(
                        filepath + file,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        bufferSize: 4096,
                        useAsync: true
                    )
                )
                {
                    Console.WriteLine($"Parsing {file}");
                    Collectiondb = OsuParsers.Decoders.DatabaseDecoder.DecodeCollection(
                        $"{filepath}{file}"
                    );
                    Console.WriteLine($"Parsed {file}");
                }
            }

            file = "/scores.db";
            if (File.Exists(filepath + file))
            {
                using (
                    FileStream fileStream = new FileStream(
                        filepath + file,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        bufferSize: 4096,
                        useAsync: true
                    )
                )
                {
                    Console.WriteLine($"Parsing {file}");
                    Scoredb = OsuParsers.Decoders.DatabaseDecoder.DecodeScores($"{filepath}{file}");
                    Console.WriteLine($"Parsed {file}");
                }
            }

            file = "/presence.db";
            if (File.Exists(filepath + file))
            {
                using (
                    FileStream fileStream = new FileStream(
                        filepath + file,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        bufferSize: 4096,
                        useAsync: true
                    )
                )
                {
                    Console.WriteLine($"Parsing {file}");
                    Presencedb = OsuParsers.Decoders.DatabaseDecoder.DecodePresence(
                        $"{filepath}{file}"
                    );
                    Console.WriteLine($"Parsed {file}");
                }
            }
        }

        public static void MatchallScorestobeatmaps()
        {
            Scoredb.Scores.ForEach(s =>
            {
                var beatmap = Osudb.Beatmaps.FirstOrDefault(b => b.MD5Hash == s.Item1);

                if (beatmap != null)
                {
                    foreach (var score in s.Item2)
                    {
                        Console.WriteLine(
                            $"{beatmap.Title} : {score.ScoreId} O {score.BeatmapMD5Hash}"
                        );
                    }
                }
            });
        }

        public static OsuParsers.Database.Objects.DbBeatmap GetBeatmapbyHash(string Hash)
        {
            if(Hash == null || Osudb == null)
            {
                return null;
            }
            return Osudb.Beatmaps.FirstOrDefault(beatmap => beatmap.MD5Hash == Hash);
        }
    }
}
