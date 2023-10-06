function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function createBanchoTimeChart(data) {
    // Extract the dates and labels
    const dates = data.map(item => moment(item.Date).toDate());
    const labels = Object.keys(data[0]).filter(key => key !== "Date");

    const numberToName = {
        '11': 'MultiplayerRooms',
        '12': 'MultiplayerPlaying',
        '13': 'Unknown', // Add a default value for unknown keys
        '4': 'Editing',
        '5': 'MultiplayerRoom',
        '8': 'Unknown', // Add a default value for unknown keys
        'Afk': 'Afk',
        'Idle': 'Idle',
        'Playing': 'Playing'
    };

    const colors = {
        'MultiplayerRooms': 'blue',
        'MultiplayerPlaying': 'purple',
        'Unknown': 'gray',
        'Editing': 'cyan',
        'MultiplayerRoom': 'orange',
        'Afk': 'red',
        'Idle': 'green',
        'Playing': 'pink'
    };

    const datasets = labels.map(label => {
        const name = numberToName[label] || 'Unknown'; // Use the name from numberToName or 'Unknown' as default
        return {
            label: name, // Use the name here
            data: data.map(item => item[label] || 0), // Map the data for the label
            backgroundColor: colors[label] || getRandomColor(),
            stack: "stack",
        };
    });


    const ctx = document.getElementById('BanchoTimeChart').getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dates,
            datasets: datasets
        },
        options: {
            plugins: {
                legend: {
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
                            day: 'YYYY-MM-DD'
                        }
                    },
                    beginAtZero: true,
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {

                    }
                },
                y: {
                    stacked: true,
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {

                    }
                }
            }
        }
    });
}


function createTimeWastedChart(data) {
    // Extract the unique dates


    const numberToScreen = {
        '0': 'Mainmenu',
        '1': 'EditingMap',
        '2': 'Playing',
        '3': 'GameShutDown',
        '4': 'SongSelectEdit',
        '5': 'SongSelect',
        '7': 'ResultScreen',
        '11': 'MultiplayerRooms',
        '12': 'MultiPlayerRoom',
        '15': 'OsuDirect',
        '16': 'OffsetAssistent',
        '19': 'ProcessingBeatmaps',
    }

    for (let i = 0; i < data.length; i++) {
        const item = data[i];
        if (item.Key in numberToScreen && numberToScreen[item.Key] && item.Value >= 10) {
            labels.push(numberToScreen[item.Key]);
            item.Value = item.Value / 3600;
            values.push(item.Value);
            colors.push('#' + (Math.random() * 0xFFFFFF << 0).toString(16));
        }
    }


    const dates = [...new Set(data.map(entry => entry.Date))];

    // Extract the labels and values for each date
    const labels = Object.keys(data[0]).filter(key => key !== "Date");

    const colors = {
        "Afk": 'rgba(255, 0, 0, 0.6)',     // Red
        "Playing": 'rgba(0, 255, 0, 0.6)', // Green
        "Idle": 'rgba(0, 0, 255, 0.6)'    // Blue
    };

    const datasets = labels.map(label => {
        const name = numberToScreen[label] || 'Unknown'; // Use the name from numberToScreen or 'Unknown' as default
        return {
            label: name, // Use the name here
            data: dates.map(date => {
                const entry = data.find(entry => entry.Date === date);
                return entry[label] || 0;
            }),
            backgroundColor: colors[label] || getRandomColor(),
            stack: "stack",
        };
    });


    const ctx = document.getElementById('TimeWastedChart').getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dates,
            datasets: datasets
        },
        options: {
            plugins: {
                legend: {
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
                            day: 'YYYY-MM-DD'
                        }
                    },
                    beginAtZero: true,
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        
                    }
                },
                y: {
                    stacked: true,
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                      
                    }
                }
            }
        }
    });
}
