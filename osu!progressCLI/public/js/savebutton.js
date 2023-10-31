// Save config settings
function saveSettings() {
    const username = document.getElementById('username_input').value;
    const rank = document.getElementById('rank_input').value;
    const country = document.getElementById('country_input').value;
    const avatarUrl = document.getElementById('avatarurl_input').value;
    const coverUrl = document.getElementById('coverurl_input').value;
    const port = document.getElementById('port_input').value;
    const userid = document.getElementById('userid').value;
    const localConfigEnabled = document.getElementById('localsettingstoggle').checked;

    const data = {
        username: username,
        rank: rank,
        country: country,
        avatarUrl: avatarUrl,
        coverUrl: coverUrl,
        port: port,
        localsettings: localConfigEnabled,
        userid: userid,
    };

        // Send a POST request to the api/save endpoint
        fetch('api/save', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
            .then(response => {
                if (response.ok) {
                    return response.json(); // Assuming the server sends a JSON response
                } else {
                    throw new Error('Failed to save settings');
                }
            })
            .then(data => {
                console.log(data);
                alert("Settings saved, to Avoid overloading the Osu api will users only be refreshed every 5 minutes (or restart the program)");
                location.reload();

            })
            .catch(error => {
                console.error(error);
                alert('An error occurred while saving settings. Please try again later.');
            });
}