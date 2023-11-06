//     < !--Charts -->
//    <div class="" border-b rounded-lg backdrop--light"" >
//      <div style=""height: 0px"" >
//      <canvas id="" myChart"" ></canvas >
//      </div >
//    </div >

function renderChart(data) {
  const date = [];
  const BeatmapSetid = [];
  const Beatmapid = [];
  const Osufilename = [];
  const Ar = [];
  const Cs = [];
  const Hp = [];
  const Od = [];
  const Status = [];
  const StarRating = [];
  const Bpm = [];
  const Artist = [];
  const Creator = [];
  const Username = [];
  const Accuracy = [];
  const MaxCombo = [];
  const Score = [];
  const Combo = [];
  const Hit50 = [];
  const Hit100 = [];
  const Hit300 = [];
  const Ur = [];
  const HitMiss = [];
  const Mode = [];
  const Mods = [];

  // Extract the relevant data from the JSON
  data.forEach((entry) => {
    date.push(entry.Date);
    BeatmapSetid.push(entry.BeatmapSetid);
    Beatmapid.push(entry.Beatmapid);
    Osufilename.push(entry.Osufilename.slice(0, 5) + "...");
    Ar.push(entry.Ar);
    Cs.push(entry.Cs);
    Hp.push(entry.Hp);
    Od.push(entry.Od);
    Status.push(entry.Status);
    StarRating.push(parseFloat(entry.StarRating));
    Bpm.push(entry.Bpm);
    Artist.push(entry.Artist);
    Creator.push(entry.Creator);
    Username.push(entry.Username);
    Accuracy.push(parseFloat(entry.Accuracy));
    MaxCombo.push(entry.MaxCombo);
    Score.push(entry.Score);
    Combo.push(entry.Combo);
    Hit50.push(entry.Hit50);
    Hit100.push(entry.Hit100);
    Hit300.push(entry.Hit300);
    Ur.push(entry.Ur);
    HitMiss.push(entry.HitMiss);
    Mode.push(entry.Mode);
    Mods.push(entry.Mods);
  });

  const ctx = document.getElementById("myChart").getContext("2d");

  const validStarRatingValues = StarRating.filter((value) => !isNaN(value));
  const starRatingAverage = (
    validStarRatingValues.reduce((sum, value) => sum + value, 0) /
    validStarRatingValues.length
  ).toFixed(2);

  const validBpmValues = Bpm.filter((value) => !isNaN(value));
  const BpmAverage = (
    validBpmValues.reduce((sum, value) => sum + value, 0) /
    validBpmValues.length
  ).toFixed(2);

  console.log(starRatingAverage);
  const existingChart = Chart.getChart("myChart");
  if (existingChart) {
    existingChart.destroy();
  }

  const myChart = new Chart(ctx, {
    type: "bar",
    data: {
      labels: Osufilename,
      datasets: [
        {
          label: "50",
          data: Hit50,
          backgroundColor: "rgba(220, 135, 39, 0.8)",
          borderColor: "rgba(0, 0, 0, 1)",
          borderWidth: 1,
          yAxisID: "y",
          order: 1,
        },
        {
          label: "100",
          data: Hit100,
          backgroundColor: "rgba(63, 220, 39, 0.8)",
          borderColor: "rgba(0, 0, 0, 1)",
          borderWidth: 1,
          yAxisID: "y",
          order: 1,
        },
        {
          label: "X",
          data: HitMiss,
          backgroundColor: "rgba(204, 19, 19, 0.8)",
          borderColor: "rgba(0, 0, 0, 1)",
          borderWidth: 1,
          yAxisID: "y",
          order: 1,
        },
        {
          label: "Stars",
          data: StarRating,
          backgroundColor: "rgba(204, 19, 19, 0.8)",
          borderColor: "rgba(0, 0, 0, 1)",
          borderWidth: 1,
          type: "scatter",
          yAxisID: "y2",
          order: 0,
        },
        {
          label: "Bpm",
          data: Bpm,
          backgroundColor: "rgba(204, 19, 19, 0.8)",
          borderColor: "rgba(0, 0, 0, 1)",
          borderWidth: 1,
          type: "scatter",
          yAxisID: "y3",
          order: 0,
        },
        {
          data: Array(StarRating.length).fill(starRatingAverage),
          fill: false,
          borderColor: "rgba(250, 192, 32, 0.8)",
          borderWidth: 3,
          type: "line",
          yAxisID: "y4",
          order: 0,
          pointRadius: 0,
          label: "Stars average",
        },
        {
          data: Array(Bpm.length).fill(BpmAverage),
          fill: false,
          borderColor: "rgba(250, 192, 32, 0.8)",
          borderWidth: 3,
          type: "line",
          yAxisID: "y5",
          order: 0,
          pointRadius: 0,
          label: "Bpm average",
        },
      ],
    },
    options: {
      plugins: {
        title: {
          display: true,
          text: "ScoreExample",
        },
        legend: {},
      },
      responsive: true,
      scales: {
        x: {
          stacked: true,
        },
        y: {
          stacked: true,
        },
        y2: {
          position: "right",
          ticks: {
            color: "rgba(204, 19, 19, 0.8)",
          },
          grid: {
            drawOnChartArea: false, // only want the grid lines for one axis to show up
          },
        },
        y3: {
          position: "right",
          ticks: {
            color: "rgba(243, 19, 19, 0.8)",
          },
          grid: {
            drawOnChartArea: false, // only want the grid lines for one axis to show up
          },
        },
        y4: {
          position: "right",
          suggestedMax: Math.max(...StarRating), // Adjust to your data's max value
          suggestedMin: 0,
          ticks: {
            color: "rgba(250, 192, 32, 0.8)",
            display: false, // Hide ticks for this axis
          },
          grid: {
            drawOnChartArea: false,
          },
        },
        y5: {
          position: "right",
          suggestedMax: Math.max(...Bpm), // Adjust to your data's max value
          suggestedMin: 0,
          ticks: {
            color: "rgba(250, 192, 32, 0.8)",
            display: false, // Hide ticks for this axis
          },
          grid: {
            drawOnChartArea: false,
          },
        },
      },
    },
  });
}
