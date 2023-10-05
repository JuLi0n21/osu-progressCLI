// Save config settings
function saveSettings() {
    const clientId = document.getElementById('ClientId').value;
    const clientSecret = document.getElementById('ClientSecret').value;
    const username = document.getElementById('username_input').value;
    const rank = document.getElementById('rank_input').value;
    const country = document.getElementById('country_input').value;
    const avatarUrl = document.getElementById('avatarurl_input').value;
    const coverUrl = document.getElementById('coverurl_input').value;
    const port = document.getElementById('port_input').value;
    const userid = document.getElementById('userid').value;
    const localConfigEnabled = document.getElementById('localsettingstoggle').checked;

    const data = {
        clientId: clientId,
        clientSecret: clientSecret,
        username: username,
        rank: rank,
        country: country,
        avatarUrl: avatarUrl,
        coverUrl: coverUrl,
        port: port,
        localsettings: localConfigEnabled,
        userid: userid
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
                alert('Settings saved successfully');
            } else {
                console.error('Failed to save settings');
            }
        })
        .catch(error => {
            alert('An error occurred: ' + error);
        });
}
