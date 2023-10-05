function renderPie(data) {
    // Extract labels and values from the fetched data
    const labels = [];
    const values = [];

    const labelColors = {
        "Afk": "red",
        "Editing": "cyan",
        "Playing": "rgb(204, 51, 255)",
        "Idle": "green",
    };

    for (let i = 0; i < data.length; i++) {
        const item = data[i];
        if (item.Key !== "" && item.Value >= 10) {
            labels.push(item.Key);
            item.Value = item.Value / 3600;
            values.push(item.Value);
        }
    }

    const colors = labels.map(label => labelColors[label]);

    console.log(labels, values);
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
                display: true, // Set to true to display the legend
                position: 'right', // Position of the legend (you can adjust it as needed)
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

    for (let i = 0; i < data.length; i++) {
        const item = data[i];
        if (item.Key !== "" && item.Value >= 10) {
            labels.push(item.Key);
            item.Value = item.Value / 3600;
            values.push(item.Value);
            colors.push('#' + (Math.random() * 0xFFFFFF << 0).toString(16));
        }
    }

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
                display: true, // Set to true to display the legend
                position: 'right', // Position of the legend (you can adjust it as needed)
            },
        },
    };

    // Get the canvas element
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
