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
                    label: 'Average BPM',
                    data: averageBpm,
                    yAxisID: 'bpm',
                    borderColor: 'blue',
                    fill: false,
                },
                {
                    label: 'Average SR',
                    data: averageSR,
                    yAxisID: 'sr',
                    borderColor: 'red',
                    fill: false,
                },
                {
                    label: 'Average Accuracy',
                    data: averageAccuracy,
                    yAxisID: 'acc',
                    borderColor: 'green',
                    fill: false,
                },
                {
                    label: 'Average AR',
                    data: averageAr,
                    yAxisID: 'arcshpod',
                    borderColor: 'purple',
                    fill: false,
                },
                {
                    label: 'Average CS',
                    data: averageCs,
                    yAxisID: 'arcshpod',
                    borderColor: 'orange',
                    fill: false,
                },
                {
                    label: 'Average HP',
                    data: averageHp,
                    yAxisID: 'arcshpod',
                    borderColor: 'brown',
                    fill: false,
                },
                {
                    label: 'Average OD',
                    data: averageOd,
                    yAxisID: 'arcshpod',
                    borderColor: 'pink',
                    fill: false,
                },
            ],
        },
        options: {
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'day',
                        displayFormats: {
                            day: 'D MMM YYYY',
                        },
                    },
                },
                bpm: {
                    position: 'right',
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'BPM',
                        color: 'red',
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
                    grid: {
                        drawOnChartArea: true,
                    },
                },
            },
        },
    });
}
