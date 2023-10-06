function createchart(data) {
    const threeMonthsAgo = moment().subtract(3, 'months');

    // Filter data to include only data points from the last 3 months
    const filteredData = data.filter(item => moment(item.Date, "YYYY-MM-DD HH:mm:ss.SSSSSSS").isAfter(threeMonthsAgo));

    const dates = filteredData.map(item => moment(item.Date, "YYYY-MM-DD HH:mm:ss.SSSSSSS").toDate());
    const averageBpm = filteredData.map(item => item.AverageBpm);
    const averageSR = filteredData.map(item => item.AverageSR);
    const averageAccuracy = filteredData.map(item => item.AverageAccuracy);
    const averageAr = filteredData.map(item => item.AverageAr);
    const averageCs = filteredData.map(item => item.AverageCs);
    const averageHp = filteredData.map(item => item.AverageHp);
    const averageOd = filteredData.map(item => item.AverageOd);

    const ctx = document.getElementById('averageschart').getContext('2d');

    const myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: [
                {
                    label: 'BPM',
                    data: averageBpm,
                    yAxisID: 'bpm',
                    borderColor: 'blue',
                    fill: false,
                },
                {
                    label: 'SR',
                    data: averageSR,
                    yAxisID: 'sr',
                    borderColor: 'red',
                    fill: false,
                },
                {
                    label: 'Acc',
                    data: averageAccuracy,
                    yAxisID: 'acc',
                    borderColor: 'green',
                    fill: false,
                },
                {
                    label: 'AR',
                    data: averageAr,
                    yAxisID: 'arcshpod',
                    borderColor: 'purple',
                    fill: false,
                },
                {
                    label: 'CS',
                    data: averageCs,
                    yAxisID: 'arcshpod',
                    borderColor: 'orange',
                    fill: false,
                },
                {
                    label: 'HP',
                    data: averageHp,
                    yAxisID: 'arcshpod',
                    borderColor: 'brown',
                    fill: false,
                },
                {
                    label: 'OD',
                    data: averageOd,
                    yAxisID: 'arcshpod',
                    borderColor: 'pink',
                    fill: false,
                },
            ],
        },
        options: {
            plugins: {
                legend: {
                  //  position: 'right', 
                    labels: {
                        color: "rgba(255, 102, 171, 1)",
                        font: {
                            size: 14
                        }
                    }
                }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'day',
                        displayFormats: {
                            day: 'D MMM YYYY',
                        },
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                },
                bpm: {
                    position: 'right',
                    beginAtZero: true,
                    suggestedMin: 0,
                    title: {
                        display: true,
                        text: 'BPM',
                        color: 'red',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                },
                sr: {
                    position: 'right',
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Star Rating',
                        color: 'blue',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                },
                acc: {
                    position: 'left',
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Accuracy',
                        color: 'green',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                    plugins: {
                        regression: {
                            type: 'linear',
                        },
                    },
                },
                arcshpod: {
                    position: 'left',
                    beginAtZero: true,
                    suggestedMax: 12,
                    suggestedMin: 0,
                    title: {
                        display: true,
                        text: 'Map Attributes',
                        color: 'white',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        drawOnChartArea: true,
                    },
                },
            },
        },
    });
}
