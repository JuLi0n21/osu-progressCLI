/*RawStatus
0 = MainMenu
1 = EditingMap
2 = Playing
3 = GameShutDownAnimation
4 = SongSelectEdit
5 = SongSelect
6 =
7 = ResultScreen
8 =
9 =
10 =
11 = MultiplayerRooms
12 = MultiPlayerRoom
13 =
14 =
15 = OsuDirect 
16 = OffsetAssistent
17 =
18 =
19 = ProcessingBeatmaps
*/

/*BanchoStatus
Afk
Idle
Playing
4 Editing
5 MultiplayerRoom
11 MultiplayerRooms
12 MultiplayerPlaying
 */

function renderPie(data) {
    // Extract labels and values from the fetched data
    const labels = [];
    const values = [];

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

  const labelColors = {
  'MultiplayerRooms': 'blue',
  'MultiplayerPlaying': 'purple',
  'Unknown': 'gray',
  'Editing': 'cyan',
  'MultiplayerRoom': 'orange',
  'Afk': 'red',
  'Idle': 'green',
  'Playing': 'pink'
    };

    console.log(data);
    for (let i = 0; i < data.length; i++) {
        let item = data[i].Key;
        let itemvalue = data[i].Value;

        if (item in numberToName && numberToName[item] !== "" && itemvalue >= 10) {
            labels.push(numberToName[item]);
            itemvalue = itemvalue / 3600;
            values.push(itemvalue);
        }
    }

    console.log(labels);
    const colors = labels.map(label => labelColors[label]);

    //console.log(labels, values);
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
        plugins: {
            legend: {
                display: true, 
                position: 'right', 
                labels: {
                    color: "rgba(255, 102, 171, 1)",
                    font: {
                        size: 14
                    }
                }
            },
        },
    };

    // Get the canvas element
    const pie = document.getElementById('pieBanchoTimeChart').getContext('2d');

    const existingChart = Chart.getChart('pieBanchoTimeChart');
    if (existingChart) {
        existingChart.destroy();
    }

    const myChart = new Chart(pie, {
        type: 'pie',
        data: pieData,
        options: options,
    });
}

function renderTimewastedPie(data) {
    // Extract labels and values from the fetched data
    const labels = [];
    const values = [];
    const colors = [];

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

    const pieData = {
        labels: labels,
        datasets: [{
            data: values,
            backgroundColor: colors,
        }]
    };

    const options = {
        responsive: false,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true, 
                position: 'right', 
                labels: {
                    color: "rgba(255, 102, 171, 1)",
                    font: {
                        size: 14
                    }
                }
            },
        },
    };

    const pie = document.getElementById('pieTimeWastedChart').getContext('2d');

    const existingChart = Chart.getChart('pieTimeWastedChart');
    if (existingChart) {
        existingChart.destroy();
    }

    const myChart = new Chart(pie, {
        type: 'pie',
        data: pieData,
        options: options,
    });
}
