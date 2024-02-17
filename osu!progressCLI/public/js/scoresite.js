let beatmapid, rowid, myChart;
const audioPlayer = document.getElementById("audioPlayer");

function updateVolume() {
    var volumeSlider = document.getElementById('volumeSlider');

    audioPlayer.volume = volumeSlider.value;
}

function fetchOsuBeatmap() {
    const urlParams = new URLSearchParams(window.location.search);
    const rowid = urlParams.get("id");

    beatmapid = document.getElementById("beatmapid").value;
    osufilename = document.getElementById("Osufilename").value;
    const playButton = document.getElementById("playButton");
    audioPlayer.volume = 0.2;
    playButton.addEventListener("click", function () {
        event.preventDefault();
        if (audioPlayer.paused) {
            audioPlayer.play();
            playButton.innerHTML = '<i class="fas fa-pause"></i>';
        } else {
            audioPlayer.pause();
            audioPlayer.currentTime = 0;
            playButton.innerHTML = '<i class="fas fa-play"></i>';
        }
    });

    let from = document.getElementById("fromDate").value;
    let to = document.getElementById("toDate").value;

    let apiUrl = `/api/beatmaps/search?query=&from=${from}&to=${to}&Beatmapid=${beatmapid}&Osufilename=${osufilename}`;

    fetch(apiUrl)
        .then((response) => response.json())
        .then((data) => {
            console.log(data);

            createLineChart("mapprogress", data, rowid);
        })
        .catch((error) => {
            console.error("Error fetching recent score data:", error);
        });

    function createLineChart(canvasId, data, highlightedScoreId) {
        const filteredData = data.filter((entry) => entry.Time > 5);

        filteredData.reverse();

        console.log(highlightedScoreId);
        const dates = filteredData.map((entry) => entry.Date);
        const accValues = filteredData.map((entry) => entry.Acc);
        const ppValues = filteredData.map((entry) => entry.PP);
        const comboValues = filteredData.map((entry) => entry.Combo);

        const highlightedScoreIndex =
            highlightedScoreId !== undefined
                ? filteredData.findIndex((entry) => entry.id == highlightedScoreId)
                : -1;

        console.log(filteredData);
        console.log(highlightedScoreId);
        console.log(highlightedScoreIndex);

        const ctx = document.getElementById(canvasId).getContext("2d");
        ctx;
        if (myChart) {
            myChart.destroy();
        }

        myChart = new Chart(ctx, {
            type: "line",
            data: {
                labels: dates,
                datasets: [
                    {
                        label: "Acc",
                        data: accValues,
                        borderColor: "blue",
                        borderWidth: 2,
                        fill: false,
                        yAxisID: "y",
                    },
                    {
                        label: "PP",
                        data: ppValues,
                        borderColor: "green",
                        borderWidth: 2,
                        fill: false,
                        yAxisID: "y2",
                    },
                    {
                        label: "Combo",
                        data: comboValues,
                        borderColor: "red",
                        borderWidth: 2,
                        fill: false,
                        yAxisID: "y3",
                    },
                ],
            },
            options: {
                interaction: {
                    intersect: false
                },
                responsive: true,
                plugins: {
                    annotation: {
                        annotations: [
                            {
                                type: "line",
                                mode: "vertical",
                                scaleID: "x",
                                value: highlightedScoreIndex,
                                borderColor: "rgba(255, 255, 255, 0.1)",
                                borderWidth: 3,
                                label: {
                                    content: "Vertical Line",
                                    enabled: true,
                                    position: "top",
                                },
                            },
                        ],
                    },
                },
                legend: {
                    labels: {
                        color: "rgba(255, 102, 171, 1)",
                        font: {
                            size: 14,
                        },
                    },
                },
                scales: {
                    x: {
                        title: {
                            display: false,
                        },
                        ticks: {
                            color: "rgba(255, 102, 171, 1)",
                        },
                        grid: {
                            display: true,
                        },
                    },
                    y: {
                        position: "left",
                        title: {
                            display: true,
                            text: "Acc",
                            color: "blue",
                        },
                        ticks: {
                            color: "rgba(255, 102, 171, 1)",
                        },
                        grid: {
                            display: false,
                        },
                    },
                    y2: {
                        position: "right",
                        title: {
                            display: true,
                            text: "PP",
                            color: "green",
                        },
                        ticks: {
                            color: "rgba(255, 102, 171, 1)",
                        },
                        grid: {
                            display: false,
                        },
                    },
                    y3: {
                        position: "right",
                        title: {
                            display: true,
                            text: "Combo",
                            color: "red",
                        },
                        ticks: {
                            color: "rgba(255, 102, 171, 1)",
                        },
                        grid: {
                            display: true,
                        },
                    },
                },
            },
        });
    }
}

fetchOsuBeatmap();