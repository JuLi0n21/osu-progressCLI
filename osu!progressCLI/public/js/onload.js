document.getElementById('loadDataButton').addEventListener('click', function () {
    {
        {
            {
                loaddata();
            }
        }
    }
});

//get data from internal api
function loaddata() {
    {
        fetch('/api/beatmaps')
            .then(response => response.json())
            .then(data => {
                {
                     console.log(data);
                }
            })
            .catch(error => {
                {
                    console.error('Error loading data:', error);
                }
            });

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
        // You can choose to load data immediately or wait for the button click
        // For example, you can uncomment the line below to load data on page load:
        // document.getElementById('loadDataButton').click();
        loaddata();
    }
});