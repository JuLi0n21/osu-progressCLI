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

    // get data from osu website (api) call here or through api controller or refactor
    // Extract the relevant data from the JSON
    data.forEach(entry => {
        console.log(entry);
        date.push(entry[0].Date);
        BeatmapSetid.push(entry[1].BeatmapSetid);
        Beatmapid.push(entry[2].Beatmapid);
        Osufilename.push(entry[3].Osufilename);
        Ar.push(entry[4].Ar);
        Cs.push(entry[5].Cs);
        Hp.push(entry[6].Hp);
        Od.push(entry[7].Od);
        Status.push(entry[8].Status);
        StarRating.push(parseFloat(entry[9].StarRating));
        Bpm.push(entry[10].Bpm);
        Artist.push(entry[11].Artist);
        Creator.push(entry[12].Creator);
        Username.push(entry[13].Username);
        Accuracy.push(parseFloat(entry[14].Accuracy));
        MaxCombo.push(entry[15].MaxCombo);
        Score.push(entry[16].Score);
        Combo.push(entry[17].Combo);
        Hit50.push(entry[18].Hit50);
        Hit100.push(entry[19].Hit100);
        Hit300.push(entry[20].Hit300);
        Ur.push(entry[21].Ur);
        HitMiss.push(entry[22].HitMiss);
        Mode.push(entry[23].Mode);
        Mods.push(entry[24].Mods);
    });


    const ctx = document.getElementById('myChart').getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: Osufilename,
            datasets: [
                {
                    label: '50',
                    data: Hit50,
                    backgroundColor: 'rgba(220, 135, 39, 0.8)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1,
                    yAxisID: 'y',
                    order: 1,
                },
                {
                    label: '100',
                    data: Hit100,
                    backgroundColor: 'rgba(63, 220, 39, 0.8)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1,
                    yAxisID: 'y',
                    order: 1,
                },
                {
                    label: 'X',
                    data: HitMiss,
                    backgroundColor: 'rgba(204, 19, 19, 0.8)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1,
                    yAxisID: 'y',
                    order: 1,
                },
                {
                    label: 'Stars',
                    data: StarRating,
                    backgroundColor: 'rgba(204, 19, 19, 0.8)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1,
                    type: 'line',
                    yAxisID: 'y2',
                    order: 0,
                },
                {
                    label: 'Bpm',
                    data: Bpm,
                    backgroundColor: 'rgba(204, 19, 19, 0.8)',
                    borderColor: 'rgba(0, 0, 0, 1)',
                    borderWidth: 1,
                    type: 'line',
                    yAxisID: 'y3',
                    order: 0,
                },
            ]
        },
        options: {
            plugins: {
                title: {
                    display: true,
                    text: 'ScoreExample'
                },
                legend: {

                }
            },
            responsive: false,
            scales: {
                x: {
                    stacked: true,
                },
                y: {
                    stacked: true
                },
                y2: {
                    position: 'right',

                    ticks: {
                        color: 'rgba(204, 19, 19, 0.8)'
                    },
                    grid: {
                        drawOnChartArea: false // only want the grid lines for one axis to show up
                    }
                },
                y3: {
                    position: 'right',

                    ticks: {
                        color: 'rgba(243, 19, 19, 0.8)'
                    },
                    grid: {
                        drawOnChartArea: false // only want the grid lines for one axis to show up
                    }
                },
            }
        }
    });
}

function renderPie(data) {
    // Extract labels and values from the fetched data
    const labels = [];
    const values = [];

    for (let i = 0; i < data.length; i++) {
        const item = data[i];
        if (item.Key !== "" && item.Value >= 10000) {
            labels.push(item.Key);
            item.Value = item.Value / 3600000;
            values.push(item.Value);
        }
    }

    // Create an array of random colors for each segment
    const colors = values.map(() => '#' + (Math.random() * 0xFFFFFF << 0).toString(16));

    console.log(labels, values)
    // Data for the pie chart
    const pieData = {
        labels: labels,
        datasets: [{
            data: values,
            backgroundColor: colors,
        }]
    };

    // Options for the pie chart
    const options = {
        responsive: false,
        maintainAspectRatio: false,
    };

    // Get the canvas element
    const pie = document.getElementById('pieChart').getContext('2d');
    const myChart = new Chart(pie, {
        type: 'pie',
        data: pieData,
        options: options
    });
}

function renderTimewastedPie(data) {
    // Extract labels and values from the fetched data
    const labels = [];
    const values = [];
    const colors = [];

    for (let i = 0; i < data.length; i++) {
        const item = data[i];
        if (item.Key !== "" && item.Value >= 10000) {
            labels.push(item.Key);
            item.Value = item.Value / 3600000;
            values.push(item.Value);
            colors.push('#' + (Math.random() * 0xFFFFFF << 0).toString(16));
        }
    }

    // Create an array of random colors for each segment

    console.log(labels, values)
    // Data for the pie chart
    const pieData = {
        labels: labels,
        datasets: [{
            data: values,
            backgroundColor: colors,
        }]
    };

    // Options for the pie chart
    const options = {
        responsive: false,
        maintainAspectRatio: false,
    };

    // Get the canvas element
    const pie = document.getElementById('pieTimeWastedChart').getContext('2d');
    const myChart = new Chart(pie, {
        type: 'pie',
        data: pieData,
        options: options
    });
}

document.getElementById('loadDataButton').addEventListener('click', function () {
    fetch('/api/beatmaps')
        .then(response => response.json())
        .then(data => {
            console.log(data);
            renderChart(data);
        })
        .catch(error => {
            console.error('Error loading data:', error);
        });

    fetch('/api/banchotime')
        .then(response => response.json())
        .then(data => {
            console.log(data);
            renderPie(data);
        })
        .catch(error => {
            console.error('Error loading data:', error);
        });

    fetch('/api/timewasted')
        .then(response => response.json())
        .then(data => {
            console.log(data);
            renderTimewastedPie(data);
        })
        .catch(error => {
            console.error('Error loading data:', error);
        });
});

// Trigger the initial data load when the page loads (optional)
window.addEventListener('DOMContentLoaded', function () {
    // You can choose to load data immediately or wait for the button click
    // For example, you can uncomment the line below to load data on page load:
    // document.getElementById('loadDataButton').click();
});