function loaddata() {
    {

        fetch('/api/banchotime')
            .then(response => response.json())
            .then(data => {
                {
                    //console.log(data);
                    renderPie(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });

        fetch('/api/timewasted')
            .then(response => response.json())
            .then(data => {
                {
                    //console.log(data);
                    renderTimewastedPie(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });

        fetch('/api/beatmaps/averages')
            .then(response => response.json())
            .then(data => {
                {
                    //console.log(data);
                    createchart(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });
        fetch('/api/timewastedbyday')
            .then(response => response.json())
            .then(data => {
                {
                    //console.log(data);
                    createTimeWastedChart(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });
        fetch('/api/banchotimebyday')
            .then(response => response.json())
            .then(data => {
                {
                    //console.log(data);
                    createBanchoTimeChart(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });


    }
}


// Trigger the initial data load when the page loads (optional)
window.addEventListener('DOMContentLoaded', function () {
    {

        loaddata();
    }
});