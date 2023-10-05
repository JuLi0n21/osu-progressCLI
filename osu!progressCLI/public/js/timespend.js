function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function createBanchoTimeChart(data) {
    // Extract the unique dates
    var dates = [...new Set(data.map(entry => entry.Date))];

    // Extract the labels and values for each date
    var labels = Object.keys(data[0]).filter(key => key !== "Date");

    var colors = {
        "Afk": 'rgba(255, 0, 0, 0.6)',     // Red
        "Playing": 'rgba(0, 255, 0, 0.6)', // Green
        "Idle": 'rgba(0, 0, 255, 0.6)'    // Blue
    };

    var datasets = [];
    labels.forEach(label => {
        if (label in colors) {
            datasets.push({
                label: label,
                data: [],
                backgroundColor: colors[label],
                yAxisID: "sharedAxis", // Specify the same axis ID for "Afk," "Playing," and "Idle"
                stack: "stack", // Stack the values
            });
        } else {
            datasets.push({
                label: label,
                data: [],
                backgroundColor: getRandomColor(), // Assign random colors for other categories
                yAxisID: "sharedAxis",
                stack: "stack",
            });
        }
    });

    dates.forEach(date => {
        var values = data.find(entry => entry.Date === date);
        labels.forEach(label => {
            datasets.find(dataset => dataset.label === label).data.push(values[label] || 0);
        });
    });

    var ctx = document.getElementById('BanchoTimeChart').getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dates,
            datasets: datasets
        },
        options: {
            scales: {
                x: {
                    beginAtZero: true,
                },
                y: {
                    stacked: true,
                    id: "sharedAxis", // Define the shared axis with the same ID
                }
            }
        }
    });
}

function createTimeWastedChart(data) {
    // Extract the unique dates
    var dates = [...new Set(data.map(entry => entry.Date))];

    // Extract the labels and values for each date
    var labels = Object.keys(data[0]).filter(key => key !== "Date");

    var colors = {
        "Afk": 'rgba(255, 0, 0, 0.6)',     // Red
        "Playing": 'rgba(0, 255, 0, 0.6)', // Green
        "Idle": 'rgba(0, 0, 255, 0.6)'    // Blue
    };

    var datasets = [];
    labels.forEach(label => {
        if (label in colors) {
            datasets.push({
                label: label,
                data: [],
                backgroundColor: colors[label],
                yAxisID: "sharedAxis", // Specify the same axis ID for "Afk," "Playing," and "Idle"
                stack: "stack", // Stack the values
            });
        } else {
            datasets.push({
                label: label,
                data: [],
                backgroundColor: getRandomColor(), // Assign random colors for other categories
                yAxisID: "sharedAxis",
                stack: "stack",
            });
        }
    });

    dates.forEach(date => {
        var values = data.find(entry => entry.Date === date);
        labels.forEach(label => {
            datasets.find(dataset => dataset.label === label).data.push(values[label] || 0);
        });
    });

    var ctx = document.getElementById('TimeWastedChart').getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dates,
            datasets: datasets
        },
        options: {
            scales: {
                x: {
                    beginAtZero: true,
                },
                y: {
                    stacked: true,
                    id: "sharedAxis", // Define the shared axis with the same ID
                }
            }
        }
    });
}
