const searchbar = document.getElementById('mapsearch');
let throttleTimeout;
let lastSearchText = '';

searchbar.addEventListener('input', function () {
    const searchText = searchbar.value.trim();

    if (searchText.length > 3 && searchText !== lastSearchText) {
        lastSearchText = searchText;

        scorecontainer.innerHTML = "";

        clearTimeout(throttleTimeout);
        throttleTimeout = setTimeout(() => {
            const apiUrl = 'api/beatmaps/search';

            fetch(`${apiUrl}?searchquery=${searchText}`)
                .then(response => response.json())
                .then(data => {
                    console.log(data);
                    createScoreElements(data);
                })
                .catch(error => {
                    console.error(error);
                });
        }, 500); // Delay for 2 seconds (2000 milliseconds)
    }

    if (searchText.length == 0) {

        const apiUrl = 'api/beatmaps';

        fetch(apiUrl)
            .then(response => response.json())
            .then(data => {
                console.log(data);
                createScoreElements(data);
            })
            .catch(error => {
                console.error(error);
            });
    }
});
