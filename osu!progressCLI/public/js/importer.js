function uploadFile() {
    const fileInput = document.getElementById('fileInput');
    const downloadBeatmaps = document.getElementById('downloadBeatmaps');
    const importScores = document.getElementById('importScores');
    const file = fileInput.files[0];

    if (!file) {
        alert('Please select a file to upload.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    fetch('/api/upload', {
        method: 'POST',
        body: formData,
        headers: {
            'filename': file.name,
            'download': downloadBeatmaps.checked,
            'import': importScores.checked,
        },
    })
        .then((response) => {
            console.log(response);
            if (response.ok) {
                console.log(response);
                alert(response.body.message);
            } else {
                alert(response.body.message);
            }
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('An error occurred while uploading the file.');
        });
}